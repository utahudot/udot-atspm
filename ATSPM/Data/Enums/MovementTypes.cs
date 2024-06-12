#region license
// Copyright 2024 Utah Departement of Transportation
// for Data - ATSPM.Data.Enums/MovementTypes.cs
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
    public enum MovementTypes
    {
        [Display(Name = "Unknown", Order = 6)]
        NA = 0,
        [Display(Name = "Thru", Order = 3)]
        T = 1,
        [Display(Name = "Right", Order = 5)]
        R = 2,
        [Display(Name = "Left", Order = 1)]
        L = 3,
        [Display(Name = "Thru-Right", Order = 4)]
        TR = 4,
        [Display(Name = "Thru-Left", Order = 2)]
        TL = 5,
        [Display(Name = "Northwest", Order = 6)]
        NW = 6,
    }
}
