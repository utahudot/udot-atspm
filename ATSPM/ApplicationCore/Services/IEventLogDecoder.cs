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
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;
using System.IO;

namespace ATSPM.Application.Services
{
    /// <summary>
    /// When executed, decodes event log files to their corresponding <see cref="EventLogModelBase"/> data model
    /// </summary>
    /// <typeparam name="T"><see cref="EventLogModelBase"/> data model</typeparam>
    public interface IEventLogDecoder<T> : IExecutableServiceWithProgressAsync<Tuple<Device, FileInfo>, Tuple<Device, T>, ControllerDecodeProgress> where T : EventLogModelBase
    {
        /// <summary>
        /// Checks to see if the data stream is compressed
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        bool IsCompressed(Stream stream);

        /// <summary>
        /// Checks to see if the data stream is encoded (not plain ascii text)
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        bool IsEncoded(Stream stream);

        /// <summary>
        /// Decompresses data if compressed
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        Stream Decompress(Stream stream);

        /// <summary>
        /// Decodes the data stream to an <see cref="IEnumerable{T}"/>
        /// </summary>
        /// <param name="device">Device that generated the data stream</param>
        /// <param name="stream">Event data</param>
        /// <returns></returns>
        /// <exception cref="ControllerLoggerDecoderException"></exception>
        /// <exception cref="ArgumentNullException">Thrown if location is unknown</exception>
        /// <exception cref="InvalidDataException">Thrown if stream is empty</exception>
        IEnumerable<T> Decode(Device device, Stream stream);
    }
}
