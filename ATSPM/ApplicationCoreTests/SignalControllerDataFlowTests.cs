using ATSPM.Application;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using Xunit;
using Xunit.Abstractions;
using Moq;
using ATSPM.Application.Services.SignalControllerProtocols;
using ATSPM.Application.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ApplicationCoreTests
{
    public class SignalControllerDataFlowTests : IDisposable
    {
        private readonly ITestOutputHelper _output;

        public SignalControllerDataFlowTests(ITestOutputHelper output)
        {
            _output = output;
        }

        #region IExecuteAsyncWithProgress

        #endregion

        [Fact]
        public void Test1()
        {
            var downloader = new Mock<ISignalControllerDownloader>();
            downloader.Setup(s => s.CanExecute(It.Is<Signal>(p => true))).Returns(true);
            downloader.Setup(s => s.Execute(It.Is<Signal>(p => true), default, default)).Returns(new List<FileInfo>(new FileInfo[] { new FileInfo(Path.GetTempFileName()) }).ToAsyncEnumerable());
            
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddScoped<ISignalControllerDownloader>(sp => downloader.Object);
            var sut = new SignalControllerDataFlow(new NullLogger<SignalControllerDataFlow>(), serviceCollection.BuildServiceProvider());


        }

        public void Dispose()
        {
            //_output.WriteLine($"Disposing database: {_db.GetHashCode()}");
        }
    }
}
