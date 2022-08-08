using ATSPM.Data.Models;
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
                    IPAddress = "10.209.2.120",
                    Enabled = true,
                    PrimaryName = "Maxtime Test",
                    SignalId = "0",
                    ControllerTypeID = 4,
                    ControllerType = new ControllerType() { ControllerTypeID = 4 }
                }
            };

            yield return new object[]
            {
                new Signal()
                {
                    IPAddress = "10.209.2.108",
                    Enabled = true,
                    PrimaryName = "Cobalt Test",
                    SignalId = "9731",
                    ControllerTypeID = 2,
                    ControllerType = new ControllerType() { ControllerTypeID = 2 }
                }
            };
        }
    }
}
