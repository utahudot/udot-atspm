using System;
using Xunit;
using Xunit.Extensions;
using ATSPM.Domain.Extensions;
using System.Linq;
using System.Text;
using System.IO;

namespace DomainCoreTests
{
    public class CompressionExtensionsTests
    {
        public const string TestString = "";

        [Fact]
        public void GZipCompressToByteArray()
        {
            var bytes = TestString.GZipCompressToByte();
            var gzipheader = FileSignatureExtensions.FileSignatures.FirstOrDefault(f => f.Extension == ".gz").MagicHeader;

            Assert.Equal(bytes.Take(gzipheader.Length), gzipheader);
        }

        [Fact]
        public void IsCompressedFromByteValid()
        {
            var bytes = TestString.GZipCompressToByte();

            Assert.True(bytes.IsCompressed());
        }

        [Fact]
        public void IsCompressedFromByteNotValid()
        {
            byte[] bytes = new byte[] { };

            Assert.False(bytes.IsCompressed());
        }

        [Fact]
        public void IsCompressedFromStreamValid()
        {
            var bytes = TestString.GZipCompressToByte();

            var memoryStream = new MemoryStream(bytes);

            Assert.True(memoryStream.IsCompressed());
        }

        [Fact]
        public void GZipDecompressToStreamValidStream()
        {
            var originalBytes = Encoding.UTF8.GetBytes(TestString);

            var compressedBytes = TestString.GZipCompressToByte();

            var memoryStream = new MemoryStream(compressedBytes);

            var result = memoryStream.GZipDecompressToStream().ToArray();

            Assert.True(originalBytes.SequenceEqual(result)); ;
        }

        [Fact]
        public void GZipDecompressToStreamValidByte()
        {
            var originalBytes = Encoding.UTF8.GetBytes(TestString);

            var compressedBytes = TestString.GZipCompressToByte();

            var result = compressedBytes.GZipDecompressToStream().ToArray();

            Assert.True(originalBytes.SequenceEqual(result)); ;
        }

        [Fact]
        public void GZipDecompressToByteValidStream()
        {
            var originalBytes = Encoding.UTF8.GetBytes(TestString);

            var compressedBytes = TestString.GZipCompressToByte();

            var memoryStream = new MemoryStream(compressedBytes);

            var result = memoryStream.GZipDecompressToByteArray();

            Assert.True(originalBytes.SequenceEqual(result)); ;
        }

        [Fact]
        public void GZipDecompressToByteValidByte()
        {
            var originalBytes = Encoding.UTF8.GetBytes(TestString);

            var compressedBytes = TestString.GZipCompressToByte();

            var result = compressedBytes.GZipDecompressToByteArray();

            Assert.True(originalBytes.SequenceEqual(result)); ;
        }

        [Fact]
        public void GZipDecompressToStringValidStream()
        {
            var originalBytes = Encoding.UTF8.GetBytes(TestString);

            var compressedBytes = TestString.GZipCompressToByte();

            var memoryStream = new MemoryStream(compressedBytes);

            var result = Encoding.UTF8.GetBytes(memoryStream.GZipDecompressToString());

            Assert.True(originalBytes.SequenceEqual(result)); ;
        }

        [Fact]
        public void GZipDecompressToStringValidByte()
        {
            var originalBytes = Encoding.UTF8.GetBytes(TestString);

            var compressedBytes = TestString.GZipCompressToByte();

            var result = Encoding.UTF8.GetBytes(compressedBytes.GZipDecompressToString());

            Assert.True(originalBytes.SequenceEqual(result)); ;
        }
    }
}
