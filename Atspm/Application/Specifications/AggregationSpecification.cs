#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Specifications/AggregationSpecification.cs
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

using Utah.Udot.NetStandardToolkit.Specifications;

namespace Utah.Udot.Atspm.Specifications
{
    public class AggregationDateRangeSpecification : BaseSpecification<CompressedAggregationBase>
    {
        public AggregationDateRangeSpecification(string locationIdentifier, DateTime start, DateTime end) : base()
        {
            Criteria = c => c.LocationIdentifier == locationIdentifier && c.End > start && c.Start < end;

            ApplyOrderBy(o => o.Start);
        }
    }

    public class AggregationDateTimeRangeSpecification : BaseSpecification<AggregationModelBase>
    {
        //public AggregationDateTimeRangeSpecification(DateTime start, DateTime end) : base()
        //{
        //    Criteria = c => c.Start >= start && c.End <= end;

        //    ApplyOrderBy(o => o.Start);
        //}

        public AggregationDateTimeRangeSpecification(string locationIdentifier, DateTime start, DateTime end) : base()
        {
            Criteria = c => c.LocationIdentifier == locationIdentifier && c.Start >= start && c.End <= end;

            ApplyOrderBy(o => o.Start);
        }

        //public AggregationDateTimeRangeSpecification(int startHour, int startMinute, int endHour, int endMinute) : base()
        //{
        //    Criteria = l => l.Start.Hour > startHour && l.End.Hour < endHour
        //    || l.Start.Hour == startHour && l.End.Hour == endHour
        //    && l.Start.Minute >= startMinute && l.End.Minute <= endMinute
        //    || l.Start.Hour == startHour && l.End.Hour < endHour
        //    && l.Start.Minute >= startMinute
        //    || l.Start.Hour < startHour && l.End.Hour == endHour
        //    && l.Start.Minute <= endMinute;

        //    ApplyOrderBy(o => o.Start);
        //}
    }
}
