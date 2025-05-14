using System.Net;
using System.Net.Sockets;

namespace Utah.Udot.Atspm.Infrastructure.Services.Receivers
{
    public class TcpReceiver : ITcpReceiver
    {
        private readonly TcpListener _listener;
        public TcpReceiver(int port)
        {
            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Start();
        }

        public async Task ReceiveAsync(Func<byte[], EndPoint, Task> onMessage, CancellationToken ct)
        {
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
