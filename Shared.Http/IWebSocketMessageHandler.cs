using System.IO;

namespace Shared.Http
{
    interface IWebSocketMessageHandler
    {
        void OnMessageReceived(MemoryStream ms);
    }
}