using ATSPM.Application.Common;
using ATSPM.Application.Enums;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Domain.Common;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;
using System.IO;

namespace ATSPM.Application.Services
{
    /// <summary>
    /// When executed, downloads data from device
    /// </summary>
    public interface IDeviceDownloader : IExecutableServiceWithProgressAsync<Device, FileInfo, ControllerDownloadProgress>
    {
        /// <inheritdoc>
        TransportProtocols Protocol { get; }
    }
}
