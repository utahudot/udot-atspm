#region license
// Copyright 2024 Utah Departement of Transportation
// for Data - ATSPM.Data.Enums/DirectionTypes.cs
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
    public enum DirectionTypes
    {
        [Display(Name = "Unknown", Order = 0)]
        NA = 0,
        [Display(Name = "Northbound", Order = 3)]
        NB = 1,
        [Display(Name = "Southbound", Order = 4)]
        SB = 2,
        [Display(Name = "Eastbound", Order = 1)]
        EB = 3,
        [Display(Name = "Westbound", Order = 2)]
        WB = 4,
        [Display(Name = "Northeast", Order = 5)]
        NE = 5,
        [Display(Name = "Northwest", Order = 6)]
        NW = 6,
        [Display(Name = "Southeast", Order = 7)]
        SE = 7,
        [Display(Name = "Southwest", Order = 8)]
        SW = 8
    }
}
