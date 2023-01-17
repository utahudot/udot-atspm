using ATSPM.Data.Models;

namespace ATSPM.Application.Extensions
{
    public static class SignalExtensions
    {
        public static string SignalDescription(this Signal signal)
        {
            return $"{signal.SignalId} - {signal.PrimaryName} {signal.SecondaryName}";
        }       
    }
}
