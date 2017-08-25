using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NBinance.API.Push
{
    /// <summary>
    /// Generic class for subscribing to websockets that pump json data
    /// with help from https://gist.github.com/xamlmonkey/4737291
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class JsonWebSocketClient<T> : IDisposable
    {
        const int BUFFER_SIZE = 1024;
        readonly byte[] receiveBuffer = new byte[BUFFER_SIZE];
        readonly ClientWebSocket client;
        readonly Uri uri;

        static readonly Encoding readEncoding = Encoding.UTF8;
        static readonly JsonSerializer jsonSerializer =
            new JsonSerializer { NullValueHandling = NullValueHandling.Ignore };

        bool isConnected = false;
        bool isReceiving = false;
        CancellationToken cancellationToken;

        public JsonWebSocketClient(string uri)
        {
            this.uri = new Uri(uri);
            client = new ClientWebSocket();
        }

        public IOnDataHandler<T> OnDataHandler { get; set; }
        public bool ReconnectOnServerClose { get; set; }

        public Task Connect() { return Connect(CancellationToken.None); }
        public async Task Connect(CancellationToken cancellationToken)
        {
            this.cancellationToken = cancellationToken;

            if (isConnected || client.State == WebSocketState.Connecting)
                return;

            await client.ConnectAsync(uri, cancellationToken);
            isConnected = true;

            Console.WriteLine($"Connected to {uri}");

            BeginReceiveMessages();
        }

        async void BeginReceiveMessages()
        {
            isReceiving = true;

            while (isReceiving && client.State == WebSocketState.Open)
            {
                var receiveResult = await client.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), cancellationToken);

                // close socket if requested by server, return
                if (receiveResult.MessageType == WebSocketMessageType.Close)
                {
                    await CloseSocket(receiveResult.CloseStatusDescription);
                    return;
                }

                var messageLength = receiveResult.Count;

                // skip over 0-length messages
                if (messageLength <= 0)
                {
                    OnZeroLengthMessage();
                    break;
                }

                // currently cannot handle partially sent messages
                if (!receiveResult.EndOfMessage)
                {
                    OnIncompleteMessage();
                    break;
                }

                if (receiveResult.MessageType == WebSocketMessageType.Binary)
                {
                    ProcessBinaryMessage();
                }
                else if (receiveResult.MessageType == WebSocketMessageType.Text)
                {
                    var tick = DeserializeJsonToObject();
                    OnDataHandler?.HandleData(tick);
                }

                Array.Clear(receiveBuffer, 0, messageLength);

                T DeserializeJsonToObject()
                {
                    using (var ms = new MemoryStream(receiveBuffer, 0, messageLength, false))
                    using (var sr = new StreamReader(ms, readEncoding, false))
                    using (var reader = new JsonTextReader(sr))
                    {
                        return jsonSerializer.Deserialize<T>(reader);
                    }
                }

                void OnZeroLengthMessage() { }
                void OnIncompleteMessage() { }
                void ProcessBinaryMessage() { }
            }
        }

        void OnHeartBeat()
        {

        }

        async Task CloseSocket(string closeDescription, bool isClientInitiated = false)
        {
            OnDisconnect();

            await client.CloseAsync(
                WebSocketCloseStatus.NormalClosure, 
                closeDescription ?? "Server requested closure", 
                CancellationToken.None);

            if (isClientInitiated && ReconnectOnServerClose)
            {
                await Connect(cancellationToken);
            }

            void OnDisconnect()
            {
                isReceiving = false;
                isConnected = false;
            }
        }

        public async void Dispose()
        {
            try
            {
                await CloseSocket("Client requested closure", true);
                client.Dispose();
            }
            catch { }
        }
    }
}