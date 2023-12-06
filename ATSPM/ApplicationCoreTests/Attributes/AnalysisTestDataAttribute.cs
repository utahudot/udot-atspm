using ApplicationCoreTests.Analysis.TestObjects;
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
