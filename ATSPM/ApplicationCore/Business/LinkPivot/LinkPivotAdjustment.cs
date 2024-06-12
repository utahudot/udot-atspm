#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.LinkPivot/LinkPivotAdjustment.cs
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
using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Business.LinkPivot
{
    public class LinkPivotAdjustment
    {
        public LinkPivotAdjustment(int linkNumber, string locationIdentifier, string location, int delta, int adjustment)
        {
            LinkNumber = linkNumber;
            LocationIdentifier = locationIdentifier;
            Location = location;
            Delta = delta;
            Adjustment = adjustment;
        }

        public int LinkNumber { get; set; }
        public string LocationIdentifier { get; set; }
        public string Location { get; set; }
        public int Delta { get; set; }
        public int Adjustment { get; set; }
    }
}
