#region license
// Copyright 2025 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Services.EventLogDecoders/EventLogDecoderBase.cs
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

namespace Utah.Udot.Atspm.Infrastructure.Services.EventLogDecoders
{
    ///<inheritdoc cref="IEventLogDecoder{T}"/>
    public abstract class EventLogDecoderBase<T> : IEventLogDecoder<T> where T : EventLogModelBase
    {
        /// <inheritdoc/>
        public virtual bool IsCompressed(Stream stream)
        {
            return stream.IsCompressed();
        }

        /// <inheritdoc/>
        public virtual bool IsEncoded(Stream stream)
        {
            MemoryStream memoryStream = (MemoryStream)stream;
            var bytes = memoryStream.ToArray();

            //ASCII doesn't have anything above 0x80
            return bytes.Any(b => b >= 0x80);
        }

        /// <inheritdoc/>
        public virtual Stream Decompress(Stream stream)
        {
            return stream.GZipDecompressToStream();
        }

        /// <inheritdoc/>
        public abstract IEnumerable<T> Decode(Device device, Stream stream, CancellationToken cancelToken = default);

        /// <inheritdoc/>
        IEnumerable<EventLogModelBase> IEventLogDecoder.Decode(Device device, Stream stream, CancellationToken cancelToken)
        {
            return Decode(device, stream, cancelToken);
        }
    }
}
