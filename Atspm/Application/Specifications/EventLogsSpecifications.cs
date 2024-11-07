#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Specifications/EventLogsSpecifications.cs
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
    public class CompressedEventLogSpecification : BaseSpecification<CompressedEventLogBase>
    {
        public CompressedEventLogSpecification(Location Location) : base()
        {
            Criteria = c => c.LocationIdentifier == Location.LocationIdentifier;

            ApplyOrderBy(o => o.ArchiveDate);
        }

        public CompressedEventLogSpecification(string locationId, DateOnly startDate, DateOnly endDate) : base()
        {
            Criteria = c => c.LocationIdentifier == locationId && c.ArchiveDate >= startDate && c.ArchiveDate <= endDate;

            ApplyOrderBy(o => o.ArchiveDate);
        }

        public CompressedEventLogSpecification(string locationId, DateOnly startDate, DateOnly endDate, int deviceId) : base()
        {
            Criteria = c => c.LocationIdentifier == locationId && c.ArchiveDate >= startDate && c.ArchiveDate <= endDate && c.DeviceId == deviceId;

            ApplyOrderBy(o => o.ArchiveDate);
        }
    }

    public class EventLogSpecification : BaseSpecification<EventLogModelBase>
    {
        public EventLogSpecification(Location Location) : base()
        {
            Criteria = c => c.LocationIdentifier == Location.LocationIdentifier;

            ApplyOrderBy(o => o.Timestamp);
        }

        public EventLogSpecification(DateTime startDate, DateTime endDate) : base()
        {
            Criteria = c => c.Timestamp >= startDate && c.Timestamp <= endDate;

            ApplyOrderBy(o => o.Timestamp);
        }

        public EventLogSpecification(string locationId, DateTime startDate, DateTime endDate) : base()
        {
            Criteria = c => c.LocationIdentifier == locationId && c.Timestamp >= startDate && c.Timestamp <= endDate;

            ApplyOrderBy(o => o.Timestamp);
        }

        public EventLogSpecification(int startHour, int startMinute, int endHour, int endMinute) : base()
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
