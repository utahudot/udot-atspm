#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.Bins/BinsContainer.cs
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

namespace ATSPM.Application.Business.Bins
{
    public class BinsContainer
    {
        public BinsContainer(DateTime start, DateTime end)
        {
            Start = start;
            End = end;
        }

        public List<Bin> Bins { get; set; } = new List<Bin>();

        public double SumValue
        {
            get { return Bins.Sum(b => b.Sum); }
        }

        public int AverageValue
        {
            get { return Convert.ToInt32(Math.Round(Bins.Average(b => b.Sum))); }
        }

        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }
}