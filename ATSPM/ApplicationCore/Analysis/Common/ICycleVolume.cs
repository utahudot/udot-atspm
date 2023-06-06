using ATSPM.Domain.Common;

namespace ATSPM.Application.Analysis.Common
{
    public interface ICycleVolume : IStartEndRange //: ICycle
    {
        double TotalDelay { get; }
        double TotalVolume { get; }
    }
}
