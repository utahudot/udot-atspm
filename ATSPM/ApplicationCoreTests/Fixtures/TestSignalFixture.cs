using ATSPM.Data.Models;
using Newtonsoft.Json;
using System;
using System.IO;

namespace ApplicationCoreTests.Fixtures
{
    public class TestSignalFixture : IDisposable
    {
        public TestSignalFixture()
        {
            var json = File.ReadAllText(new FileInfo(@"C:\Users\christianbaker\source\repos\udot-atspm\ATSPM\ApplicationCoreTests\Analysis\TestData\Signal7115TestData.json").FullName);
            TestSignal = JsonConvert.DeserializeObject<Location>(json);
        }

        public Location TestSignal { get; set; }

        public void Dispose()
        {
        }
    }
}
