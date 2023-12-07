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
