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

            yield return new object[] { typeof(ASC3SignalControllerDownloader), Mock.Of<IFTPDownloaderClient>(MockBehavior.Strict), new NullLogger<ASC3SignalControllerDownloader>(), mockConfig };
            yield return new object[] { typeof(CobaltSignalControllerDownloader), Mock.Of<IFTPDownloaderClient>(MockBehavior.Strict), new NullLogger<CobaltSignalControllerDownloader>(), mockConfig };
            yield return new object[] { typeof(MaxTimeSignalControllerDownloader), Mock.Of<IHTTPDownloaderClient>(MockBehavior.Strict), new NullLogger<MaxTimeSignalControllerDownloader>(), mockConfig };
            yield return new object[] { typeof(EOSSignalControllerDownloader), Mock.Of<IFTPDownloaderClient>(MockBehavior.Strict), new NullLogger<EOSSignalControllerDownloader>(), mockConfig };
            yield return new object[] { typeof(NewCobaltSignalControllerDownloader), Mock.Of<ISFTPDownloaderClient>(MockBehavior.Strict), new NullLogger<NewCobaltSignalControllerDownloader>(), mockConfig };
        }
    }
}
