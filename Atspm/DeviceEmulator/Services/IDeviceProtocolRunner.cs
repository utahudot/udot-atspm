using System.Threading;
using System.Threading.Tasks;

namespace DeviceEmulator.Services
{
    public interface IDeviceProtocolRunner
    {
        Task StartAsync(CancellationToken cancellationToken);
        Task GenerateLogAsync();
    }
}
