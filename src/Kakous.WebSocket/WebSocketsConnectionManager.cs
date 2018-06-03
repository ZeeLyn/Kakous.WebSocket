using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Kakous.WebSocket
{
	public class WebSocketsConnectionManager
	{
		private readonly ConcurrentDictionary<string, System.Net.WebSockets.WebSocket> _connectioner = new ConcurrentDictionary<string, System.Net.WebSockets.WebSocket>();

		public string AddSocket(System.Net.WebSockets.WebSocket socket, string connectionid = "")
		{
			if (string.IsNullOrWhiteSpace(connectionid))
				connectionid = Guid.NewGuid().ToString();
			_connectioner.AddOrUpdate(connectionid, socket, (key, oldvalue) => socket);
			return connectionid;
		}

		public async Task RemoveSocket(string connectionid)
		{
			try
			{
				if (_connectioner.TryRemove(connectionid, out var sc))
				{
					await sc.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "leave", CancellationToken.None);
				}
			}
			catch
			{
				// ignored
			}
		}

		public System.Net.WebSockets.WebSocket GetSocket(string connectionid)
		{
			return _connectioner.TryGetValue(connectionid, out var sc) ? sc : null;
		}

		public string GetConnectionId(System.Net.WebSockets.WebSocket sc) => _connectioner.FirstOrDefault(p => p.Value == sc).Key;

		public ConcurrentDictionary<string, System.Net.WebSockets.WebSocket> GetAllSocket()
		{
			return _connectioner;
		}
	}
}
