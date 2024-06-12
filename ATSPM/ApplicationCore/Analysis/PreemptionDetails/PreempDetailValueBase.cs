#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Analysis.PreemptionDetails/PreempDetailValueBase.cs
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
using ATSPM.Data.Interfaces;
using ATSPM.Domain.Common;
using System;
using System.Text.Json;

namespace ATSPM.Application.Analysis.PreemptionDetails
{
    public abstract class PreempDetailValueBase : StartEndRange, ILocationLayer
    {
        #region ILocationLayer

        /// <inheritdoc/>
        public string LocationIdentifier { get; set; }

        #endregion

        public int PreemptNumber { get; set; }
        public TimeSpan Seconds { get; set; }

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
