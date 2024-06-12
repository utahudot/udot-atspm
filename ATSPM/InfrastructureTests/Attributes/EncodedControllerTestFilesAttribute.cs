#region license
// Copyright 2024 Utah Departement of Transportation
// for InfrastructureTests - InfrastructureTests.Attributes/EncodedControllerTestFilesAttribute.cs
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
using System.Linq;
using System.Reflection;
using System.Text;
using Xunit.Sdk;

namespace InfrastructureTests.Attributes
{
    public class EncodedControllerTestFilesAttribute : DataAttribute
    {
        private const string TestDataPath = "C:\\Projects\\udot-atsmp\\ATSPM\\InfrastructureTexts\\TestData";

        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            yield return new object[] { new FileInfo(Path.Combine(TestDataPath, "1053(dat)\\ECON_10.204.12.167_2021_08_09_1831.dat")), false, true };
            yield return new object[] { new FileInfo(Path.Combine(TestDataPath, "1210(datz)\\ECON_10.204.7.239_2021_08_09_1841.datZ")), true, true };
            yield return new object[] { new FileInfo(Path.Combine(TestDataPath, "XML\\0_637709235474596368.xml")), false, false };
        }
    }
}
