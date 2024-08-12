﻿#region license
// Copyright 2024 Utah Departement of Transportation
// for InfrastructureTests - InfrastructureTests.DownloaderClientTests/FluentFTPDownloaderClientTests.cs
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

using FluentFTP;
using Moq;
using System;
using Utah.Udot.Atspm.Infrastructure.Services.DownloaderClients;
using Xunit.Abstractions;

namespace Utah.Udot.Atspm.InfrastructureTests.DownloaderClientTests
{
    public class FluentFTPDownloaderClientTests : DownloaderClientTestsBase<IAsyncFtpClient>
    {
        public FluentFTPDownloaderClientTests(ITestOutputHelper output) : base(output) { }

        public override void ConnectAsyncSucceeded()
        {
            var client = new Mock<IAsyncFtpClient>();
            var config = new Mock<FtpConfig>();

            client.SetupAllProperties();

            client.Setup(s => s.AutoConnect(default)).Callback(() => client.SetupGet(p => p.IsConnected).Returns(true));

            Client = client.Object;
            Client.Config = config.Object;

            Sut = new FluentFTPDownloaderClient(Client);

            base.ConnectAsyncSucceeded();
        }

        public override void ConnectAsyncControllerConnectionException()
        {
            var client = new Mock<IAsyncFtpClient>();

            client.Setup(s => s.AutoConnect(default)).Throws<Exception>();

            Sut = new FluentFTPDownloaderClient(client.Object);

            base.ConnectAsyncControllerConnectionException();
        }

        public override void ConnectAsyncOperationCanceledException()
        {
            Sut = new FluentFTPDownloaderClient();

            base.ConnectAsyncOperationCanceledException();
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

        public override void DeleteFileAsyncOperationCanceledException()
        {
            Sut = new FluentFTPDownloaderClient();

            base.DeleteFileAsyncOperationCanceledException();
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

        public override void DisconnectAsyncOperationCanceledException()
        {
            Sut = new FluentFTPDownloaderClient();

            base.DisconnectAsyncOperationCanceledException();
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

        public override void DownloadFileAsyncOperationCanceledException()
        {
            Sut = new FluentFTPDownloaderClient();

            base.DownloadFileAsyncOperationCanceledException();
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

        public override void ListDirectoryAsyncOperationCanceledException()
        {
            Sut = new FluentFTPDownloaderClient();

            base.ListDirectoryAsyncOperationCanceledException();
        }

        public override void ConnectAsyncConnectionProperties()
        {
            var client = new Mock<IAsyncFtpClient>();
            var config = new Mock<FtpConfig>();

            client.SetupAllProperties();

            client.Setup(s => s.AutoConnect(default)).Callback(() => client.SetupGet(p => p.IsConnected).Returns(true));

            Client = client.Object;
            Client.Config = config.Object;
            Sut = new FluentFTPDownloaderClient(Client);

            base.ConnectAsyncConnectionProperties();
        }

        public override bool VerifyIpAddress(IAsyncFtpClient client, string ipAddress)
        {
            return client.Host == ipAddress.ToString();
        }

        public override bool VerifyPort(IAsyncFtpClient client, int port)
        {
            return client.Port == port;
        }

        public override bool VerifyUserName(IAsyncFtpClient client, string userName)
        {
            return client.Credentials.UserName == userName;
        }

        public override bool VerifyPassword(IAsyncFtpClient client, string password)
        {
            return client.Credentials.Password == password;
        }

        public override bool VerifyConnectionTimeout(IAsyncFtpClient client, int connectionTimeout)
        {
            return client.Config.ConnectTimeout == connectionTimeout;
        }

        public override bool VerifyOperationTimeout(IAsyncFtpClient client, int operationTImeout)
        {
            return client.Config.ReadTimeout == operationTImeout;
        }
    }
}
