#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.Aggregation.FilterExtensions/FilterMovement.cs
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
using System.Runtime.Serialization;

namespace ATSPM.Application.Business.Aggregation.FilterExtensions
{
    [DataContract]
    public class FilterMovement
    {
        public FilterMovement()
        {
        }

        public FilterMovement(int movementTypeId, string description, bool include)
        {
            MovementTypeId = movementTypeId;
            Include = include;
            Description = description;
        }

        [DataMember]
        public int MovementTypeId { get; set; }

        public string Description { get; set; }

        [DataMember]
        public bool Include { get; set; }
    }
}