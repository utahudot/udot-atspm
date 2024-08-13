﻿#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.LinkPivot/LinkPivotPcdResult.cs
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

using Utah.Udot.Atspm.Business.PurdueCoordinationDiagram;

namespace Utah.Udot.Atspm.Business.LinkPivot
{
    public class LinkPivotPcdResult
    {
        public LinkPivotPcdResult()
        {
            pcdExisting = new List<PurdueCoordinationDiagramResult>();
            pcdPredicted = new List<PurdueCoordinationDiagramResult>();
        }
        public double ExistingTotalAOG { get; set; }
        public double ExistingTotalPAOG { get; set; } = 0;
        public double PredictedTotalAOG { get; set; }
        public double PredictedTotalPAOG { get; set; } = 0;
        public double PredictedVolume { get; set; }
        public double ExistingVolume { get; set; }
        public List<PurdueCoordinationDiagramResult> pcdExisting { get; set; }
        public List<PurdueCoordinationDiagramResult> pcdPredicted { get; set; }
    }
}
