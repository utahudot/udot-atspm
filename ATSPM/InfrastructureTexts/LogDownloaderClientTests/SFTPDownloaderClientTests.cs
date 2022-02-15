using ATSPM.Application.Configuration;
using ATSPM.Application.Enums;
using ATSPM.Application.Exceptions;
using ATSPM.Application.Models;
using ATSPM.Application.Services.SignalControllerProtocols;
using ATSPM.Domain.Extensions;
using ATSPM.Infrasturcture.Services.ControllerDecoders;
using ATSPM.Infrasturcture.Services.ControllerDownloaders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using SignalControllerLoggerTests.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace InfrastructureTests.LogDownloaderClientTests
{
    public class SFTPDownloaderClientTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private ISFTPDownloaderClient _client;

        public SFTPDownloaderClientTests(ITestOutputHelper output)
        {
            _output = output;
            _client = new SSHNetSFTPDownloader();

            _output.WriteLine($"Created Instance: {_client.GetHashCode()}");
        }

        #region ConnectAsync

        [Fact]
        public async Task ConnectAsyncSuccess()
        {
            var client = new Mock<ISftpClientWrapper>();
            client.Setup(s => s.Connect()).Callback(() => client.SetupGet(p => p.IsConnected).Returns(true)).Verifiable();

            if (_client is SSHNetSFTPDownloader sut)
            {
                sut.Client = client.Object;

                var credentials = new NetworkCredential("username", "password", "domain");

                await sut.ConnectAsync(credentials, 0);

                var condition = sut.IsConnected;

                _output.WriteLine($"condition: {condition}");

                client.Verify();

                Assert.True(condition);
            }
            else
            {
                Assert.False(true);
            }
        }

        [Fact]
        public async Task ConnectAsyncCredentialsSuccess()
        {
            if (_client is SSHNetSFTPDownloader sut)
            {
                var expected1 = "username";
                var expected2 = "password";
                var expected3 = "domain";
                var expected4 = TimeSpan.FromSeconds(1000);

                var credentials = new NetworkCredential(expected1, expected2, expected3);

                try
                {
                    await sut.ConnectAsync(credentials, (int)expected4.TotalSeconds);
                }
                catch (Exception)
                {
                    //connection will fail, just want to see if credentials are set correctly
                }

                var actual1 = sut.Client.ConnectionInfo.Username;
                var actual2 = sut.Client.ConnectionInfo.AuthenticationMethods[0].Username;
                var actual3 = sut.Client.ConnectionInfo.AuthenticationMethods[0].Name;
                var actual4 = sut.Client.ConnectionInfo.Host;
                var actual5 = sut.Client.ConnectionInfo.Timeout;

                Assert.Equal(expected1, actual1);
                Assert.Equal(expected1, actual2);
                Assert.Equal(expected2, actual3);
                Assert.Equal(expected3, actual4);
                Assert.Equal(expected4, actual5);
            }
        }

        [Theory]
        [InlineData(null, "password", "domain")]
        [InlineData("username", null, "domain")]
        [InlineData("username", "password", null)]
        public async Task ConnectAsyncCredentialsFail(string username, string password, string domain)
        {
            if (_client is SSHNetSFTPDownloader sut)
            {
                var credentials = new NetworkCredential(username, password, domain);

                await Assert.ThrowsAsync<ControllerConnectionException>(async () => await sut.ConnectAsync(credentials, 0));
            }
            else
            {
                Assert.False(true);
            }
        }

        [Fact]
        public async Task ConnectAsyncCancelledToken()
        {
            var tokenSource = new CancellationTokenSource();

            tokenSource.Cancel();

            if (_client is SSHNetSFTPDownloader sut)
            {
                var credentials = new NetworkCredential("username", "password", "domain");

                await Assert.ThrowsAsync<OperationCanceledException>(async () => await sut.ConnectAsync(credentials, 0, tokenSource.Token));
            }
            else
            {
                Assert.False(true);
            }
        }

        #endregion

        #region DownloadFileAsync

        [Fact]
        public async Task DownloadFileAsyncSuccess()
        {
            var expected = Path.GetTempFileName();
            var fileInfo = new FileInfo(expected);
            var remotePath = "remotepath";

            _output.WriteLine($"expected: {expected}");

            var client = new Mock<ISftpClientWrapper>();

            client.Setup(s => s.DownloadFileAsync(It.Is<string>(l => l == expected), It.Is<string>(r => r == remotePath))).ReturnsAsync(fileInfo).Verifiable();

            if (_client is SSHNetSFTPDownloader sut)
            {
                sut.Client = client.Object;

                var actual = await sut.DownloadFileAsync(expected, remotePath);

                client.Verify();

                _output.WriteLine($"actual: {actual}");

                Assert.Equal(expected, actual.FullName);
            }
            else
            {
                Assert.False(true);
            }
        }

        [Fact]
        public async Task DownloadFileAsyncIsConnectedFalse()
        {
            if (_client is SSHNetSFTPDownloader sut)
            {
                await Assert.ThrowsAsync<ControllerDownloadFileException>(async () => await sut.DownloadFileAsync(string.Empty, string.Empty));
            }
            else
            {
                Assert.False(true);
            }
        }

        [Fact]
        public async Task DownloadFileAsyncCancelledToken()
        {
            var tokenSource = new CancellationTokenSource();

            tokenSource.Cancel();

            if (_client is SSHNetSFTPDownloader sut)
            {
                await Assert.ThrowsAsync<OperationCanceledException>(async () => await sut.DownloadFileAsync(string.Empty, string.Empty, tokenSource.Token));
            }
            else
            {
                Assert.False(true);
            }
        }

        #endregion






        public void Dispose()
        {
            if (_client is IDisposable d)
            {
                d.Dispose();
            }

            _output.WriteLine($"Disposing Instance: {_client.GetHashCode()}");

            _client = null;
        }
    }
}
