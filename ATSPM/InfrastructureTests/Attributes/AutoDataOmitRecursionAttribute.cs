using AutoFixture;
using AutoFixture.Xunit2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace InfrastructureTests.Attributes
{
    //https://github.com/AutoFixture/AutoFixture/wiki/Examples-of-using-behaviors
    
    public class AutoDataOmitRecursionAttribute : AutoDataAttribute
    {
        public AutoDataOmitRecursionAttribute() : base(() =>
        {
            var fixture = new Fixture();
            fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => fixture.Behaviors.Remove(b));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            return fixture;
        })
        {

        }

        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            var original = base.GetData(testMethod);

            foreach (var objs in original)
            {
                foreach (var obj in objs)
                {
                    foreach (var p in obj.GetType().GetProperties().Where(p => p.PropertyType.IsGenericType))
                    {
                        p.PropertyType.GetMethod("Clear")?.Invoke(p.GetValue(obj, null), null);
                    }
                }
            }

            return original;
        }
    }
}
