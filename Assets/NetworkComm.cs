using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace NetworkAPI
{
    public class NetworkComm
    {
        private ClientWebSocket ws = new ClientWebSocket();
        public delegate void MsgHandler(string message);
        public event MsgHandler MsgReceived;

        public async Task ConnectToServer(string uri)
        {
            try
            {
                Uri serverUri = new Uri(uri);
                var source = new CancellationTokenSource();
                source.CancelAfter(5000); // Set a connection timeout
                await ws.ConnectAsync(serverUri, source.Token);
                Debug.Log("Connected to WebSocket server at " + uri);
            }
            catch (Exception e)
            {
                Debug.LogError("WebSocket connection error: " + e.Message);
            }
        }

        public async void SendMessage(string message)
        {
            if (ws.State == WebSocketState.Open)
            {
                var encoded = Encoding.UTF8.GetBytes(message);
                var buffer = new ArraySegment<byte>(encoded, 0, encoded.Length);
                await ws.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

        public async Task ReceiveMessages()
        {
            var receiveBuffer = new byte[1024];

            try
            {
                while (ws.State == WebSocketState.Open)
                {
                    var result = await ws.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                    }
                    else
                    {
                        var message = Encoding.UTF8.GetString(receiveBuffer, 0, result.Count);
                        Debug.Log("Received: " + message);

                        // Handle the message (make sure to marshal back to the main thread if interacting with Unity objects)
                        UnityMainThreadDispatcher.Instance().Enqueue(() => MsgReceived?.Invoke(message));
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error receiving WebSocket message: " + e.Message);
            }
        }
    }
}
