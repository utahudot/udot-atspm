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
