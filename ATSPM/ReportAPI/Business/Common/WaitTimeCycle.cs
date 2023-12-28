using ATSPM.Data.Models;

namespace ATSPM.ReportApi.Business.Common
{
    /// <summary>
    ///     Data that represents a red to red cycle for a Location phase
    /// </summary>
    public class WaitTimeCycle
    {
        public WaitTimeCycle(DateTime redEvent, DateTime greenEvent)
        {
            PhaseRegisterDroppedCalls = new List<ControllerEventLog>();
            RedEvent = redEvent;
            GreenEvent = greenEvent;
        }

        public List<ControllerEventLog> PhaseRegisterDroppedCalls { get; set; }
        public DateTime RedEvent { get; }
        public DateTime GreenEvent { get; }
    }
}