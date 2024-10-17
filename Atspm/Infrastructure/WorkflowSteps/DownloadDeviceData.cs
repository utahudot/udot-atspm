using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks.Dataflow;
using Utah.Udot.NetStandardToolkit.Workflows;

namespace Utah.Udot.ATSPM.Infrastructure.WorkflowSteps
{
    /// <summary>
    /// Connects to <see cref="Device"/> and downloads event logs using the applicable <see cref="IDeviceDownloader"/>
    /// </summary>
    public class DownloadDeviceData : TransformManyProcessStepBaseAsync<Device, Tuple<Device, FileInfo>>
    {
        private readonly IServiceScopeFactory _services;

        /// <summary>
        /// Connects to <see cref="Device"/> and downloads event logs using the applicable <see cref="IDeviceDownloader"/>
        /// </summary>
        /// <param name="services"></param>
        /// <param name="dataflowBlockOptions"></param>
        public DownloadDeviceData(IServiceScopeFactory services, ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            _services = services;
        }

        /// <inheritdoc/>
        protected override IAsyncEnumerable<Tuple<Device, FileInfo>> Process(Device input, CancellationToken cancelToken = default)
        {
            using (var scope = _services.CreateAsyncScope())
            {
                var downloader = scope.ServiceProvider.GetServices<IDeviceDownloader>().First(c => c.CanExecute(input));

                return downloader.Execute(input, cancelToken);
            }
        }
    }
}
