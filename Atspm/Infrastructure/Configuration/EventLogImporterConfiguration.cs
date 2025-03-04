#region license
// Copyright 2025 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Configuration/EventLogImporterConfiguration.cs
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

namespace Utah.Udot.Atspm.Infrastructure.Configuration
{
    /// <summary>
    /// Options pattern model for services that implement <see cref="IEventLogImporter"/>
    /// </summary>
    public class EventLogImporterConfiguration
    {
        /// <summary>
        /// Earliest acceptable date for importing from source
        /// </summary>
        public DateTime EarliestAcceptableDate { get; set; } = DateTime.Parse("01/01/1980");

        /// <summary>
        /// Flag for deleting source after importing
        /// </summary>
        public bool DeleteSource { get; set; }

        public override string ToString()
        {
            return $"{EarliestAcceptableDate} - {DeleteSource}";
        }
    }
}
