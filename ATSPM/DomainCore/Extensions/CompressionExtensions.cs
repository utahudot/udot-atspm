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
        // reference https://en.wikipedia.org/wiki/List_of_file_signatures
        // Found the two bytes for EOS by reading the file.
        public static byte[] ZlibHeaderNoCompression = { 0x78, 0x01 };
        public static byte[] ZlibHeaderDefaultCompression = { 0x78, 0x9C };
        public static byte[] ZlibHeaderBestCompression = { 0x78, 0xDA };
        public static byte[] GZipHeader = { 0x1f, 0x8b };
        public static byte[] EOSHeader = { 0x18, 0x95 };

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
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(stream, CompressionMode.Decompress))
                {
                    gs.CopyTo(mso);
                }

                return mso;
            }
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
            if (bytes.Length >= 2)
            {
                var magicHeader = bytes?.Take(2);

                return magicHeader.SequenceEqual(ZlibHeaderNoCompression)
                    || magicHeader.SequenceEqual(ZlibHeaderDefaultCompression)
                    || magicHeader.SequenceEqual(ZlibHeaderBestCompression)
                    || magicHeader.SequenceEqual(GZipHeader)
                    || magicHeader.SequenceEqual(EOSHeader);
            }

            return false;
        }
    }
}
