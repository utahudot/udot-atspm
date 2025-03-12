#region license
// Copyright 2025 Utah Departement of Transportation
// for Data - Utah.Udot.Atspm.Data.Utility/DictionaryValueComparer.cs
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

using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Utah.Udot.Atspm.Data.Utility
{
    /// <summary>
    /// Value comparer for change tracking dictionaries
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    internal class DictionaryValueComparer<TKey, TValue> : ValueComparer<Dictionary<TKey, TValue>>
    {
        public DictionaryValueComparer() : base(
            (a, b) => Compare(a, b),
            o => GetHashCode(o),
            o => GetSnapshot(o))
        { }

        private static bool Compare(Dictionary<TKey, TValue> a, Dictionary<TKey, TValue> b)
        {
            var e1 = Enumerable.SequenceEqual(a.Keys.ToList(), b.Keys.ToList());
            var e2 = Enumerable.SequenceEqual(a.Values.ToList(), b.Values.ToList());

            return e1 && e2;
        }

        private static int GetHashCode(Dictionary<TKey, TValue> obj)
        {
            var hash = new HashCode();

            foreach (var h in obj)
            {
                hash.Add(h.GetHashCode());
            }

            return hash.ToHashCode();
        }

        private static Dictionary<TKey, TValue> GetSnapshot(Dictionary<TKey, TValue> obj)
        {
            return obj.ToDictionary();
        }
    }
}
