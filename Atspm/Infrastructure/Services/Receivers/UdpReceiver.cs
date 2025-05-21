using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Utah.Udot.Atspm.Infrastructure.Services.Receivers
{
    public class UdpReceiver : IUdpReceiver
    {
        private readonly Socket _socket;
        private readonly ILogger<UdpReceiver> _logger;

        public UdpReceiver(int port, ILogger<UdpReceiver> logger)
        {
            _logger = logger;
            // Specify IPv4, datagram (UDP) protocol
            _socket = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Dgram,
                ProtocolType.Udp
            );

            _socket.Bind(new IPEndPoint(IPAddress.Any, port));
            _logger.LogInformation("UDPReceiver bound and listening on port {Port}", port);
        }

        public async Task ReceiveAsync(Func<byte[], EndPoint, Task> onDatagram, CancellationToken ct)
        {
            _logger.LogInformation("UdpReceiver awaiting incoming datagrams...");
            var buffer = new byte[65536];
            EndPoint ep = new IPEndPoint(IPAddress.Any, 0);

            while (!ct.IsCancellationRequested)
            {
                var len = _socket.ReceiveFrom(buffer, ref ep);
                await onDatagram(buffer[..len], ep);
            }
        }

        public void Dispose() => _socket.Dispose();
    }

}
