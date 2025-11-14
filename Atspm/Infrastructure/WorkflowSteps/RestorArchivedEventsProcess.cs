using System.Threading.Tasks.Dataflow;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.NetStandardToolkit.Workflows;

namespace Utah.Udot.Atspm.Infrastructure.WorkflowSteps
{
    /// <summary>
    /// Transforms a tuple containing a <see cref="Location"/> and a collection of <see cref="CompressedEventLogBase"/> objects
    /// into a tuple of <see cref="Location"/> and a flattened, filtered collection of <see cref="EventLogModelBase"/> events.
    /// This step is used to "unbox" archived or compressed event logs, extracting and filtering the underlying event data
    /// for further workflow processing and aggregation.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="RestorArchivedEventsProcess"/> class.
    /// </remarks>
    /// <param name="dataflowBlockOptions">Options for configuring the dataflow block execution. Optional.</param>
    public class RestorArchivedEventsProcess(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : TransformProcessStepBase<Tuple<Location, IEnumerable<CompressedEventLogBase>>, Tuple<Location, IEnumerable<EventLogModelBase>>>(dataflowBlockOptions)
    {
        /// <summary>
        /// Processes the input tuple by extracting and filtering event data from the compressed event logs
        /// associated with the specified <see cref="Location"/>.
        /// </summary>
        /// <param name="input">A tuple containing the <see cref="Location"/> and its associated compressed event logs.</param>
        /// <param name="cancelToken">A cancellation token for cooperative cancellation.</param>
        /// <returns>
        /// A task that returns a tuple of <see cref="Location"/> and an enumerable of filtered <see cref="EventLogModelBase"/> events.
        /// </returns>
        protected override Task<Tuple<Location, IEnumerable<EventLogModelBase>>> Process(Tuple<Location, IEnumerable<CompressedEventLogBase>> input, CancellationToken cancelToken = default)
        {
            var location = input.Item1;
            var events = input.Item2
                .SelectMany(m => m.Data)
                .FromSpecification(new EventLogSpecification(location))
                .ToList()
                .AsEnumerable();

            return Task.FromResult(Tuple.Create(location, events));
        }
    }
}
