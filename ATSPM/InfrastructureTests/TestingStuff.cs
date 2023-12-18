using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace InfrastructureTests
{
    public class TestingStuffs : IDisposable
    {
        private readonly ITestOutputHelper _output;

        public TestingStuffs(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Test()
        {

        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
