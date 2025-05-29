using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utah.Udot.Atspm.Services
{
    public interface IEventPublisher<T>
    {
        Task PublishAsync(T message, CancellationToken ct = default);

        Task PublishAsync(IReadOnlyList<T> batch, CancellationToken ct = default);
    }
}
