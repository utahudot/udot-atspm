#region license
// Copyright 2024 Utah Departement of Transportation
// for Data - ATSPM.Data.Utility/CompressedListConverter.cs
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
using ATSPM.Domain.Extensions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;

#nullable disable

namespace ATSPM.Data.Utility
{
    /// <summary>
    /// <see cref="ValueConverter"/> used to convert compressed list of <typeparamref name="T"/>
    /// to/from gzip compressed json
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class CompressedListConverter<T> : ValueConverter<IEnumerable<T>, byte[]>
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public CompressedListConverter() : base(
            v => Newtonsoft.Json.JsonConvert.SerializeObject(v, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Arrays,
                SerializationBinder = new CompressedSerializationBinder<T>()
            }).GZipCompressToByte(),
            v => JsonConvert.DeserializeObject<IEnumerable<T>>(v.GZipDecompressToString(), new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Arrays,
                SerializationBinder = new CompressedSerializationBinder<T>()
            }))
        { }
    }
}
