using System.Runtime.Serialization;

namespace ATSPM.Application.Business.Aggregation.FilterExtensions
{
    [DataContract]
    public class FilterDetector
    {
        [DataMember]
        public int Id { get; set; }

        public string Description { get; set; }

        [DataMember]
        public bool Exclude { get; set; }
    }
}