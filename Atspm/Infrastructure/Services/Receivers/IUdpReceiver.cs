using System.Net;

namespace Utah.Udot.Atspm.Infrastructure.Services.Receivers
{
    public interface IUdpReceiver : IDisposable
    {
        /// <summary>
        /// Start receiving datagrams; returns when cancelled.
        /// </summary>
        Task ReceiveAsync(Func<byte[], EndPoint, Task> onDatagram, CancellationToken ct);
    }
}
