#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Services/ISignalControllerDecoder.cs
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
using ATSPM.Application.Exceptions;
using ATSPM.Data.Models;
using ATSPM.Data.Models.EventLogModels;
using ATSPM.Domain.Common;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace ATSPM.Application.Services
{
    public interface ILocationControllerDecoder<T> : IExecutableServiceWithProgressAsync<Tuple<Device, FileInfo>, Tuple<Device, T>, ControllerDecodeProgress> where T : EventLogModelBase
    {
        bool IsCompressed(Stream stream);

        bool IsEncoded(Stream stream);

        Stream Decompress(Stream stream);

        /// <exception cref="ControllerLoggerDecoderException"></exception>
        IEnumerable<T> Decode(Device device, Stream stream);
    }
}
