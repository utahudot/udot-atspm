#region license
// Copyright 2024 Utah Departement of Transportation
// for DomainCore - ATSPM.Domain.Exceptions/UdotExceptionBase.cs
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
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

#nullable enable

namespace ATSPM.Domain.Exceptions
{
    /// <summary>
    /// Base class for all domain exceptions
    /// </summary>
    public abstract class UdotExceptionBase : Exception
    {
        /// <summary>
        /// Initializes a new instance of the System.Exception class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public UdotExceptionBase(string? message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the System.Exception class with a specified error
        /// message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference</param>
        public UdotExceptionBase(string? message, Exception? innerException) : base(message, innerException) { }
    }
}
