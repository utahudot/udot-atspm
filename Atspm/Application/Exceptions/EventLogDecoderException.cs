#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Exceptions/EventLogDecoderException.cs
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

using Utah.Udot.Atspm.Services;

#nullable enable
namespace Utah.Udot.Atspm.Exceptions
{
    /// <summary>
    /// Exception decoding event log for <see cref="IEventLogDecoder{T}"/> implementations
    /// </summary>
    public class EventLogDecoderException : AtspmException
    {
        /// <summary>
        /// Exception decoding event log for <see cref="IEventLogDecoder{T}"/> implementations
        /// </summary>
        public EventLogDecoderException() : base("Exception decoding stream") { }

        /// <summary>
        /// Exception decoding event log for <see cref="IEventLogDecoder{T}"/> implementations
        /// </summary>
        /// <param name="innerException">Exception thrown by the <see cref="IEventLogDecoder{T}"/> implementation</param>
        public EventLogDecoderException(Exception? innerException) : base("Exception decoding stream", innerException) { }
    }
}
