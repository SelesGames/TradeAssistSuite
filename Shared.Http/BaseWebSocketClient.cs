using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Shared.Http
{
    /// <summary>
    /// Generic class for subscribing to websockets that pump json data
    /// with help from https://gist.github.com/xamlmonkey/4737291
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class BaseWebSocketClient : IDisposable
    {
        const int BUFFER_SIZE = 16 * 1024; // from http://referencesource.microsoft.com/#System/net/System/Net/WebSockets/WebSocketHelpers.cs,285b8b64a4da6851
        readonly ArraySegment<byte> clientBuffer = ClientWebSocket.CreateClientBuffer(BUFFER_SIZE, BUFFER_SIZE);
        readonly ClientWebSocket client;
        readonly Uri uri;

        static readonly Encoding readEncoding = Encoding.UTF8;

        bool isConnected = false;
        bool isReceiving = false;
        CancellationToken cancellationToken;

        public BaseWebSocketClient(string uri)
        {
            this.uri = new Uri(uri);
            client = new ClientWebSocket();
        }

        public bool ReconnectOnServerClose { get; set; }
        public IWebSocketMessageHandler BinaryMessageHandler { get; set; }
        public IWebSocketMessageHandler TextMessageHandler { get; set; }

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
                WebSocketReceiveResult receiveResult = null;

                using (var ms = new MemoryStream())
                {
                    do
                    {
                        receiveResult = await client.ReceiveAsync(clientBuffer, CancellationToken.None);
                        ms.Write(clientBuffer.Array, clientBuffer.Offset, receiveResult.Count);
                    }
                    while (!receiveResult.EndOfMessage);


                    // close socket if requested by server, return
                    if (receiveResult.MessageType == WebSocketMessageType.Close)
                    {
                        await CloseSocket(receiveResult.CloseStatusDescription);
                        return;
                    }

                    if (receiveResult.MessageType == WebSocketMessageType.Binary)
                    {
                        BinaryMessageHandler?.OnMessageReceived(ms);
                    }
                    else if (receiveResult.MessageType == WebSocketMessageType.Text)
                    {
                        TextMessageHandler?.OnMessageReceived(ms);
                    }
                }
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