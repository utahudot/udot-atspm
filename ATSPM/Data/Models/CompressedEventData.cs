using ATSPM.Data.EventModels;
using ATSPM.Data.Interfaces;
using System;
using System.Collections.Generic;

#nullable disable

namespace ATSPM.Data.Models
{
    public partial class CompressedEventData : ILocationLayer
    {
        ///<inheritdoc/>
        public string LocationIdentifier { get; set; }
        
        /// <summary>
        /// Day the data is archived for
        /// </summary>
        public DateOnly ArchiveDate { get; set; }

        public IEnumerable<EventModelBase> LogData { get; set; }

        //public IReadOnlyList<T> GetData<T>() where T : EventModelBase
        //{
        //    if (LogData is IEnumerable<T> a)
        //        return a.ToList();
        //    else 
        //        return new List<T>();
        //}

        public override string ToString()
        {
            return $"{LocationIdentifier}-{ArchiveDate:dd/MM/yyyy}";
        }
    }
}
