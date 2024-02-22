using System;

namespace ATSPM.Application.Business.YellowRedActivations
{
    public class YellowRedActivation
    {
        //The actual time of the detector activation

        //Represents a time span from the start of the red to red cycle


        /// <summary>
        ///     Constructor for the DetectorDataPoint. Sets the timestamp
        ///     and the y coordinate.
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="eventTime"></param>
        public YellowRedActivation(DateTime startDate, DateTime eventTime)
        {
            TimeStamp = eventTime;
            YPoint = (eventTime - startDate).TotalSeconds;
        }

        public double YPoint { get; }

        public DateTime TimeStamp { get; }
    }
}