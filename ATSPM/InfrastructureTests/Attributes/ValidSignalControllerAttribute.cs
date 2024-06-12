#region license
// Copyright 2024 Utah Departement of Transportation
// for InfrastructureTests - %Namespace%/ValidSignalControllerAttribute.cs
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
using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Text;
using Xunit.Sdk;

namespace InfrastructureTests.Attributes
{
    public class ValidLocationControllerAttribute : DataAttribute
    {
        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            yield return new object[]
            {
                new Location()
                {
                    Ipaddress = new IPAddress(new byte[] { 10,209,2,120 }),
                    ChartEnabled = true,
                    PrimaryName = "Maxtime Test",
                    LocationIdentifier = "0",
                    ControllerTypeId = 4,
                    ControllerType = new ControllerType() { Id = 4 }
                }
            };

            yield return new object[]
            {
                new Location()
                {
                    Ipaddress = new IPAddress(new byte[] { 10,209,2,108 }),
                    ChartEnabled = true,
                    PrimaryName = "Cobalt Test",
                    LocationIdentifier = "9731",
                    ControllerTypeId = 2,
                    ControllerType = new ControllerType() { Id = 2 }
                }
            };
        }
    }
}
