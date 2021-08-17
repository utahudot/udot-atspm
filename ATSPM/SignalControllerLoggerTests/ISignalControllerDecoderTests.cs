using ATSPM.Application.Configuration;
using ATSPM.Application.Enums;
using ATSPM.Application.Models;
using ATSPM.Application.Services.SignalControllerProtocols;
using ATSPM.Domain.Extensions;
using ATSPM.Infrasturcture.Services.ControllerDecoders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SignalControllerLoggerTests.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace SignalControllerLoggerTests
{
    public class ISignalControllerDecoderTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private ISignalControllerDecoder _decoder;
        private ILogger<ASCSignalControllerDecoder> _nullLogger;
        private IOptions<SignalControllerDownloaderConfiguration> _nullOptions;

        public ISignalControllerDecoderTests(ITestOutputHelper output)
        {
            _output = output;
            _nullLogger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<ASCSignalControllerDecoder>();
            _nullOptions = Options.Create(new SignalControllerDownloaderConfiguration() { EarliestAcceptableDate = new DateTime() });
            _decoder = new ASCSignalControllerDecoder(_nullLogger, new ServiceCollection().BuildServiceProvider(), _nullOptions);

            _output.WriteLine($"Created ISignalControllerDecoder Instance: {_decoder.GetHashCode()}");
        }

        #region ISignalControllerDecoder

        #region ISignalControllerDecoder.ControllerType

        [Fact]
        public void ControllerTypeNotValid()
        {
            var expected = SignalControllerType.Unknown;

            var actual = _decoder.ControllerType;

            Assert.NotEqual(expected, actual);
        }

        #endregion

        #region ISignalControllerDecoder.IsCompressed

        [Fact]
        public void IsCompressedTrueFromDatz()
        {
            FileInfo fileInfo = new FileInfo("C:\\Projects\\udot-atsmp\\ATSPM\\SignalControllerLoggerTests\\TestData\\1210(datz)\\ECON_10.204.7.239_2021_08_09_1841.datZ");

            var condition = _decoder.IsCompressed(fileInfo.ToMemoryStream());

            Assert.True(condition);
        }

        [Fact]
        public void IsCompressedFalseFromDat()
        {
            FileInfo fileInfo = new FileInfo("C:\\Projects\\udot-atsmp\\ATSPM\\SignalControllerLoggerTests\\TestData\\1053(dat)\\ECON_10.204.12.167_2021_08_09_1831.dat");

            var condition = _decoder.IsCompressed(fileInfo.ToMemoryStream());

            Assert.False(condition);
        }

        #endregion

        #region ISignalControllerDecoder.IsEncoded

        [Fact]
        public void IsEncodedNotEncoded()
        {
            FileInfo fileInfo = new FileInfo("C:\\Projects\\udot-atsmp\\ATSPM\\SignalControllerLoggerTests\\TestData\\Hello.csv");
            var memoryStream = fileInfo.ToMemoryStream();

            var condition = _decoder.IsEncoded(memoryStream);

            Assert.False(condition);
        }

        [Fact]
        public void IsEncodedIsEncoded()
        {
            FileInfo fileInfo = new FileInfo("C:\\Projects\\udot-atsmp\\ATSPM\\SignalControllerLoggerTests\\TestData\\1053(dat)\\ECON_10.204.12.167_2021_08_09_1831.dat");
            var memoryStream = fileInfo.ToMemoryStream();

            var condition = _decoder.IsEncoded(memoryStream);

            Assert.True(condition);
        }

        #endregion

        #region ISignalControllerDecoder.Decompress

        [Fact]
        public void DecompressValidFromDatz()
        {
            FileInfo fileInfo = new FileInfo("C:\\Projects\\udot-atsmp\\ATSPM\\SignalControllerLoggerTests\\TestData\\1210(datz)\\ECON_10.204.7.239_2021_08_09_1841.datZ");
            var memoryStream = fileInfo.ToMemoryStream();

            var result = _decoder.Decompress(memoryStream);

            Assert.NotNull(result);
        }

        [Fact(Skip = "Method isn't testing if stream is compressed or not")]
        public void DecompressNotValidFromUncompressedStream()
        {
            var expected = new MemoryStream(Encoding.UTF8.GetBytes(string.Join("", Enumerable.Range(1, 500).Select(i => i.ToString()))));

            _output.WriteLine($"expected length: {expected.Length}");

            var actual = _decoder.Decompress(expected);

            _output.WriteLine($"actual length: {actual.Length}");

            Assert.Equal(expected.Length, actual.Length);
        }

        [Fact]
        public void DecompressNotValidFromNullStream()
        {
            Assert.Throws<ArgumentNullException>(() => _decoder.Decompress(null));
        }

        #endregion

        #region ISignalControllerDecoder.Decode

        [Fact]
        public void DecodeValidFromDat()
        {
            FileInfo fileInfo = new FileInfo("C:\\Projects\\udot-atsmp\\ATSPM\\SignalControllerLoggerTests\\TestData\\1053(dat)\\ECON_10.204.12.167_2021_08_09_1831.dat");

            var expected = "1053";
            var collection = _decoder.Decode("1053", fileInfo.ToMemoryStream());
            var condition = collection.Count > 0;

            Assert.True(condition);
            Assert.All(collection, l => Assert.Equal(expected, l.SignalId));
        }

        [Fact]
        public void DecodeValidFromDatz()
        {
            FileInfo fileInfo = new FileInfo("C:\\Projects\\udot-atsmp\\ATSPM\\SignalControllerLoggerTests\\TestData\\1210(datz)\\ECON_10.204.7.239_2021_08_09_1841.datZ");

            var memoryStream = fileInfo.ToMemoryStream();
            memoryStream = _decoder.IsCompressed(memoryStream) ? (MemoryStream)_decoder.Decompress(memoryStream) : memoryStream;

            var expected = "1210";
            var collection = _decoder.Decode("1210", memoryStream);
            var condition = collection.Count > 0;

            Assert.True(condition);
            Assert.All(collection, l => Assert.Equal(expected, l.SignalId));
        }

        [Fact]
        public void DecodeNotValidFromDatz()
        {
            FileInfo fileInfo = new FileInfo("C:\\Projects\\udot-atsmp\\ATSPM\\SignalControllerLoggerTests\\TestData\\1210(datz)\\ECON_10.204.7.239_2021_08_09_1841.datZ");

            var memoryStream = fileInfo.ToMemoryStream();

            Assert.Throws<InvalidDataException>(() => _decoder.Decode("1210", memoryStream));
        }

        [Fact]
        public void DecodeNotValidFromNullData()
        {
            var memoryStream = new MemoryStream();

            Assert.Throws<InvalidDataException>(() => _decoder.Decode("1210", memoryStream));
        }

        [Fact]
        public void DecodeNotValidFromBadData()
        {
            byte[] badData = Encoding.UTF8.GetBytes(string.Join("", Enumerable.Range(1, 500).Select(i => i.ToString())));

            var memoryStream = new MemoryStream(badData);

            Assert.Throws<InvalidDataException>(() => _decoder.Decode("1210", memoryStream));
        }

        [Fact]
        public void DecodeWithProgress()
        {
            FileInfo fileInfo = new FileInfo("C:\\Projects\\udot-atsmp\\ATSPM\\SignalControllerLoggerTests\\TestData\\1053(dat)\\ECON_10.204.12.167_2021_08_09_1831.dat");

            int actual = 0;
            var progress = new Progress<int>(i => actual = i);

            var collection = _decoder.Decode("1053", fileInfo.ToMemoryStream(), progress);
            var expected = collection.Count;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void DecodeWithTokenCancelled()
        {
            FileInfo fileInfo = new FileInfo("C:\\Projects\\udot-atsmp\\ATSPM\\SignalControllerLoggerTests\\TestData\\1053(dat)\\ECON_10.204.12.167_2021_08_09_1831.dat");

            CancellationTokenSource cts = new CancellationTokenSource();
            cts.Cancel();

            var collection = _decoder.Decode("1053", fileInfo.ToMemoryStream(), null, cts.Token);

            Assert.Null(collection);
        }

        #endregion

        #endregion

        #region IExecuteAsyncWithProgress

        #region IExecuteAsyncWithProgress.CanExecute

        [Fact]
        public void CanExecuteValidFromDat()
        {
            FileInfo fileInfo = new FileInfo("C:\\Projects\\udot-atsmp\\ATSPM\\SignalControllerLoggerTests\\TestData\\1053(dat)\\ECON_10.204.12.167_2021_08_09_1831.dat");

            var condition = _decoder.CanExecute(fileInfo);

            Assert.True(condition);
        }

        [Fact]
        public void CanExecuteValidFromDatz()
        {
            FileInfo fileInfo = new FileInfo("C:\\Projects\\udot-atsmp\\ATSPM\\SignalControllerLoggerTests\\TestData\\1210(datz)\\ECON_10.204.7.239_2021_08_09_1841.datZ");

            var condition = _decoder.CanExecute(fileInfo);

            Assert.True(condition);
        }

        [Fact]
        public void CanExecuteNotValid()
        {
            FileInfo fileInfo = new FileInfo("C:\\Projects\\udot-atsmp\\ATSPM\\SignalControllerLoggerTests\\TestData\\1210(datz)\\ECON_10.204.12.179_2021_08_09_1831.bad");

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
            var condition = collection.Count > 0;

            Assert.True(condition);
            Assert.All(collection, l => Assert.Equal(expected, l.SignalId));
        }

        [Fact]
        public void ExecuteAsyncTokenCancelled()
        {
            FileInfo fileInfo = new FileInfo("C:\\Projects\\udot-atsmp\\ATSPM\\SignalControllerLoggerTests\\TestData\\1053(dat)\\ECON_10.204.12.167_2021_08_09_1831.dat");

            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromMilliseconds(1));

            Assert.Throws<OperationCanceledException>(() => _decoder.ExecuteAsync(fileInfo, cts.Token).Result);
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

        #endregion

        public void Dispose()
        {
            if (_decoder is IDisposable d)
            {
                d.Dispose();
            }

            _output.WriteLine($"Disposing ISignalControllerDecoder Instance: {_decoder.GetHashCode()}");

            _decoder = null;
            _nullLogger = null;
            _nullOptions = null;
        }
    }
}
