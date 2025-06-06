#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Business.TransitSignalPriority/TransitSignalPhase.cs
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

namespace Utah.Udot.Atspm.Business.TransitSignalPriority
{
    public class TransitSignalPhase
    {
        public int PhaseNumber { get; set; }
        public double MinGreen { get; set; }
        public double Yellow { get; set; }
        public double RedClearance { get; set; }
        public double MinTime { get; set; }
        public double ProgrammedSplit { get; set; }
        public double PercentileSplit85th { get; set; }
        public double PercentileSplit50th { get; set; }
        public double AverageSplit { get; set; }
        public double PercentMaxOutsForceOffs { get; set; }
        public double PercentGapOuts { get; set; }
        public double PercentSkips { get; set; }
        public double? RecommendedTSPMax { get; set; }
        public double SkipsGreaterThan70TSPMax { get; set; }
        public double ForceOffsLessThan40TSPMax { get; set; }
        public double ForceOffsLessThan60TSPMax { get; set; }
        public double ForceOffsLessThan80TSPMax { get; set; }

        public bool IsSkipsGreaterThan70TSPMax { get; set; }
        public bool IsForceOffsLessThan40TSPMax { get; set; }
        public bool IsForceOffsLessThan60TSPMax { get; set; }
        public bool IsForceOffsLessThan80TSPMax { get; set; }

        public int MaxReduction { get; set; } //TSP MAX
        public int MaxExtension { get; set; } //Sum of the non designated phase TSP Max for ring assume 16 phases 4 rings
        public int PriorityMin { get; set; } // Program split minus tsp max
        public int PriorityMax { get; set; } //Program split plus the max extension
        public string Notes { get; set; }

    }
}