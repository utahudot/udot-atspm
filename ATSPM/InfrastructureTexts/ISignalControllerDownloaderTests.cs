using ATSPM.Application.Configuration;
using ATSPM.Application.Enums;
using ATSPM.Application.Services.SignalControllerProtocols;
using ATSPM.Infrasturcture.Services.ControllerDownloaders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using Xunit;
using Xunit.Abstractions;
using FluentFTP;
using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using InfrastructureTests.Attributes;
using ATSPM.Application.Models;
using ATSPM.Domain.Exceptions;
using System.IO;

namespace SignalControllerLoggerTests
{
    public class ISignalControllerDownloaderTests : IDisposable
    {
        private const string TestDataPath = "C:\\Projects\\udot-atsmp\\ATSPM\\InfrastructureTexts\\TestData";

        private readonly ITestOutputHelper _output;
        private ISignalControllerDownloader _downloader;
        private ILogger _nullLogger;
        private IOptions<SignalControllerDownloaderConfiguration> _nullOptions;

        public ISignalControllerDownloaderTests(ITestOutputHelper output)
        {
            _output = output;
            _nullLogger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<StubSignalControllerDownloader>();
            _nullOptions = Options.Create(new SignalControllerDownloaderConfiguration() { EarliestAcceptableDate = new DateTime(), PingControllerToVerify = false });



            //var s = new ServiceCollection();
            //s.AddTransient<ISignalControllerDownloader, StubSignalControllerDownloader>();






            _downloader = new StubSignalControllerDownloader((ILogger<StubSignalControllerDownloader>)_nullLogger, new ServiceCollection().BuildServiceProvider(), _nullOptions);

            _output.WriteLine($"Created ISignalControllerDownloader Instance: {_downloader.GetHashCode()}");
        }

        #region ISignalControllerDownloader

        [Fact]
        public void CanExecuteValid()
        {
            Signal signal = new Signal()
            {
                ControllerType = new ControllerType() { ControllerTypeId = 4 }
            };

            _output.WriteLine($"ControllerTypeId: {signal.ControllerType.ControllerTypeId}");

            var condition = _downloader.CanExecute(signal);

            Assert.True(condition);
        }

        [Fact]
        public void CanExecuteInValid()
        {
            Signal signal = new Signal()
            {
                ControllerType = new ControllerType() { ControllerTypeId = 2 }
            };

            _output.WriteLine($"ControllerTypeId: {signal.ControllerType.ControllerTypeId}");

            var condition = _downloader.CanExecute(signal);

            Assert.False(condition);
        }

        [Fact]
        public async void ExecuteAsyncCanExecuteInvalid()
        {
            Signal signal = new Signal()
            {
                Ipaddress = "10.209.2.108",
                Enabled = true,
                PrimaryName = "Cobalt Test",
                SignalId = "9731",
                ControllerTypeId = 2,
                ControllerType = new ControllerType() { ControllerTypeId = 2 }
            };

            _output.WriteLine($"Signal: {signal}");

            await Assert.ThrowsAsync<ExecuteException>(async () => await _downloader.ExecuteAsync(signal));
        }

        [Fact]
        public async void ExecuteAsyncIpAddressInvalid()
        {
            Signal signal = new Signal()
            {
                //Ipaddress = "10.209.2.120",
                Ipaddress = "hello",
                Enabled = true,
                PrimaryName = "Maxtime Test",
                SignalId = "0",
                ControllerTypeId = 4,
                ControllerType = new ControllerType() { ControllerTypeId = 4 }
            };

            _output.WriteLine($"Signal: {signal}");

            await Assert.ThrowsAsync<FormatException>(async () => await _downloader.ExecuteAsync(signal));
        }






        #endregion

        public void Dispose()
        {
            if (_downloader is IDisposable d)
            {
                d.Dispose();
            }

            _output.WriteLine($"Disposing ISignalControllerDownloader Instance: {_downloader.GetHashCode()}");

            _downloader = null;
            _nullLogger = null;
            _nullOptions = null;
        }

        
    }
}
