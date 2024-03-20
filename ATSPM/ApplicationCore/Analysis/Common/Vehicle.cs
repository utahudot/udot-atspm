using ATSPM.Application.Enums;
using ATSPM.Data.Enums;
using System.Text.Json;

namespace ATSPM.Application.Analysis.Common
{
    /// <summary>
    /// Arrival type and dely for vehicle actuation events
    /// </summary>
    public interface IVehicle
    {
        /// <summary>
        /// Delay is only calculated for vehicles that arrive on red. Zero delay is assumed for vehicles that arrive on green.
        /// Arrivals on red are the vehicle actuations with the adjusted time stamp calculated in Step 1 between <see cref="IndianaEnumerations.PhaseEndYellowChange"/> and <see cref="IndianaEnumerations.PhaseBeginGreen"/>.
        /// The delay for each arrival on red is determined from the adjusted detection time to the start of the next green phase
        /// </summary>
        ArrivalType ArrivalType { get; }

        /// <summary>
        /// Delay is only calculated for vehicles that arrive on red. Zero delay is assumed for vehicles that arrive on green.
        /// Arrivals on red are the vehicle actuations with the adjusted time stamp calculated in Step 1 between <see cref="IndianaEnumerations.PhaseEndYellowChange"/> and <see cref="IndianaEnumerations.PhaseBeginGreen"/>.
        /// The delay for each arrival on red is determined from the adjusted detection time to the start of the next green phase
        /// </summary>
        double Delay { get; }

        IRedToRedCycle RedToRedCycle { get; set; }
    }

    /// <summary>
    /// Compares a <see cref="CorrectedDetectorEvent"/> to a <see cref="Common.RedToRedCycle"/> to calculate the <see cref="IVehicle"/> properties.
    /// </summary>
    public class Vehicle : CorrectedDetectorEvent, IVehicle
    {
        /// <inheritdoc/>
        public Vehicle() { }

        /// <inheritdoc/>
        public Vehicle(IDetectorEvent detectorEvent, IRedToRedCycle redToRedCycle)
        {
            LocationIdentifier = detectorEvent.LocationIdentifier;
            PhaseNumber = detectorEvent.PhaseNumber;
            DetectorChannel = detectorEvent.DetectorChannel;
            Direction = detectorEvent.Direction;
            Timestamp = detectorEvent.Timestamp;

            RedToRedCycle = redToRedCycle;
        }

        #region IVehicle

        /// <inheritdoc/>
        public double Delay => ArrivalType == ArrivalType.ArrivalOnRed ? (RedToRedCycle.GreenEvent - Timestamp).TotalSeconds : 0;

        /// <inheritdoc/>
        public ArrivalType ArrivalType
        {
            get
            {
                if (Timestamp < RedToRedCycle.GreenEvent && Timestamp >= RedToRedCycle.Start)
                {
                    return ArrivalType.ArrivalOnRed;
                }

                else if (Timestamp >= RedToRedCycle.GreenEvent && Timestamp < RedToRedCycle.YellowEvent)
                {
                    return ArrivalType.ArrivalOnGreen;
                }

                else if (Timestamp >= RedToRedCycle.YellowEvent && Timestamp <= RedToRedCycle.End)
                {
                    return ArrivalType.ArrivalOnYellow;
                }

                return ArrivalType.Unknown;
            }
        }

        public IRedToRedCycle RedToRedCycle { get; set; } = new RedToRedCycle();

        #endregion

        /// <inheritdoc/>
        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
