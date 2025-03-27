#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Common.EqualityComparers/ConfigEntityIdComparer.cs
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

using System.Diagnostics.CodeAnalysis;
using Utah.Udot.Atspm.Data.Models.ConfigurationModels;

namespace Utah.Udot.Atspm.Common.EqualityComparers
{
    /// <summary>
    /// Compares the <typeparamref name="Tid"/> of models derived from <see cref="AtspmConfigModelBase{T}"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="Tid"></typeparam>
    public class ConfigEntityIdComparer<T, Tid> : EqualityComparer<T> where T : AtspmConfigModelBase<Tid>
    {
        /// <inheritdoc/>
        public override bool Equals(T x, T y)
        {
            return x.Id.Equals(y.Id);
        }

        /// <inheritdoc/>
        public override int GetHashCode([DisallowNull] T obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}
