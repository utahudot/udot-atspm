using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

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
            var bytes = Encoding.UTF8.GetBytes(str);

            using var stream = new MemoryStream();
            using var compressionStream = new GZipStream(stream, CompressionMode.Compress);
            compressionStream.Write(bytes, 0, bytes.Length);

            return stream.ToArray();
        }

        /// <summary>
        /// GZip stream and encode to <see cref="MemoryStream"/>
        /// </summary>
        /// <param name="stream">Stream to compress and convert</param>
        /// <returns><see cref="MemoryStream"/> of compressed stream</returns>
        public static MemoryStream GZipDecompressToStream(this Stream stream)
        {
            using var gs = new GZipStream(stream, CompressionMode.Decompress);
            var mso = new MemoryStream();
            gs.CopyTo(mso);
            mso.Position = 0;
            return mso;
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
