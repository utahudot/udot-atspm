#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.ApproachVolume/MetricInfo.cs
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
namespace ATSPM.Application.Business.ApproachVolume
{
    public class MetricInfo
    {
        public string Direction2PeakHourString { get; set; }
        public double Direction2PeakHourDFactor { get; set; }
        public double Direction2PeakHourKFactor { get; set; }
        public double Direction2PeakHourFactor { get; set; }
        public int Direction2PeakHourMaxValue { get; set; }
        public int Direction2PeakHourVolume { get; set; }
        public string Direction1PeakHourString { get; set; }
        public double Direction1PeakHourDFactor { get; set; }
        public double Direction1PeakHourKFactor { get; set; }
        public double Direction1PeakHourFactor { get; set; }
        public int Direction1PeakHourMaxValue { get; set; }
        public int Direction1PeakHourVolume { get; set; }
        public int CombinedVolume { get; set; }
        public string CombinedPeakHourString { get; set; }
        public double CombinedPeakHourKFactor { get; set; }
        public double CombinedPeakHourFactor { get; set; }
        public int CombinedPeakHourValue { get; set; }
        public string ImageLocation { get; set; }
        public string Direction1 { get; set; }
        public string Direction2 { get; set; }
        public int Direction2Volume { get; set; }
        public int Direction1Volume { get; set; }
        public int CombinedPeakHourVolume { get; set; }
    }
}