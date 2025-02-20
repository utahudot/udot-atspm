#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Services/IEventLogDecoder.cs
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

using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Exceptions;

namespace Utah.Udot.Atspm.Services
{
    /// <summary>
    /// Decodes event log files to their corresponding <see cref="EventLogModelBase"/> data model
    /// </summary>
    public interface IEventLogDecoder
    {
        ///// <summary>
        ///// Checks to see if stream can be decoded by this deocder
        ///// </summary>
        ///// <param name="device"></param>
        ///// <param name="stream"></param>
        ///// <returns></returns>
        //bool CanDecode(Device device, Stream stream);

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
        /// Decodes the data stream to an <see cref="IEnumerable{EventLogModelBase}"/>
        /// </summary>
        /// <param name="device">Device that generated the data stream</param>
        /// <param name="stream">Event data</param>
        /// <param name="cancelToken">Cancellation token</param>
        /// <returns></returns>
        /// <exception cref="EventLogDecoderException">Thrown when the decoding process catches an error</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="device"/> is null</exception>
        /// <exception cref="InvalidDataException">Thrown if <paramref name="stream"/> is empty</exception>
        /// <exception cref="OperationCanceledException">Thrown on <paramref name="cancelToken"/> cancelled</exception>
        IEnumerable<EventLogModelBase> Decode(Device device, Stream stream, CancellationToken cancelToken = default);
    }

    /// <summary>
    /// Decodes event log files to their corresponding <see cref="EventLogModelBase"/> data model
    /// </summary>
    public interface IEventLogDecoder<T> : IEventLogDecoder where T : EventLogModelBase
    {
        /// <summary>
        /// Decodes the data stream to an <see cref="IEnumerable{T}"/>
        /// </summary>
        /// <param name="device">Device that generated the data stream</param>
        /// <param name="stream">Event data</param>
        /// <param name="cancelToken">Cancellation token</param>
        /// <returns></returns>
        /// <exception cref="EventLogDecoderException">Thrown when the decoding process catches an error</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="device"/> is null</exception>
        /// <exception cref="InvalidDataException">Thrown if <paramref name="stream"/> is empty</exception>
        /// <exception cref="OperationCanceledException">Thrown on <paramref name="cancelToken"/> cancelled</exception>
        new IEnumerable<T> Decode(Device device, Stream stream, CancellationToken cancelToken = default);
    }
}
