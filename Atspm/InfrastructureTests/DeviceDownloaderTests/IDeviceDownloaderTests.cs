#region license
// Copyright 2025 Utah Departement of Transportation
// for InfrastructureTests - Utah.Udot.Atspm.InfrastructureTests.DeviceDownloaderTests/IDeviceDownloaderTests.cs
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

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Exceptions;
using Utah.Udot.Atspm.Infrastructure.Configuration;
using Utah.Udot.Atspm.Infrastructure.Services.DeviceDownloaders;
using Utah.Udot.Atspm.InfrastructureTests.Attributes;
using Utah.Udot.Atspm.Services;
using Utah.Udot.NetStandardToolkit.Exceptions;
using Xunit;
using Xunit.Abstractions;

namespace Utah.Udot.Atspm.InfrastructureTests.DeviceDownloaderTests
{
    //public class Startup
    //{
    //    public void ConfigureServices(IServiceCollection services, HostBuilderContext context) => services
    //        .AddLogging(lb => lb.AddXunitOutput())
    //        .PostConfigureAll<DeviceDownloaderConfiguration>(o =>
    //                {
    //                    o.BasePath = "C:\\temp3";
    //                    o.Ping = true;
    //                    o.DeleteSource = false;
    //                })
    //        .AddDownloaderClients()
    //        .AddDeviceDownloaders(context);
    //        //.AddScoped<IDeviceDownloader, DeviceDownloader>();
    //}

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
        [Trait(nameof(IDeviceDownloader), nameof(IDeviceDownloader.CanExecute))]
        public void CanExecuteValid(Type downloader, IEnumerable<IDownloaderClient> mockClients, ILogger log, IOptionsSnapshot<DeviceDownloaderConfiguration> mockConfig)
        {
            var sut = (IDeviceDownloader)Activator.CreateInstance(downloader, new object[] { mockClients, log, mockConfig });

            var device = new Device()
            {
                LoggingEnabled = true
            };

            var condition = sut.CanExecute(device);

            Assert.True(condition);
        }

        [Theory]
        [DeviceDownloader]
        [Trait(nameof(IDeviceDownloader), nameof(IDeviceDownloader.CanExecute))]
        public void CanExecuteLoggingEnabledNotValid(Type downloader, IEnumerable<IDownloaderClient> mockClients, ILogger log, IOptionsSnapshot<DeviceDownloaderConfiguration> mockConfig)
        {
            var sut = (IDeviceDownloader)Activator.CreateInstance(downloader, new object[] { mockClients, log, mockConfig });

            var device = new Device()
            {
                LoggingEnabled = false,
                DeviceConfiguration = new DeviceConfiguration() { Protocol = TransportProtocols.Ftp }
            };

            var condition = sut.CanExecute(device);

            Assert.False(condition);
        }

        [Theory]
        [DeviceDownloader]
        [Trait(nameof(IDeviceDownloader), nameof(IDeviceDownloader.CanExecute))]
        public void CanExecuteProtocolNotValid(Type downloader, IEnumerable<IDownloaderClient> mockClients, ILogger log, IOptionsSnapshot<DeviceDownloaderConfiguration> mockConfig)
        {
            var sut = (IDeviceDownloader)Activator.CreateInstance(downloader, new object[] { mockClients, log, mockConfig });

            var device = new Device()
            {
                LoggingEnabled = true,
                DeviceConfiguration = new DeviceConfiguration() { Protocol = TransportProtocols.Unknown }
            };

            var condition = sut.CanExecute(device);

            Assert.False(condition);
        }

        [Theory]
        [DeviceDownloader]
        [Trait(nameof(IDeviceDownloader), nameof(IDeviceDownloader.Execute))]
        public async Task ExecuteWithNullParameter(Type downloader, IEnumerable<IDownloaderClient> mockClients, ILogger log, IOptionsSnapshot<DeviceDownloaderConfiguration> mockConfig)
        {
            var sut = (IDeviceDownloader)Activator.CreateInstance(downloader, new object[] { mockClients, log, mockConfig });

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await foreach (var file in sut.Execute(null)) { }
            });
        }

        [Theory]
        [DeviceDownloader]
        [Trait(nameof(IDeviceDownloader), nameof(IDeviceDownloader.Execute))]
        public async Task ExecuteWithNullIPAddress(Type downloader, IEnumerable<IDownloaderClient> mockClients, ILogger log, IOptionsSnapshot<DeviceDownloaderConfiguration> mockConfig)
        {
            Mock.Get(mockConfig).Setup(s => s.Value).Returns(new DeviceDownloaderConfiguration() { Ping = false });

            var sut = (IDeviceDownloader)Activator.CreateInstance(downloader, new object[] { mockClients, log, mockConfig });

            var device = new Device()
            {
                LoggingEnabled = true,
                DeviceConfiguration = new DeviceConfiguration() { Protocol = TransportProtocols.Ftp }
            };

            await Assert.ThrowsAsync<InvalidDeviceIpAddressException>(async () =>
            {
                await foreach (var file in sut.Execute(device)) { }
            });
        }

        [Theory]
        [DeviceDownloader]
        [Trait(nameof(IDeviceDownloader), nameof(IDeviceDownloader.Execute))]
        public async Task ExecuteWithInvalidIPAddress(Type downloader, IEnumerable<IDownloaderClient> mockClients, ILogger log, IOptionsSnapshot<DeviceDownloaderConfiguration> mockConfig)
        {
            Mock.Get(mockConfig).Setup(s => s.Value).Returns(new DeviceDownloaderConfiguration() { Ping = false });

            var sut = (IDeviceDownloader)Activator.CreateInstance(downloader, new object[] { mockClients, log, mockConfig });

            var device = new Device()
            {
                Ipaddress = "256.256.256.256",
                LoggingEnabled = true,
                DeviceConfiguration = new DeviceConfiguration() { Protocol = TransportProtocols.Ftp }
            };

            await Assert.ThrowsAsync<InvalidDeviceIpAddressException>(async () =>
            {
                await foreach (var file in sut.Execute(device)) { }
            });
        }

        [Theory]
        [DeviceDownloader]
        [Trait(nameof(IDeviceDownloader), nameof(IDeviceDownloader.Execute))]
        public async Task ExecuteWithFailedCanExecute(Type downloader, IEnumerable<IDownloaderClient> mockClients, ILogger log, IOptionsSnapshot<DeviceDownloaderConfiguration> mockConfig)
        {
            var sut = (IDeviceDownloader)Activator.CreateInstance(downloader, new object[] { mockClients, log, mockConfig });

            var device = new Device()
            {
                LoggingEnabled = false,
                DeviceConfiguration = new DeviceConfiguration() { Protocol = TransportProtocols.Unknown }
            };

            await Assert.ThrowsAsync<ExecuteException>(async () =>
            {
                await foreach (var file in sut.Execute(device)) { }
            });
        }

        [Theory]
        [DeviceDownloader]
        [Trait(nameof(IDeviceDownloader), nameof(IDeviceDownloader.Execute))]
        public async Task ExecuteWithTokenCancelled(Type downloader, IEnumerable<IDownloaderClient> mockClients, ILogger log, IOptionsSnapshot<DeviceDownloaderConfiguration> mockConfig)
        {
            var ts = new CancellationTokenSource();
            ts.Cancel();

            var sut = (IDeviceDownloader)Activator.CreateInstance(downloader, new object[] { mockClients, log, mockConfig });

            await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            {
                await foreach (var file in sut.Execute(null, ts.Token)) { }
            });
        }

        //[Theory]
        //[DeviceDownloader]
        //[Trait(nameof(IDeviceDownloader), nameof(IDeviceDownloader.Execute))]
        //public async Task TestParser(Type downloader, IEnumerable<IDownloaderClient> mockClients, ILogger log, IOptionsSnapshot<DeviceDownloaderConfiguration> mockConfig)
        //{
        //    Mock.Get(mockConfig).Setup(s => s.Value).Returns(new DeviceDownloaderConfiguration() { Ping = false });

        //    var sut = (IDeviceDownloader)Activator.CreateInstance(downloader, new object[] { mockClients, log, mockConfig });

        //    var device = new Device()
        //    {
        //        DeviceIdentifier = "hello",
        //        Ipaddress = "192.168.1.1",
        //        LoggingEnabled = true,
        //        DeviceConfiguration = new DeviceConfiguration()
        //        {
        //            Protocol = TransportProtocols.Ftp,
        //            Path = "path1 [Device:Ipaddress] path 2 [DateTime:HH:mm:ss.f] - [LogStartTime:MM-dd-yyyy HH:mm:ss.f]",
        //            Query = ["this is another test [Device:DeviceIdentifier] and another [DateTime:MM-dd-yyyy]"]
        //        }
        //    };

        //    //_output.WriteLine($"path: {new ObjectPropertyParser(device, device.DeviceConfiguration.Path)}");

        //    await foreach (var file in sut.Execute(device))
        //    {
        //        _output.WriteLine($"{file.Item1} - {file.Item2}");
        //    }
        //}

        //[Theory]
        //[DeviceDownloader]
        //public async Task ExecuteWithDownloaderClientConnectionException(Type downloader, IEnumerable<IDownloaderClient> mockClients, ILogger log, IOptionsSnapshot<DeviceDownloaderConfiguration> mockConfig)
        //{
        //    Mock.Get(mockConfig).Setup(s => s.Value).Returns(new DeviceDownloaderConfiguration() { Ping = false });

        //    foreach (var client in mockClients)
        //    {
        //        Mock.Get(client).Setup(s => s.ConnectAsync(It.IsAny<IPEndPoint>(), It.IsAny<NetworkCredential>(), 0, 0, null, default))
        //        .ThrowsAsync(new DownloaderClientConnectionException(It.IsAny<string>(), client, null))
        //        .Verifiable();

        //        Mock.Get(client).SetupGet(s => s.IsConnected).Returns(false).Verifiable();

        //        Mock.Get(client).Setup(s => s.DisconnectAsync(default)).Returns(Task.CompletedTask).Verifiable();

        //        Mock.Get(client).Setup(s => s.Dispose()).Verifiable();
        //    }

        //    var sut = (IDeviceDownloader)Activator.CreateInstance(downloader, new object[] { mockClients, log, mockConfig });

        //    var device = new Device()
        //    {
        //        Ipaddress = "192.168.1.1",
        //        LoggingEnabled = true,
        //        DeviceConfiguration = new DeviceConfiguration() { Protocol = TransportProtocols.Ftp },
        //    };

        //    await foreach (var file in sut.Execute(device)) { }

        //    Mock.Verify();
        //}

        //[Theory]
        //[DeviceDownloader]
        //public async Task ExecuteWithControllerDownloadFileException(Type downloader, IEnumerable<IDownloaderClient> mockClients, ILogger log, IOptionsSnapshot<DeviceDownloaderConfiguration> mockConfig)
        //{
        //    Mock.Get(mockConfig).Setup(s => s.Value).Returns(new DeviceDownloaderConfiguration() { Ping = false });

        //    foreach (var client in mockClients)
        //    {
        //        Mock.Get(client).Setup(s => s.ConnectAsync(It.IsAny<IPEndPoint>(), It.IsAny<NetworkCredential>(), 0, 0, default))
        //            .Callback(() => Mock.Get(client).SetupGet(p => p.IsConnected).Returns(true));

        //        //Mock.Get(client).SetupGet(s => s.IsConnected).Returns(false).Verifiable();

        //        Mock.Get(client).Setup(s => s.DisconnectAsync(default)).Returns(Task.CompletedTask).Verifiable();

        //        Mock.Get(client).Setup(s => s.Dispose()).Verifiable();
        //    }

        //    var sut = (IDeviceDownloader)Activator.CreateInstance(downloader, new object[] { mockClients, log, mockConfig });

        //    var device = new Device()
        //    {
        //        Ipaddress = "192.168.1.1",
        //        LoggingEnabled = true,
        //        DeviceConfiguration = new DeviceConfiguration() { Protocol = TransportProtocols.Ftp },
        //    };

        //    await foreach (var file in sut.Execute(device)) { }

        //    Mock.Verify();
        //}

        //[Theory]
        //[DeviceDownloader]
        //public async Task ExecuteWithControllerDownloadFileTokenCancelled(Type downloader, IDownloaderClient mockClient, ILogger log, IOptionsSnapshot<DeviceDownloaderConfiguration> mockConfig)
        //{

        //}

        //[Theory]
        //[DeviceDownloader]
        //public void ExecuteWithControllerDownloadFileSuccess(Type downloader, IDownloaderClient mockClient, ILogger log, IOptionsSnapshot<DeviceDownloaderConfiguration> mockConfig)
        //{
        //    Assert.False(true);

        //    //Mock.Get(mockConfig).Setup(s => s.Value).Returns(new DeviceDownloaderConfiguration()
        //    //{
        //    //    BasePath = Path.GetTempPath(),
        //    //    Ping = false
        //    //});

        //    //var verifyPath = Path.Combine(mockConfig.Value.BasePath, downloader.Name);
        //    //var directory = "\\dir";

        //    //Mock.Get(mockClient).Setup(s => s.ConnectAsync(It.IsAny<IPEndPoint>(), It.IsAny<NetworkCredential>(), 0, 0, default)).Returns(Task.CompletedTask).Verifiable();

        //    //Mock.Get(mockClient).SetupGet(s => s.IsConnected).Returns(true).Verifiable();

        //    //var returnValue = Enumerable.Range(1, 10).Select(s => $"{s}.txt");

        //    //var sut = (IDeviceDownloader)Activator.CreateInstance(downloader, new object[] { mockClient, log, mockConfig });

        //    //var device = new Device()
        //    //{
        //    //    Ipaddress = "192.168.1.1",
        //    //    LoggingEnabled = true,
        //    //    DeviceConfiguration = new DeviceConfiguration() { Protocol = sut.Protocol, Path = directory }
        //    //};

        //    //Mock.Get(mockClient).Setup(s => s.ListResourcesAsync(It.Is<string>(i => i == directory), default, It.IsAny<string[]>())).ReturnsAsync(returnValue).Verifiable();

        //    //Mock.Get(mockClient).Setup(v => v.DownloadResourceAsync(It.Is<string>(i => i.StartsWith(verifyPath)), It.IsIn(returnValue), default))
        //    //    .ReturnsAsync((string localPath, string remotePath, CancellationToken token) => new FileInfo(localPath)).Verifiable();

        //    //Mock.Get(mockClient).Setup(s => s.DisconnectAsync(default)).Returns(Task.CompletedTask).Verifiable();
        //    //Mock.Get(mockClient).Setup(s => s.Dispose()).Verifiable();

        //    //var files = new List<FileInfo>();

        //    //await foreach (var result in sut.Execute(device))
        //    //{
        //    //    files.Add(result.Item2);
        //    //}

        //    //Mock.Verify();

        //    //var expected = returnValue;
        //    //var actual = files.Select(s => Path.GetFileName(s.FullName));

        //    //Assert.Equal(expected, actual);
        //}

        //[Theory]
        //[DeviceDownloader]
        //public void ExecuteWithControllerDownloadFileWithProgressValid(Type downloader, IDownloaderClient mockClient, ILogger log, IOptionsSnapshot<DeviceDownloaderConfiguration> mockConfig)
        //{
        //    Assert.False(true);

        //    //var Location = new Location()
        //    //{
        //    //    Ipaddress = new IPAddress(new byte[] { 127, 0, 0, 1 }),
        //    //    ChartEnabled = true,
        //    //    PrimaryName = "Controller",
        //    //    LocationIdentifier = "999"
        //    //};

        //    //Mock.Get(mockConfig).Setup(s => s.Value).Returns(new DeviceDownloaderConfiguration()
        //    //{
        //    //    BasePath = "C:\\TestPath",
        //    //    Ping = false
        //    //});

        //    //var verifyPath = Path.Combine(mockConfig.Value.BasePath, Location.LocationIdentifier);
        //    //var ftpDirectory = "\\dir";

        //    //Mock.Get(mockClient).Setup(s => s.ConnectAsync(It.IsAny<NetworkCredential>(), 0, 0, default)).Returns(Task.CompletedTask).Verifiable();

        //    //Mock.Get(mockClient).SetupGet(s => s.IsConnected).Returns(true).Verifiable();

        //    //var returnValue = Enumerable.Range(1, 10).Select(s => $"{s}.txt");

        //    //var d = (IDeviceDownloader)Activator.CreateInstance(downloader, new object[] { mockClient, log, mockConfig });

        //    //Mock.Get(mockClient).Setup(s => s.ListResourcesAsync(It.Is<string>(i => i == ftpDirectory), default, It.Is<string[]>(i => i == d.FileFilters))).ReturnsAsync(returnValue).Verifiable();

        //    //Mock.Get(mockClient).Setup(v => v.DownloadResourceAsync(It.Is<string>(i => i.StartsWith(verifyPath)), It.IsIn(returnValue), default))
        //    //    .ReturnsAsync((string localPath, string remotePath, CancellationToken token) => new FileInfo(localPath)).Verifiable();

        //    //Mock.Get(mockClient).Setup(s => s.DisconnectAsync(default)).Returns(Task.CompletedTask).Verifiable();
        //    //Mock.Get(mockClient).Setup(s => s.Dispose()).Verifiable();

        //    //Location.ControllerType = new ControllerType() { Id = d.ControllerType, Path = ftpDirectory };

        //    //var progressList = new List<ControllerDownloadProgress>();

        //    //Progress<ControllerDownloadProgress> progress = new Progress<ControllerDownloadProgress>(p =>
        //    //{
        //    //    progressList.Add(p);
        //    //    _output.WriteLine($"Progress: {p.File.FullName} {p}");
        //    //});

        //    //var files = new List<FileInfo>();

        //    //await foreach (var file in d.Execute(Location, progress))
        //    //{
        //    //    files.Add(file);
        //    //}

        //    //Mock.Verify();

        //    //var condition = progressList.Where(p => p.IsSuccessful).Count() == files.Count;

        //    //Assert.True(condition);

        //    //var expected = files;
        //    //var actual = progressList.OrderBy(o => o.Current).Select(s => s.File);

        //    //Assert.Equal(expected, actual);
        //}

        //[Theory]
        //[DeviceDownloader]
        //public void IsDisposing(Type downloader, IDownloaderClient mockClient, ILogger log, IOptionsSnapshot<DeviceDownloaderConfiguration> mockConfig)
        //{
        //    var sut = (IDeviceDownloader)Activator.CreateInstance(downloader, new object[] { mockClient, log, mockConfig });

        //    Mock.Get(mockClient).SetupGet(s => s.IsConnected).Returns(true).Verifiable();
        //    Mock.Get(mockClient).Setup(s => s.DisconnectAsync(default)).Returns(Task.CompletedTask).Verifiable();
        //    Mock.Get(mockClient).Setup(s => s.Dispose()).Verifiable();

        //    sut.Dispose();

        //    Mock.Verify();
        //}



        #endregion

        public void Dispose()
        {
        }
    }
}
