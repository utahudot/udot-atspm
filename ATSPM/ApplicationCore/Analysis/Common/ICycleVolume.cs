#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Analysis.Common/ICycleVolume.cs
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
using ATSPM.Data.Enums;
using ATSPM.Domain.Common;

namespace ATSPM.Application.Analysis.Common
{
    /// <summary>
    /// Defines <see cref="IndianaEnumerations.VehicleDetectorOn"/> event volume information for cycles
    /// </summary>
    public interface ICycleVolume : IStartEndRange //: ICycle
    {
        /// <summary>
        /// The total delay of <see cref="IndianaEnumerations.VehicleDetectorOn"/> events
        /// arriving on red before a <see cref="1"/> event
        /// </summary>
        double TotalDelay { get; }

        /// <summary>
        /// The total amount of <see cref="IndianaEnumerations.VehicleDetectorOn"/> events with corrected timestamps
        /// arriving within cycle
        /// </summary>
        double TotalVolume { get; }
    }
}
