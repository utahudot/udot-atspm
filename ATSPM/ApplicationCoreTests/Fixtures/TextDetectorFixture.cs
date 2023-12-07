using ATSPM.Data.Models;
using Newtonsoft.Json;
using System;
using System.IO;

namespace ApplicationCoreTests.Fixtures
{
    public class TextDetectorFixture : IDisposable
    {
        public TextDetectorFixture()
        {
            var json = File.ReadAllText(new FileInfo(@"C:\Users\christianbaker\source\repos\udot-atspm\ATSPM\ApplicationCoreTests\Analysis\TestData\Detector63014TestData.json").FullName);
            TestDetector = JsonConvert.DeserializeObject<Detector>(json);
        }

        public Detector TestDetector { get; set; }

        public void Dispose()
        {
        }
    }
}
