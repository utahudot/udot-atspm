#region license
// Copyright 2026 Utah Departement of Transportation
// for ApplicationTests - Utah.Udot.ATSPM.ApplicationTests.Business.Common/VolumeCollectionTests.cs
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

using System;
using System.Collections.Generic;
using System.Linq;
using Utah.Udot.Atspm.Business.Common;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.TempExtensions;
using Xunit;

namespace Utah.Udot.ATSPM.ApplicationTests.Business.Common;

public class VolumeCollectionTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void NonPositiveBinSize_IsRejected(int binSize)
    {
        var start = new DateTime(2026, 4, 1, 8, 0, 0);
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new VolumeCollection(start, start.AddHours(1), new List<IndianaEvent>(), binSize));
    }

    [Fact]
    public void PartialBin_StopsAtReportEndAndRetainsHalfOpenBoundaries()
    {
        var start = new DateTime(2026, 4, 1, 8, 0, 0);
        var end = start.AddMinutes(16);
        var events = new[] { start.AddTicks(-1), start, start.AddMinutes(15), end.AddTicks(-1), end, end.AddMinutes(1) }
            .Select(time => new IndianaEvent { Timestamp = time }).ToList();

        var volumes = new VolumeCollection(start, end, events, 15);

        Assert.Equal(new[] { 1, 2 }, volumes.Items.Select(v => v.DetectorCount));
        Assert.Equal(end, volumes.Items[1].EndTime);
        Assert.Equal(3, volumes.TotalDetectorCounts);
    }

    [Theory]
    [InlineData(1000, 0)]
    [InlineData(0, -1)]
    [InlineData(2000, 1)]
    public void Correction_AppliesOffsetAndLatencyBeforeRangeFilter(double offset, double latency)
    {
        var start = new DateTime(2026, 4, 1, 8, 0, 0);
        var end = start.AddHours(1);
        var events = new[]
        {
            new IndianaEvent { LocationIdentifier = "1001", EventCode = 82, EventParam = 1, Timestamp = start.AddMilliseconds(-500) },
            new IndianaEvent { LocationIdentifier = "1001", EventCode = 82, EventParam = 1, Timestamp = end.AddMilliseconds(-500) }
        };

        var result = events.GetEventsByEventCodesParamWithOffsetAndLatencyCorrection(start, end, new short[] { 82 }, 1, offset, latency);

        var kept = Assert.Single(result);
        Assert.Equal(start.AddMilliseconds(500), kept.Timestamp);
        Assert.Equal("1001", kept.LocationIdentifier);
        Assert.Equal(start.AddMilliseconds(-500), events[0].Timestamp);
    }
}
