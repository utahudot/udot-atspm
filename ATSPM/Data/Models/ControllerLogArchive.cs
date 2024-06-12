#region license
// Copyright 2024 Utah Departement of Transportation
// for Data - ATSPM.Data.Models/ControllerLogArchive.cs
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
#nullable disable

namespace ATSPM.Data.Models
{
    public partial class ControllerLogArchive
    {
        public string SignalIdentifier { get; set; }
        public DateTime ArchiveDate { get; set; }

        public ICollection<ControllerEventLog> LogData { get; set; } = new List<ControllerEventLog>();

        public override string ToString()
        {
            return $"{SignalIdentifier}-{ArchiveDate:dd/MM/yyyy}-{LogData.Count}";
        }
    }
}
