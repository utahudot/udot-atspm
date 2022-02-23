using ATSPM.Application.Configuration;
using ATSPM.Application.Exceptions;
using ATSPM.Application.Models;
using ATSPM.Application.Services.SignalControllerProtocols;
using ATSPM.Domain.Common;
using ATSPM.Infrasturcture.Services.ControllerDownloaders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SignalControllerLoggerTests.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace SignalControllerLoggerTests
{
    public class ISignalControllerDownloaderTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private ILoggerFactory _loggerFactory;

        private StringWriter _consoleOut = new StringWriter();

        //private readonly IServiceCollection _serviceCollection;
        //private ILogger _nullLogger;
        //private IOptions<SignalControllerDownloaderConfiguration> _nullOptions;

        public ISignalControllerDownloaderTests(ITestOutputHelper output)
        {
            _output = output;
            
            Console.SetOut(_consoleOut);

            //_loggerFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder.SetMinimumLevel(LogLevel.Trace).AddProvider(new XUnitLoggerProvider(_consoleOut)).AddSystemdConsole());

            _loggerFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder.SetMinimumLevel(LogLevel.Trace).AddProvider(new GenericLoggerProvider<StringWriter>(_consoleOut)).AddProvider(new GenericLoggerProvider<ITestOutputHelper>(output)));


            //_serviceCollection = new ServiceCollection();

            ////downloaders
            //_serviceCollection.AddScoped<ISignalControllerDownloader, FTPSignalControllerDownloader>();
            //_serviceCollection.AddScoped<ISignalControllerDownloader, MaxTimeSignalControllerDownloader>();
            //_serviceCollection.AddScoped<ISignalControllerDownloader, SFTPSignalControllerDownloader>();
        }

        #region ISignalControllerDownloader

        [Theory]
        [SignalControllerDownloaders]
        public void CanExecuteValid(Type downloader, IDownloaderClient mockClient, ILogger log, IOptionsSnapshot<SignalControllerDownloaderConfiguration> mockConfig)
        {
            var d = (ISignalControllerDownloader)Activator.CreateInstance(downloader, new object[] { mockClient, log, mockConfig });

            var signal = new Signal()
            {
                Enabled = true,
                ControllerType = new ControllerType() { ControllerTypeId = d.ControllerType }
            };

            Mock.Get(mockClient).Setup(s => s.Dispose());

            var condition = d.CanExecute(signal);

            Assert.True(condition);
        }

        [Theory]
        [SignalControllerDownloaders]
        public void CanExecuteEnabledNotValid(Type downloader, IDownloaderClient mockClient, ILogger log, IOptionsSnapshot<SignalControllerDownloaderConfiguration> mockConfig)
        {
            var d = (ISignalControllerDownloader)Activator.CreateInstance(downloader, new object[] { mockClient, log, mockConfig });

            var signal = new Signal()
            {
                Enabled = false,
                ControllerType = new ControllerType() { ControllerTypeId = d.ControllerType }
            };

            Mock.Get(mockClient).Setup(s => s.Dispose());

            var condition = d.CanExecute(signal);

            Assert.False(condition);
        }

        [Theory]
        [SignalControllerDownloaders]
        public void CanExecuteTypeNotValid(Type downloader, IDownloaderClient mockClient, ILogger log, IOptionsSnapshot<SignalControllerDownloaderConfiguration> mockConfig)
        {
            var d = (ISignalControllerDownloader)Activator.CreateInstance(downloader, new object[] { mockClient, log, mockConfig });

            var signal = new Signal()
            {
                Enabled = true,
                ControllerType = new ControllerType() { ControllerTypeId = d.ControllerType + 1 }
            };

            Mock.Get(mockClient).Setup(s => s.Dispose());

            var condition = d.CanExecute(signal);

            Assert.False(condition);
        }

        //[Fact]
        //public async void ExecuteAsyncCanExecuteInvalid()
        //{
        //    var client = new Mock<ISFTPDownloaderClient>();
        //    client.Setup()



        //    Signal signal = new Signal()
        //    {
        //        ControllerType = new ControllerType() { ControllerTypeId = 4 }
        //    };

        //    _output.WriteLine($"ControllerTypeId: {signal.ControllerType.ControllerTypeId}");

        //    var condition = _downloader.CanExecute(signal);

        //    Assert.True(condition);
        //}

        [Theory]
        [SignalControllerDownloaders]
        public async void ExecuteWithNullParameter(Type downloader, IDownloaderClient mockClient, ILogger log, IOptionsSnapshot<SignalControllerDownloaderConfiguration> mockConfig)
        {
            var d = (ISignalControllerDownloader)Activator.CreateInstance(downloader, new object[] { mockClient, log, mockConfig });

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await foreach (var file in d.Execute(null)) { }
            });
        }

        [Theory]
        [SignalControllerDownloaders]
        public async void ExecuteWithInvalidIPAddress(Type downloader, IDownloaderClient mockClient, ILogger log, IOptionsSnapshot<SignalControllerDownloaderConfiguration> mockConfig)
        {
            Mock.Get(mockConfig).Setup(s => s.Value).Returns(new SignalControllerDownloaderConfiguration() { PingControllerToVerify = false });

            var d = (ISignalControllerDownloader)Activator.CreateInstance(downloader, new object[] { mockClient, log, mockConfig });

            var signal = new Signal()
            {
                Ipaddress = "Invalid Address",
                ControllerType = new ControllerType() { ControllerTypeId = d.ControllerType }
            };

            await Assert.ThrowsAsync<FormatException>(async () =>
            {
                await foreach (var file in d.Execute(signal)) { }
            });
        }

        [Theory]
        [SignalControllerDownloaders]
        public async void ExecuteWithControllerConnectionException(Type downloader, IDownloaderClient mockClient, ILogger log, IOptionsSnapshot<SignalControllerDownloaderConfiguration> mockConfig)
        {
            var signal = new Signal()
            {
                Ipaddress = "127.0.0.1"
            };

            Mock.Get(mockConfig).Setup(s => s.Value).Returns(new SignalControllerDownloaderConfiguration() { PingControllerToVerify = false });

            Mock.Get(mockClient).Setup(s => s.ConnectAsync(It.IsAny<NetworkCredential>(), 0, 0, default))
                .ThrowsAsync(new ControllerConnectionException(It.Is<string>(s => s == signal.Ipaddress), null, null))
                .Verifiable();

            Mock.Get(mockClient).SetupGet(s => s.IsConnected).Returns(false).Verifiable();

            //var logger = _loggerFactory.CreateLogger<FTPSignalControllerDownloader>();
            var logger = GenericLogger<ITestOutputHelper>.CreateLogger<FTPSignalControllerDownloader>(_output);

            if (logger is GenericLogger<ITestOutputHelper, FTPSignalControllerDownloader> testing)
            {
                testing.OnLogged += (o, e) =>
                {
                    _output.WriteLine($"just wrote this!!! {e.LogLevel} : {e.EventId.Id}|{e.EventId.Name}");

                };
            }

            var d = (ISignalControllerDownloader)Activator.CreateInstance(downloader, new object[] { mockClient, logger, mockConfig });

            signal.ControllerType = new ControllerType() { ControllerTypeId = d.ControllerType };

            await foreach (var file in d.Execute(signal)) { }

            Mock.Verify();

            //_output.WriteLine($"{_consoleOut}");

            //var exception = await Assert.ThrowsAsync<ControllerConnectionException>(async () =>
            //{
            //    await foreach (var file in d.Execute(signal)) { }
            //});

            //var expected = signal.Ipaddress;
            //var actual = exception.Host;

            //Assert.Equal(expected, actual);
        }

        [Theory]
        [SignalControllerDownloaders]
        public async void ExecuteWithControllerConnectionTokenCancelled(Type downloader, IDownloaderClient mockClient, ILogger log, IOptionsSnapshot<SignalControllerDownloaderConfiguration> mockConfig)
        {

        }

        [Theory]
        [SignalControllerDownloaders]
        public async void ExecuteWithControllerDownloadFileException(Type downloader, IDownloaderClient mockClient, ILogger log, IOptionsSnapshot<SignalControllerDownloaderConfiguration> mockConfig)
        {

        }

        [Theory]
        [SignalControllerDownloaders]
        public async void ExecuteWithControllerDownloadFileTokenCancelled(Type downloader, IDownloaderClient mockClient, ILogger log, IOptionsSnapshot<SignalControllerDownloaderConfiguration> mockConfig)
        {

        }

        [Theory]
        [SignalControllerDownloaders]
        public async void ExecuteWithControllerDownloadFileSuccess(Type downloader, IDownloaderClient mockClient, ILogger log, IOptionsSnapshot<SignalControllerDownloaderConfiguration> mockConfig)
        {
            var signal = new Signal()
            {
                Ipaddress = "127.0.0.1",
                Enabled = true,
                PrimaryName = "Controller",
                SignalId = "999"
            };

            Mock.Get(mockConfig).Setup(s => s.Value).Returns(new SignalControllerDownloaderConfiguration()
            {
                LocalPath = "C:\\TestPath",
                PingControllerToVerify = false
            });

            var verifyPath = Path.Combine(mockConfig.Value.LocalPath, signal.SignalId);
            var ftpDirectory = "\\dir";

            Mock.Get(mockClient).Setup(s => s.ConnectAsync(It.IsAny<NetworkCredential>(), 0, 0, default)).Returns(Task.CompletedTask).Verifiable();

            Mock.Get(mockClient).SetupGet(s => s.IsConnected).Returns(true).Verifiable();

            var returnValue = Enumerable.Range(1, 10).Select(s => $"{s}.txt");

            var d = (ISignalControllerDownloader)Activator.CreateInstance(downloader, new object[] { mockClient, log, mockConfig });

            Mock.Get(mockClient).Setup(s => s.ListDirectoryAsync(It.Is<string>(i => i == ftpDirectory), default, It.Is<string[]>(i => i == d.FileFilters))).ReturnsAsync(returnValue).Verifiable();

            Mock.Get(mockClient).Setup(v => v.DownloadFileAsync(It.Is<string>(i => i.StartsWith(verifyPath)), It.IsIn<string>(returnValue), default))
                .ReturnsAsync((string localPath, string remotePath, CancellationToken token) => new FileInfo(localPath)).Verifiable();

            Mock.Get(mockClient).Setup(s => s.DisconnectAsync(default)).Returns(Task.CompletedTask).Verifiable();
            Mock.Get(mockClient).Setup(s => s.Dispose()).Verifiable();

            signal.ControllerType = new ControllerType() { ControllerTypeId = d.ControllerType, Ftpdirectory = ftpDirectory };

            var files = new List<FileInfo>();

            await foreach (var file in d.Execute(signal)) 
            {
                files.Add(file);
            }

            Mock.Verify();

            var expected = returnValue;
            var actual = files.Select(s => Path.GetFileName(s.FullName));

            Assert.Equal(expected, actual);
        }

        [Theory]
        [SignalControllerDownloaders]
        public async void ExecuteWithControllerDownloadFileWithProgressValid(Type downloader, IDownloaderClient mockClient, ILogger log, IOptionsSnapshot<SignalControllerDownloaderConfiguration> mockConfig)
        {

        }

        [Theory]
        [SignalControllerDownloaders]
        public async void ExecuteWithControllerDownloadFileWithProgressNoFiles(Type downloader, IDownloaderClient mockClient, ILogger log, IOptionsSnapshot<SignalControllerDownloaderConfiguration> mockConfig)
        {

        }

        [Theory]
        [SignalControllerDownloaders]
        public void IsDisposing(Type downloader, IDownloaderClient mockClient, ILogger log, IOptionsSnapshot<SignalControllerDownloaderConfiguration> mockConfig)
        {
            var d = (ISignalControllerDownloader)Activator.CreateInstance(downloader, new object[] { mockClient, log, mockConfig });

            Mock.Get(mockClient).SetupGet(s => s.IsConnected).Returns(true).Verifiable();
            Mock.Get(mockClient).Setup(s => s.DisconnectAsync(default)).Returns(Task.CompletedTask).Verifiable();
            Mock.Get(mockClient).Setup(s => s.Dispose()).Verifiable();

            d.Dispose();

            Mock.Verify();
        }

        #endregion

        public void Dispose()
        {
            _loggerFactory.Dispose();
            _loggerFactory = null;

            _consoleOut.Dispose();
            _consoleOut = null;

            //if (_downloader is IDisposable d)
            //{
            //    d.Dispose();
            //}

            //_output.WriteLine($"Disposing ISignalControllerDownloader Instance: {_downloader.GetHashCode()}");

            //_downloader = null;
            //_nullLogger = null;
            //_nullOptions = null;
        }
    }
}
