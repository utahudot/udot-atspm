using ATSPM.ReportApi.Business;
using ATSPM.ReportApi.Business.TimeSpaceDiagram;

namespace ATSPM.ReportApi.ReportServices
{
    /// <summary>
    /// Time space diagram report service
    /// </summary>
    public class TimeSpaceDiagramReportService : ReportServiceBase<TimeSpaceDiagramOptions, IEnumerable<TimeSpaceDiagramResults>>
    {
        /// <inheritdoc/>
        public override Task<IEnumerable<TimeSpaceDiagramResults>> ExecuteAsync(TimeSpaceDiagramOptions parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            return default;
        }
    }
}
