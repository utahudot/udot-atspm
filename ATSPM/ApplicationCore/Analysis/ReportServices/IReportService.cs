using ATSPM.Domain.Common;

namespace ATSPM.Application.Analysis.ReportServices
{
    /// <summary>
    /// Report service
    /// </summary>
    /// <typeparam name="Tin"></typeparam>
    /// <typeparam name="Tout"></typeparam>
    public interface IReportService<Tin, Tout> : IExecuteAsyncWithProgress<Tin, Tout, int> { }
}
