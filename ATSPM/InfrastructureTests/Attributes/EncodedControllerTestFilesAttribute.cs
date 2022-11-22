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
