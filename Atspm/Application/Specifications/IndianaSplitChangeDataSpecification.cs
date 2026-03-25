#region license
// Copyright 2026 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Specifications/IndianaPhaseCycleChangesDataSpecification.cs
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

using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.NetStandardToolkit.Specifications;

namespace Utah.Udot.Atspm.Specifications
{
    /// <summary>
    /// Filters <see cref="IEnumerable{IndianaEvent}"/> by:
    /// <list type="bullet">
    /// <item><see cref="IndianaEnumerations.Split1Change"/></item>
    /// <item><see cref="IndianaEnumerations.Split2Change"/></item>
    /// <item><see cref="IndianaEnumerations.Split3Change"/></item>
    /// <item><see cref="IndianaEnumerations.Split4Change"/></item>
    /// <item><see cref="IndianaEnumerations.Split5Change"/></item>
    /// <item><see cref="IndianaEnumerations.Split6Change"/></item>
    /// <item><see cref="IndianaEnumerations.Split7Change"/></item>
    /// <item><see cref="IndianaEnumerations.Split8Change"/></item>
    /// <item><see cref="IndianaEnumerations.Split9Change"/></item>
    /// <item><see cref="IndianaEnumerations.Split10Change"/></item>
    /// <item><see cref="IndianaEnumerations.Split11Change"/></item>
    /// <item><see cref="IndianaEnumerations.Split12Change"/></item>
    /// <item><see cref="IndianaEnumerations.Split13Change"/></item>
    /// <item><see cref="IndianaEnumerations.Split14Change"/></item>
    /// <item><see cref="IndianaEnumerations.Split15Change"/></item>
    /// <item><see cref="IndianaEnumerations.Split16Change"/></item>
    /// </list>
    /// </summary>
    public class IndianaSplitChangeDataSpecification : BaseSpecification<IndianaEvent>
    {
        /// <inheritdoc cref="IndianaPhaseIntervalChangesDataSpecification"/>
        public IndianaSplitChangeDataSpecification()
        {
            var codes = new HashSet<short>()
            {
                (short)IndianaEnumerations.Split1Change,
                (short)IndianaEnumerations.Split2Change,
                (short)IndianaEnumerations.Split3Change,
                (short)IndianaEnumerations.Split4Change,
                (short)IndianaEnumerations.Split5Change,
                (short)IndianaEnumerations.Split6Change,
                (short)IndianaEnumerations.Split7Change,
                (short)IndianaEnumerations.Split8Change,
                (short)IndianaEnumerations.Split9Change,
                (short)IndianaEnumerations.Split10Change,
                (short)IndianaEnumerations.Split11Change,
                (short)IndianaEnumerations.Split12Change,
                (short)IndianaEnumerations.Split13Change,
                (short)IndianaEnumerations.Split14Change,
                (short)IndianaEnumerations.Split15Change,
                (short)IndianaEnumerations.Split16Change,
            };

            Criteria = c => codes.Contains(c.EventCode);

            ApplyOrderBy(o => o.Timestamp);
        }
    }
}
