#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Extensions/IAggregationRepositoryExtensions.cs
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

using System.Collections;
using Utah.Udot.Atspm.Repositories.AggregationRepositories;

namespace Utah.Udot.Atspm.Extensions
{
    public static class IAggregationRepositoryExtensions
    {
        public static async Task<T> Upsert<T>(this IAggregationRepository repo, T input) where T : CompressedAggregationBase
        {
            var search = await repo.LookupAsync(input);

            if (search != null)
            {
                dynamic list = Activator.CreateInstance(typeof(List<>).MakeGenericType(input.DataType));

                foreach (var i in input.Data)
                {
                    if (list is IList l)
                    {
                        l.Add(i);
                    }
                }

                search.Data = list;

                await repo.UpdateAsync(search);

                return (T)search;
            }
            else
            {
                await repo.AddAsync(input);

                return input;
            }
        }
    }
}
