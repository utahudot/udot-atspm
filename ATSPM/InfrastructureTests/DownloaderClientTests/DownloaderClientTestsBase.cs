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
using ATSPM.Application.Exceptions;
using ATSPM.Application.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace InfrastructureTests.DownloaderClientTests
{
    public abstract class DownloaderClientTestsBase<T> : IDisposable where T : class
    {
        protected ITestOutputHelper Output;
        protected IDownloaderClient Sut;
        protected T Client;

        public DownloaderClientTestsBase(ITestOutputHelper output)
        {
            Output = output;
        }

        [Fact]
        [Trait(nameof(IDownloaderClient), "ConnectAsync")]
        public async virtual void ConnectAsyncSucceeded()
        {
            var connection = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1);
            var credentials = new NetworkCredential("user", "password", "127.0.0.1");

            await Sut.ConnectAsync(connection, credentials, 2000, 2000);

            var condition = Sut.IsConnected;

            Assert.True(condition);
        }

        [Fact]
        [Trait(nameof(IDownloaderClient), "ConnectAsync")]
        public async virtual void ConnectAsyncControllerConnectionException()
        {
            var connection = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1);
            var credentials = new NetworkCredential("user", "password", "127.0.0.1");

            await Assert.ThrowsAsync<ControllerConnectionException>(async () => await Sut.ConnectAsync(connection, credentials, 2000, 2000));
        }

        [Fact]
        [Trait(nameof(IDownloaderClient), "DeleteFileAsync")]
        public async virtual void DeleteFileAsyncSucceeded()
        {
            await Sut.DeleteFileAsync("");
        }

        [Fact]
        [Trait(nameof(IDownloaderClient), "DeleteFileAsync")]
        public async virtual void DeleteFileAsyncNotConnected()
        {
            await Assert.ThrowsAsync<ControllerConnectionException>(async () => await Sut.DeleteFileAsync(""));
        }

        [Fact]
        [Trait(nameof(IDownloaderClient), "DeleteFileAsync")]
        public async virtual void DeleteFileAsyncControllerDeleteFileException()
        {
            await Assert.ThrowsAsync<ControllerDeleteFileException>(async () => await Sut.DeleteFileAsync(""));
        }

        [Fact]
        [Trait(nameof(IDownloaderClient), "DisconnectAsync")]
        public async virtual void DisconnectAsyncSucceeded()
        {
            await Sut.DisconnectAsync();

            var condition = Sut.IsConnected;

            Assert.False(condition);
        }

        [Fact]
        [Trait(nameof(IDownloaderClient), "DisconnectAsync")]
        public async virtual void DisconnectAsyncNotConnected()
        {
            await Assert.ThrowsAsync<ControllerConnectionException>(async () => await Sut.DisconnectAsync());
        }

        [Fact]
        [Trait(nameof(IDownloaderClient), "DisconnectAsync")]
        public async virtual void DisconnectAsyncControllerConnectionException()
        {
            await Assert.ThrowsAsync<ControllerConnectionException>(async () => await Sut.DisconnectAsync());
        }

        [Fact]
        [Trait(nameof(IDownloaderClient), "DownloadFileAsync")]
        public async virtual void DownloadFileAsyncSucceeded()
        {
            var condition = await Sut.DownloadFileAsync(Path.GetTempFileName(), "");

            Assert.True(condition is FileInfo);
        }

        [Fact]
        [Trait(nameof(IDownloaderClient), "DownloadFileAsync")]
        public async virtual void DownloadFileAsyncNotConnected()
        {
            await Assert.ThrowsAsync<ControllerConnectionException>(async () => await Sut.DownloadFileAsync("", ""));
        }

        [Fact]
        [Trait(nameof(IDownloaderClient), "DownloadFileAsync")]
        public async virtual void DownloadFileAsyncControllerDownloadFileException()
        {
            await Assert.ThrowsAsync<ControllerDownloadFileException>(async () => await Sut.DownloadFileAsync(Path.GetTempFileName(), ""));
        }

        [Fact]
        [Trait(nameof(IDownloaderClient), "ListDirectoryAsync")]
        public async virtual void ListDirectoryAsyncSucceeded()
        {
            var condition = await Sut.ListDirectoryAsync("");

            Assert.True(condition is IEnumerable<string>);
        }

        [Fact]
        [Trait(nameof(IDownloaderClient), "ListDirectoryAsync")]
        public async virtual void ListDirectoryAsyncNotConnected()
        {
            await Assert.ThrowsAsync<ControllerConnectionException>(async () => await Sut.ListDirectoryAsync(""));
        }

        [Fact]
        [Trait(nameof(IDownloaderClient), "ListDirectoryAsync")]
        public async virtual void ListDirectoryAsyncControllerDownloadFileException()
        {
            await Assert.ThrowsAsync<ControllerListDirectoryException>(async () => await Sut.ListDirectoryAsync(""));
        }

        [Fact]
        [Trait(nameof(IDownloaderClient), "ConnectAsync")]
        public async virtual void ConnectAsyncConnectionProperties()
        {
            var ipAddress = "127.0.0.1";
            var port = Random.Shared.Next(1, 10);
            var userName = new Guid().ToString();
            var password = new Guid().ToString();
            var connectionTimeout = Random.Shared.Next(1000, 10000);
            var operationTImeout = Random.Shared.Next(1000, 10000);

            var connection = new IPEndPoint(IPAddress.Parse(ipAddress), port);
            var credentials = new NetworkCredential(userName, password, ipAddress);

            await Sut.ConnectAsync(connection, credentials, connectionTimeout, operationTImeout);

            Assert.True(VerifyIpAddress(Client, ipAddress));
            Assert.True(VerifyPort(Client, port));
            Assert.True(VerifyUserName(Client, userName));
            Assert.True(VerifyPassword(Client, password));
            Assert.True(VerifyConnectionTimeout(Client, connectionTimeout));
            Assert.True(VerifyOperationTimeout(Client, operationTImeout));
        }

        public abstract bool VerifyIpAddress(T client, string ipAddress);
        public abstract bool VerifyPort(T client, int port);
        public abstract bool VerifyUserName(T client, string userName);
        public abstract bool VerifyPassword(T client, string password);
        public abstract bool VerifyConnectionTimeout(T client, int connectionTimeout);
        public abstract bool VerifyOperationTimeout(T client, int operationTImeout);

        public virtual void Dispose()
        {
        }
    }
}
