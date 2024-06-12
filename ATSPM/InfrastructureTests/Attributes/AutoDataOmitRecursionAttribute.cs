#region license
// Copyright 2024 Utah Departement of Transportation
// for InfrastructureTests - InfrastructureTests.Attributes/AutoDataOmitRecursionAttribute.cs
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
