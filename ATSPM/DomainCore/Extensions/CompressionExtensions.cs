#region license
// Copyright 2024 Utah Departement of Transportation
// for DomainCore - ATSPM.Domain.Extensions/CompressionExtensions.cs
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
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace ATSPM.Domain.Extensions
{
    /// <summary>
    /// Data compression extension helpers
    /// </summary>
    public static class CompressionExtensions
    {
        /// <summary>
        /// GZip string and encode to byte array
        /// </summary>
        /// <param name="str">string to compress and encode</param>
        /// <returns>byte array of compressed string</returns>
        public static byte[] GZipCompressToByte(this string str)
        {
            using (var mso = new MemoryStream())
            {
                using (var cs = new GZipStream(mso, CompressionLevel.SmallestSize))
                using (var msi = new MemoryStream(Encoding.UTF8.GetBytes(str)))
                    msi.CopyTo(cs);

                return mso.ToArray();
            }
        }

        /// <summary>
        /// GZip stream and encode to <see cref="MemoryStream"/>
        /// </summary>
        /// <param name="msi">Stream to compress and convert</param>
        /// <returns><see cref="MemoryStream"/> of compressed stream</returns>
        public static MemoryStream GZipDecompressToStream(this Stream msi)
        {
            using (var cs = new GZipStream(msi, CompressionMode.Decompress))
            using (var mso = new MemoryStream())
            {
                cs.CopyTo(mso);
                return mso;
            }
        }

        /// <summary>
        /// Decompress and decode byte array to <see cref="MemoryStream"/>
        /// </summary>
        /// <param name="bytes">byte array to decompress</param>
        /// <returns><see cref="MemoryStream"/> of decompressed byte array</returns>
        public static MemoryStream GZipDecompressToStream(this byte[] bytes)
        {
            using var msi = new MemoryStream(bytes);
            return msi.GZipDecompressToStream();
        }

        /// <summary>
        /// Decompress GZip stream to byte array
        /// </summary>
        /// <param name="stream">Stream to decompress</param>
        /// <returns>byte array of decompressed stream</returns>
        public static byte[] GZipDecompressToByteArray(this Stream stream)
        {
            return stream.GZipDecompressToStream().ToArray();
        }

        /// <summary>
        /// Decompress GZip byte array to byte array
        /// </summary>
        /// <param name="bytes">byte array to decompress</param>
        /// <returns>byte array of decompressed byte array</returns>
        public static byte[] GZipDecompressToByteArray(this byte[] bytes)
        {
            return bytes.GZipDecompressToStream().ToArray();
        }

        /// <summary>
        /// Decompress GZip stream to string
        /// </summary>
        /// <param name="stream">Stream to decompress</param>
        /// <returns>String of decompressed Stream</returns>
        public static string GZipDecompressToString(this Stream stream)
        {
            return Encoding.UTF8.GetString(stream.GZipDecompressToByteArray());
        }

        /// <summary>
        /// Decompress GZip byte array to string
        /// </summary>
        /// <param name="bytes">byte array to decompress</param>
        /// <returns>string of decompressed byte array</returns>
        public static string GZipDecompressToString(this byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes.GZipDecompressToByteArray());
        }

        /// <summary>
        /// Determines if Stream is compressed
        /// </summary>
        /// <param name="stream">Stream to check</param>
        /// <returns>True if Stream is compressed</returns>
        public static bool IsCompressed(this Stream stream)
        {
            var memoryStream = (MemoryStream)stream;
            return memoryStream.ToArray().IsCompressed();
        }

        /// <summary>
        /// Determines if byte array is compressed
        /// </summary>
        /// <param name="bytes">byte array to check</param>
        /// <returns>True if byte array is compressed</returns>
        public static bool IsCompressed(this byte[] bytes)
        {
            return bytes.GetFileSignatureFromMagicHeader().IsCompressed;
        }
    }
}
