#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Specifications/IndianaEventLogSpecifications.cs
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

using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.NetStandardToolkit.Specifications;

namespace Utah.Udot.Atspm.Specifications
{
    public class IndianaLogLocationFilterSpecification : BaseSpecification<IndianaEvent>
    {
        public IndianaLogLocationFilterSpecification(Location Location) : base()
        {
            Criteria = c => c.LocationIdentifier == Location.LocationIdentifier;

            ApplyOrderBy(o => o.Timestamp);
        }
    }

    public class IndianaLogLocationAndParamterFilterSpecification : BaseSpecification<IndianaEvent>
    {
        public IndianaLogLocationAndParamterFilterSpecification(Location Location, int param) : base()
        {
            Criteria = c => c.LocationIdentifier == Location.LocationIdentifier && c.EventParam == param;

            ApplyOrderBy(o => o.Timestamp);
        }
    }

    public class IndianaLogDateRangeSpecification : BaseSpecification<CompressedEventLogBase>
    {
        public IndianaLogDateRangeSpecification(string locationId, DateTime startDate, DateTime endDate) : base()
        {
            Criteria = c => c.LocationIdentifier == locationId && c.ArchiveDate >= DateOnly.FromDateTime(startDate) && c.ArchiveDate <= DateOnly.FromDateTime(endDate);

            ApplyOrderBy(o => o.ArchiveDate);
        }
    }

    public class IndianaLogCodeAndParamSpecification : BaseSpecification<IndianaEvent>
    {
        public IndianaLogCodeAndParamSpecification(short eventCode) : base()
        {
            Criteria = c => c.EventCode == eventCode;

            ApplyOrderBy(o => o.Timestamp);
        }

        public IndianaLogCodeAndParamSpecification(short eventCode, int param) : base()
        {
            Criteria = c => c.EventCode == eventCode && c.EventParam == param;

            ApplyOrderBy(o => o.Timestamp);
        }

        public IndianaLogCodeAndParamSpecification(IEnumerable<short> eventCodes) : base()
        {
            Criteria = c => eventCodes != null && eventCodes.Contains(c.EventCode);

            ApplyOrderBy(o => o.Timestamp);
        }

        public IndianaLogCodeAndParamSpecification(IEnumerable<short> eventCodes, int param) : base()
        {
            Criteria = c => eventCodes != null && eventCodes.Contains(c.EventCode) && c.EventParam == param;

            ApplyOrderBy(o => o.Timestamp);
        }

        public IndianaLogCodeAndParamSpecification(IEnumerable<short> eventCodes, IEnumerable<int> paramCodes) : base()
        {
            Criteria = c => eventCodes != null && eventCodes.Contains(c.EventCode) && paramCodes != null && paramCodes.Contains(c.EventParam);

            ApplyOrderBy(o => o.Timestamp);
        }
    }

    public class IndianaLogDateTimeRangeSpecification : BaseSpecification<IndianaEvent>
    {
        public IndianaLogDateTimeRangeSpecification(DateTime startDate, DateTime endDate) : base()
        {
            Criteria = c => c.Timestamp >= startDate && c.Timestamp <= endDate;

            ApplyOrderBy(o => o.Timestamp);
        }

        public IndianaLogDateTimeRangeSpecification(string locationId, DateTime startDate, DateTime endDate) : base()
        {
            Criteria = c => c.LocationIdentifier == locationId && c.Timestamp >= startDate && c.Timestamp <= endDate;

            ApplyOrderBy(o => o.Timestamp);
        }

        public IndianaLogDateTimeRangeSpecification(int startHour, int startMinute, int endHour, int endMinute) : base()
        {
            Criteria = l => l.Timestamp.Hour > startHour && l.Timestamp.Hour < endHour
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
