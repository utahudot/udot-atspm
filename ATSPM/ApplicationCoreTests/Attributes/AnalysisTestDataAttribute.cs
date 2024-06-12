#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCoreTests - ApplicationCoreTests.Attributes/AnalysisTestDataAttribute.cs
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
using ApplicationCoreTests.Analysis.TestObjects;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit.Sdk;

namespace ApplicationCoreTests.Attributes
{
    public class AnalysisTestDataAttribute<T> : DataAttribute where T : AnalysisTestDataBase
    {
        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            var dir = new DirectoryInfo(Path.Combine(Path.GetFullPath(@"..\..\..\"), "Analysis", "TestData"));

            if (dir.Exists)
            {
                foreach (var f in dir.GetFiles("*.json").Where(f => f.Name.Contains(typeof(T).Name)))
                {
                    var json = File.ReadAllText(f.FullName);
                    var testFile = JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings()
                    {
                        TypeNameHandling = TypeNameHandling.All,
                    });

                    yield return new object[] { testFile.Configuration, testFile.Input, testFile.Output };
                }
            }
        }
    }
}
