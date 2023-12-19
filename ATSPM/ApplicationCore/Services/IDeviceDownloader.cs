using ATSPM.Application.Common;
using ATSPM.Application.Enums;
using ATSPM.Data.Models;
using ATSPM.Domain.Common;
using System;
using System.Collections.Generic;
using System.IO;

namespace ATSPM.Application.Services
{
    /// <summary>
    /// When executed, downloads data from device
    /// </summary>
    public interface IDeviceDownloader : IExecuteWithProgress<Device, IAsyncEnumerable<FileInfo>, ControllerDownloadProgress>, IDisposable
    {
        int ControllerType { get; }

        string[] FileFilters { get; set; }
    }
}
