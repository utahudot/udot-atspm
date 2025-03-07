#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Business.ApproachVolume/ApproachVolumeResult.cs
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
using Utah.Udot.Atspm.Data.Enums;

namespace Utah.Udot.Atspm.Business.ApproachVolume;

/// <summary>
/// Approach Volume Chart
/// </summary>
public class ApproachVolumeResult : LocationResult
{
    public ApproachVolumeResult(
        string locationId,
        DateTime start,
        DateTime end,
        DirectionTypes primaryDirectionType,
        DirectionTypes opposingDirectionType) : base(locationId, start, end)
    {
        PrimaryDirectionName = primaryDirectionType.ToString();
        OpposingDirectionName = opposingDirectionType.ToString();
    }

    public ApproachVolumeResult(
        string locationIdentifier,
        DateTime start,
        DateTime end,
        string detectorType,
        int distanceFromStopBar,
        string primaryDirectionName,
        string opposingDirectionName) : base(locationIdentifier, start, end)
    {
        DetectorType = detectorType;
        DistanceFromStopBar = distanceFromStopBar;
        PrimaryDirectionName = primaryDirectionName;
        OpposingDirectionName = opposingDirectionName;
    }

    public string PrimaryDirectionName { get; set; }
    public string OpposingDirectionName { get; set; }
    public int DistanceFromStopBar { get; set; }
    public string DetectorType { get; set; }
    public ICollection<DataPointForInt> PrimaryDirectionVolumes { get; set; }
    public ICollection<DataPointForInt> OpposingDirectionVolumes { get; set; }
    public ICollection<DataPointForInt> CombinedDirectionVolumes { get; set; }
    public ICollection<DataPointForDouble> PrimaryDFactors { get; set; }
    public ICollection<DataPointForDouble> OpposingDFactors { get; set; }
    public SummaryData SummaryData { get; set; }

}

public class SummaryData
{

    public string PeakHour { get; set; }
    public double KFactor { get; set; }
    public int PeakHourVolume { get; set; }
    public double PeakHourFactor { get; set; }
    public int TotalVolume { get; set; }
    public string PrimaryPeakHour { get; set; }
    public double PrimaryKFactor { get; set; }
    public int PrimaryPeakHourVolume { get; set; }
    public double PrimaryPeakHourFactor { get; set; }
    public int PrimaryTotalVolume { get; set; }
    public double PrimaryDFactor { get; set; }
    public string OpposingPeakHour { get; set; }
    public double OpposingKFactor { get; set; }
    public int OpposingPeakHourVolume { get; set; }
    public double OpposingPeakHourFactor { get; set; }
    public int OpposingTotalVolume { get; set; }
    public double OpposingDFactor { get; set; }
}
