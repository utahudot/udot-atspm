using System;
using Xunit;
using Xunit.Extensions;
using ATSPM.Domain.Extensions;
using System.Linq;
using System.Text;
using System.IO;

namespace DomainCoreTests
{
    public class FileSignatureExtensionTests
    {
        [Fact]
        public void GetFileSignatureFromExtensionValidFileInfo()
        {
            FileInfo fileInfo = new FileInfo("C:\\Projects\\udot-atsmp\\ATSPM\\DomainCoreTests\\TestData\\1053(dat)\\ECON_10.204.12.167_2021_08_09_1831.dat");

            var test = fileInfo.GetFileSignatureFromExtension();

            Assert.All(test, i => Assert.Equal(fileInfo.Extension, i.Extension));
        }

        [Fact]
        public void GetFileSignatureFromExtensionValidString()
        {
            string extension = ".txt";

            var test = extension.GetFileSignatureFromExtension();

            Assert.All(test, i => Assert.Equal(extension, i.Extension));
        }

        [Fact]
        public void GetFileSignatureFromMagicHeaderValidBytes()
        {
            byte[] magicHeader = new byte[] { 0xCE, 0xFA, 0xED, 0xFE };

            var test = magicHeader.GetFileSignatureFromMagicHeader();

            Assert.Equal(magicHeader, test.MagicHeader);
        }

        [Fact]
        public void GetFileSignatureFromMagicHeaderNotValidBytes()
        {
            byte[] magicHeader = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF };

            var test = magicHeader.GetFileSignatureFromMagicHeader();

            Assert.NotEqual(magicHeader, test.MagicHeader);
        }
    }
}
