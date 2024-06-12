#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.Common/DataPointDateDouble.cs
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

namespace ATSPM.Application.Business.Common
{
    public class DataPointDateDouble : IAggregationDataPoint
    {

        public DataPointDateDouble()
        {
        }

        public DataPointDateDouble(DateTime start, double value)
        {
            this.Start = start;
            this.Value = value;
        }

        public DateTime Start { get; set; }

        public double Value { get; set; }
    }
}
