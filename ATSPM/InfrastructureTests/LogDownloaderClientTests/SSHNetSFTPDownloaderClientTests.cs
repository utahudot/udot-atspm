using ATSPM.Application.Exceptions;
using ATSPM.Application.Services;
using ATSPM.Infrastructure.Services.ControllerDownloaders;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace InfrastructureTests.LogDownloaderClientTests
{
    public class SSHNetSFTPDownloaderClientTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private ISFTPDownloaderClient _client;

        private const string ClientNotConnectedMessage = "Client not connected";

        public SSHNetSFTPDownloaderClientTests(ITestOutputHelper output)
        {
            _output = output;
            _client = new SSHNetSFTPDownloaderClient();

            //var s = new ServiceCollection();
            //s.AddTransient<IDeviceDownloader, StubLocationControllerDownloader>();



            _output.WriteLine($"Created Instance: {_client.GetHashCode()}");
        }

        #region ConnectAsync

        [Fact]
        [Trait(nameof(IDownloaderClient), "ConnectAsync")]
        public async void ConnectAsyncSuccess()
        {
            var client = new Mock<ISftpClientWrapper>();
            client.Setup(s => s.Connect()).Callback(() => client.SetupGet(p => p.IsConnected).Returns(true)).Verifiable();

            if (_client is SSHNetSFTPDownloaderClient sut)
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
        [Trait(nameof(IDownloaderClient), "ConnectAsync")]
        public async void ConnectAsyncFailed()
        {
            var client = new Mock<ISftpClientWrapper>();
            client.Setup(s => s.Connect()).Throws<Exception>();

            if (_client is SSHNetSFTPDownloaderClient sut)
            {
                sut.Client = client.Object;

                var credentials = new NetworkCredential("username", "password", "domain");

                await Assert.ThrowsAsync<ControllerConnectionException>(async () => await sut.ConnectAsync(credentials, 0, 0));
            }
            else
            {
                Assert.False(true);
            }
        }

        [Fact]
        [Trait(nameof(IDownloaderClient), "ConnectAsync")]
        public async void ConnectAsyncCredentialsSuccess()
        {
            if (_client is SSHNetSFTPDownloaderClient sut)
            {
                var expected1 = "username";
                var expected2 = "password";
                var expected3 = "127.0.0.1";
                var expected4 = TimeSpan.FromSeconds(2);
                var expected5 = TimeSpan.FromSeconds(4);

                var credentials = new NetworkCredential(expected1, expected2, expected3);

                try
                {
                    await sut.ConnectAsync(credentials, (int)expected4.TotalSeconds, (int)expected5.TotalSeconds);
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
                var actual6 = sut.Client.OperationTimeout;

                Assert.Equal(expected1, actual1);
                Assert.Equal(expected1, actual2);
                Assert.Equal(expected2, actual3);
                Assert.Equal(expected3, actual4);
                Assert.Equal(expected4, actual5);
                Assert.Equal(expected5, actual6);
            }
        }

        [Theory]
        [Trait(nameof(IDownloaderClient), "ConnectAsync")]
        [InlineData(null, "password", "domain")]
        [InlineData("username", null, "domain")]
        [InlineData("username", "password", null)]
        public async void ConnectAsyncCredentialsFail(string username, string password, string domain)
        {
            if (_client is SSHNetSFTPDownloaderClient sut)
            {
                var credentials = new NetworkCredential(username, password, domain);

                await Assert.ThrowsAsync<ControllerConnectionException>(async () => await sut.ConnectAsync(credentials, 0, 0));
            }
            else
            {
                Assert.False(true);
            }
        }

        [Fact]
        [Trait(nameof(IDownloaderClient), "ConnectAsync")]
        public async void ConnectAsyncCancelledToken()
        {
            var tokenSource = new CancellationTokenSource();

            tokenSource.Cancel();

            if (_client is SSHNetSFTPDownloaderClient sut)
            {
                var credentials = new NetworkCredential("username", "password", "domain");

                await Assert.ThrowsAsync<OperationCanceledException>(async () => await sut.ConnectAsync(credentials, 0, 0, tokenSource.Token));
            }
            else
            {
                Assert.False(true);
            }
        }

        #endregion

        #region DeleteFileAsync

        [Fact]
        [Trait(nameof(IDownloaderClient), "DeleteFileAsync")]
        public async void DeleteFileAsyncSuccess()
        {
            var expected = "path";

            _output.WriteLine($"expected: {expected}");

            var client = new Mock<ISftpClientWrapper>();
            client.SetupGet(s => s.IsConnected).Returns(true);
            client.Setup(s => s.DeleteFile(It.Is<string>(r => r == expected))).Verifiable();

            if (_client is SSHNetSFTPDownloaderClient sut)
            {
                sut.Client = client.Object;

                await sut.DeleteFileAsync(expected);

                client.Verify();
            }
            else
            {
                Assert.False(true);
            }
        }

        [Fact]
        [Trait(nameof(IDownloaderClient), "DeleteFileAsync")]
        public async void DeleteFileAsyncFailed()
        {
            var client = new Mock<ISftpClientWrapper>();
            client.SetupGet(s => s.IsConnected).Returns(true);
            client.Setup(s => s.DeleteFile(It.IsNotNull<string>())).Throws<ArgumentNullException>();

            if (_client is SSHNetSFTPDownloaderClient sut)
            {
                sut.Client = client.Object;

                await Assert.ThrowsAsync<ControllerDeleteFileException>(async () => await sut.DeleteFileAsync(string.Empty));
            }
            else
            {
                Assert.False(true);
            }
        }

        [Fact]
        [Trait(nameof(IDownloaderClient), "DeleteFileAsync")]
        public async void DeleteFileAsyncNotConnected()
        {
            if (_client is SSHNetSFTPDownloaderClient sut)
            {
                var exception = await Assert.ThrowsAsync<ControllerConnectionException>(async () => await sut.DeleteFileAsync(string.Empty));

                var expected = ClientNotConnectedMessage;
                var actual = exception.Message;

                Assert.Equal(expected, actual);
            }
            else
            {
                Assert.False(true);
            }
        }

        [Fact]
        [Trait(nameof(IDownloaderClient), "DeleteFileAsync")]
        public async void DeleteFileAsyncCancelledToken()
        {
            var tokenSource = new CancellationTokenSource();

            tokenSource.Cancel();

            if (_client is SSHNetSFTPDownloaderClient sut)
            {
                await Assert.ThrowsAsync<OperationCanceledException>(async () => await sut.DeleteFileAsync(string.Empty, tokenSource.Token));
            }
            else
            {
                Assert.False(true);
            }
        }

        #endregion

        #region DownloadFileAsync

        [Fact]
        [Trait(nameof(IDownloaderClient), "DownloadFileAsync")]
        public async void DownloadFileAsyncSuccess()
        {
            var expected = Path.GetTempFileName();
            var fileInfo = new FileInfo(expected);
            var remotePath = "remotepath";

            _output.WriteLine($"expected: {expected}");

            var client = new Mock<ISftpClientWrapper>();
            client.SetupGet(s => s.IsConnected).Returns(true);

            client.Setup(s => s.DownloadFileAsync(It.Is<string>(l => l == expected), It.Is<string>(r => r == remotePath))).ReturnsAsync(fileInfo).Verifiable();

            if (_client is SSHNetSFTPDownloaderClient sut)
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
        [Trait(nameof(IDownloaderClient), "DownloadFileAsync")]
        public async void DownloadFileAsyncFailed()
        {
            var client = new Mock<ISftpClientWrapper>();
            client.SetupGet(s => s.IsConnected).Returns(true);
            client.Setup(s => s.DownloadFileAsync(It.IsNotNull<string>(), It.IsNotNull<string>())).Throws<ArgumentNullException>();

            if (_client is SSHNetSFTPDownloaderClient sut)
            {
                sut.Client = client.Object;
                
                await Assert.ThrowsAsync<ControllerDownloadFileException>(async () => await sut.DownloadFileAsync(string.Empty, string.Empty));
            }
            else
            {
                Assert.False(true);
            }
        }

        [Fact]
        [Trait(nameof(IDownloaderClient), "DownloadFileAsync")]
        public async void DownloadFileAsyncNotConnected()
        {
            if (_client is SSHNetSFTPDownloaderClient sut)
            {
                var exception = await Assert.ThrowsAsync<ControllerConnectionException>(async () => await sut.DownloadFileAsync(string.Empty, string.Empty));

                var expected = ClientNotConnectedMessage;
                var actual = exception.Message;

                Assert.Equal(expected, actual);
            }
            else
            {
                Assert.False(true);
            }
        }

        [Fact]
        [Trait(nameof(IDownloaderClient), "DownloadFileAsync")]
        public async void DownloadFileAsyncCancelledToken()
        {
            var tokenSource = new CancellationTokenSource();

            tokenSource.Cancel();

            if (_client is SSHNetSFTPDownloaderClient sut)
            {
                await Assert.ThrowsAsync<OperationCanceledException>(async () => await sut.DownloadFileAsync(string.Empty, string.Empty, tokenSource.Token));
            }
            else
            {
                Assert.False(true);
            }
        }

        #endregion

        #region ListDirectoryAsync

        [Fact]
        [Trait(nameof(IDownloaderClient), "ListDirectoryAsync")]
        public async void ListDirectoryAsyncSuccess()
        {
            var directory = "directory";

            List<string> directoryFiles = new List<string>(new string[] { "a", "b", "c", "d" });
            List<string> returnFiles = new List<string>(new string[] { "a", "b" });
            string[] filters = { "a", "b"};

            var client = new Mock<ISftpClientWrapper>();
            client.SetupGet(s => s.IsConnected).Returns(true);
            client.Setup(s => s.ListDirectoryAsync(It.Is<string>(r => r == directory), It.Is<string[]>(f => f == filters)))
                .ReturnsAsync(returnFiles)
                .Verifiable();

            if (_client is SSHNetSFTPDownloaderClient sut)
            {
                sut.Client = client.Object;

                var actual = await sut.ListDirectoryAsync(directory, default, filters);
                var expected = returnFiles;

                client.Verify();

                Assert.Equal(expected, actual);
            }
            else
            {
                Assert.False(true);
            }
        }

        [Fact]
        [Trait(nameof(IDownloaderClient), "ListDirectoryAsync")]
        public async void ListDirectoryAsyncNotConnected()
        {
            if (_client is SSHNetSFTPDownloaderClient sut)
            {
                var exception = await Assert.ThrowsAsync<ControllerConnectionException>(async () => await sut.ListDirectoryAsync(string.Empty));

                var expected = ClientNotConnectedMessage;
                var actual = exception.Message;

                Assert.Equal(expected, actual);
            }
            else
            {
                Assert.False(true);
            }
        }

        [Fact]
        [Trait(nameof(IDownloaderClient), "ListDirectoryAsync")]
        public async void ListDirectoryAsyncCancelledToken()
        {
            var tokenSource = new CancellationTokenSource();

            tokenSource.Cancel();

            if (_client is SSHNetSFTPDownloaderClient sut)
            {
                await Assert.ThrowsAsync<OperationCanceledException>(async () => await sut.ListDirectoryAsync(string.Empty, tokenSource.Token, null));
            }
            else
            {
                Assert.False(true);
            }
        }

        #endregion

        #region DisconnectAsync

        [Fact]
        [Trait(nameof(IDownloaderClient), "DisconnectAsync")]
        public async void DisconnectAsyncSuccess()
        {
            var client = new Mock<ISftpClientWrapper>();
            client.SetupGet(s => s.IsConnected).Returns(true);
            client.Setup(s => s.Disconnect()).Callback(() => client.SetupGet(p => p.IsConnected).Returns(false)).Verifiable();

            if (_client is SSHNetSFTPDownloaderClient sut)
            {
                sut.Client = client.Object;

                await sut.DisconnectAsync();

                var condition = sut.IsConnected;

                _output.WriteLine($"condition: {condition}");

                client.Verify();

                Assert.False(condition);
            }
            else
            {
                Assert.False(true);
            }
        }

        [Fact]
        [Trait(nameof(IDownloaderClient), "DisconnectAsync")]
        public async void DisconnectAsyncFailed()
        {
            var client = new Mock<ISftpClientWrapper>();
            client.Setup(s => s.Disconnect()).Throws<Exception>();

            if (_client is SSHNetSFTPDownloaderClient sut)
            {
                sut.Client = client.Object;

                await Assert.ThrowsAsync<ControllerConnectionException>(async () => await sut.DisconnectAsync());
            }
            else
            {
                Assert.False(true);
            }
        }

        [Fact]
        [Trait(nameof(IDownloaderClient), "DisconnectAsync")]
        public async void DisconnectAsyncNotConnected()
        {
            if (_client is SSHNetSFTPDownloaderClient sut)
            {
                var exception = await Assert.ThrowsAsync<ControllerConnectionException>(async () => await sut.DisconnectAsync());

                var expected = ClientNotConnectedMessage;
                var actual = exception.Message;

                Assert.Equal(expected, actual);
            }
            else
            {
                Assert.False(true);
            }
        }

        [Fact]
        [Trait(nameof(IDownloaderClient), "DisconnectAsync")]
        public async void DisconnectAsyncCancelledToken()
        {
            var tokenSource = new CancellationTokenSource();

            tokenSource.Cancel();

            if (_client is SSHNetSFTPDownloaderClient sut)
            {
                await Assert.ThrowsAsync<OperationCanceledException>(async () => await sut.DisconnectAsync(tokenSource.Token));
            }
            else
            {
                Assert.False(true);
            }
        }

        #endregion

        [Fact]
        [Trait(nameof(IDownloaderClient), nameof(IDisposable))]
        public void IsDisposing()
        {
            var client = new Mock<ISftpClientWrapper>();
            client.SetupGet(p => p.IsConnected).Returns(true);
            client.Setup(s => s.Disconnect()).Callback(() => client.SetupGet(p => p.IsConnected).Returns(false)).Verifiable();

            if (_client is SSHNetSFTPDownloaderClient sut)
            {
                sut.Client = client.Object;

                sut.Dispose();

                client.Verify(v => v.Dispose());
                client.Verify();

                Assert.False(sut.IsConnected);
                Assert.Null(sut.Client);
            }
            else
            {
                Assert.False(true);
            }
        }

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
