using System;
using System.Collections.Generic;
using System.Text;

namespace ATSPM.Application.ValueObjects
{
    public class InfoBox
    {
        protected SortedDictionary<int, bool> availReports;

        protected string id;

        protected string location;


        /// <summary>
        ///     Default constructor for the infobox class.
        ///     Usually there will be one of these per pin.
        /// </summary>
        /// <param name="number"></param>
        /// <param name="loc"></param>
        /// <param name="reports"></param>
        public InfoBox(string SignalID, string loc, SortedDictionary<int, bool> reports)
        {
            id = SignalID;
            location = loc;
            availReports = reports;
        }

        public string ID => id;

        public string Location => location;

        public SortedDictionary<int, bool> AvailReports => availReports;
    }
}
