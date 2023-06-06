using ATSPM.Domain.Common;

namespace ATSPM.Application.Analysis.Common
{
    public interface ICycle : IStartEndRange
    {
        //DateTime GreenEvent { get; set; }
        //DateTime YellowEvent { get; set; }

        double TotalGreenTime { get; }
        double TotalYellowTime { get; }
        double TotalRedTime { get; }
        double TotalTime { get; }
    }
}
