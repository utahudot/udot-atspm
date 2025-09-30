using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;

namespace Utah.Udot.Atspm.Infrastructure.Services.Receivers
{
    public class UdpReceiver : IUdpReceiver
    {
        private readonly int _port;
        private readonly ILogger<UdpReceiver> _logger;

        public UdpReceiver(int port, ILogger<UdpReceiver> logger)
        {
            _port = port;
            _logger = logger;
            _logger.LogInformation("UDPReceiver bound and listening on port {Port}", port);
        }

        public Task ReceiveAsync(Func<byte[], EndPoint, Task> handle, CancellationToken ct)
        {
            _logger.LogInformation("UdpReceiver awaiting incoming datagrams...");

            return Task.Run(async () =>
            {
                using var udpClient = new UdpClient(_port);
                while (!ct.IsCancellationRequested)
                {
                    try
                    {
                        var result = await udpClient.ReceiveAsync(ct);
                        _logger.LogDebug("UDP received {Length} bytes from {RemoteEndPoint}", result.Buffer.Length, result.RemoteEndPoint);

                        await handle(result.Buffer, result.RemoteEndPoint);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error during UDP receive loop.");
                        await Task.Delay(100, ct); // avoid tight crash loop
                    }
                }

                _logger.LogInformation("UdpReceiver stopped receiving due to cancellation.");
            }, ct);
        }


        public void Dispose()
        {
        }
    }

}
