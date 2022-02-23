using ATSPM.Application.Configuration;
using ATSPM.Application.Services.SignalControllerProtocols;
using ATSPM.Infrasturcture.Services.ControllerDownloaders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Xunit.Sdk;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging.Abstractions;
using Castle.Core.Logging;
using Xunit.Abstractions;
using Microsoft.Extensions.Logging;

namespace SignalControllerLoggerTests.Attributes
{
    public class SignalControllerDownloadersAttribute : DataAttribute
    {
        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            var mockConfig = Mock.Of<IOptionsSnapshot<SignalControllerDownloaderConfiguration>>();

            yield return new object[] { typeof(FTPSignalControllerDownloader), Mock.Of<IFTPDownloaderClient>(MockBehavior.Strict), new NullLogger<FTPSignalControllerDownloader>(), mockConfig };
        }
    }
}
