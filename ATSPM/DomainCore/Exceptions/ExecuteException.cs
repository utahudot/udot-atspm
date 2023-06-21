using ATSPM.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace ATSPM.Domain.Exceptions
{
    /// <summary>
    /// Used with <see cref="IExecute"/> when <c>CanExecute</c> is false
    /// </summary>
    public class ExecuteException : Exception
    {
        /// <summary>
        /// Used with <see cref="IExecute"/> when <c>CanExecute</c> is false
        /// </summary>
        public ExecuteException() : base("Did not pass CanExecute")
        {
        }

        /// <summary>
        /// Used with <see cref="IExecute"/> when <c>CanExecute</c> is false
        /// </summary>
        /// <param name="message">Enter custom <c>CanExecute</c> failed message</param>
        public ExecuteException(string message) : base(message)
        {
        }
    }
}
