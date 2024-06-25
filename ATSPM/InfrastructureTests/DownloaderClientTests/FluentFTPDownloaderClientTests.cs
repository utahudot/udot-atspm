#region license
// Copyright 2024 Utah Departement of Transportation
// for InfrastructureTests - InfrastructureTests.LogDownloaderClientTests/FluentFTPDownloaderClientTests.cs
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
using FluentFTP;
using Moq;
using System;
using Xunit.Abstractions;

namespace InfrastructureTests.DownloaderClientTests
{
    public class FluentFTPDownloaderClientTests : DownloaderClientTestsBase
    {
        public FluentFTPDownloaderClientTests(ITestOutputHelper output) : base(output) { }

        public override void ConnectAsyncSucceeded()
        {
            var client = new Mock<IAsyncFtpClient>();

            client.Setup(s => s.AutoConnect(default)).Callback(() => client.SetupGet(p => p.IsConnected).Returns(true));

            Sut = new FluentFTPDownloaderClient(client.Object);

            base.ConnectAsyncSucceeded();
        }

        public override void ConnectAsyncArgumentNullException()
        {
            Sut = new FluentFTPDownloaderClient();

            base.ConnectAsyncArgumentNullException();
        }

        public override void ConnectAsyncControllerConnectionException()
        {
            var client = new Mock<IAsyncFtpClient>();

            client.Setup(s => s.AutoConnect(default)).Throws<Exception>();

            Sut = new FluentFTPDownloaderClient(client.Object);

            base.ConnectAsyncControllerConnectionException();
        }

        public override void DeleteFileAsyncSucceeded()
        {
            var client = new Mock<IAsyncFtpClient>();

            client.SetupGet(p => p.IsConnected).Returns(true);

            Sut = new FluentFTPDownloaderClient(client.Object);

            base.DeleteFileAsyncSucceeded();
        }

        public override void DeleteFileAsyncNotConnected()
        {
            var client = new Mock<IAsyncFtpClient>();

            Sut = new FluentFTPDownloaderClient(client.Object);

            base.DeleteFileAsyncNotConnected();
        }

        public override void DeleteFileAsyncControllerDeleteFileException()
        {
            var client = new Mock<IAsyncFtpClient>();

            client.SetupGet(p => p.IsConnected).Returns(true);

            client.Setup(s => s.DeleteFile("", default)).Throws<Exception>();

            Sut = new FluentFTPDownloaderClient(client.Object);

            base.DeleteFileAsyncControllerDeleteFileException();
        }

        public override void DisconnectAsyncSucceeded()
        {
            var client = new Mock<IAsyncFtpClient>();

            client.SetupGet(p => p.IsConnected).Returns(true);

            client.Setup(s => s.Disconnect(default)).Callback(() => client.SetupGet(p => p.IsConnected).Returns(false));

            Sut = new FluentFTPDownloaderClient(client.Object);

            base.DisconnectAsyncSucceeded();
        }

        public override void DisconnectAsyncNotConnected()
        {
            var client = new Mock<IAsyncFtpClient>();

            Sut = new FluentFTPDownloaderClient(client.Object);

            base.DisconnectAsyncNotConnected();
        }

        public override void DisconnectAsyncControllerConnectionException()
        {
            var client = new Mock<IAsyncFtpClient>();

            client.Setup(p => p.IsConnected).Returns(true);

            client.Setup(s => s.Disconnect(default)).Throws<Exception>();

            Sut = new FluentFTPDownloaderClient(client.Object);

            base.DisconnectAsyncControllerConnectionException();
        }

        public override void DownloadFileAsyncSucceeded()
        {
            var client = new Mock<IAsyncFtpClient>();

            client.SetupGet(p => p.IsConnected).Returns(true);

            Sut = new FluentFTPDownloaderClient(client.Object);

            base.DownloadFileAsyncSucceeded();
        }

        public override void DownloadFileAsyncNotConnected()
        {
            var client = new Mock<IAsyncFtpClient>();

            Sut = new FluentFTPDownloaderClient(client.Object);

            base.DownloadFileAsyncNotConnected();
        }

        public override void DownloadFileAsyncControllerDownloadFileException()
        {
            var client = new Mock<IAsyncFtpClient>();

            client.Setup(p => p.IsConnected).Returns(true);

            client.Setup(s => s.DownloadFile(It.IsAny<string>(), "", FtpLocalExists.Overwrite, FtpVerify.None, null, default)).Throws<Exception>();

            Sut = new FluentFTPDownloaderClient(client.Object);

            base.DownloadFileAsyncControllerDownloadFileException();
        }

        public override void ListDirectoryAsyncSucceeded()
        {
            var client = new Mock<IAsyncFtpClient>();

            client.SetupGet(p => p.IsConnected).Returns(true);

            Sut = new FluentFTPDownloaderClient(client.Object);

            base.ListDirectoryAsyncSucceeded();
        }

        public override void ListDirectoryAsyncNotConnected()
        {
            var client = new Mock<IAsyncFtpClient>();

            Sut = new FluentFTPDownloaderClient(client.Object);

            base.ListDirectoryAsyncNotConnected();
        }

        public override void ListDirectoryAsyncControllerDownloadFileException()
        {
            var client = new Mock<IAsyncFtpClient>();

            client.Setup(p => p.IsConnected).Returns(true);

            client.Setup(s => s.GetListing(It.IsAny<string>(), FtpListOption.Auto, default)).Throws<Exception>();

            Sut = new FluentFTPDownloaderClient(client.Object);

            base.ListDirectoryAsyncControllerDownloadFileException();
        }
    }
}
