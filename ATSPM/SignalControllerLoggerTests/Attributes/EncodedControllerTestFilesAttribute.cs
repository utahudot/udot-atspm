using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Xunit.Sdk;

namespace SignalControllerLoggerTests.Attributes
{
    public class EncodedControllerTestFilesAttribute : DataAttribute
    {
        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            yield return new object[] { new FileInfo("C:\\Projects\\udot-atsmp\\ATSPM\\SignalControllerLoggerTests\\TestData\\1053(dat)\\ECON_10.204.12.167_2021_08_09_1831.dat"), "1053(dat)" };
            yield return new object[] { new FileInfo("C:\\Projects\\udot-atsmp\\ATSPM\\SignalControllerLoggerTests\\TestData\\1053(dat)\\ECON_10.204.12.167_2021_08_09_1841.dat"), "1053(dat)" };
            yield return new object[] { new FileInfo("C:\\Projects\\udot-atsmp\\ATSPM\\SignalControllerLoggerTests\\TestData\\1210(datz)\\ECON_10.204.7.239_2021_08_09_1841.datZ"), "1210(datz)" };
            yield return new object[] { new FileInfo("C:\\Projects\\udot-atsmp\\ATSPM\\SignalControllerLoggerTests\\TestData\\1210(datz)\\ECON_10.204.7.239_2021_08_09_1911.datZ"), "1210(datz)" };
        }
    }
}
