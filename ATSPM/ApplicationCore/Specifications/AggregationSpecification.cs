using ATSPM.Data.Models;
using ATSPM.Data.Models.AggregationModels;
using ATSPM.Domain.Specifications;
using System;

namespace ATSPM.Application.Specifications
{
    public class AggregationDateRangeSpecification : BaseSpecification<CompressedAggregationBase>
    {
        public AggregationDateRangeSpecification(string locationId, DateOnly startDate, DateOnly endDate) : base()
        {
            base.Criteria = c => c.LocationIdentifier == locationId && c.ArchiveDate >= startDate && c.ArchiveDate <= endDate;

            ApplyOrderBy(o => o.ArchiveDate);
        }
    }

    public class AggregationDateTimeRangeSpecification : BaseSpecification<AggregationModelBase>
    {
        public AggregationDateTimeRangeSpecification(DateTime startDate, DateTime endDate) : base()
        {
            base.Criteria = c => c.Start >= startDate && c.End <= endDate;

            ApplyOrderBy(o => o.Start);
        }

        public AggregationDateTimeRangeSpecification(string locationId, DateTime startDate, DateTime endDate) : base()
        {
            base.Criteria = c => c.LocationIdentifier == locationId && c.Start >= startDate && c.End <= endDate;

            ApplyOrderBy(o => o.Start);
        }

        public AggregationDateTimeRangeSpecification(int startHour, int startMinute, int endHour, int endMinute) : base()
        {
            base.Criteria = l => l.Start.Hour > startHour && l.End.Hour < endHour
            || l.Start.Hour == startHour && l.End.Hour == endHour
            && l.Start.Minute >= startMinute && l.End.Minute <= endMinute
            || l.Start.Hour == startHour && l.End.Hour < endHour
            && l.Start.Minute >= startMinute
            || l.Start.Hour < startHour && l.End.Hour == endHour
            && l.Start.Minute <= endMinute;

            ApplyOrderBy(o => o.Start);
        }
    }



























    //public class ControllerLogLocationFilterSpecification : BaseSpecification<ControllerEventLog>
    //{
    //    public ControllerLogLocationFilterSpecification(Location Location) : base()
    //    {
    //        base.Criteria = c => c.LocationIdentifier == Location.LocationIdentifier;

    //        ApplyOrderBy(o => o.Timestamp);
    //    }
    //}

    //public class ControllerLogLocationAndParamterFilterSpecification : BaseSpecification<ControllerEventLog>
    //{
    //    public ControllerLogLocationAndParamterFilterSpecification(Location Location, int param) : base()
    //    {
    //        base.Criteria = c => c.LocationIdentifier == Location.LocationIdentifier && c.EventParam == param;

    //        ApplyOrderBy(o => o.Timestamp);
    //    }
    //}

    

    //public class ControllerLogCodeAndParamSpecification : BaseSpecification<ControllerEventLog>
    //{
    //    public ControllerLogCodeAndParamSpecification(int eventCode) : base()
    //    {
    //        base.Criteria = c => c.EventCode == eventCode;

    //        ApplyOrderBy(o => o.Timestamp);
    //    }

    //    public ControllerLogCodeAndParamSpecification(int eventCode, int param) : base()
    //    {
    //        base.Criteria = c => c.EventCode == eventCode && c.EventParam == param;

    //        ApplyOrderBy(o => o.Timestamp);
    //    }

    //    public ControllerLogCodeAndParamSpecification(IEnumerable<int> eventCodes) : base()
    //    {
    //        base.Criteria = c => eventCodes != null && eventCodes.Contains(c.EventCode);

    //        ApplyOrderBy(o => o.Timestamp);
    //    }

    //    public ControllerLogCodeAndParamSpecification(IEnumerable<int> eventCodes, int param) : base()
    //    {
    //        base.Criteria = c => eventCodes != null && eventCodes.Contains(c.EventCode) && c.EventParam == param;

    //        ApplyOrderBy(o => o.Timestamp);
    //    }

    //    public ControllerLogCodeAndParamSpecification(IEnumerable<int> eventCodes, IEnumerable<int> paramCodes) : base()
    //    {
    //        base.Criteria = c => eventCodes != null && eventCodes.Contains(c.EventCode) && paramCodes != null && paramCodes.Contains(c.EventParam);

    //        ApplyOrderBy(o => o.Timestamp);
    //    }
    //}

    
}
