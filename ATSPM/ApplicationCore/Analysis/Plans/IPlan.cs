#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Analysis.Plans/IPlan.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Analysis.Plans
{
    /// <summary>
    /// Definition for Location controller plans which are derrived from <see cref="131"/> events
    /// </summary>
    public interface IPlan : IStartEndRange, ILocationLayer, IPlanLayer
    {
        /// <summary>
        /// Tries to assign an <see cref="IStartEndRange"/> object to the plan
        /// </summary>
        /// <param name="range"></param>
        /// <returns>Returns true if successful</returns>
        void AssignToPlan<T>(IEnumerable<T> range) where T : IStartEndRange;
    }
}
