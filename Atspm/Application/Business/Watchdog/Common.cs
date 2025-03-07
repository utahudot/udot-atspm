#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Business.Watchdog/Common.cs
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

namespace Utah.Udot.Atspm.Business.Watchdog
{
    public class WatchDogProductInfo
    {
        public string Name { get; set; }
        public List<WatchDogModel<WatchDogFirmwareCount>> Model { get; set; }
    }

    public class WatchDogModel<T>
    {
        public string Name { get; set; }
        public List<T> Firmware { get; set; }
    }

    public class WatchDogFirmwareCount
    {
        public string Name { get; set; }
        public int Counts { get; set; }
    }
    public class WatchDogHardwareCount
    {
        public string Name { get; set; }
        public int Counts { get; set; }
    }

    public class WatchDogIssueTypeCount
    {
        public string Name { get; set; }
        public int Counts { get; set; }
    }

    public class WatchDogFirmwareWithIssueType
    {
        public string Name { get; set; }
        public List<WatchDogIssueTypeCount> IssueType { get; set; }
    }

    //public class WatchDogControllerInfo
    //{
    //    public string Name { get; set; }
    //    public List<WatchDogModel<WatchDogFirmwareWithIssueType>> Model { get; set; }
    //}
}
