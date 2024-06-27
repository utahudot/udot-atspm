#region license
// Copyright 2024 Utah Departement of Transportation
// for InfrastructureTests - InfrastructureTests.Attributes/SignalControllerDownloadersAttribute.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion
using ATSPM.Application.Configuration;
using ATSPM.Application.Services;
using ATSPM.Infrastructure.Services.ControllerDownloaders;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using System.Collections.Generic;
using System.Reflection;
using Xunit.Sdk;

namespace InfrastructureTests.Attributes
{
    public class DeviceDownloaderAttribute : DataAttribute
    {
        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            var mockConfig = Mock.Of<IOptionsSnapshot<SignalControllerDownloaderConfiguration>>();

            yield return new object[] { typeof(DeviceFtpDownloader), Mock.Of<IFTPDownloaderClient>(MockBehavior.Strict), new NullLogger<DeviceFtpDownloader>(), mockConfig };
            yield return new object[] { typeof(DeviceHttpDownloader), Mock.Of<IHTTPDownloaderClient>(MockBehavior.Strict), new NullLogger<DeviceHttpDownloader>(), mockConfig };
            yield return new object[] { typeof(DeviceSftpDownloader), Mock.Of<ISFTPDownloaderClient>(MockBehavior.Strict), new NullLogger<DeviceSftpDownloader>(), mockConfig };
        }
    }
}
