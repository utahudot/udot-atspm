using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Xunit.Sdk;

namespace DomainCoreTests
{
    public class SpecificationTestAttribute : DataAttribute
    {
        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            Random random = new Random();
            List<TestModel> models = new List<TestModel>();

            for (int i = 1; i <= 1000; i++)
            {
                models.Add(new TestModel() { Name = ((char)random.Next(65, 91)).ToString() });
            }

            yield return new object[] { models };
        }
    }
}
