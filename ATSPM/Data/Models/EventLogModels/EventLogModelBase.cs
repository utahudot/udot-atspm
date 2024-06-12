#region license
// Copyright 2024 Utah Departement of Transportation
// for Data - ATSPM.Data.Models.EventLogModels/EventLogModelBase.cs
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
using ATSPM.Data.Interfaces;
using ATSPM.Domain.Common;
using Newtonsoft.Json;

#nullable disable

namespace ATSPM.Data.Models.EventLogModels
{
    /// <summary>
    /// Event log model base for models used in logging Atspm device data
    /// </summary>
    public abstract class EventLogModelBase : ILocationLayer, ITimestamp
    {
        ///<inheritdoc/>
        [JsonIgnore]
        public string LocationIdentifier { get; set; }

        ///<inheritdoc/>
        public DateTime Timestamp { get; set; }
    }
}
