using ATSPM.Application.Enums;
using ATSPM.Data.Enums;
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
        public DateTime Timestamp { get; set; }
        public DateTime GreenEvent { get; set; }
        public DateTime YellowEvent { get; set; }

        public double Delay => ArrivalType == ArrivalType.ArrivalOnRed ? (GreenEvent - Timestamp).TotalSeconds : 0;

        public ArrivalType ArrivalType
        {
            get
            {
                if (Timestamp < GreenEvent && Timestamp >= Start)
                {
                    return ArrivalType.ArrivalOnRed;
                }

                else if (Timestamp >= GreenEvent && Timestamp < YellowEvent)
                {
                    return ArrivalType.ArrivalOnGreen;
                }

                else if (Timestamp >= YellowEvent && Timestamp <= End)
                {
                    return ArrivalType.ArrivalOnYellow;
                }

                return ArrivalType.Unknown;
            }
        }

        public DirectionTypes Direction { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
