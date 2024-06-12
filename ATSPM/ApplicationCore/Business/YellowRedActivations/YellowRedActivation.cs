#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.YellowRedActivations/YellowRedActivation.cs
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

namespace ATSPM.Application.Business.YellowRedActivations
{
    public class YellowRedActivation
    {
        //The actual time of the detector activation

        //Represents a time span from the start of the red to red cycle


        /// <summary>
        ///     Constructor for the DetectorDataPoint. Sets the timestamp
        ///     and the y coordinate.
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="eventTime"></param>
        public YellowRedActivation(DateTime startDate, DateTime eventTime)
        {
            TimeStamp = eventTime;
            YPoint = (eventTime - startDate).TotalSeconds;
        }

        public double YPoint { get; }

        public DateTime TimeStamp { get; }
    }
}