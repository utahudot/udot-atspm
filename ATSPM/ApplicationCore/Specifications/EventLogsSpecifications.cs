using ATSPM.Data.Models;
using ATSPM.Application.ValueObjects;
using ATSPM.Domain.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ATSPM.Data.Models.EventModels;

namespace ATSPM.Application.Specifications
{
    public class EventLogDateRangeSpecification : BaseSpecification<CompressedEventsBase>
    {
        public EventLogDateRangeSpecification(string locationId, DateTime startDate, DateTime endDate) : base()
        {
            base.Criteria = c => c.LocationIdentifier == locationId && c.ArchiveDate >= DateOnly.FromDateTime(startDate.Date) && c.ArchiveDate <= DateOnly.FromDateTime(endDate.Date);

            ApplyOrderBy(o => o.ArchiveDate);
        }
    }

    public class EventLogDateTimeRangeSpecification : BaseSpecification<AtspmEventModelBase>
    {
        public EventLogDateTimeRangeSpecification(DateTime startDate, DateTime endDate) : base()
        {
            base.Criteria = c => c.Timestamp >= startDate && c.Timestamp <= endDate;

            ApplyOrderBy(o => o.Timestamp);
        }

        public EventLogDateTimeRangeSpecification(string locationId, DateTime startDate, DateTime endDate) : base()
        {
            base.Criteria = c => c.LocationIdentifier == locationId && c.Timestamp >= startDate && c.Timestamp <= endDate;

            ApplyOrderBy(o => o.Timestamp);
        }

        public EventLogDateTimeRangeSpecification(int startHour, int startMinute, int endHour, int endMinute) : base()
        {
            base.Criteria = l => l.Timestamp.Hour > startHour && l.Timestamp.Hour < endHour
            || l.Timestamp.Hour == startHour && l.Timestamp.Hour == endHour
            && l.Timestamp.Minute >= startMinute && l.Timestamp.Minute <= endMinute
            || l.Timestamp.Hour == startHour && l.Timestamp.Hour < endHour
            && l.Timestamp.Minute >= startMinute
            || l.Timestamp.Hour < startHour && l.Timestamp.Hour == endHour
            && l.Timestamp.Minute <= endMinute;

            ApplyOrderBy(o => o.Timestamp);
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
