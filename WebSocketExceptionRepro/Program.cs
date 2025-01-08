using System.Net.WebSockets;
using WebSocketExceptionRepro;

using var host = HostFactory.CreateHost();

await host.StartAsync();

var webSocket = new ClientWebSocket();
await webSocket.ConnectAsync(new Uri("ws://localhost:5555/"), CancellationToken.None);

await webSocket.SendAsync(new ArraySegment<byte>(new byte[128]), WebSocketMessageType.Binary, WebSocketMessageFlags.EndOfMessage, CancellationToken.None);
webSocket.Dispose();

await Task.Delay(TimeSpan.FromMinutes(5));

await host.StopAsync();
