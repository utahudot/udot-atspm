using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit;
using ATSPM.Domain.BaseClasses;
using System.ComponentModel;
using System.Runtime.InteropServices;
using static Xunit.Assert;

namespace DomainCoreTests.ServiceTests
{
    public class ServiceBaseTests : IDisposable
    {
        class TestService : ServiceObjectBase
        {
            public TestService(bool initialize = false) : base(initialize) { }

            public int InitStatus { get; set; }

            public bool UnManagedDisposed { get; set; }

            public bool ManagedDisposed { get; set; }

            public async override Task Initialize()
            {
                await Task.Delay(TimeSpan.FromMilliseconds(10));

                InitStatus += 1;
            }

            public async Task TestRaiseInitialized()
            {
                BeginInit();
                await Task.Delay(TimeSpan.FromMilliseconds(20));
            }

            protected override void DisposeManagedCode()
            {
                ManagedDisposed = true;
            }

            protected override void DisposeUnManagedCode()
            {
                UnManagedDisposed = true;
            }
        }

        private readonly ITestOutputHelper _output;

        public ServiceBaseTests(ITestOutputHelper output)
        {
            _output = output;
        }

        #region ServiceObjectBase

        #region ISupportInitializeNotification

        [Fact]
        [Trait(nameof(ServiceBaseTests), "Not Initialized From Constructor")]
        public void ServiceBaseTestsNotInitializedFromConstructor()
        {
            var sut = new TestService();

            var condition = sut.IsInitialized;

            Assert.False(condition);
        }

        /// <summary>
        /// When the initialize flag is set, IsInitialized should be set to true
        /// </summary>
        [Fact]
        [Trait(nameof(ServiceBaseTests), "Is Initialized From Constructor")]
        public async void ServiceBaseTestsIsInitializedFromConstructor()
        {
            var sut = new TestService(true);

            await Task.Delay(TimeSpan.FromMilliseconds(20));

            var condition1 = sut.IsInitialized;
            var condition2 = sut.InitStatus == 1;

            Assert.True(condition1);
            Assert.True(condition2);
        }

        /// <summary>
        /// When BeginInit is called, IsInitialized should be set to true
        /// </summary>
        [Fact]
        [Trait(nameof(ServiceBaseTests), "Is Initialized From BeginInit")]
        public async void ServiceBaseTestsIsInitializedFromBeginInit()
        {
            var sut = new TestService();

            sut.BeginInit();

            await Task.Delay(TimeSpan.FromMilliseconds(20));

            var condition1 = sut.IsInitialized;
            var condition2 = sut.InitStatus == 1;

            Assert.True(condition1);
            Assert.True(condition2);
        }

        /// <summary>
        /// Calling BeginInit while IsInitialized is true will re-initialize the service.
        /// </summary>
        [Fact]
        [Trait(nameof(ServiceBaseTests), "Double Initialize")]
        public async void ServiceBaseTestsDoubleInitialize()
        {
            var sut = new TestService();

            sut.BeginInit();

            await Task.Delay(TimeSpan.FromMilliseconds(20));

            sut.BeginInit();

            await Task.Delay(TimeSpan.FromMilliseconds(20));

            var condition1 = sut.IsInitialized;
            var condition2 = sut.InitStatus == 2;

            Assert.True(condition1);
            Assert.True(condition2);
        }

        /// <summary>
        /// Initilized should be raised whenever IsInitialized is set to true
        /// </summary>
        [Fact]
        [Trait(nameof(ServiceBaseTests), "Raises Initialized")]
        public async void ServiceBaseTestsRaisesInitialized()
        {
            var sut = new TestService();

            var evt = await Assert.RaisesAnyAsync(h => sut.Initialized += h, h => sut.Initialized -= h, () => sut.TestRaiseInitialized());

            Assert.NotNull(evt);
            Assert.Equal(sut, evt.Sender);
            Assert.IsType<EventArgs>(evt.Arguments);
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// When disposed, the service IsInitialized should be set to false
        /// Managed and UnManaged code should have also been disposed
        /// </summary>
        [Fact]
        [Trait(nameof(ServiceBaseTests), "Not Initialized When Disposed")]
        public async void ServiceBaseTestsNotInitializedWhenDisposed()
        {
            var sut = new TestService(true);

            await Task.Delay(TimeSpan.FromMilliseconds(20));

            sut.Dispose();

            var condition1 = sut.IsInitialized;
            var condition2 = sut.ManagedDisposed;
            var condition3 = sut.UnManagedDisposed;

            Assert.False(condition1);
            Assert.True(condition2);
            Assert.True(condition3);
        }

        #endregion

        #endregion

        public void Dispose()
        {
        }
    }
}
