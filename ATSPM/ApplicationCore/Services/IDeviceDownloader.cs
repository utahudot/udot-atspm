#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Services/IDeviceDownloader.cs
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
using ATSPM.Application.Common;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Domain.Services;
using System;
using System.IO;

namespace ATSPM.Application.Services
{
    /// <summary>
    /// When executed, downloads data from device using the specified <see cref="TransportProtocols"/>
    /// </summary>
    public interface IDeviceDownloader : IExecutableServiceWithProgressAsync<Device, Tuple<Device, FileInfo>, ControllerDownloadProgress>
    {
        ///<inheritdoc/>
        TransportProtocols Protocol { get; }
    }
}
