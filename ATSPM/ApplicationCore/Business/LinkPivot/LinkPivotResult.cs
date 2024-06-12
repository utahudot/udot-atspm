#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.LinkPivot/LinkPivotResult.cs
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
using System.Collections.Generic;
using System;

namespace ATSPM.Application.Business.LinkPivot
{
    public class LinkPivotResult
    {

        public LinkPivotResult()
        {
            Adjustments = new List<LinkPivotAdjustment>();
            ApproachLinks = new List<LinkPivotApproachLink>();
        }

        public List<LinkPivotAdjustment> Adjustments { get; set; }
        public List<LinkPivotApproachLink> ApproachLinks { get; set; }

        //Summary Info
        public double TotalAogDownstreamBefore { get; set; }

        public int TotalPaogDownstreamBefore { get; set; }
        public double TotalAogDownstreamPredicted { get; set; }
        public int TotalPaogDownstreamPredicted { get; set; }
        public double TotalAogUpstreamBefore { get; set; }
        public int TotalPaogUpstreamBefore { get; set; }
        public double TotalAogUpstreamPredicted { get; set; }
        public int TotalPaogUpstreamPredicted { get; set; }
        public double TotalAogBefore { get; set; }
        public int TotalPaogBefore { get; set; }
        public double TotalAogPredicted { get; set; }
        public int TotalPaogPredicted { get; set; }

        //Total change chart
        public int TotalChartExisting { get; set; }

        public int TotalChartPositiveChange { get; set; }
        public int TotalChartNegativeChange { get; set; }
        public int TotalChartRemaining { get; set; }

        //Total upstream change chart
        public int TotalUpstreamChartExisting { get; set; }

        public int TotalUpstreamChartPositiveChange { get; set; }
        public int TotalUpstreamChartNegativeChange { get; set; }
        public int TotalUpstreamChartRemaining { get; set; }

        //Total downstream change chart
        public int TotalDownstreamChartExisting { get; set; }

        public int TotalDownstreamChartPositiveChange { get; set; }
        public int TotalDownstreamChartNegativeChange { get; set; }
        public int TotalDownstreamChartRemaining { get; set; }

        public void SetSummary()
        {
            //Get the Total Summary Chart Settings
            var tempChange = TotalPaogPredicted - TotalPaogBefore;
            if (tempChange < 0)
            {
                TotalChartPositiveChange = 0;
                TotalChartNegativeChange = Math.Abs(tempChange);
                TotalChartExisting = TotalPaogBefore - TotalChartNegativeChange;
            }
            else
            {
                TotalChartNegativeChange = 0;
                TotalChartPositiveChange = tempChange;
                TotalChartExisting = TotalPaogBefore;
            }
            TotalChartRemaining = 100 - (TotalChartExisting + TotalChartPositiveChange + TotalChartNegativeChange);

            //Get the Upstream Summary Chart Settings
            tempChange = TotalPaogUpstreamPredicted - TotalPaogUpstreamBefore;
            if (tempChange < 0)
            {
                TotalUpstreamChartPositiveChange = 0;
                TotalUpstreamChartNegativeChange = Math.Abs(tempChange);
                TotalUpstreamChartExisting = TotalPaogUpstreamBefore - TotalUpstreamChartNegativeChange;
            }
            else
            {
                TotalUpstreamChartNegativeChange = 0;
                TotalUpstreamChartPositiveChange = tempChange;
                TotalUpstreamChartExisting = TotalPaogUpstreamBefore;
            }
            TotalUpstreamChartRemaining = 100 - (TotalUpstreamChartExisting + TotalUpstreamChartPositiveChange +
                                                 TotalUpstreamChartNegativeChange);

            //Get the Downstream Summary Chart Settings
            tempChange = TotalPaogDownstreamPredicted - TotalPaogDownstreamBefore;
            if (tempChange < 0)
            {
                TotalDownstreamChartPositiveChange = 0;
                TotalDownstreamChartNegativeChange = Math.Abs(tempChange);
                TotalDownstreamChartExisting = TotalPaogDownstreamBefore - TotalDownstreamChartNegativeChange;
            }
            else
            {
                TotalDownstreamChartNegativeChange = 0;
                TotalDownstreamChartPositiveChange = tempChange;
                TotalDownstreamChartExisting = TotalPaogDownstreamBefore;
            }
            TotalDownstreamChartRemaining = 100 - (TotalDownstreamChartExisting + TotalDownstreamChartPositiveChange +
                                                   TotalDownstreamChartNegativeChange);
        }
    }
}
