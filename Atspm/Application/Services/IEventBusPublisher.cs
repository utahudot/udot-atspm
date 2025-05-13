using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utah.Udot.Atspm.Services
{
    public interface IEventBusPublisher<T>
    {
        Task PublishAsync(T message, CancellationToken ct = default);
    }
}
