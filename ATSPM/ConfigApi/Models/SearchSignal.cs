#region license
// Copyright 2024 Utah Departement of Transportation
// for ConfigApi - ATSPM.ConfigApi.Models/SearchSignal.cs
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
namespace ATSPM.ConfigApi.Models
{
    public class SearchLocation
    {
        public int Id { get; set; }
        public string locationIdentifier { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string PrimaryName { get; set; }
        public string SecondaryName { get; set; }
        public bool ChartEnabled { get; set; }
        public int? RegionId { get; set; }
        public int? JurisdictionId { get; set; }
        public IEnumerable<int> Areas { get; set; }
        public IEnumerable<int> Charts { get; set; }
        public DateTime Start { get; set; }
        public int LocationTypeId { get; set; }
    }
}
