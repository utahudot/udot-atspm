using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace ATSPM.Domain.Extensions
{
    public static class CompressionExtensions
    {
        public static byte[] GZipCompressToByte(this string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);

            using (var stream = new MemoryStream())
            {
                using (var compressionStream = new GZipStream(stream, CompressionMode.Compress))
                {
                    compressionStream.Write(bytes, 0, bytes.Length);
                }
                return stream.ToArray();
            }
        }

        public static MemoryStream GZipDecompressToStream(this Stream stream)
        {
            using var gs = new GZipStream(stream, CompressionMode.Decompress);
            var mso = new MemoryStream();
            gs.CopyTo(mso);
            mso.Position = 0;
            return mso;
        }

        public static MemoryStream GZipDecompressToStream(this byte[] bytes)
        {
            using (var msi = new MemoryStream(bytes))
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

        public static bool IsCompressed(this Stream stream)
        {
            var memoryStream = (MemoryStream)stream;
            return memoryStream.ToArray().IsCompressed();
        }

        public static bool IsCompressed(this byte[] bytes)
        {
            return bytes.GetFileSignatureFromMagicHeader().IsCompressed;
        }
    }
}
