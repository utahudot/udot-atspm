using ATSPM.Domain.Common;
using System;
using System.Text.Json;

namespace ATSPM.Application.Analysis.Common
{
    public class RedToRedCycle : StartEndRange, ICycle, ISignalPhase
    {
        #region ICycle

        public string SignalId { get; set; }
        public int PhaseNumber { get; set; }

        public DateTime GreenEvent { get; set; }
        public DateTime YellowEvent { get; set; }

        #region ICycle

        public double TotalGreenTime => (YellowEvent - GreenEvent).TotalSeconds;
        public double TotalYellowTime => (End - YellowEvent).TotalSeconds;
        public double TotalRedTime => (GreenEvent - Start).TotalSeconds;
        public double TotalTime => (End - Start).TotalSeconds;

        #endregion

        #endregion

        public override bool InRange(DateTime time)
        {
            return time >= Start && time <= End;
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
