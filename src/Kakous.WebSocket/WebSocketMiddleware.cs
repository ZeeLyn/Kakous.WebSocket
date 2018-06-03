using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Kakous.WebSocket
{
	public class WebSocketMiddleware
	{
		private readonly RequestDelegate _next;
		private WebSocketHandler WebSocketHandler { get; }

		public WebSocketMiddleware(RequestDelegate next, WebSocketHandler webSocketHandler)
		{
			_next = next;
			WebSocketHandler = webSocketHandler;
		}

		public async Task Invoke(HttpContext context)
		{
			if (!context.WebSockets.IsWebSocketRequest)
				return;
			var sc = await context.WebSockets.AcceptWebSocketAsync();
			WebSocketHandler.OnConnected(context, sc);
			await Receive(sc, async (result, buffer) =>
			 {
				 if (result.MessageType == WebSocketMessageType.Text)
				 {
					 var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
					 await WebSocketHandler.ReceiveAsync(sc, message);
				 }
				 else if (result.MessageType == WebSocketMessageType.Close)
				 {
					 await WebSocketHandler.OnDisconnected(sc);
				 }
			 });
			await _next.Invoke(context);
		}

		private async Task Receive(System.Net.WebSockets.WebSocket socket, Action<WebSocketReceiveResult, byte[]> handleMessage)
		{
			try
			{
				var buffer = new byte[1024 * 4];
				while (socket.State == WebSocketState.Open)
				{
					var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
					handleMessage(result, buffer);
				}
				await WebSocketHandler.OnDisconnected(socket);
			}
			catch
			{
				// ignored
			}
		}
	}
}
