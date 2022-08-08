using ATSPM.Application.Common;
using ATSPM.Application.Configuration;
using ATSPM.Application.Exceptions;
using ATSPM.Application.Services.SignalControllerProtocols;
using ATSPM.Data.Models;
using ATSPM.Domain.Common;
using ATSPM.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SignalControllerLoggerTests.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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

        public ISignalControllerDownloaderTests(ITestOutputHelper output)
        {
            _output = output;
            
            _loggerFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder
            .SetMinimumLevel(LogLevel.Trace)
            .AddProvider(new GenericLoggerProvider<StringWriter>(_consoleOut))
            .AddProvider(new GenericLoggerProvider<ITestOutputHelper>(output)));
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
                ControllerType = new ControllerType() { ControllerTypeID = d.ControllerType }
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
                ControllerType = new ControllerType() { ControllerTypeID = d.ControllerType }
            };

            Mock.Get(mockClient).Setup(s => s.Dispose());

            var condition = d.CanExecute(signal);

            Assert.False(condition);
        }

        [Theory]
        [SignalControllerDownloaders]
        public void CanExecuteTypeNotValid(Type downloader, IDownloaderClient mockClient, ILogger log, IOptionsSnapshot<SignalControllerDownloaderConfiguration> mockConfig)
        {
            Mock.Get(mockClient).Setup(s => s.Dispose());

            var d = (ISignalControllerDownloader)Activator.CreateInstance(downloader, new object[] { mockClient, log, mockConfig });

            var signal = new Signal()
            {
                Enabled = true,
                ControllerType = new ControllerType() { ControllerTypeID = d.ControllerType + 1 }
            };

            var condition = d.CanExecute(signal);

            Assert.False(condition);
        }

        [Theory]
        [SignalControllerDownloaders]
        public async void ExecuteWithNullParameter(Type downloader, IDownloaderClient mockClient, ILogger log, IOptionsSnapshot<SignalControllerDownloaderConfiguration> mockConfig)
        {
            Mock.Get(mockClient).Setup(s => s.Dispose());

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
            Mock.Get(mockClient).Setup(s => s.Dispose());

            Mock.Get(mockConfig).Setup(s => s.Value).Returns(new SignalControllerDownloaderConfiguration() { PingControllerToVerify = false });

            var d = (ISignalControllerDownloader)Activator.CreateInstance(downloader, new object[] { mockClient, log, mockConfig });

            var signal = new Signal()
            {
                IPAddress = "Invalid Address",
                Enabled = true,
                PrimaryName = "Controller",
                SignalID = "999",
                ControllerType = new ControllerType() { ControllerTypeID = d.ControllerType }
            };

            await Assert.ThrowsAsync<InvalidSignalControllerIpAddressException>(async () =>
            {
                await foreach (var file in d.Execute(signal)) { }
            });
        }

        [Theory]
        [SignalControllerDownloaders]
        public async Task ExecuteWithFailedCanExecute(Type downloader, IDownloaderClient mockClient, ILogger log, IOptionsSnapshot<SignalControllerDownloaderConfiguration> mockConfig)
        {
            Mock.Get(mockClient).Setup(s => s.Dispose());

            var d = (ISignalControllerDownloader)Activator.CreateInstance(downloader, new object[] { mockClient, log, mockConfig });

            var signal = new Signal()
            {
                Enabled = true,
                PrimaryName = "Controller",
                SignalID = "999",
                ControllerType = new ControllerType() { ControllerTypeID = 0 }
            };

            await Assert.ThrowsAsync<ExecuteException>(async () =>
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
                IPAddress = "127.0.0.1",
                Enabled = true
            };

            Mock.Get(mockConfig).Setup(s => s.Value).Returns(new SignalControllerDownloaderConfiguration() { PingControllerToVerify = false });

            Mock.Get(mockClient).Setup(s => s.ConnectAsync(It.IsAny<NetworkCredential>(), 0, 0, default))
                .ThrowsAsync(new ControllerConnectionException(It.Is<string>(s => s == signal.IPAddress), mockClient, null))
                .Verifiable();

            Mock.Get(mockClient).SetupGet(s => s.IsConnected).Returns(false).Verifiable();

            Mock.Get(mockClient).Setup(s => s.DisconnectAsync(default)).Returns(Task.CompletedTask).Verifiable();
            Mock.Get(mockClient).Setup(s => s.Dispose()).Verifiable();

            var d = (ISignalControllerDownloader)Activator.CreateInstance(downloader, new object[] { mockClient, log, mockConfig });

            signal.ControllerType = new ControllerType() { ControllerTypeID = d.ControllerType };

            await foreach (var file in d.Execute(signal)) { }

            Mock.Verify();
        }

        //[Theory]
        //[SignalControllerDownloaders]
        //public async void ExecuteWithControllerConnectionTokenCancelled(Type downloader, IDownloaderClient mockClient, ILogger log, IOptionsSnapshot<SignalControllerDownloaderConfiguration> mockConfig)
        //{

        //}

        //[Theory]
        //[SignalControllerDownloaders]
        //public async void ExecuteWithControllerDownloadFileException(Type downloader, IDownloaderClient mockClient, ILogger log, IOptionsSnapshot<SignalControllerDownloaderConfiguration> mockConfig)
        //{

        //}

        //[Theory]
        //[SignalControllerDownloaders]
        //public async void ExecuteWithControllerDownloadFileTokenCancelled(Type downloader, IDownloaderClient mockClient, ILogger log, IOptionsSnapshot<SignalControllerDownloaderConfiguration> mockConfig)
        //{

        //}

        [Theory]
        [SignalControllerDownloaders]
        public async void ExecuteWithControllerDownloadFileSuccess(Type downloader, IDownloaderClient mockClient, ILogger log, IOptionsSnapshot<SignalControllerDownloaderConfiguration> mockConfig)
        {
            var signal = new Signal()
            {
                IPAddress = "127.0.0.1",
                Enabled = true,
                PrimaryName = "Controller",
                SignalID = "999"
            };

            Mock.Get(mockConfig).Setup(s => s.Value).Returns(new SignalControllerDownloaderConfiguration()
            {
                LocalPath = "C:\\TestPath",
                PingControllerToVerify = false
            });

            var verifyPath = Path.Combine(mockConfig.Value.LocalPath, signal.SignalID);
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

            signal.ControllerType = new ControllerType() { ControllerTypeID = d.ControllerType, Ftpdirectory = ftpDirectory };
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
            var signal = new Signal()
            {
                IPAddress = "127.0.0.1",
                Enabled = true,
                PrimaryName = "Controller",
                SignalID = "999"
            };

            Mock.Get(mockConfig).Setup(s => s.Value).Returns(new SignalControllerDownloaderConfiguration()
            {
                LocalPath = "C:\\TestPath",
                PingControllerToVerify = false
            });

            var verifyPath = Path.Combine(mockConfig.Value.LocalPath, signal.SignalID);
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

            signal.ControllerType = new ControllerType() { ControllerTypeID = d.ControllerType, Ftpdirectory = ftpDirectory };

            var progressList = new List<ControllerDownloadProgress>();

            Progress<ControllerDownloadProgress> progress = new Progress<ControllerDownloadProgress>(p =>
            {
                progressList.Add(p);
                _output.WriteLine($"Progress: {p.File.FullName} {p}");
            });

            var files = new List<FileInfo>();

            await foreach (var file in d.Execute(signal, progress))
            {
                files.Add(file);
            }

            Mock.Verify();

            var condition = (progressList.Where(p => p.IsSuccessful).Count() == files.Count);

            Assert.True(condition);

            var expected = files;
            var actual = progressList.OrderBy(o => o.Current).Select(s => s.File);

            Assert.Equal(expected, actual);
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
