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

namespace SignalControllerLoggerTests
{
    public class ISignalControllerDownloaderTests : IDisposable
    {
        private const string TestDataPath = "C:\\Projects\\udot-atsmp\\ATSPM\\InfrastructureTexts\\TestData";

        private readonly ITestOutputHelper _output;
        private ISignalControllerDownloader _downloader;
        private ILogger<ASCSignalControllerDownloader> _nullLogger;
        private IOptions<SignalControllerDownloaderConfiguration> _nullOptions;

        public ISignalControllerDownloaderTests(ITestOutputHelper output)
        {
            _output = output;
            _nullLogger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<ASCSignalControllerDownloader>();
            _nullOptions = Options.Create(new SignalControllerDownloaderConfiguration() { EarliestAcceptableDate = new DateTime() });
            _downloader = new ASCSignalControllerDownloader(_nullLogger, new ServiceCollection().BuildServiceProvider(), _nullOptions);

            _output.WriteLine($"Created ISignalControllerDownloader Instance: {_downloader.GetHashCode()}");
        }

        #region ISignalControllerDownloader

        [Theory]
        [InlineData(SignalControllerType.Unknown)]
        [InlineData(SignalControllerType.ASC3)]
        [InlineData(SignalControllerType.Cobalt)]
        [InlineData(SignalControllerType.ASC32070)]
        [InlineData(SignalControllerType.MaxTime)]
        [InlineData(SignalControllerType.Trafficware)]
        [InlineData(SignalControllerType.SiemensSEPAC)]
        [InlineData(SignalControllerType.McCainATCEX)]
        [InlineData(SignalControllerType.Peek)]
        [InlineData(SignalControllerType.EOS)]
        public void SignalControllerTypeValid(SignalControllerType type)
        {
            _output.WriteLine($"ControllerType: {_downloader.ControllerType}");
            _output.WriteLine($"TestType: {type}");

            var expected = type;
            var actual = _downloader.ControllerType & type;

            Assert.Equal(expected, actual);
        }

        #endregion

        #region IExecuteAsyncWithProgress

        #region IExecuteAsyncWithProgress.CanExecute

        [Fact]
        public async Task CanExecuteSucceedAsync()
        {
            //arrange
            var mockResults = new List<FtpResult>() { new FtpResult() { Exception = null, IsDownload = true, IsFailed = false, IsSkipped = false, IsSuccess = true } };
            var cts = new CancellationTokenSource();

            var sut = new Mock<IFtpClient>();
            sut.Setup(s => s.ConnectAsync(It.IsAny<CancellationToken>()));
            sut.Setup(s => s.DownloadDirectoryAsync(It.IsAny<string>(), It.IsAny<string>(), FtpFolderSyncMode.Update, FtpLocalExists.Skip, FtpVerify.None, null, null, cts.Token).Result).Returns(() => mockResults);

            IFtpClient client = sut.Object;

            //act
            //await client.ConnectAsync(cts.Token);
            var results = await client.DownloadDirectoryAsync("C:\\", "C:\\", FtpFolderSyncMode.Update, FtpLocalExists.Skip, FtpVerify.None, null, null, cts.Token);

            //assert
            Assert.True(results.Count > 0);
        }

        #endregion

        #region IExecuteAsyncWithProgress.ExecuteAsync

        #endregion

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
