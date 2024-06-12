#region license
// Copyright 2024 Utah Departement of Transportation
// for Data - ATSPM.Data.Utility/EntityFrameworkExtensions.cs
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
using Microsoft.EntityFrameworkCore.Metadata.Builders;

#nullable disable

namespace ATSPM.Data.Utility
{
    /// <summary>
    /// Entity framework extensions specific to Atspm
    /// </summary>
    internal static class EntityFrameworkExtensions
    {
        /// <summary>
        /// Used with compressed data hierarchy tables to get discrimators from and generic type
        /// </summary>
        /// <typeparam name="TType">Data type of the property to use as the discriminator</typeparam>
        /// <param name="builder"></param>
        /// <param name="model">Data model name to use in the discriminator property</param>
        /// <param name="generic">Generic of type <paramref name="model"/> that acts as the model for the db table</param>
        internal static void AddCompressedTableDiscriminators<TType>(this DiscriminatorBuilder<TType> builder, Type model, Type generic) where TType : Type
        {
            foreach (var t in model.Assembly.GetTypes().Where(w => w.IsSubclassOf(model)))
            {
                if (generic.IsGenericType)
                {
                    var g = generic.MakeGenericType(t);

                    builder.HasValue(g, (TType)t);
                }
            }
        }
    }
}
