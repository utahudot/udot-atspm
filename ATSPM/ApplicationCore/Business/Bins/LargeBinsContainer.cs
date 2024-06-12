#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.Bins/LargeBinsContainer.cs
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
    public class LargeBinsContainer
    {
        public List<BinsContainer> BinsContainers = new List<BinsContainer>();

        public double SumValue
        {
            get
            {
                double sum = 0;

                foreach (var containter in BinsContainers)
                    sum = sum + containter.Bins.Sum(b => b.Sum);

                return sum;
            }
        }

        public int BinCount
        {
            get
            {
                var count = 0;

                foreach (var containter in BinsContainers)
                    count = count + containter.Bins.Count;

                return count;
            }
        }

        public int AverageValue
        {
            get
            {
                {
                    double sum = 0;
                    double count = 0;

                    foreach (var containter in BinsContainers)
                    {
                        sum = sum + containter.Bins.Sum(b => b.Sum);
                        count = count + containter.Bins.Count;
                    }

                    if (sum > count && sum > 0 && count > 0)
                        return Convert.ToInt32(Math.Round(sum / count));

                    return 0;
                }
            }
        }

        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }
}