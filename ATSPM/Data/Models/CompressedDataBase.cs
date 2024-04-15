using ATSPM.Data.Interfaces;
using ATSPM.Data.Models.AggregationModels;
using ATSPM.Data.Models.EventLogModels;

#nullable disable

namespace ATSPM.Data.Models
{
    /// <summary>
    /// Base for compressed database table models
    /// </summary>
    public abstract class CompressedDataBase : ILocationLayer
    {
        private IEnumerable<ILocationLayer> data;

        ///<inheritdoc/>
        public string LocationIdentifier { get; set; }

        /// <summary>
        /// Model type that is used in the compressed <see cref="Data"/>
        /// </summary>
        public Type DataType { get; set; }

        /// <summary>
        /// Day the data is compressed into
        /// </summary>
        public DateOnly ArchiveDate { get; set; }

        /// <summary>
        /// Compressed data, ovverride or use <c>new</c> in derrived class for specific type
        /// </summary>
        public virtual IEnumerable<ILocationLayer> Data
        {
            get
            {
                foreach (var d in data)
                {
                    d.LocationIdentifier = LocationIdentifier;
                }
                return data.ToList();
            }
            set => data = value;
        }
    }

    /// <summary>
    /// Base for compressed events database table models
    /// </summary>
    public abstract class CompressedEventLogBase : CompressedDataBase
    {
        /// <summary>
        /// Id of the device the logs came from
        /// </summary>
        public int DeviceId { get; set; }

        ///<inheritdoc cref="CompressedDataBase.Data"/>
        public new IEnumerable<EventLogModelBase> Data
        {
            get => base.Data.Cast<EventLogModelBase>().ToList();
            set => base.Data = value;
        }
    }

    /// <summary>
    /// Generic type to use when compressing <see cref="EventLogModelBase"/> objects
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CompressedEventLogs<T> : CompressedEventLogBase where T : EventLogModelBase
    {
        ///<inheritdoc cref="CompressedDataBase.Data"/>
        public new ICollection<T> Data
        {
            get => Enumerable.ToHashSet(base.Data.Cast<T>()).ToList();
            set => base.Data = Enumerable.ToHashSet(value.Cast<T>()).ToList();
        }
    }

    /// <summary>
    /// Base for compressed aggregation database table models
    /// </summary>
    public abstract class CompressedAggregationBase : CompressedDataBase
    {
        ///<inheritdoc cref="CompressedDataBase.Data"/>
        public new IEnumerable<AggregationModelBase> Data
        {
            get => base.Data.Cast<AggregationModelBase>().ToList();
            set => base.Data = value;
        }
    }

    /// <summary>
    /// Generic type to use when compressing <see cref="AggregationModelBase"/> objects
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CompressedAggregations<T> : CompressedAggregationBase where T : AggregationModelBase
    {
        ///<inheritdoc cref="CompressedDataBase.Data"/>
        public new ICollection<T> Data
        {
            get => Enumerable.ToHashSet(base.Data.Cast<T>()).ToList();
            set => base.Data = Enumerable.ToHashSet(value.Cast<T>()).ToList();
        }
    }
}
