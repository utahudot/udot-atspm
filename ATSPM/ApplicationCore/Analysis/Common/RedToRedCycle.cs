#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Analysis.Common/RedToRedCycle.cs
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
using ATSPM.Data.Interfaces;
using ATSPM.Domain.Common;
using System;
using System.Text.Json;

namespace ATSPM.Application.Analysis.Common
{

    /// <summary>
    /// A cycle which is the time between two <see cref="9"/> events including
    /// <see cref="1"/> and <see cref="8"/>
    /// </summary>
    public interface IRedToRedCycle : IStartEndRange, ILocationPhaseLayer, ICycleTotal
    {
        /// <summary>
        /// Timestamp of <see cref="1"/> event
        /// </summary>
        DateTime GreenEvent { get; set; }

        /// <summary>
        /// Timestamp of <see cref="8"/> event
        /// </summary>
        DateTime YellowEvent { get; set; }
    }

    /// <summary>
    /// A cycle which is the time between two <see cref="9"/> events including
    /// <see cref="1"/> and <see cref="8"/>
    /// </summary>
    public class RedToRedCycle : StartEndRange, IRedToRedCycle
    {
        #region IRedToRedCycle

        /// <inheritdoc/>
        public DateTime GreenEvent { get; set; }

        /// <inheritdoc/>
        public DateTime YellowEvent { get; set; }

        #region ILocationPhaseLayer

        /// <inheritdoc/>
        public string LocationIdentifier { get; set; }

        /// <inheritdoc/>
        public int PhaseNumber { get; set; }

        #endregion

        #region ICycle

        /// <inheritdoc/>
        public double TotalGreenTime => (YellowEvent - GreenEvent).TotalSeconds;

        /// <inheritdoc/>
        public double TotalYellowTime => (End - YellowEvent).TotalSeconds;

        /// <inheritdoc/>
        public double TotalRedTime => (GreenEvent - Start).TotalSeconds;

        /// <inheritdoc/>
        public double TotalTime => (End - Start).TotalSeconds;

        #endregion

        #endregion

        /// <inheritdoc/>
        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
