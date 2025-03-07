#region license
// Copyright 2025 Utah Departement of Transportation
// for Data - Utah.Udot.Atspm.Data.Utility/CompressedListConverter.cs
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

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;
using System.IO.Compression;
using System.Text;
//using Utah.Udot.NetStandardToolkit.Extensions;

#nullable disable

namespace Utah.Udot.Atspm.Data.Utility
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
            v => JsonConvert.SerializeObject(v, new JsonSerializerSettings()
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

    public static class CompressionExtensions
    {
        public static byte[] GZipCompressToByte(this string str)
        {
            using MemoryStream memoryStream = new MemoryStream();
            using (GZipStream destination = new GZipStream(memoryStream, CompressionLevel.SmallestSize))
            {
                using MemoryStream memoryStream2 = new MemoryStream(Encoding.UTF8.GetBytes(str));
                memoryStream2.CopyTo(destination);
            }

            return memoryStream.ToArray();
        }

        public static MemoryStream GZipDecompressToStream(this Stream msi)
        {
            using GZipStream gZipStream = new GZipStream(msi, CompressionMode.Decompress);
            using MemoryStream memoryStream = new MemoryStream(1024 * 1024 * 5);
            gZipStream.CopyToAsync(memoryStream);
            return memoryStream;
        }

        public static MemoryStream GZipDecompressToStream(this byte[] bytes)
        {
            using MemoryStream msi = new MemoryStream(bytes);
            return msi.GZipDecompressToStream();
        }

        public static byte[] GZipDecompressToByteArray(this Stream stream)
        {
            return stream.GZipDecompressToStream().ToArray();
        }

        public static byte[] GZipDecompressToByteArray(this byte[] bytes)
        {
            return bytes.GZipDecompressToStream().ToArray();
        }

        public static string GZipDecompressToString(this Stream stream)
        {
            return Encoding.UTF8.GetString(stream.GZipDecompressToByteArray());
        }

        public static string GZipDecompressToString(this byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes.GZipDecompressToByteArray());
        }
    }
}
