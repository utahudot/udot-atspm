#region license
// Copyright 2024 Utah Departement of Transportation
// for InfrastructureTests - %Namespace%/ISignalControllerDownloaderTests.cs
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
using ATSPM.Application.Exceptions;
using ATSPM.Application.Services;
using ATSPM.Data.Models;
using ATSPM.Domain.Exceptions;
using InfrastructureTests.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace InfrastructureTests.DeviceDownloaderTests
{
    public class IDeviceDownloaderTests : IDisposable
    {
        private readonly ITestOutputHelper _output;

        public IDeviceDownloaderTests(ITestOutputHelper output)
        {
            _output = output;
        }

        #region IDeviceDownloader

        [Theory]
        [DeviceDownloader]
        public void CanExecuteValid(Type downloader, IDownloaderClient mockClient, ILogger log, IOptionsSnapshot<SignalControllerDownloaderConfiguration> mockConfig)
        {
            var sut = (IDeviceDownloader)Activator.CreateInstance(downloader, new object[] { mockClient, log, mockConfig });

            var device = new Device()
            {
                LoggingEnabled = true,
                DeviceConfiguration = new DeviceConfiguration() { Protocol = sut.Protocol }
            };

            Mock.Get(mockClient).Setup(s => s.Dispose());

            var condition = sut.CanExecute(device);

            Assert.True(condition);
        }

        [Theory]
        [DeviceDownloader]
        public void CanExecuteLoggingEnabledNotValid(Type downloader, IDownloaderClient mockClient, ILogger log, IOptionsSnapshot<SignalControllerDownloaderConfiguration> mockConfig)
        {
            var sut = (IDeviceDownloader)Activator.CreateInstance(downloader, new object[] { mockClient, log, mockConfig });

            var device = new Device()
            {
                LoggingEnabled = false,
                DeviceConfiguration = new DeviceConfiguration() { Protocol = sut.Protocol }
            };

            Mock.Get(mockClient).Setup(s => s.Dispose());

            var condition = sut.CanExecute(device);

            Assert.False(condition);
        }

        [Theory]
        [DeviceDownloader]
        public void CanExecuteProtocolNotValid(Type downloader, IDownloaderClient mockClient, ILogger log, IOptionsSnapshot<SignalControllerDownloaderConfiguration> mockConfig)
        {
            Mock.Get(mockClient).Setup(s => s.Dispose());

            var sut = (IDeviceDownloader)Activator.CreateInstance(downloader, new object[] { mockClient, log, mockConfig });

            var device = new Device()
            {
                LoggingEnabled = true,
                DeviceConfiguration = new DeviceConfiguration() { Protocol = ATSPM.Data.Enums.TransportProtocols.Unknown }
            };

            var condition = sut.CanExecute(device);

            Assert.False(condition);
        }

        [Theory]
        [DeviceDownloader]
        public async void ExecuteWithNullParameter(Type downloader, IDownloaderClient mockClient, ILogger log, IOptionsSnapshot<SignalControllerDownloaderConfiguration> mockConfig)
        {
            Mock.Get(mockClient).Setup(s => s.Dispose());

            var sut = (IDeviceDownloader)Activator.CreateInstance(downloader, new object[] { mockClient, log, mockConfig });

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await foreach (var file in sut.Execute(null)) { }
            });
        }

        [Theory]
        [DeviceDownloader]
        public async void ExecuteWithInvalidIPAddress(Type downloader, IDownloaderClient mockClient, ILogger log, IOptionsSnapshot<SignalControllerDownloaderConfiguration> mockConfig)
        {
            Mock.Get(mockClient).Setup(s => s.Dispose());

            Mock.Get(mockConfig).Setup(s => s.Value).Returns(new SignalControllerDownloaderConfiguration() { PingControllerToVerify = false });

            var sut = (IDeviceDownloader)Activator.CreateInstance(downloader, new object[] { mockClient, log, mockConfig });

            var device = new Device()
            {
                LoggingEnabled = true,
                DeviceConfiguration = new DeviceConfiguration() { Protocol = sut.Protocol }
            };

            await Assert.ThrowsAsync<InvalidDeviceIpAddressException>(async () =>
            {
                await foreach (var file in sut.Execute(device)) { }
            });
        }

        [Theory]
        [DeviceDownloader]
        public async void ExecuteWithFailedCanExecute(Type downloader, IDownloaderClient mockClient, ILogger log, IOptionsSnapshot<SignalControllerDownloaderConfiguration> mockConfig)
        {
            Mock.Get(mockClient).Setup(s => s.Dispose());

            var sut = (IDeviceDownloader)Activator.CreateInstance(downloader, new object[] { mockClient, log, mockConfig });

            var device = new Device()
            {
                LoggingEnabled = true,
                DeviceConfiguration = new DeviceConfiguration() { Protocol = ATSPM.Data.Enums.TransportProtocols.Unknown }
            };

            await Assert.ThrowsAsync<ExecuteException>(async () =>
            {
                await foreach (var file in sut.Execute(device)) { }
            });
        }

        [Theory]
        [DeviceDownloader]
        public async void ExecuteWithControllerConnectionException(Type downloader, IDownloaderClient mockClient, ILogger log, IOptionsSnapshot<SignalControllerDownloaderConfiguration> mockConfig)
        {
            Mock.Get(mockConfig).Setup(s => s.Value).Returns(new SignalControllerDownloaderConfiguration() { PingControllerToVerify = false });

            Mock.Get(mockClient).Setup(s => s.ConnectAsync(It.IsAny<IPEndPoint>(), It.IsAny<NetworkCredential>(), 0, 0, default))
                .ThrowsAsync(new ControllerConnectionException(It.IsAny<string>(), mockClient, null))
                .Verifiable();

            Mock.Get(mockClient).SetupGet(s => s.IsConnected).Returns(false).Verifiable();

            Mock.Get(mockClient).Setup(s => s.DisconnectAsync(default)).Returns(Task.CompletedTask).Verifiable();
            Mock.Get(mockClient).Setup(s => s.Dispose()).Verifiable();

            var sut = (IDeviceDownloader)Activator.CreateInstance(downloader, new object[] { mockClient, log, mockConfig });

            var device = new Device()
            {
                Ipaddress = "192.168.1.1",
                LoggingEnabled = true,
                DeviceConfiguration = new DeviceConfiguration() { Protocol = sut.Protocol }
            };

            await foreach (var file in sut.Execute(device)) { }

            Mock.Verify();
        }

        //[Theory]
        //[DeviceDownloader]
        //public async void ExecuteWithControllerConnectionTokenCancelled(Type downloader, IDownloaderClient mockClient, ILogger log, IOptionsSnapshot<SignalControllerDownloaderConfiguration> mockConfig)
        //{

        //}

        //[Theory]
        //[DeviceDownloader]
        //public async void ExecuteWithControllerDownloadFileException(Type downloader, IDownloaderClient mockClient, ILogger log, IOptionsSnapshot<SignalControllerDownloaderConfiguration> mockConfig)
        //{

        //}

        //[Theory]
        //[DeviceDownloader]
        //public async void ExecuteWithControllerDownloadFileTokenCancelled(Type downloader, IDownloaderClient mockClient, ILogger log, IOptionsSnapshot<SignalControllerDownloaderConfiguration> mockConfig)
        //{

        //}

        [Theory]
        [DeviceDownloader]
        public void ExecuteWithControllerDownloadFileSuccess(Type downloader, IDownloaderClient mockClient, ILogger log, IOptionsSnapshot<SignalControllerDownloaderConfiguration> mockConfig)
        {
            Assert.False(true);

            //Mock.Get(mockConfig).Setup(s => s.Value).Returns(new SignalControllerDownloaderConfiguration()
            //{
            //    LocalPath = "C:\\TestPath",
            //    PingControllerToVerify = false
            //});

            //var verifyPath = Path.Combine(mockConfig.Value.LocalPath, downloader.Name);
            //var directory = "\\dir";

            //Mock.Get(mockClient).Setup(s => s.ConnectAsync(It.IsAny<IPEndPoint>(), It.IsAny<NetworkCredential>(), 0, 0, default)).Returns(Task.CompletedTask).Verifiable();

            //Mock.Get(mockClient).SetupGet(s => s.IsConnected).Returns(true).Verifiable();

            //var returnValue = Enumerable.Range(1, 10).Select(s => $"{s}.txt");

            //var sut = (IDeviceDownloader)Activator.CreateInstance(downloader, new object[] { mockClient, log, mockConfig });

            //var device = new Device()
            //{
            //    Ipaddress = "192.168.1.1",
            //    LoggingEnabled = true,
            //    DeviceConfiguration = new DeviceConfiguration() { Protocol = sut.Protocol, Directory = directory }
            //};

            //Mock.Get(mockClient).Setup(s => s.ListDirectoryAsync(It.Is<string>(i => i == directory), default, It.IsAny<string[]>())).ReturnsAsync(returnValue).Verifiable();

            //Mock.Get(mockClient).Setup(v => v.DownloadFileAsync(It.Is<string>(i => i.StartsWith(verifyPath)), It.IsIn(returnValue), default))
            //    .ReturnsAsync((string localPath, string remotePath, CancellationToken token) => new FileInfo(localPath)).Verifiable();

            //Mock.Get(mockClient).Setup(s => s.DisconnectAsync(default)).Returns(Task.CompletedTask).Verifiable();
            //Mock.Get(mockClient).Setup(s => s.Dispose()).Verifiable();

            //var files = new List<FileInfo>();

            //await foreach (var result in sut.Execute(device))
            //{
            //    files.Add(result.Item2);
            //}

            //Mock.Verify();

            //var expected = returnValue;
            //var actual = files.Select(s => Path.GetFileName(s.FullName));

            //Assert.Equal(expected, actual);
        }

        [Theory]
        [DeviceDownloader]
        public void ExecuteWithControllerDownloadFileWithProgressValid(Type downloader, IDownloaderClient mockClient, ILogger log, IOptionsSnapshot<SignalControllerDownloaderConfiguration> mockConfig)
        {
            Assert.False(true);
            
            //var Location = new Location()
            //{
            //    Ipaddress = new IPAddress(new byte[] { 127, 0, 0, 1 }),
            //    ChartEnabled = true,
            //    PrimaryName = "Controller",
            //    LocationIdentifier = "999"
            //};

            //Mock.Get(mockConfig).Setup(s => s.Value).Returns(new SignalControllerDownloaderConfiguration()
            //{
            //    LocalPath = "C:\\TestPath",
            //    PingControllerToVerify = false
            //});

            //var verifyPath = Path.Combine(mockConfig.Value.LocalPath, Location.LocationIdentifier);
            //var ftpDirectory = "\\dir";

            //Mock.Get(mockClient).Setup(s => s.ConnectAsync(It.IsAny<NetworkCredential>(), 0, 0, default)).Returns(Task.CompletedTask).Verifiable();

            //Mock.Get(mockClient).SetupGet(s => s.IsConnected).Returns(true).Verifiable();

            //var returnValue = Enumerable.Range(1, 10).Select(s => $"{s}.txt");

            //var d = (IDeviceDownloader)Activator.CreateInstance(downloader, new object[] { mockClient, log, mockConfig });

            //Mock.Get(mockClient).Setup(s => s.ListDirectoryAsync(It.Is<string>(i => i == ftpDirectory), default, It.Is<string[]>(i => i == d.FileFilters))).ReturnsAsync(returnValue).Verifiable();

            //Mock.Get(mockClient).Setup(v => v.DownloadFileAsync(It.Is<string>(i => i.StartsWith(verifyPath)), It.IsIn(returnValue), default))
            //    .ReturnsAsync((string localPath, string remotePath, CancellationToken token) => new FileInfo(localPath)).Verifiable();

            //Mock.Get(mockClient).Setup(s => s.DisconnectAsync(default)).Returns(Task.CompletedTask).Verifiable();
            //Mock.Get(mockClient).Setup(s => s.Dispose()).Verifiable();

            //Location.ControllerType = new ControllerType() { Id = d.ControllerType, Directory = ftpDirectory };

            //var progressList = new List<ControllerDownloadProgress>();

            //Progress<ControllerDownloadProgress> progress = new Progress<ControllerDownloadProgress>(p =>
            //{
            //    progressList.Add(p);
            //    _output.WriteLine($"Progress: {p.File.FullName} {p}");
            //});

            //var files = new List<FileInfo>();

            //await foreach (var file in d.Execute(Location, progress))
            //{
            //    files.Add(file);
            //}

            //Mock.Verify();

            //var condition = progressList.Where(p => p.IsSuccessful).Count() == files.Count;

            //Assert.True(condition);

            //var expected = files;
            //var actual = progressList.OrderBy(o => o.Current).Select(s => s.File);

            //Assert.Equal(expected, actual);
        }

        [Theory]
        [DeviceDownloader]
        public void IsDisposing(Type downloader, IDownloaderClient mockClient, ILogger log, IOptionsSnapshot<SignalControllerDownloaderConfiguration> mockConfig)
        {
            var sut = (IDeviceDownloader)Activator.CreateInstance(downloader, new object[] { mockClient, log, mockConfig });

            Mock.Get(mockClient).SetupGet(s => s.IsConnected).Returns(true).Verifiable();
            Mock.Get(mockClient).Setup(s => s.DisconnectAsync(default)).Returns(Task.CompletedTask).Verifiable();
            Mock.Get(mockClient).Setup(s => s.Dispose()).Verifiable();

            sut.Dispose();

            Mock.Verify();
        }

        #endregion

        public void Dispose()
        {
        }
    }
}
