using System;

namespace ATSPM.Application.Business.Common
{
    public class AnalysisPhaseCycle
    {
        public enum NextEventResponse
        {
            CycleOK,
            CycleMissingData,
            CycleComplete
        }

        public enum TerminationType
        {
            GapOut = 4,
            MaxOut = 5,
            ForceOff = 6,
            Unknown = 0
        }

        private double pedDuration;

        /// <summary>
        ///     Phase Objects primarily for the split monitor and terminaiton chart
        /// </summary>
        /// <param name="locationId"></param>
        /// <param name="phasenumber"></param>
        /// <param name="starttime"></param>
        public AnalysisPhaseCycle(string locationId, int phasenumber, DateTime starttime)
        {
            locationId = locationId;
            PhaseNumber = phasenumber;
            StartTime = starttime;
            HasPed = false;
            TerminationEvent = null;
        }

        public int PhaseNumber { get; }

        public string locationId { get; }

        public DateTime StartTime { get; }

        public DateTime EndTime { get; private set; }

        public DateTime PedStartTime { get; private set; }

        public DateTime PedEndTime { get; private set; }

        public short? TerminationEvent { get; private set; }

        public TimeSpan Duration { get; private set; }

        public double PedDuration
        {
            get
            {
                if (pedDuration > 0)
                    return pedDuration;
                return 0;
            }
        }


        public bool HasPed { get; set; }

        public DateTime YellowEvent { get; set; }

        public void SetTerminationEvent(short terminatonCode)
        {
            TerminationEvent = terminatonCode;
        }

        public void SetEndTime(DateTime endtime)
        {
            EndTime = endtime;
            Duration = EndTime.Subtract(StartTime);
        }

        public void SetPedStart(DateTime starttime)
        {
            PedStartTime = starttime;
            HasPed = true;
        }

        public void SetPedEnd(DateTime endtime)
        {
            PedEndTime = endtime;
            pedDuration = PedEndTime.Subtract(PedStartTime).TotalSeconds;
        }
    }
}