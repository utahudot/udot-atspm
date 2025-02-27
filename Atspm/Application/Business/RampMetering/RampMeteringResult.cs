#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Business.RampMetering/RampMeteringResult.cs
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
using Utah.Udot.Atspm.Business.TimeSpaceDiagram;
using Utah.Udot.Atspm.Business.TimingAndActuation;

namespace Utah.Udot.Atspm.Business.RampMetering
{
    public class RampMeteringResult : LocationResult
    {
        public RampMeteringResult(string locationId, DateTime start, DateTime end) : base(locationId, start, end)
        { }

        public List<DataPointForDouble> MainlineAvgFlow { get; set; }
        public List<DataPointForDouble> MainlineAvgOcc { get; set; }
        public List<DataPointForDouble> MainlineAvgSpeed { get; set; }
        public List<TimeSpaceEventBase> StartUpWarning { get; set; }
        public List<TimeSpaceEventBase> ShutdownWarning { get; set; }
        public List<DescriptionWithDataPoints> LanesActiveRate { get; set; }
        public List<DescriptionWithDataPoints> LanesBaseRate { get; set; }
        public List<DataPointForDetectorEvent> LanesQueueOnAndOffEvents { get; set; }
    }
}
