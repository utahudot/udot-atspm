using ATSPM.Application.Common;
using ATSPM.Application.Enums;
using ATSPM.Data.Models;
using ATSPM.Domain.Common;
using System;
using System.Collections.Generic;
using System.IO;

namespace ATSPM.Application.Services.SignalControllerProtocols
{
    public interface ISignalControllerDownloader : IExecuteWithProgress<Location, IAsyncEnumerable<FileInfo>, ControllerDownloadProgress>, IDisposable
    {
        int ControllerType { get; }

        string[] FileFilters { get; set; }
    }
}
