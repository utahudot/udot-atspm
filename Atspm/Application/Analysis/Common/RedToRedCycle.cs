#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Analysis.Common/RedToRedCycle.cs
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

using System.Text.Json;
using Utah.Udot.Atspm.Data.Interfaces;

namespace Utah.Udot.Atspm.Analysis.Common
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

    public abstract class CycleBase : StartEndRange, ILocationPhaseLayer
    {
        #region ILocationPhaseLayer

        /// <inheritdoc/>
        public string LocationIdentifier { get; set; }

        /// <inheritdoc/>
        public int PhaseNumber { get; set; }

        #endregion


        public IntervalSpan GreenInterval { get; set; }
        public IntervalSpan RedInterval { get; set; }
        public IntervalSpan YellowInterval { get; set; }

        //public abstract TimeSpan TotalGreenTime { get; }
        //public abstract TimeSpan TotalYellowTime { get; }
        //public abstract TimeSpan TotalRedTime { get; }
        //public abstract TimeSpan TotalTime { get; }

        public override string ToString()
        {
            return $"{GetType().Name}: {LocationIdentifier}|{PhaseNumber} {Start} - {RedInterval.Span.TotalSeconds} - {YellowInterval.Span.TotalSeconds} - {GreenInterval.Span.TotalSeconds}";
        }
    }

    public class RedToRedCycle : CycleBase
    {
        /// <inheritdoc/>
        [Obsolete]
        public DateTime GreenEvent { get; set; }

        /// <inheritdoc/>
        [Obsolete]
        public DateTime YellowEvent { get; set; }

        //public override TimeSpan TotalGreenTime => YellowEvent - GreenEvent;
        //public override TimeSpan TotalYellowTime => End - YellowEvent;
        //public override TimeSpan TotalRedTime => GreenEvent - Start;
        //public override TimeSpan TotalTime => End - Start;
    }

    public class GreenToGreenCycle : CycleBase
    {
        //public DateTime RedEvent { get; set; }

        //public DateTime YellowEvent { get; set; }

        //public override TimeSpan TotalGreenTime => YellowEvent - Start;
        //public override TimeSpan TotalYellowTime => RedEvent - YellowEvent;
        //public override TimeSpan TotalRedTime => End - RedEvent;
        //public override TimeSpan TotalTime => End - Start;
    }

    public class IntervalSpan : StartEndRange
    {
        public TimeSpan Span => End - Start;

        public override string ToString()
        {
            return $"{GetType().Name}: {Start:HH:mm:ss.f} - {Span.TotalSeconds} - {End:HH:mm:ss.f}";
        }
    }
}
