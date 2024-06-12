#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCoreTests - ApplicationCoreTests.Fixtures/TestApproachFixture.cs
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
using ATSPM.Data.Models;
using Newtonsoft.Json;
using System;
using System.IO;

namespace ApplicationCoreTests.Fixtures
{
    public class TestApproachFixture : IDisposable
    {
        public TestApproachFixture()
        {
            var json = File.ReadAllText(new FileInfo(@"C:\Users\christianbaker\source\repos\udot-atspm\ATSPM\ApplicationCoreTests\Analysis\TestData\Approach2880TestData.json").FullName);
            TestApproach = JsonConvert.DeserializeObject<Approach>(json);
        }

        public Approach TestApproach { get; set; }

        public void Dispose()
        {
        }
    }
}
