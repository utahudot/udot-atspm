using System;
using Xunit;
using Xunit.Extensions;
using ATSPM.Domain.Extensions;
using System.Linq;
using System.IO;

namespace DomainCoreTests
{
    public class StreamExtensionTests
    {
        [Fact]
        public void FileInfoToMemoryStream()
        {
            FileInfo fileInfo = new FileInfo("C:\\Projects\\udot-atsmp\\ATSPM\\DomainCoreTests\\TestData\\1053(dat)\\ECON_10.204.12.167_2021_08_09_1831.dat");

            MemoryStream memoryStream = fileInfo.ToMemoryStream();

            Assert.True(memoryStream.Length > 0);
        }

        [Fact]
        public void FileInfoToMemoryStreamException()
        {
            FileInfo fileInfo = new FileInfo("C:\\Projects\\udot-atsmp\\ATSPM\\DomainCoreTests\\TestData\\1053(dat)\\ECON_10.204.12.167_2021_08_09_18311.dat");

            Assert.Throws<FileNotFoundException>(() => fileInfo.ToMemoryStream());
        }
    }
}
