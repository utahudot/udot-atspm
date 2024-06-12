#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Analysis.Common/ICycleTotal.cs
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
    /// Defines the start and end of a red to red cycle
    /// which is the time between two <see cref="9"/> events including
    /// <see cref="1"/> and <see cref="8"/>
    /// </summary>
    public interface ICycleTotal : IStartEndRange
    {
        /// <summary>
        /// The total green time is defined as the time from start of <see cref="1"/> to the start of <see cref="8"/> in seconds
        /// </summary>
        double TotalGreenTime { get; }

        /// <summary>
        /// The total yellow time is defined as the time from start of <see cref="8"/> to the second <see cref="9"/> in seconds
        /// </summary>
        double TotalYellowTime { get; }

        /// <summary>
        /// The total red time is defined as the first <see cref="9"/> to the <see cref="1"/> in seconds
        /// </summary>
        double TotalRedTime { get; }

        /// <summary>
        /// The total time is defined as the time between the first and second <see cref="9"/> in seconds
        /// </summary>
        double TotalTime { get; }
    }
}
