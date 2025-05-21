using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;

namespace Utah.Udot.Atspm.Infrastructure.Services.Receivers
{
    public class TcpReceiver : ITcpReceiver
    {
        private readonly TcpListener _listener;
        private readonly ILogger<TcpReceiver> _logger;

        public TcpReceiver(int port, ILogger<TcpReceiver> logger)
        {
            _logger = logger;
            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Start();
            _logger.LogInformation("TCPListener bound and listening on port {Port}", port);
        }

        public async Task ReceiveAsync(Func<byte[], EndPoint, Task> onMessage, CancellationToken ct)
        {
            _logger.LogInformation("TcpReceiver awaiting incoming connections...");
            while (!ct.IsCancellationRequested)
            {
                var client = await _listener.AcceptTcpClientAsync(ct);
                _ = Handle(client, onMessage, ct);
            }
        }

        private async Task Handle(TcpClient client, Func<byte[], EndPoint, Task> onMessage, CancellationToken ct)
        {
            using var ms = new MemoryStream();
            await client.GetStream().CopyToAsync(ms, ct);
            var ep = client.Client.RemoteEndPoint!;
            await onMessage(ms.ToArray(), ep);
            client.Dispose();
        }

        public void Dispose() => _listener.Stop();
    }
}
