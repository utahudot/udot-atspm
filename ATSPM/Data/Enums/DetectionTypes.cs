#region license
// Copyright 2024 Utah Departement of Transportation
// for Data - ATSPM.Data.Enums/DetectionTypes.cs
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
    public enum DetectionTypes
    {
        [Display(Name = "Unknown", Order = 0)]
        NA = 0,
        [Display(Name = "Basic", Order = 1)]
        B = 1,
        [Display(Name = "Advanced Count", Order = 2)]
        AC = 2,
        [Display(Name = "Advanced Speed", Order = 3)]
        AS = 3,
        [Display(Name = "Lane-by-lane Count", Order = 4)]
        LLC = 4,
        [Display(Name = "Lane-by-lane with Speed Restriction", Order = 5)]
        LLS = 5,
        [Display(Name = "Stop Bar Presence", Order = 6)]
        SBP = 6,
        [Display(Name = "Advanced Presence", Order = 7)]
        AP = 7,
    }
}
