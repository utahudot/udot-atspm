#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Analysis.Plans/Plan.cs
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
using ATSPM.Domain.Common;
using ATSPM.Data.Enums;
using System.Text.Json;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Analysis.Plans
{
    /// <summary>
    /// Base for Location controller plans which are derrived from <see cref="131"/> events
    /// </summary>
    public class Plan : StartEndRange, IPlan
    {
        #region IPlan

        /// <inheritdoc/>
        public string LocationIdentifier { get; set; }

        /// <inheritdoc/>
        public int PlanNumber { get; set; }

        /// <inheritdoc/>
        public virtual void AssignToPlan<T>(IEnumerable<T> range) where T : IStartEndRange
        {
            throw new NotImplementedException();
        }

        #endregion

        public override bool InRange(DateTime time)
        {
            return time >= Start && time <= End;
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
