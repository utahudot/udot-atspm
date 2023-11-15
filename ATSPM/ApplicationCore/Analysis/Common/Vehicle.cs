using ATSPM.Application.Enums;
using ATSPM.Data.Interfaces;
using ATSPM.Domain.Common;
using System;
using System.Text.Json;

namespace ATSPM.Application.Analysis.Common
{
    //TODO: this has the same props as redtoredcycle start, end, greenevent and yellow event. make the same base class?
    public class Vehicle : StartEndRange, ISignalPhaseLayer, IDetectorEvent
    {
        //public Vehicle() { }

        //public Vehicle(CorrectedDetectorEvent detectorEvent, RedToRedCycle redToRedCycle)
        //{
        //    SignalIdentifier = detectorEvent.SignalIdentifier;
        //    CorrectedTimeStamp = detectorEvent.CorrectedTimeStamp;
        //    DetectorChannel = detectorEvent.DetectorChannel;
        //    PhaseNumber = redToRedCycle.PhaseNumber;
        //    Start = redToRedCycle.Start;
        //    End = redToRedCycle.End;
        //    YellowEvent = redToRedCycle.YellowEvent;
        //    GreenEvent = redToRedCycle.GreenEvent;
        //}

        #region ISignalPhaseLayer

        /// <inheritdoc/>
        public string SignalIdentifier { get; set; }

        /// <inheritdoc/>
        public int PhaseNumber { get; set; }

        #endregion

        public int DetectorChannel { get; set; }
        public DateTime CorrectedTimeStamp { get; set; }
        public DateTime GreenEvent { get; set; }
        public DateTime YellowEvent { get; set; }

        public double Delay => ArrivalType == ArrivalType.ArrivalOnRed ? (GreenEvent - CorrectedTimeStamp).TotalSeconds : 0;

        public ArrivalType ArrivalType
        {
            get
            {
                if (CorrectedTimeStamp < GreenEvent && CorrectedTimeStamp >= Start)
                {
                    return ArrivalType.ArrivalOnRed;
                }

                else if (CorrectedTimeStamp >= GreenEvent && CorrectedTimeStamp < YellowEvent)
                {
                    return ArrivalType.ArrivalOnGreen;
                }

                else if (CorrectedTimeStamp >= YellowEvent && CorrectedTimeStamp <= End)
                {
                    return ArrivalType.ArrivalOnYellow;
                }

                return ArrivalType.Unknown;
            }
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
