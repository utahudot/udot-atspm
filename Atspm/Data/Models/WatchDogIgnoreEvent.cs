#region license
// Copyright 2025 Utah Departement of Transportation
// for Data - Utah.Udot.Atspm.Data.Models/WatchDogIgnoreEvent.cs
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

using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Interfaces;
using Utah.Udot.Atspm.Data.Models.ConfigurationModels;

namespace Utah.Udot.Atspm.Data.Models
{
    public class WatchDogIgnoreEvent : AtspmConfigModelBase<int>, ILocationLayer
    {
        /// <summary>
        /// Location id
        /// </summary>
        public int LocationId { get; set; }

        public virtual Location? Location { get; set; }

        /// <summary>
        /// Location identifier
        /// </summary>
        public string LocationIdentifier { get; set; }

        /// <summary>
        /// Timestamp
        /// </summary>
        public DateTime Start { get; set; }

        /// <summary>
        /// Timestamp
        /// </summary>
        public DateTime End { get; set; }

        /// <summary>
        /// Component type
        /// </summary>
        public WatchDogComponentTypes? ComponentType { get; set; }

        /// <summary>
        /// Component id
        /// </summary>
        public int? ComponentId { get; set; }

        /// <summary>
        /// Issue type
        /// </summary>
        public WatchDogIssueTypes IssueType { get; set; }

        /// <summary>
        /// Phase
        /// </summary>
        public int? Phase { get; set; }

    }
}
