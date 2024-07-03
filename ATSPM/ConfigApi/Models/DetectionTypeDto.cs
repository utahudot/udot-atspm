using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Data.Relationships;

namespace ATSPM.ConfigApi.Models
{
    public class DetectionTypeDto
    {
        public DetectionTypes? Id { get; set; }
        public string Description { get; set; }
        public string Abbreviation { get; set; }
        public int DisplayOrder { get; set; }
        public ICollection<MeasureTypeDto> MeasureTypes { get; set; }
    }

    public class MeasureTypeDto
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string Abbreviation { get; set; }
        public bool ShowOnWebsite { get; set; }
        public bool ShowOnAggregationSite { get; set; }
        public int DisplayOrder { get; set; }
        public virtual ICollection<MeasureCommentsDto> MeasureComments { get; set; } = new HashSet<MeasureCommentsDto>();
        public virtual ICollection<MeasureOptionDto> MeasureOptions { get; set; } = new HashSet<MeasureOptionDto>();

    }

    public class MeasureCommentsDto
    {
        public int? Id { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Comment { get; set; }
        public string LocationIdentifier { get; set; }

    }

    public class MeasureOptionDto
    {
        public int? Id { get; set; }
        public string Option { get; set; }
        public string Value { get; set; }
        public int MeasureTypeId { get; set; }
    }
}
