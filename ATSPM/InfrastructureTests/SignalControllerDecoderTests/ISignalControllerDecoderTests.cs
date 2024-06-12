#region license
// Copyright 2024 Utah Departement of Transportation
// for InfrastructureTests - InfrastructureTests.LocationControllerDecoderTests/ISignalControllerDecoderTests.cs
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
using ATSPM.Application.Configuration;
using ATSPM.Domain.Exceptions;
using ATSPM.Infrastructure.Services.ControllerDecoders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace InfrastructureTests.LocationControllerDecoderTests
{
    public class ILocationControllerDecoderTests : IDisposable
    {
        private const string TestDataPath = "C:\\Projects\\udot-atsmp\\ATSPM\\InfrastructureTests\\TestData";

        private readonly ITestOutputHelper _output;
        //private ILocationControllerDecoder _decoder;
        //private ILogger _nullLogger;
        //private IOptions<SignalControllerDecoderConfiguration> _nullOptions;

        public ILocationControllerDecoderTests(ITestOutputHelper output)
        {
            _output = output;
            //_nullLogger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<StubLocationControllerDecoder>();
            //_nullOptions = Options.Create(new SignalControllerDecoderConfiguration() { EarliestAcceptableDate = new DateTime() });
            //_decoder = new StubLocationControllerDecoder((ILogger<StubLocationControllerDecoder>)_nullLogger, _nullOptions);

            //_output.WriteLine($"Created ILocationControllerDecoder Instance: {_decoder.GetHashCode()}");
        }

        #region ILocationControllerDecoder

        //#region ILocationControllerDecoder.IsCompressed

        //[Theory]
        //[EncodedControllerTestFiles]
        //public void IsCompressed(FileInfo fileInfo, bool isCompressed, bool isEncoded)
        //{
        //    var expected = isCompressed;
        //    _output.WriteLine($"Expected: {expected}");

        //    var actual = _decoder.IsCompressed(fileInfo.ToMemoryStream());
        //    _output.WriteLine($"Actual: {actual}");

        //    Assert.Equal(expected, actual);
        //}

        //#endregion

        //#region ILocationControllerDecoder.IsEncoded

        //[Theory]
        //[EncodedControllerTestFiles]
        //public void IsEncoded(FileInfo fileInfo, bool isCompressed, bool isEncoded)
        //{
        //    var expected = isEncoded;
        //    _output.WriteLine($"Expected: {expected}");

        //    var actual = _decoder.IsEncoded(fileInfo.ToMemoryStream());
        //    _output.WriteLine($"Actual: {actual}");

        //    Assert.Equal(expected, actual);
        //}

        //#endregion

        //#region ILocationControllerDecoder.Decompress

        //[Theory]
        //[EncodedControllerTestFiles]
        //public void Decompress(FileInfo fileInfo, bool isCompressed, bool isEncoded)
        //{
        //    if (isCompressed)
        //    {
        //        var decompressed =_decoder.Decompress(fileInfo.ToMemoryStream());

        //        var condition = decompressed.IsCompressed();
        //        _output.WriteLine($"Condition: {condition}");

        //        Assert.False(condition);
        //    }
        //    else
        //    {
        //        Assert.False(isCompressed);
        //    }
        //}

        //#endregion

        //#region ILocationControllerDecoder.Decode

        //[Theory]
        //[EncodedControllerTestFiles]
        //public async void DecodeAsync(FileInfo fileInfo, bool isCompressed, bool isEncoded)
        //{
        //    if (_decoder.CanExecute(fileInfo))
        //    {
        //        var logs = await _decoder.DecodeAsync("0", fileInfo.ToMemoryStream());

        //        var condition = logs.Count > 0;

        //        Assert.True(condition);
        //    }
        //    else
        //    {
        //        Assert.False(true);
        //    }
        //}

        //[Fact]
        //public async Task DecodeNotValidFromNullData()
        //{
        //    var memoryStream = new MemoryStream();

        //    await Assert.ThrowsAnyAsync<ArgumentNullException>(() => _decoder.DecodeAsync(string.Empty, memoryStream));
        //}

        //[Fact]
        //public async Task DecodeNotValidFromInvalidData()
        //{
        //    var memoryStream = new MemoryStream();

        //    await Assert.ThrowsAnyAsync<InvalidDataException>(() => _decoder.DecodeAsync("0", memoryStream));
        //}

        //[Fact]
        //public async Task DecodeNotValidFromBadData()
        //{
        //    //byte[] badData = Encoding.UTF8.GetBytes(string.Join(",", Enumerable.Range(1, 100).Select(i => i.ToString())));

        //    //var memoryStream = new MemoryStream(badData);

        //    var test = await _decoder.DecodeAsync("0", new MemoryStream());

        //    _output.WriteLine($"test: {test.Count}");

        //    //await Assert.ThrowsAnyAsync<InvalidDataException>(() => _decoder.DecodeAsync("0", memoryStream));
        //    Assert.True(false);
        //}

        //[Fact]
        //public async Task DecodeWithProgress()
        //{
        //    //FileInfo fileInfo = new FileInfo(Path.Combine(TestDataPath, "1053(dat)\\ECON_10.204.12.167_2021_08_09_1831.dat"));

        //    int actual = 0;
        //    var progress = new Progress<int>(i => actual = i);

        //    var collection = await _decoder.DecodeAsync("0", new MemoryStream(), progress);
        //    var expected = collection.Count;

        //    Assert.Equal(expected, actual);
        //}

        //[Fact]
        //public async Task DecodeWithTokenCancelled()
        //{
        //    CancellationTokenSource cts = new CancellationTokenSource();
        //    cts.Cancel();

        //    await Assert.ThrowsAnyAsync<OperationCanceledException>(() => _decoder.DecodeAsync("0", new MemoryStream(), null, cts.Token));
        //}

        //#endregion

        #region ILocationControllerDecoder.ExecuteAsync

        //[Fact]
        //public async Task ExecuteAsyncWithNullFileInfoParameter()
        //{
        //    var sut = new Mock<StubLocationControllerDecoder>(Mock.Of<ILogger<StubLocationControllerDecoder>>(), Mock.Of<IOptions<SignalControllerDecoderConfiguration>>()).Object;

        //    await Assert.ThrowsAnyAsync<ArgumentNullException>(() => sut.ExecuteAsync(null, null));
        //}

        //[Fact]
        //public async Task ExecuteAsyncWithInvalidFileInfoParameter()
        //{
        //    var sut = new Mock<StubLocationControllerDecoder>(Mock.Of<ILogger<StubLocationControllerDecoder>>(), Mock.Of<IOptions<SignalControllerDecoderConfiguration>>()).Object;

        //    await Assert.ThrowsAnyAsync<FileNotFoundException>(() => sut.ExecuteAsync(new FileInfo("C:\\invalid.txt"), null));
        //}

        //[Fact]
        //public async Task ExecuteAsyncWithFailedCanExecute()
        //{
        //    var sut = new Mock<StubLocationControllerDecoder>(Mock.Of<ILogger<StubLocationControllerDecoder>>(), Mock.Of<IOptions<SignalControllerDecoderConfiguration>>());

        //    sut.Setup(m => m.CanExecute(It.IsAny<FileInfo>())).Returns(false);

        //    await Assert.ThrowsAnyAsync<ExecuteException>(() => sut.Object.ExecuteAsync(new FileInfo(Path.GetTempFileName()), null));
        //}

        #endregion

        /*

        

        #endregion

        #region IExecuteAsyncWithProgress

        #region IExecuteAsyncWithProgress.CanExecute

        [Fact]
        public void CanExecuteValidFromDat()
        {
            FileInfo fileInfo = new FileInfo(Path.Combine(TestDataPath,"1053(dat)\\ECON_10.204.12.167_2021_08_09_1831.dat"));

            var condition = _decoder.CanExecute(fileInfo);

            Assert.True(condition);
        }

        [Fact]
        public void CanExecuteValidFromDatz()
        {
            FileInfo fileInfo = new FileInfo(Path.Combine(TestDataPath,"1210(datz)\\ECON_10.204.7.239_2021_08_09_1841.datZ"));

            var condition = _decoder.CanExecute(fileInfo);

            Assert.True(condition);
        }

        [Fact]
        public void CanExecuteNotValid()
        {
            FileInfo fileInfo = new FileInfo(Path.Combine(TestDataPath,"1210(datz)\\ECON_10.204.12.179_2021_08_09_1831.bad"));

            var condition = _decoder.CanExecute(fileInfo);

            Assert.False(condition);
        }

        #endregion

        #region IExecuteAsyncWithProgress.ExecuteAsync

        [Theory]
        [EncodedControllerTestFiles]
        public void ExecuteAsyncValidFromDat(FileInfo fileInfo, string expected)
        {
            var collection = _decoder.ExecuteAsync(fileInfo).Result;
            var condition = collection?.Count > 0;

            Assert.True(condition);
            Assert.All(collection, l => Assert.Equal(expected, l.LocationId));
        }

        [Fact]
        public void ExecuteAsyncTokenCancelled()
        {
            FileInfo fileInfo = new FileInfo(Path.Combine(TestDataPath,"1053(dat)\\ECON_10.204.12.167_2021_08_09_1831.dat"));

            CancellationTokenSource cts = new CancellationTokenSource();
            cts.Cancel();

            var expected = TaskStatus.Canceled;
            TaskStatus actual = _decoder.ExecuteAsync(fileInfo, cts.Token).Status;

            _output.WriteLine($"expected: {expected}");
            _output.WriteLine($"actual: {actual}");

            Assert.Equal(expected, actual);
        }

        [Theory]
        [EncodedControllerTestFiles]
        public void ExecuteAsyncWithProgress(FileInfo fileInfo, string expected)
        {
            int actual = 0;
            var progress = new Progress<int>(i => actual = i);

            var collection = _decoder.ExecuteAsync(fileInfo, progress: progress).Result;
            var realExpected = collection.Count;

            Assert.Equal(realExpected, actual);
        }

        #endregion

        */

        #endregion

        public void Dispose()
        {
            //_output.WriteLine($"Disposing ILocationControllerDecoder Instance: {_decoder.GetHashCode()}");
        }
    }
}
