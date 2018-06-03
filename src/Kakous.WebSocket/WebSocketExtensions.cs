using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Kakous.WebSocket
{
	public static class WebSocketExtensions
	{
		public static IServiceCollection AddKakousWebSocket(this IServiceCollection services)
		{
			services.AddSingleton<WebSocketsConnectionManager>();
			foreach (var type in Assembly.GetEntryAssembly().ExportedTypes)
			{
				if (type.GetTypeInfo().BaseType == typeof(WebSocketHandler))
					services.AddSingleton(type);
			}

			return services;
		}

		public static IApplicationBuilder UseKakousWebSocket(this IApplicationBuilder app, PathString path,
			WebSocketHandler handler)
		{
			app.UseWebSockets();
			return app.Map(path, _app => _app.UseMiddleware<WebSocketMiddleware>(handler));
		}
	}
}
