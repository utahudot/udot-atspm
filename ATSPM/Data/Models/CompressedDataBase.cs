using ATSPM.Data.Interfaces;
using ATSPM.Data.Models.AggregationModels;
using ATSPM.Data.Models.EventModels;

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
    public abstract class CompressedEventsBase : CompressedDataBase
    {
        /// <summary>
        /// Id of the device the logs came from
        /// </summary>
        public int DeviceId { get; set; }

        ///<inheritdoc cref="CompressedDataBase.Data"/>
        public new IEnumerable<AtspmEventModelBase> Data { get; set; }

    }

    /// <summary>
    /// Generic type to use when compressing <see cref="AtspmEventModelBase"/> objects
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CompressedEvents<T> : CompressedEventsBase where T : AtspmEventModelBase
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
        public new IEnumerable<AtspmAggregationModelBase> Data { get; set; }
    }

    /// <summary>
    /// Generic type to use when compressing <see cref="AtspmAggregationModelBase"/> objects
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CompressedAggregations<T> : CompressedAggregationBase where T : AtspmAggregationModelBase
    {
        ///<inheritdoc cref="CompressedDataBase.Data"/>
        public new ICollection<T> Data { get => (ICollection<T>)base.Data; set => base.Data = value; }
    }
}
