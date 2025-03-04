#region license
// Copyright 2025 Utah Departement of Transportation
// for InfrastructureTests - Utah.Udot.Atspm.InfrastructureTests.DownloaderClientTests/HttpDownloaderClientTests.cs
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

using Moq;
using Moq.Protected;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Utah.Udot.Atspm.Infrastructure.Services.DownloaderClients;
using Xunit.Abstractions;

namespace Utah.Udot.Atspm.InfrastructureTests.DownloaderClientTests
{
    public class HttpDownloaderClientTests : DownloaderClientTestsBase<HttpClient>
    {
        public HttpDownloaderClientTests(ITestOutputHelper output) : base(output) { }

        #region MyRegion

        public override async Task ConnectAsyncSucceeded()
        {
            Sut = new HttpDownloaderClient();

            await base.ConnectAsyncSucceeded();
        }

        public override async Task ConnectAsyncNullConnection()
        {
            Sut = new HttpDownloaderClient();

            await base.ConnectAsyncNullConnection();
        }

        public override async Task ConnectAsyncNullCredentials()
        {
            Sut = new HttpDownloaderClient();

            await base.ConnectAsyncNullCredentials();
        }

        public override Task ConnectAsyncControllerConnectionException()
        {
            return Task.CompletedTask;
        }

        public override async Task ConnectAsyncOperationCanceledException()
        {
            Sut = new SSHNetSFTPDownloaderClient();

            await base.ConnectAsyncOperationCanceledException();
        }

        #endregion

        #region DeleteResource

        public override async Task DeleteResourceAsyncSucceeded()
        {
            var uri = new UriBuilder()
            {
                Scheme = Uri.UriSchemeHttp,
                Host = "127.0.0.1",
                Port = 80,
                Path = "/a/b/c",
                Query = "?key=value"
            }.Uri;

            Sut = new HttpDownloaderClient(new HttpClient() { BaseAddress = uri });

            await Sut.DeleteResourceAsync(uri);
        }

        public override Task DeleteResourceAsyncNullResource()
        {
            Sut = new HttpDownloaderClient(new HttpClient() { BaseAddress = new Uri("http://192.168.1.1") });

            return base.DeleteResourceAsyncNullResource();
        }

        public override Task DeleteResourceAsyncInvalidResource()
        {
            Sut = new HttpDownloaderClient(new HttpClient() { BaseAddress = new Uri("http://192.168.1.1") });

            return base.DeleteResourceAsyncInvalidResource();
        }

        public override async Task DeleteResourceAsyncNotConnected()
        {
            Sut = new HttpDownloaderClient();

            await base.DeleteResourceAsyncNotConnected();
        }

        public override async Task DeleteResourceAsyncControllerDeleteResourceException()
        {
            Sut = new HttpDownloaderClient(new HttpClient() { BaseAddress = new Uri("http://192.168.1.1") });

            await base.DeleteResourceAsyncControllerDeleteResourceException();
        }

        public override async Task DeleteResourceAsyncOperationCanceledException()
        {
            Sut = new HttpDownloaderClient();

            await base.DeleteResourceAsyncOperationCanceledException();
        }

        #endregion

        #region Disconnect

        public override Task DisconnectAsyncSucceeded()
        {
            return Task.CompletedTask;
        }

        public override async Task DisconnectAsyncNotConnected()
        {
            Sut = new HttpDownloaderClient();

            await base.DisconnectAsyncNotConnected();
        }

        public override Task DisconnectAsyncControllerConnectionException()
        {
            return Task.CompletedTask;
        }

        public override async Task DisconnectAsyncOperationCanceledException()
        {
            Sut = new HttpDownloaderClient();

            await base.DisconnectAsyncOperationCanceledException();
        }

        #endregion

        #region DownloadResource

        public override async Task DownloadResourceAsyncSucceed()
        {
            var client = new Mock<HttpMessageHandler>();

            var mockResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("test content")
            };

            client.Protected().Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(m => m.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(mockResponse);

            Sut = new HttpDownloaderClient(new HttpClient(client.Object) { BaseAddress = new Uri("http://192.168.1.1") });

            await base.DownloadResourceAsyncSucceed();
        }

        public override async Task DownloadResourceAsyncNullLocal()
        {
            Sut = new HttpDownloaderClient(new HttpClient() { BaseAddress = new Uri("http://192.168.1.1") });

            await base.DownloadResourceAsyncNullLocal();
        }

        public override async Task DownloadResourceAsyncNullRemote()
        {
            Sut = new HttpDownloaderClient(new HttpClient() { BaseAddress = new Uri("http://192.168.1.1") });

            await base.DownloadResourceAsyncNullRemote();
        }

        public override async Task DownloadResourceAsyncInvalidLocal()
        {
            Sut = new HttpDownloaderClient(new HttpClient() { BaseAddress = new Uri("http://192.168.1.1") });

            await base.DownloadResourceAsyncInvalidLocal();
        }

        public override async Task DownloadResourceAsyncNotConnected()
        {
            Sut = new HttpDownloaderClient();

            await base.DownloadResourceAsyncNotConnected();
        }

        public override async Task DownloadResourceAsyncDownloaderClientDownloadResourceException()
        {
            var client = new Mock<HttpMessageHandler>();

            client.Protected().Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(m => m.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>())
                .Throws<Exception>();

            Sut = new HttpDownloaderClient(new HttpClient(client.Object) { BaseAddress = new Uri("http://192.168.1.1") });

            await base.DownloadResourceAsyncDownloaderClientDownloadResourceException();
        }

        public override async Task DownloadResourceAsyncOperationCanceledException()
        {
            Sut = new HttpDownloaderClient(new HttpClient() { BaseAddress = new Uri("http://192.168.1.1") });

            await base.DownloadResourceAsyncOperationCanceledException();
        }

        #endregion

        #region ListResources

        public override async Task ListResourcesAsyncSucceeded()
        {
            Sut = new HttpDownloaderClient(new HttpClient() { BaseAddress = new Uri("http://192.168.1.1") });

            await base.ListResourcesAsyncSucceeded();
        }

        public override async Task ListResourcesAsyncNotConnected()
        {
            Sut = new HttpDownloaderClient();

            await base.ListResourcesAsyncNotConnected();
        }

        public override Task ListDirectoryAsyncControllerDownloadFileException()
        {
            return Task.CompletedTask;
        }

        public override async Task ListResourcesAsyncDownloaderClientListResourcesException()
        {
            Sut = new HttpDownloaderClient();

            await base.ListResourcesAsyncDownloaderClientListResourcesException();
        }

        #endregion

        public override async Task ConnectAsyncConnectionProperties()
        {
            Client = new HttpClient();
            Sut = new HttpDownloaderClient(Client);

            await base.ConnectAsyncConnectionProperties();
        }

        public override bool VerifyIpAddress(HttpClient client, string ipAddress)
        {
            return client.BaseAddress.Host.Contains(ipAddress.ToString());
        }

        public override bool VerifyPort(HttpClient client, int port)
        {
            return client.BaseAddress.Port == port;
        }

        public override bool VerifyUserName(HttpClient client, string userName)
        {
            return client.BaseAddress.UserInfo.StartsWith(userName);
        }

        public override bool VerifyPassword(HttpClient client, string password)
        {
            return client.BaseAddress.UserInfo.EndsWith(password);
        }

        public override bool VerifyConnectionTimeout(HttpClient client, int connectionTimeout)
        {
            return true;
        }

        public override bool VerifyOperationTimeout(HttpClient client, int operationTimeout)
        {
            return client.Timeout.TotalMilliseconds == operationTimeout;
        }
    }
}
