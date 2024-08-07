﻿#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.GreenTimeUtilization/ProgrammedSplit.cs
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

using Utah.Udot.Atspm.Business.Common;

namespace Utah.Udot.Atspm.Business.GreenTimeUtilization
{
    public class ProgrammedSplit : DataPointBase
    {
        public double ProgValue { get; set; }


        public ProgrammedSplit(Plan analysisPlan, DateTime analysisStart, double splitLength, double durYR) : base(analysisPlan.Start)
        {
            if (analysisStart < analysisPlan.Start)
            {
                Timestamp = analysisPlan.Start;
            }
            else
            {
                Timestamp = analysisStart;
            }

            if (splitLength >= durYR)
            {
                ProgValue = splitLength - durYR;
            }
            else
            {
                ProgValue = 0;
            }
        }

    }
}