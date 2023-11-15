using ATSPM.Data.Models;
using Google.Api;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
