#region license
// Copyright 2024 Utah Departement of Transportation
// for Data - ATSPM.Data.Enums/MetricFilterTypes.cs
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
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ATSPM.Data.Enums
{
    public enum MetricFilterTypes
    {
        [Display(Name = "Unknown", Order = 0)]
        Unknown,
        [Display(Name = "Location Id", Order = 1)]
        locationId,
        [Display(Name = "Primary Name", Order = 2)]
        PrimaryName,
        [Display(Name = "Secondary Name", Order = 3)]
        SecondaryName,
        [Display(Name = "Agency", Order = 4)]
        Agency
    }
}
