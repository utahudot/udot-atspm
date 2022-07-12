using ATSPM.Application.Models;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Xunit.Sdk;

namespace InfrastructureTests.Attributes
{
    public class ValidSignalControllerAttribute : DataAttribute
    {
        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            yield return new object[]
            {
                new Signal()
                {
                    Ipaddress = "10.209.2.120",
                    Enabled = true,
                    PrimaryName = "Maxtime Test",
                    SignalId = "0",
                    ControllerTypeId = 4,
                    ControllerType = new ControllerType() { ControllerTypeId = 4 }
                }
            };

            yield return new object[]
            {
                new Signal()
                {
                    Ipaddress = "10.209.2.108",
                    Enabled = true,
                    PrimaryName = "Cobalt Test",
                    SignalId = "9731",
                    ControllerTypeId = 2,
                    ControllerType = new ControllerType() { ControllerTypeId = 2 }
                }
            };
        }
    }
}
