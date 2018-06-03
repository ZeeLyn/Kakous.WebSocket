using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Kakous.WebSocket;
using Microsoft.AspNetCore.Http;

namespace Example
{
	public class MyWebSocketHandler : WebSocketHandler
	{
		public MyWebSocketHandler(WebSocketsConnectionManager webSocketsConnectionManager) : base(webSocketsConnectionManager)
		{
		}

		public override string OnConnected(HttpContext context, WebSocket socket)
		{
			return base.OnConnected(context, socket, "1");
		}

		public override async Task ReceiveAsync(WebSocket sender, string message)
		{
			await SendMessageAsync("1", "收到：" + message);

		}
	}
}
