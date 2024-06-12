#region license
// Copyright 2024 Utah Departement of Transportation
// for DomainCoreTests - DomainCoreTests/StreamExtensionTests.cs
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
