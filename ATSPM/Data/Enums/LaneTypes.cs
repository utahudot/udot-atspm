#region license
// Copyright 2024 Utah Departement of Transportation
// for Data - ATSPM.Data.Enums/LaneTypes.cs
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ATSPM.Data.Enums
{
    public enum LaneTypes
    {
        [Display(Name = "Unknown", Order = 0)]
        NA = 0,
        [Display(Name = "Vehicle")]
        V = 1,
        [Display(Name = "Bike")]
        Bike = 2,
        [Display(Name = "Pedestrian")]
        Ped = 3,
        [Display(Name = "Exit")]
        E = 4,
        [Display(Name = "Light Rail Transit")]
        LRT = 5,
        [Display(Name = "Bus")]
        Bus = 6,
        [Display(Name = "High Occupancy Vehicle")]
        HDV = 7,
    }
}
