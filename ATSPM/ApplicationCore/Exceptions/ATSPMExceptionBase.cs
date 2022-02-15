﻿using ATSPM.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ATSPM.Application.Exceptions
{
    public abstract class ATSPMExceptionBase : UdotExceptionBase
    {
        /// <summary>
        /// Initializes a new instance of the System.Exception class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ATSPMExceptionBase(string? message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the System.Exception class with a specified error
        /// message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference</param>
        public ATSPMExceptionBase(string? message, Exception? innerException) : base(message, innerException) { }
    }
}
