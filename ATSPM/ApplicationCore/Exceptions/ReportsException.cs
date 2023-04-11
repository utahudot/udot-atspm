using ATSPM.Data.Models;
using ATSPM.Application.Services.SignalControllerProtocols;
using System;
using System.Collections.Generic;
using System.Text;
using ATSPM.Application.Services;
using System.Threading.Tasks.Dataflow;

#nullable enable
namespace ATSPM.Application.Exceptions
{
    public class ReportsException : ATSPMException
    {
        public ReportsException(string? message) : base(message)
        {
        }

        public ReportsException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }

    public class ReportsNullAgrumentException : ATSPMException
    {
        public ReportsNullAgrumentException(string? message) : base(message)
        {
        }

        public ReportsNullAgrumentException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
