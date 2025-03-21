﻿#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Business.WaitTime/Volume.cs
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

namespace Utah.Udot.Atspm.Business.WaitTime
{
    public class Volume
    {
        public Volume(DateTime startTime, int volume)
        {
            StartTime = startTime;
            VolumePerHour = volume;
        }

        public DateTime StartTime { get; internal set; }
        public int VolumePerHour { get; internal set; }
    }
}