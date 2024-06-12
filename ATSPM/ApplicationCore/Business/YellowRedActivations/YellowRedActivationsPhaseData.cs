#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.YellowRedActivations/YellowRedActivationsPhaseData.cs
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
using ATSPM.Application.Business.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Business.YellowRedActivations
{
    public class YellowRedActivationsPhaseData
    {
        public bool GetPermissivePhase { get; set; }

        public VolumeCollection Volume { get; }

        public int PhaseNumber { get; set; }

        public double Violations
        {
            get { return Plans.Sum(d => d.Violations); }
        }

        public IList<YellowRedActivationPlan> Plans { get; set; }

        public List<YellowRedActivationsCycle> Cycles { get; set; }

        public double SevereRedLightViolationSeconds { get; set; }

        public double SevereRedLightViolations
        {
            get { return Plans.Sum(d => d.SevereRedLightViolations); }
        }

        public double TotalVolume { get; set; }

        public double PercentViolations
        {
            get
            {
                if (TotalVolume > 0)
                    return Math.Round(Violations / TotalVolume * 100, 0);
                return 0;
            }
        }

        public double PercentSevereViolations
        {
            get
            {
                if (TotalVolume > 0)
                    return Math.Round(SevereRedLightViolations / TotalVolume * 100, 2);
                return 0;
            }
        }

        public double YellowOccurrences
        {
            get { return Plans.Sum(d => d.YellowOccurrences); }
        }

        public double TotalYellowTime
        {
            get { return Plans.Sum(d => d.TotalYellowTime); }
        }

        public double AverageTYLO => Math.Round(TotalYellowTime / YellowOccurrences, 1);

        public double PercentYellowOccurrences
        {
            get
            {
                if (TotalVolume > 0)
                    return Math.Round(YellowOccurrences / TotalVolume * 100, 0);
                return 0;
            }
        }

        public double ViolationTime
        {
            get { return Plans.Sum(p => p.ViolationTime); }
        }

        public Approach Approach { get; set; }
    }
}