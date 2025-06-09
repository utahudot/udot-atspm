#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Specifications/CompressedEventLogSpecification.cs
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
    /// <summary>
    /// Specifications for <see cref="CompressedEventLogBase"/>
    /// </summary>
    public class CompressedEventLogSpecification : BaseSpecification<CompressedEventLogBase>
    {
        /// <summary>
        /// Matches <see cref="CompressedEventLogBase"/> by <see cref="CompressedDataBase.LocationIdentifier"/>
        /// and orders by <see cref="StartEndRange.Start"/>
        /// </summary>
        /// <param name="Location"></param>
        public CompressedEventLogSpecification(Location Location) : base()
        {
            Criteria = c => c.LocationIdentifier == Location.LocationIdentifier;

            ApplyOrderBy(o => o.Start);
        }

        /// <summary>
        /// Matches <see cref="CompressedEventLogBase"/> by <see cref="CompressedDataBase.LocationIdentifier"/>
        /// within <see cref="StartEndRange"/> and orders by <see cref="StartEndRange.Start"/>
        /// </summary>
        /// <param name="locationIdentifier"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public CompressedEventLogSpecification(string locationIdentifier, DateTime start, DateTime end) : base()
        {
            Criteria = c => c.LocationIdentifier == locationIdentifier && c.Start >= start && c.End <= end;

            ApplyOrderBy(o => o.Start);
        }

        /// <summary>
        /// Matches <see cref="CompressedEventLogBase"/> by <see cref="CompressedDataBase.LocationIdentifier"/> and <see cref="CompressedEventLogBase.DeviceId"/>
        /// within <see cref="StartEndRange"/> and orders by <see cref="StartEndRange.Start"/>
        /// </summary>
        /// <param name="locationIdentifier"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="deviceId"></param>
        public CompressedEventLogSpecification(string locationIdentifier, DateTime start, DateTime end, int deviceId) : base()
        {
            Criteria = c => c.LocationIdentifier == locationIdentifier && c.DeviceId == deviceId && c.Start >= start && c.End <= end;

            ApplyOrderBy(o => o.Start);
        }
    }
}
