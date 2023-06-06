using ATSPM.Application.Enums;
using ATSPM.Domain.Common;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace ATSPM.Application.Analysis.Common
{
    public class CycleArrivals : StartEndRange, ICycleArrivals, ISignalPhase
    {
        private readonly ICycle _cycle = new RedToRedCycle();

        public CycleArrivals() { }

        public CycleArrivals(ICycle cycle)
        {
            _cycle = cycle;
            Start = _cycle.Start;
            End = _cycle.End;

            if (cycle is ISignalPhase sp)
            {
                SignalId = sp.SignalId;
                PhaseNumber = sp.PhaseNumber;
            }
        }

        #region ISignalPhase

        public string SignalId { get; set; }
        public int PhaseNumber { get; set; }

        #endregion

        #region ICycleArrivals

        public double TotalArrivalOnGreen => Vehicles.Count(d => d.ArrivalType == ArrivalType.ArrivalOnGreen);
        public double TotalArrivalOnYellow => Vehicles.Count(d => d.ArrivalType == ArrivalType.ArrivalOnYellow);
        public double TotalArrivalOnRed => Vehicles.Count(d => d.ArrivalType == ArrivalType.ArrivalOnRed);

        public IReadOnlyList<Vehicle> Vehicles { get; set; } = new List<Vehicle>();

        #region ICycle

        public double TotalGreenTime => _cycle.TotalGreenTime;
        public double TotalYellowTime => _cycle.TotalYellowTime;
        public double TotalRedTime => _cycle.TotalRedTime;
        public double TotalTime => _cycle.TotalTime;

        #endregion

        #region ICycleVolume

        public double TotalDelay => Vehicles.Sum(d => d.Delay);
        public double TotalVolume => Vehicles.Count(d => InRange(d.CorrectedTimeStamp));

        #endregion

        #endregion

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
