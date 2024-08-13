﻿#region license
// Copyright 2024 Utah Departement of Transportation
// for ReportApi - ATSPM.ReportApi.DataAggregation/AggregationResult.cs
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

using System.Runtime.Serialization;

namespace Utah.Udot.Atspm.ReportApi.DataAggregation
{
    public class AggregationResult
    {
        public AggregationResult()
        {
            Series = new List<Series>();
        }

        public AggregationResult(string identifier, List<Series> series)
        {
            Identifier = identifier;
            Series = series;
        }

        public string Identifier { get; set; }
        public List<Series> Series { get; set; }
    }


    [KnownType(typeof(DataPointDateDouble))]
    public class Series
    {
        public Series()
        {
            DataPoints = new List<AggregationDataPoint>();
        }

        public string Identifier { get; set; }
        public List<AggregationDataPoint> DataPoints { get; set; }
    }
}
