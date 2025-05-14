using System.Net;

namespace Utah.Udot.Atspm.Infrastructure.Services.Receivers
{
    public interface ITcpReceiver : IDisposable
    {
        Task ReceiveAsync(Func<byte[], EndPoint, Task> onMessage, CancellationToken ct);
    }
}
