#region license
// Copyright 2025 Utah Departement of Transportation
// for Data - Utah.Udot.Atspm.Data.Models/ControllerEventLog.cs
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
using Utah.Udot.NetStandardToolkit.Interfaces;

#nullable disable

namespace Utah.Udot.Atspm.Data.Models
{
    /// <summary>
    /// Legacy event log for indiana enumeration events
    /// </summary>
    [Obsolete($"use {nameof(IndianaEvent)} instead")]
    public class ControllerEventLog : ITimestamp
    {
        public string SignalIdentifier { get; set; }
        public DateTime Timestamp { get; set; }
        public int EventCode { get; set; }
        public int EventParam { get; set; }

        public override string ToString()
        {
            return $"{SignalIdentifier}-{EventCode}-{EventParam}-{Timestamp}";
        }
    }
}
