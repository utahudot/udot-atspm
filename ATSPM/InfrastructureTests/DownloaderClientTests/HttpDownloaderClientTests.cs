#region license
// Copyright 2024 Utah Departement of Transportation
// for InfrastructureTests - InfrastructureTests.LogDownloaderClientTests/HttpDownloaderClientTests.cs
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
using Moq.Protected;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace InfrastructureTests.DownloaderClientTests
{
    public class HttpDownloaderClientTests : DownloaderClientTestsBase
    {
        public HttpDownloaderClientTests(ITestOutputHelper output) : base(output) { }

        public override void ConnectAsyncSucceeded()
        {
            Sut = new HttpDownloaderClient();

            base.ConnectAsyncSucceeded();
        }

        public override void ConnectAsyncArgumentNullException()
        {
            Sut = new HttpDownloaderClient();

            base.ConnectAsyncArgumentNullException();
        }

        public override void ConnectAsyncControllerConnectionException()
        {
        }

        public override void DeleteFileAsyncSucceeded()
        {
            Sut = new HttpDownloaderClient(new HttpClient() { BaseAddress = new Uri($"http://127.0.0.1") });

            base.DeleteFileAsyncSucceeded();
        }

        public override void DeleteFileAsyncNotConnected()
        {
            Sut = new HttpDownloaderClient();

            base.DeleteFileAsyncNotConnected();
        }

        public override void DeleteFileAsyncControllerDeleteFileException()
        {
        }

        public override void DisconnectAsyncSucceeded()
        {
        }

        public override void DisconnectAsyncNotConnected()
        {
            Sut = new HttpDownloaderClient();

            base.DisconnectAsyncNotConnected();
        }

        public override void DisconnectAsyncControllerConnectionException()
        {
        }

        public override void DownloadFileAsyncSucceeded()
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

            Sut = new HttpDownloaderClient(new HttpClient(client.Object) { BaseAddress = new Uri($"http://127.0.0.1") });

            base.DownloadFileAsyncSucceeded();
        }

        public override void DownloadFileAsyncNotConnected()
        {
            Sut = new HttpDownloaderClient();

            base.DownloadFileAsyncNotConnected();
        }

        public override void DownloadFileAsyncControllerDownloadFileException()
        {
            var client = new Mock<HttpMessageHandler>();

            client.Protected().Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(m => m.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>())
                .Throws<Exception>();

            Sut = new HttpDownloaderClient(new HttpClient(client.Object) { BaseAddress = new Uri($"http://127.0.0.1") });

            base.DownloadFileAsyncControllerDownloadFileException();
        }

        public override void ListDirectoryAsyncSucceeded()
        {
        }

        public override void ListDirectoryAsyncNotConnected()
        {
            Sut = new HttpDownloaderClient();

            base.ListDirectoryAsyncNotConnected();
        }

        public override void ListDirectoryAsyncControllerDownloadFileException()
        {
        }
    }
}
