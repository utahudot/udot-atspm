#region license
// Copyright 2024 Utah Departement of Transportation
// for Data - ATSPM.Data.Utility/CompressedListComparer.cs
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
using ATSPM.Data.Models.AggregationModels;
using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Collections;

#nullable disable

namespace ATSPM.Data.Utility
{
    /// <summary>
    /// <see cref="ValueComparer"/> used to compare an <see cref="IEnumerable"/> of <typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class CompressedListComparer<T> : ValueComparer<IEnumerable<T>>
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public CompressedListComparer() : base(
            (c1, c2) => c1.SequenceEqual(c2),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList())
        { }
    }
}
