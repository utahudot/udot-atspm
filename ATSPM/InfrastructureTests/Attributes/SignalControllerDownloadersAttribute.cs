using ATSPM.Application.Configuration;
using ATSPM.Infrastructure.Services.ControllerDownloaders;
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
using ATSPM.Application.Services;

namespace InfrastructureTests.Attributes
{
    public class LocationControllerDownloadersAttribute : DataAttribute
    {
        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            var mockConfig = Mock.Of<IOptionsSnapshot<SignalControllerDownloaderConfiguration>>();

            //yield return new object[] { typeof(ASC3SignalControllerDownloader), Mock.Of<IFTPDownloaderClient>(MockBehavior.Strict), new NullLogger<ASC3SignalControllerDownloader>(), mockConfig };
            //yield return new object[] { typeof(CobaltLocationControllerDownloader), Mock.Of<IFTPDownloaderClient>(MockBehavior.Strict), new NullLogger<CobaltLocationControllerDownloader>(), mockConfig };
            //yield return new object[] { typeof(MaxTimeLocationControllerDownloader), Mock.Of<IHTTPDownloaderClient>(MockBehavior.Strict), new NullLogger<MaxTimeLocationControllerDownloader>(), mockConfig };
            //yield return new object[] { typeof(EOSSignalControllerDownloader), Mock.Of<IFTPDownloaderClient>(MockBehavior.Strict), new NullLogger<EOSSignalControllerDownloader>(), mockConfig };
            //yield return new object[] { typeof(NewCobaltLocationControllerDownloader), Mock.Of<ISFTPDownloaderClient>(MockBehavior.Strict), new NullLogger<NewCobaltLocationControllerDownloader>(), mockConfig };

            yield return new object[] { typeof(DeviceDownloaderBase), Mock.Of<ISFTPDownloaderClient>(MockBehavior.Strict), new NullLogger<DeviceDownloaderBase>(), mockConfig };
        }
    }
}
