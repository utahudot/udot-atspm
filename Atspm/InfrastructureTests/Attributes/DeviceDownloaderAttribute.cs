﻿#region license
// Copyright 2024 Utah Departement of Transportation
// for InfrastructureTests - InfrastructureTests.Attributes/DeviceDownloaderAttribute.cs
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

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Utah.Udot.Atspm.Configuration;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Services;
using Xunit.Sdk;

namespace Utah.Udot.Atspm.InfrastructureTests.Attributes
{
    public class DeviceDownloaderAttribute : DataAttribute
    {
        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            var clients = new List<IDownloaderClient>();

            foreach (var i in Enum.GetValues(typeof(TransportProtocols)))
            {
                clients.Add(Mock.Of<IDownloaderClient>(a => a.Protocol == (TransportProtocols)i, MockBehavior.Strict));
            }

            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(m => m.GetTypes().Where(w => w.GetInterfaces().Contains(typeof(IDeviceDownloader)))).ToList();
            foreach (var t in types)
            {
                yield return new object[] { t, clients, new NullLogger<IDeviceDownloader>(), Mock.Of<IOptionsSnapshot<DeviceDownloaderConfiguration>>() };
            }
        }
    }
}
