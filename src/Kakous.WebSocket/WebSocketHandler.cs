using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Kakous.WebSocket
{
	public abstract class WebSocketHandler
	{
		private WebSocketsConnectionManager WebSocketsConnectionManager { get; }

		protected WebSocketHandler(WebSocketsConnectionManager webSocketsConnectionManager)
		{
			WebSocketsConnectionManager = webSocketsConnectionManager;
		}

		public virtual string OnConnected(HttpContext context, System.Net.WebSockets.WebSocket socket)
		{
			return WebSocketsConnectionManager.AddSocket(socket);
		}
		public virtual string OnConnected(HttpContext context, System.Net.WebSockets.WebSocket socket, string connectionid)
		{
			return WebSocketsConnectionManager.AddSocket(socket, connectionid);
		}

		public virtual async Task OnDisconnected(System.Net.WebSockets.WebSocket socket)
		{
			await WebSocketsConnectionManager.RemoveSocket(WebSocketsConnectionManager.GetConnectionId(socket));
		}

		public async Task SendMessageAsync<T>(System.Net.WebSockets.WebSocket socket, T message)
		{
			try
			{
				if (socket.State != WebSocketState.Open)
					return;
				var msg = JsonConvert.SerializeObject(message);
				await socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(msg)), WebSocketMessageType.Text, true, CancellationToken.None);
			}
			catch
			{
				// ignored
			}
		}

		public async Task SendMessageAsync<T>(string connectionId, T message)
		{
			await SendMessageAsync(WebSocketsConnectionManager.GetSocket(connectionId), message);
		}

		public async Task SendMessageToAllAsync<T>(T message)
		{
			try
			{
				foreach (var pair in WebSocketsConnectionManager.GetAllSocket())
				{
					if (pair.Value.State == WebSocketState.Open)
						await SendMessageAsync(pair.Value, message);
				}
			}
			catch
			{
				// ignored
			}
		}

		public ConcurrentDictionary<string, System.Net.WebSockets.WebSocket> GetAllClient()
		{
			return WebSocketsConnectionManager.GetAllSocket();
		}

		public abstract Task ReceiveAsync(System.Net.WebSockets.WebSocket sender, string message);
	}
}
