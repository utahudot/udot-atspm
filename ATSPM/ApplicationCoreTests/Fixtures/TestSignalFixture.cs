using ATSPM.Data.Models;
using Newtonsoft.Json;
using System;
using System.IO;

namespace ApplicationCoreTests.Fixtures
{
    public class TestLocationFixture : IDisposable
    {
        public TestLocationFixture()
        {
            var json = File.ReadAllText(new FileInfo(@"C:\Users\christianbaker\source\repos\udot-atspm\ATSPM\ApplicationCoreTests\Analysis\TestData\Location7115TestData.json").FullName);
            TestLocation = JsonConvert.DeserializeObject<Location>(json);
        }

        public Location TestLocation { get; set; }

        public void Dispose()
        {
        }
    }
}
