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
        public virtual object Data { get; set; }
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
        public new IEnumerable<EventLogModelBase> Data { get => (IEnumerable<EventLogModelBase>)base.Data; set => base.Data = value; }

    }

    /// <summary>
    /// Generic type to use when compressing <see cref="EventLogModelBase"/> objects
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CompressedEventLogs<T> : CompressedEventLogBase where T : EventLogModelBase
    {
        ///<inheritdoc cref="CompressedDataBase.Data"/>
        public new ICollection<T> Data { get => (ICollection<T>)base.Data; set => base.Data = value; }
    }

    /// <summary>
    /// Base for compressed aggregation database table models
    /// </summary>
    public abstract class CompressedAggregationBase : CompressedDataBase
    {
        ///<inheritdoc cref="CompressedDataBase.Data"/>
        public new IEnumerable<AggregationModelBase> Data { get => (IEnumerable<AggregationModelBase>)base.Data; set => base.Data = value; }
    }

    /// <summary>
    /// Generic type to use when compressing <see cref="AggregationModelBase"/> objects
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CompressedAggregations<T> : CompressedAggregationBase where T : AggregationModelBase
    {
        ///<inheritdoc cref="CompressedDataBase.Data"/>
        public new ICollection<T> Data { get => (ICollection<T>)base.Data; set => base.Data = value; }
    }
}
