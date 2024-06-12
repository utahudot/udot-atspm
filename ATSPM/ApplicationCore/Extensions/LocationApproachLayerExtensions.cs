#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Extensions/LocationApproachLayerExtensions.cs
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
using ATSPM.Domain.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Extensions
{
    /// <summary>
    /// Extensions for object that inherit <see cref="ILocationApproachLayer"/>
    /// </summary>
    public static class LocationApproachLayerExtensions
    {
        /// <summary>
        /// Gets all items that have the provided <paramref name="approachId"/>
        /// </summary>
        /// <typeparam name="T"><see cref="ILocationApproachLayer"/></typeparam>
        /// <param name="items"><see cref="IEnumerable{T}"/> of objects that inherit <see cref="ILocationApproachLayer"/></param>
        /// <param name="approachId">Id of approach to match with <paramref name="items"/></param>
        /// <returns></returns>
        public static IReadOnlyList<T> GetByApproachId<T>(this IEnumerable<T> items, int approachId) where T : ILocationApproachLayer
        {
            return items.Where(w => w.ApproachId == approachId).ToList();
        }

        /// <summary>
        /// Gets all items that have the provided <paramref name="approachId"/>
        /// </summary>
        /// <typeparam name="T"><see cref="ILocationApproachLayer"/></typeparam>
        /// <param name="items"><see cref="IEnumerable{T}"/> of objects that inherit <see cref="ILocationApproachLayer"/></param>
        /// <param name="approachId">Id of approach to match with <paramref name="items"/></param>
        /// <param name="protectedPhase">Flag to check protected phase status</param>
        /// <returns></returns>
        public static IReadOnlyList<T> GetByApproachId<T>(this IEnumerable<T> items, int approachId, bool protectedPhase) where T : ILocationApproachLayer
        {
            return items.Where(w => w.ApproachId == approachId && w.HasProperty("IsProtectedPhase") ? w.GetPropertyValue<bool>("IsProtectedPhase") == protectedPhase : true).ToList();
        }
    }
}
