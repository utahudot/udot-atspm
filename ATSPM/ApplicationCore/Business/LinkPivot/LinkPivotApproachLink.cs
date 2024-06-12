#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.LinkPivot/LinkPivotApproachLink.cs
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
using System;

namespace ATSPM.Application.Business.LinkPivot
{
    public class LinkPivotApproachLink
    {
        public LinkPivotApproachLink(string locationIdentifier, string location,
            string upstreamApproachDirection, string downstreamLocationIdentifier,
            string downstreamLocation, string downstreamApproachDirection,
            int pAOGUpstreamBefore, int pAOGUpstreamPredicted,
            int pAOGDownstreamBefore, int pAOGDownstreamPredicted,
            double aOGUpstreamBefore, double aOGUpstreamPredicted,
            double aOGDownstreamBefore, double aOGDownstreamPredicted,
            double delta, string resultChartLocation, double aogTotalBefore,
            int pAogTotalBefore, double aogTotalPredicted, int pAogTotalPredicted,
            int linkNumber)
        {
            LocationIdentifier = locationIdentifier;
            Location = location;
            UpstreamApproachDirection = upstreamApproachDirection;
            DownstreamLocationIdentifier = downstreamLocationIdentifier;
            DownstreamLocation = downstreamLocation;
            DownstreamApproachDirection = downstreamApproachDirection;
            PAOGUpstreamBefore = pAOGUpstreamBefore;
            PAOGUpstreamPredicted = pAOGUpstreamPredicted;
            PAOGDownstreamBefore = pAOGDownstreamBefore;
            PAOGDownstreamPredicted = pAOGDownstreamPredicted;
            AOGUpstreamBefore = aOGUpstreamBefore;
            AOGUpstreamPredicted = aOGUpstreamPredicted;
            AOGDownstreamBefore = aOGDownstreamBefore;
            AOGDownstreamPredicted = aOGDownstreamPredicted;
            Delta = delta;
            ResultChartLocation = resultChartLocation;
            AogTotalBefore = aogTotalBefore;
            PAogTotalBefore = pAogTotalBefore;
            AogTotalPredicted = aogTotalPredicted;
            PAogTotalPredicted = pAogTotalPredicted;
            LinkNumber = linkNumber;

            //Get the Total Chart Settings
            var tempChange = PAogTotalPredicted - PAogTotalBefore;
            if (tempChange < 0)
            {
                TotalChartPositiveChange = 0;
                TotalChartNegativeChange = Math.Abs(tempChange);
                TotalChartExisting = PAogTotalBefore - TotalChartNegativeChange;
            }
            else
            {
                TotalChartNegativeChange = 0;
                TotalChartPositiveChange = tempChange;
                TotalChartExisting = PAogTotalBefore;
            }
            TotalChartRemaining = 100 - (TotalChartExisting + TotalChartPositiveChange + TotalChartNegativeChange);

            //Get the Upstream Chart Settings
            tempChange = PAOGUpstreamPredicted - PAOGUpstreamBefore;
            if (tempChange < 0)
            {
                UpstreamChartPositiveChange = 0;
                UpstreamChartNegativeChange = Math.Abs(tempChange);
                UpstreamChartExisting = PAOGUpstreamBefore - UpstreamChartNegativeChange;
            }
            else
            {
                UpstreamChartNegativeChange = 0;
                UpstreamChartPositiveChange = tempChange;
                UpstreamChartExisting = PAOGUpstreamBefore;
            }
            UpstreamChartRemaining =
                100 - (UpstreamChartExisting + UpstreamChartPositiveChange + UpstreamChartNegativeChange);

            //Get the Downstream Chart Settings
            tempChange = PAOGDownstreamPredicted - PAOGDownstreamBefore;
            if (tempChange < 0)
            {
                DownstreamChartPositiveChange = 0;
                DownstreamChartNegativeChange = Math.Abs(tempChange);
                DownstreamChartExisting = PAOGDownstreamBefore - DownstreamChartNegativeChange;
            }
            else
            {
                DownstreamChartNegativeChange = 0;
                DownstreamChartPositiveChange = tempChange;
                DownstreamChartExisting = PAOGDownstreamBefore;
            }
            DownstreamChartRemaining =
                100 - (DownstreamChartExisting + DownstreamChartPositiveChange + DownstreamChartNegativeChange);
        }

        public string LocationIdentifier { get; set; }
        public string Location { get; set; }
        public string UpstreamApproachDirection { get; set; }
        public string DownstreamLocationIdentifier { get; set; }
        public string DownstreamLocation { get; set; }
        public string DownstreamApproachDirection { get; set; }
        public int PAOGUpstreamBefore { get; set; }
        public int PAOGUpstreamPredicted { get; set; }
        public int PAOGDownstreamBefore { get; set; }
        public int PAOGDownstreamPredicted { get; set; }
        public double AOGUpstreamBefore { get; set; }
        public double AOGUpstreamPredicted { get; set; }
        public double AOGDownstreamBefore { get; set; }
        public double AOGDownstreamPredicted { get; set; }
        public double Delta { get; set; }
        public string ResultChartLocation { get; set; }
        public string UpstreamCombinedLocation => LocationIdentifier + "\n" + UpstreamApproachDirection;
        public string DownstreamCombinedLocation => DownstreamLocationIdentifier + "\n" + DownstreamApproachDirection;
        public double AogTotalBefore { get; set; }
        public int PAogTotalBefore { get; set; }
        public double AogTotalPredicted { get; set; }
        public int PAogTotalPredicted { get; set; }

        public double TotalChartExisting { get; set; }
        public int TotalChartPositiveChange { get; set; }
        public int TotalChartNegativeChange { get; set; }
        public double TotalChartRemaining { get; set; }

        public int UpstreamChartExisting { get; set; }
        public int UpstreamChartPositiveChange { get; set; }
        public int UpstreamChartNegativeChange { get; set; }
        public int UpstreamChartRemaining { get; set; }

        public int DownstreamChartExisting { get; set; }
        public int DownstreamChartPositiveChange { get; set; }
        public int DownstreamChartNegativeChange { get; set; }
        public int DownstreamChartRemaining { get; set; }

        public string TotalChartName => "Total" + LocationIdentifier + "Chart";
        public string UpstreamChartName => "Up" + LocationIdentifier + "Chart";
        public string DownstreamChartName => "Down" + LocationIdentifier + "Chart";

        public int LinkNumber { get; set; }
    }
}
