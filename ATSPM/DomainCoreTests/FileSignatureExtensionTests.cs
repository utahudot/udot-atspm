#region license
// Copyright 2024 Utah Departement of Transportation
// for DomainCoreTests - DomainCoreTests/FileSignatureExtensionTests.cs
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
