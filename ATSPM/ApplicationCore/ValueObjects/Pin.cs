﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ATSPM.Application.ValueObjects
{
    public class Pin
    {
        protected InfoBox box;

        protected string description;

        protected string latitude;

        protected string longitude;

        protected string region;

        protected string _locationId;


        /// <summary>
        ///     Default Constructor Used for Map Pins
        /// </summary>
        /// <param name="number"></param>
        /// <param name="lat"></param>
        /// <param name="Long"></param>
        /// <param name="desc"></param>
        /// <param name="hasPCD"></param>
        /// <param name="hasSpeed"></param>
        /// <param name="hasPhase"></param>
        /// <param name="hasTMC"></param>
        /// <param name="Region"></param>

        //public Pin(string LocationId, string lat, string Long, string desc, bool hasPCD, bool hasSpeed, bool hasPhase, bool hasTMC, bool hasRLM, bool hasSplitFail, string Region)
        //{

        //    LocationId = LocationId;
        //    latitude = lat;
        //    longitude = Long;
        //    description = desc;
        //    region = Region;

        //    SortedDictionary<int, bool> reports = FindReports(hasPCD,hasSpeed, hasPhase, hasTMC, hasRLM, hasSplitFail);
        //    box = new InfoBox(LocationId, description, reports);
        //}
        public Pin(string locationId, string lat, string Long, string desc, string Region)
        {
            _locationId = locationId;
            latitude = lat;
            longitude = Long;
            description = desc;
            region = Region;


            //box = new InfoBox(LocationId, description, reports);
        }

        public string locationId => _locationId;

        public string Region => region;

        public string Latitude => latitude;

        public string Longitude => longitude;

        public string Description => description;

        public InfoBox Box => box;

        public string MetricTypes { get; set; }


        /// <summary>
        ///     Find the avialable report for the pin.
        /// </summary>
        /// <param name="hasPCD"></param>
        /// <param name="hasSpeed"></param>
        /// <param name="hasPhase"></param>
        /// <param name="hasTMC"></param>
        /// <returns>SortedDictionary<int Boolean></returns>
        private SortedDictionary<int, bool> FindReports(bool hasPCD, bool hasSpeed, bool hasPhase, bool hasTMC,
            bool hasRLM, bool hasSplitFail)
        {
            var reports = new SortedDictionary<int, bool>();

            if (hasPCD)
                reports.Add(1, true);
            else
                reports.Add(1, false);

            if (hasSpeed)
                reports.Add(2, true);
            else
                reports.Add(2, false);

            if (hasPhase)
                reports.Add(3, true);
            else
                reports.Add(3, false);

            if (hasTMC)
                reports.Add(4, true);
            else
                reports.Add(4, false);
            if (hasRLM)
                reports.Add(5, true);
            else
                reports.Add(5, false);
            if (hasSplitFail)
                reports.Add(6, true);
            else
                reports.Add(6, false);
            //PED Delay on all pins
            reports.Add(7, true);

            return reports;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType()) return false;
            var y = (Pin)obj;
            return this != null && y != null && locationId == y.locationId
                ;
        }


        public override int GetHashCode()
        {
            return this == null ? 0 : locationId.GetHashCode() ^ Region.GetHashCode();
        }
    }
}