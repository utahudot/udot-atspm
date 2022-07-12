using System;
using System.Collections.Generic;
using System.Text;

namespace ATSPM.Domain.Exceptions
{
    public class ExecuteException : Exception
    {
        public ExecuteException() : base("Did not pass CanExecute")
        {
        }

        public ExecuteException(string message) : base(message)
        {
        }
    }
}
