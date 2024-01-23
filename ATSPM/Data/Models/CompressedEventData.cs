using ATSPM.Data.EventModels;
using ATSPM.Data.Interfaces;
using ATSPM.Domain.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace ATSPM.Data.Models
{
    public partial class CompressedEventData : ILocationLayer
    {
        ///<inheritdoc/>
        public string LocationIdentifier { get; set; }

        /// <summary>
        /// Id of the device the logs came from
        /// </summary>
        public int DeviceId { get; set; }
        
        /// <summary>
        /// Day the data is archived for
        /// </summary>
        public DateOnly ArchiveDate { get; set; }

        /// <summary>
        /// Compressed log data
        /// </summary>
        public IEnumerable<EventModelBase> LogData { get; set; }

        ///<inheritdoc/>
        public override string ToString()
        {
            return $"{LocationIdentifier}-{ArchiveDate:dd/MM/yyyy} - {LogData.Count()}";
        }
    }

    public abstract class CompressedData : ILocationLayer
    {
        ///<inheritdoc/>
        public string LocationIdentifier { get; set; }

        public Type DataType { get; set; }

        /// <summary>
        /// Day the data is archived for
        /// </summary>
        public DateOnly ArchiveDate { get; set; }

        /// <summary>
        /// Compressed data
        /// </summary>
        public byte[] Data { get; set; }
    }

    public abstract class EventsBase : CompressedData
    {
        /// <summary>
        /// Id of the device the logs came from
        /// </summary>
        public int DeviceId { get; set; }

        
    }

    public abstract class EventsTypeBase<T> : EventsBase where T : EventModelBase
    {
        [NotMapped]
        public List<T> Events
        {
            get
            {
                return JsonConvert.DeserializeObject<List<T>>(Data.GZipDecompressToString(), new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.Arrays
                });
            }
            set
            {
                Data = JsonConvert.SerializeObject(value, new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.Arrays
                }).GZipCompressToByte();
            }
        }
    }

    public class CompressedIndiannaEvents : EventsTypeBase<IndiannaEvent>
    {
    }

    public class CompressedPedestrianCounter : EventsTypeBase<PedestrianCounter>
    {
    }
}
