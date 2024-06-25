#region license
// Copyright 2024 Utah Departement of Transportation
// for InfrastructureTests - InfrastructureTests.LogDownloaderClientTests/SSHNetSFTPDownloaderClientTests.cs
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
using ATSPM.Infrastructure.Services.DownloaderClients;
using Moq;
using System;
using System.IO;
using Xunit.Abstractions;

namespace InfrastructureTests.DownloaderClientTests
{
    public class SSHNetSFTPDownloaderClientTests : DownloaderClientTestsBase
    {
        public SSHNetSFTPDownloaderClientTests(ITestOutputHelper output) : base(output) { }

        public override void ConnectAsyncSucceeded()
        {
            var client = new Mock<ISftpClientWrapper>();

            client.Setup(s => s.Connect()).Callback(() => client.SetupGet(p => p.IsConnected).Returns(true));

            Sut = new SSHNetSFTPDownloaderClient(client.Object);

            base.ConnectAsyncSucceeded();
        }

        public override void ConnectAsyncArgumentNullException()
        {
            Sut = new SSHNetSFTPDownloaderClient();

            base.ConnectAsyncArgumentNullException();
        }

        public override void ConnectAsyncControllerConnectionException()
        {
            var client = new Mock<ISftpClientWrapper>();

            client.Setup(s => s.Connect()).Throws<Exception>();

            Sut = new SSHNetSFTPDownloaderClient(client.Object);

            base.ConnectAsyncControllerConnectionException();
        }

        public override void DeleteFileAsyncSucceeded()
        {
            var client = new Mock<ISftpClientWrapper>();

            client.SetupGet(p => p.IsConnected).Returns(true);

            Sut = new SSHNetSFTPDownloaderClient(client.Object);

            base.DeleteFileAsyncSucceeded();
        }

        public override void DeleteFileAsyncNotConnected()
        {
            var client = new Mock<ISftpClientWrapper>();

            Sut = new SSHNetSFTPDownloaderClient(client.Object);

            base.DeleteFileAsyncNotConnected();
        }

        public override void DeleteFileAsyncControllerDeleteFileException()
        {
            var client = new Mock<ISftpClientWrapper>();

            client.SetupGet(p => p.IsConnected).Returns(true);

            client.Setup(s => s.DeleteFile("")).Throws<Exception>();

            Sut = new SSHNetSFTPDownloaderClient(client.Object);

            base.DeleteFileAsyncControllerDeleteFileException();
        }

        public override void DisconnectAsyncSucceeded()
        {
            var client = new Mock<ISftpClientWrapper>();

            client.SetupGet(p => p.IsConnected).Returns(true);

            client.Setup(s => s.Disconnect()).Callback(() => client.SetupGet(p => p.IsConnected).Returns(false));

            Sut = new SSHNetSFTPDownloaderClient(client.Object);

            base.DisconnectAsyncSucceeded();
        }

        public override void DisconnectAsyncNotConnected()
        {
            var client = new Mock<ISftpClientWrapper>();

            Sut = new SSHNetSFTPDownloaderClient(client.Object);

            base.DisconnectAsyncNotConnected();
        }

        public override void DisconnectAsyncControllerConnectionException()
        {
            var client = new Mock<ISftpClientWrapper>();

            client.Setup(p => p.IsConnected).Returns(true);

            client.SetupAllProperties();

            client.Setup(s => s.Disconnect()).Throws<Exception>();

            Sut = new SSHNetSFTPDownloaderClient(client.Object);

            base.DisconnectAsyncControllerConnectionException();
        }

        public override void DownloadFileAsyncSucceeded()
        {
            var client = new Mock<ISftpClientWrapper>();

            client.SetupGet(p => p.IsConnected).Returns(true);

            client.Setup(s => s.DownloadFileAsync(It.IsAny<string>(), "")).ReturnsAsync(new FileInfo(Path.GetTempFileName()));

            Sut = new SSHNetSFTPDownloaderClient(client.Object);

            base.DownloadFileAsyncSucceeded();
        }

        public override void DownloadFileAsyncNotConnected()
        {
            var client = new Mock<ISftpClientWrapper>();

            Sut = new SSHNetSFTPDownloaderClient(client.Object);

            base.DownloadFileAsyncNotConnected();
        }

        public override void DownloadFileAsyncControllerDownloadFileException()
        {
            var client = new Mock<ISftpClientWrapper>();

            client.Setup(p => p.IsConnected).Returns(true);

            client.Setup(s => s.DownloadFileAsync(It.IsAny<string>(), "")).Throws<Exception>();

            Sut = new SSHNetSFTPDownloaderClient(client.Object);

            base.DownloadFileAsyncControllerDownloadFileException();
        }

        public override void ListDirectoryAsyncSucceeded()
        {
            var client = new Mock<ISftpClientWrapper>();

            client.SetupGet(p => p.IsConnected).Returns(true);

            Sut = new SSHNetSFTPDownloaderClient(client.Object);

            base.ListDirectoryAsyncSucceeded();
        }

        public override void ListDirectoryAsyncNotConnected()
        {
            var client = new Mock<ISftpClientWrapper>();

            Sut = new SSHNetSFTPDownloaderClient(client.Object);

            base.ListDirectoryAsyncNotConnected();
        }

        public override void ListDirectoryAsyncControllerDownloadFileException()
        {
            var client = new Mock<ISftpClientWrapper>();

            client.Setup(p => p.IsConnected).Returns(true);

            client.Setup(s => s.ListDirectoryAsync(It.IsAny<string>(), It.IsAny<string[]>())).Throws<Exception>();

            Sut = new SSHNetSFTPDownloaderClient(client.Object);

            base.ListDirectoryAsyncControllerDownloadFileException();
        }
    }
}
