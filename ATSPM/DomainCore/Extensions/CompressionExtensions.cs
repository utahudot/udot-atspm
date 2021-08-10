using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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

        public static string GZipDecompressToString(this byte[] bytes)
        {
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    gs.CopyTo(mso);
                }

                return Encoding.UTF8.GetString(mso.ToArray());
            }
        }
    }
}
