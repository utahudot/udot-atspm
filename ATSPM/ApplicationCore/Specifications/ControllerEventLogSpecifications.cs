#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Specifications/ControllerEventLogSpecifications.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion
using ATSPM.Data.Models;
using ATSPM.Domain.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Specifications
{
    public class ControllerLogLocationFilterSpecification : BaseSpecification<ControllerEventLog>
    {
        public ControllerLogLocationFilterSpecification(Location Location) : base()
        {
            base.Criteria = c => c.SignalIdentifier == Location.LocationIdentifier;

            ApplyOrderBy(o => o.Timestamp);
        }
    }

    public class ControllerLogLocationAndParamterFilterSpecification : BaseSpecification<ControllerEventLog>
    {
        public ControllerLogLocationAndParamterFilterSpecification(Location Location, int param) : base()
        {
            base.Criteria = c => c.SignalIdentifier == Location.LocationIdentifier && c.EventParam == param;

            ApplyOrderBy(o => o.Timestamp);
        }
    }

    public class ControllerLogDateRangeSpecification : BaseSpecification<ControllerLogArchive>
    {
        public ControllerLogDateRangeSpecification(string locationId, DateTime startDate, DateTime endDate) : base()
        {
            base.Criteria = c => c.SignalIdentifier == locationId && c.ArchiveDate.Date >= startDate.Date && c.ArchiveDate.Date <= endDate.Date;

            ApplyOrderBy(o => o.ArchiveDate);
        }
    }

    public class ControllerLogCodeAndParamSpecification : BaseSpecification<ControllerEventLog>
    {
        public ControllerLogCodeAndParamSpecification(int eventCode) : base()
        {
            base.Criteria = c => c.EventCode == eventCode;

            ApplyOrderBy(o => o.Timestamp);
        }

        public ControllerLogCodeAndParamSpecification(int eventCode, int param) : base()
        {
            base.Criteria = c => c.EventCode == eventCode && c.EventParam == param;

            ApplyOrderBy(o => o.Timestamp);
        }

        public ControllerLogCodeAndParamSpecification(IEnumerable<int> eventCodes) : base()
        {
            base.Criteria = c => eventCodes != null && eventCodes.Contains(c.EventCode);

            ApplyOrderBy(o => o.Timestamp);
        }

        public ControllerLogCodeAndParamSpecification(IEnumerable<int> eventCodes, int param) : base()
        {
            base.Criteria = c => eventCodes != null && eventCodes.Contains(c.EventCode) && c.EventParam == param;

            ApplyOrderBy(o => o.Timestamp);
        }

        public ControllerLogCodeAndParamSpecification(IEnumerable<int> eventCodes, IEnumerable<int> paramCodes) : base()
        {
            base.Criteria = c => eventCodes != null && eventCodes.Contains(c.EventCode) && paramCodes != null && paramCodes.Contains(c.EventParam);

            ApplyOrderBy(o => o.Timestamp);
        }
    }

    public class ControllerLogDateTimeRangeSpecification : BaseSpecification<ControllerEventLog>
    {
        public ControllerLogDateTimeRangeSpecification(DateTime startDate, DateTime endDate) : base()
        {
            base.Criteria = c => c.Timestamp >= startDate && c.Timestamp <= endDate;

            ApplyOrderBy(o => o.Timestamp);
        }

        public ControllerLogDateTimeRangeSpecification(string locationId, DateTime startDate, DateTime endDate) : base()
        {
            base.Criteria = c => c.SignalIdentifier == locationId && c.Timestamp >= startDate && c.Timestamp <= endDate;

            ApplyOrderBy(o => o.Timestamp);
        }

        public ControllerLogDateTimeRangeSpecification(int startHour, int startMinute, int endHour, int endMinute) : base()
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
}
