using ATSPM.Data.Interfaces;
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
        public new IEnumerable<EventModelBase> Data { get; set; }

    }

    /// <summary>
    /// Generic type to use when compressing <see cref="EventModelBase"/> objects
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CompressedEventsBase<T> : CompressedEventsBase where T : EventModelBase
    {
        ///<inheritdoc cref="CompressedDataBase.Data"/>
        public new ICollection<T> Data { get => (ICollection<T>)base.Data; set => base.Data = value; }
    }
}
