#region license
// Copyright 2026 Utah Departement of Transportation
// for ApplicationTests - Utah.Udot.Atspm.ApplicationTests.Fixtures/TestSignalFixture.cs
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

using Newtonsoft.Json;
using System;
using System.IO;
using Utah.Udot.Atspm.ApplicationTests.Attributes;
using Utah.Udot.Atspm.Data.Models;

namespace Utah.Udot.Atspm.ApplicationTests.Fixtures
{
    /// <summary>
    /// Class fixture providing a pre-configured <see cref="Location"/> (Signal 7115) loaded from JSON test data.
    /// </summary>
    public class TestLocationFixture : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestLocationFixture"/> class.
        /// </summary>
        public TestLocationFixture()
        {
            var testDataDir = AnalysisTestDataAttribute.ResolveTestDataDirectory();
            var filePath = Path.Combine(testDataDir.FullName, "Location7115TestData.json");
            var json = File.ReadAllText(filePath);
            TestLocation = JsonConvert.DeserializeObject<Location>(json);
        }

        /// <summary>
        /// Gets or sets the pre-configured test location.
        /// </summary>
        public Location TestLocation { get; set; }

        /// <inheritdoc/>
        public void Dispose()
        {
        }
    }
}
