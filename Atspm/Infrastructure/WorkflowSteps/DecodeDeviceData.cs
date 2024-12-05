using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks.Dataflow;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.NetStandardToolkit.Workflows;

namespace Utah.Udot.ATSPM.Infrastructure.WorkflowSteps
{
    /// <summary>
    /// Imports and decodes downloaded event logs using the <see cref="IEventLogImporter"/> implementation
    /// </summary>
    public class DecodeDeviceData : TransformManyProcessStepBaseAsync<Tuple<Device, FileInfo>, Tuple<Device, EventLogModelBase>>
    {
        private readonly IServiceScopeFactory _services;

        /// <summary>
        /// Imports and decodes downloaded event logs using the <see cref="IEventLogImporter"/> implementation
        /// </summary>
        /// <param name="services"></param>
        /// <param name="dataflowBlockOptions"></param>
        public DecodeDeviceData(IServiceScopeFactory services, ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            _services = services;
        }

        /// <inheritdoc/>
        protected override IAsyncEnumerable<Tuple<Device, EventLogModelBase>> Process(Tuple<Device, FileInfo> input, CancellationToken cancelToken = default)
        {
            using (var scope = _services.CreateAsyncScope())
            {
                var importer = scope.ServiceProvider.GetService<IEventLogImporter>();

                return importer.Execute(input, cancelToken);
            }
        }
    }
}
