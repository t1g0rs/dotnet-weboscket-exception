using System.Diagnostics;
using System.Net.WebSockets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace WebSocketExceptionRepro;

public static class HostFactory
{
    public static IHost CreateHost()
    {
        var hostBuilder = new HostBuilder();
        hostBuilder.ConfigureServices(svc => { });
        hostBuilder.ConfigureWebHost(webHostBuilder =>
        {
            //webHostBuilder.UseHttpSys(c => { c.UrlPrefixes.Add("https://+:5555/"); });
            webHostBuilder.UseKestrel(c =>
            {
                c.ListenLocalhost(5555);
            });


            webHostBuilder.Configure(ConfigureApp);
        });


        return hostBuilder.Build();
    }

    public static void ConfigureApp(IApplicationBuilder app)
    {
        app.UseWebSockets();
        app.Use(async (ctx, next) =>
        { 
            await ListenSocket(ctx);
            await next(ctx);
        });

    }

    private static async Task ListenSocket(HttpContext context)
    {
        var acceptContext = new WebSocketAcceptContext { KeepAliveInterval = Timeout.InfiniteTimeSpan };

        try
        {
            using var webSocket = await context.WebSockets.AcceptWebSocketAsync(acceptContext).ConfigureAwait(false);

            ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[1024]);

            while (true)
            {
                try
                {
                    _ = await webSocket.ReceiveAsync(buffer, CancellationToken.None).ConfigureAwait(false);
                }
                catch (WebSocketException e)
                {
                    // Network lost, can stop listening
                    break;
                }
            }

            // DoWork(); asynchronous work to cleanup after websocket connectivity had been lost

            // Simulate HttpContext had been Uninitialized, while websocket processing was running in background
            (context as DefaultHttpContext)?.Uninitialize();
        }
        catch (ObjectDisposedException e)
        {

            // Should never be reached 
            Debug.Assert(false);
        }
    }
}