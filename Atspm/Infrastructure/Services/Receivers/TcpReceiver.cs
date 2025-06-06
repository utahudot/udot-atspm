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
                _logger.LogInformation("Waiting for TCP connection...");
                var client = await _listener.AcceptTcpClientAsync(ct);
                _logger.LogInformation("TCP connection accepted from {RemoteEndPoint}", client.Client.RemoteEndPoint);


                // Enable TCP KeepAlive
     
                client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

                _ = Handle(client, onMessage, ct);
            }
        }


        private async Task Handle(TcpClient client, Func<byte[], EndPoint, Task> onMessage, CancellationToken ct)
        {
            try
            {
                using (client)
                using (var stream = client.GetStream())
                using (var ms = new MemoryStream())
                {
                    var buffer = new byte[1024];
                    int bytesRead;

                    // Read until the client closes the sending side (graceful shutdown)
                    while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, ct)) > 0)
                    {
                        ms.Write(buffer, 0, bytesRead);
                    }

                    var ep = client.Client.RemoteEndPoint!;
                    await onMessage(ms.ToArray(), ep);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling TCP client connection from {Remote}", client.Client.RemoteEndPoint);
            }
        }



        public void Dispose() => _listener.Stop();
    }
}
