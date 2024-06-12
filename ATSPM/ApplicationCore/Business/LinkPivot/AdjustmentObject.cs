#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.LinkPivot/AdjustmentObject.cs
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
using ATSPM.Data.Enums;
using ATSPM.Data.Models;

namespace ATSPM.Application.Business.LinkPivot
{
    public class AdjustmentObject
    {
        public double DownstreamVolume { get; set; }
        public double UpstreamVolume { get; set; }
        public int LinkNumber { get; set; }
        public double AogTotalBefore { get; set; }
        public int PAogTotalBefore { get; set; }
        public double AogTotalPredicted { get; set; }
        public int PAogTotalPredicted { get; set; }
        public string ResultChartLocation { get; set; }
        public string DownstreamLocation { get; set; }
        public string DownLocationIdentifier { get; set; }
        public string DownstreamApproachDirection { get; set; }
        public string UpstreamApproachDirection { get; set; }
        public double AOGDownstreamPredicted { get; set; }
        public double AOGUpstreamPredicted { get; set; }
        public double AOGDownstreamBefore { get; set; }
        public double AOGUpstreamBefore { get; set; }
        public int PAOGDownstreamPredicted { get; set; }
        public int PAOGUpstreamPredicted { get; set; }
        public int PAOGDownstreamBefore { get; set; }
        public int PAOGUpstreamBefore { get; set; }
        public int Adjustment { get; set; }
        public int Delta { get; set; }
        public string LocationIdentifier { get; set; }
        public string DownstreamLocationIdentifier { get; set; }
        public string Location { get; set; }

    }
}
