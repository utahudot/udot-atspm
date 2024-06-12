#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.Common/VolumeCollection.cs
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
using ATSPM.Data.Models.EventLogModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Business.Common
{
    public class VolumeCollection
    {
        public List<Volume> Items = new List<Volume>();
        public int TotalDetectorCounts { get { return Items.Sum(i => i.DetectorCount); } }
        public int TotalHourlyVolume { get { return Items.Sum(i => i.HourlyVolume); } }

        public VolumeCollection(VolumeCollection primaryDirectionVolume, VolumeCollection opposingDirectionVolume, int binSize)
        {
            if (primaryDirectionVolume != null && opposingDirectionVolume != null)
            {
                for (int i = 0; i < primaryDirectionVolume.Items.Count; i++)
                {
                    Volume primaryBin = primaryDirectionVolume.Items[i];
                    Volume opposingBin = opposingDirectionVolume.Items[i];
                    Volume totalBin = new Volume(primaryBin.StartTime, primaryBin.EndTime, binSize);
                    totalBin.DetectorCount = primaryBin.DetectorCount + opposingBin.DetectorCount;
                    Items.Add(totalBin);
                }
            }
        }

        public VolumeCollection(DateTime startTime, DateTime endTime, List<IndianaEvent> detectorEvents,
            int binSize)
        {
            for (DateTime start = startTime; start < endTime; start = start.AddMinutes(binSize))
            {
                var v = new Volume(start, start.AddMinutes(binSize), binSize);
                v.DetectorCount = detectorEvents.Count(d => d.Timestamp >= v.StartTime && d.Timestamp < v.EndTime);
                Items.Add(v);
            }
        }

        public VolumeCollection(List<VolumeCollection> volumeCollections, int binSize)
        {
            // Combine all the lists into a single list using SelectMany
            List<Volume> combinedList = volumeCollections.SelectMany(list => list.Items).ToList();

            // Group the combined Volume objects by their start and end times
            var groupedVolumes = combinedList.GroupBy(volume => (volume.StartTime, volume.EndTime));

            // Create a list to store the combined Volume objects with summed DetectorCount
            List<Volume> combinedVolumes = groupedVolumes.Select(group =>
            {
                var combinedVolume = new Volume(group.Key.StartTime, group.Key.EndTime, binSize); // Adjust binSizeMultiplier as needed
                combinedVolume.DetectorCount = group.Sum(volume => volume.DetectorCount);
                return combinedVolume;
            }).ToList();
            Items = combinedVolumes;
        }


    }
}