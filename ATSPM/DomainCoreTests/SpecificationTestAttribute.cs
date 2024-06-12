#region license
// Copyright 2024 Utah Departement of Transportation
// for DomainCoreTests - DomainCoreTests/SpecificationTestAttribute.cs
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
