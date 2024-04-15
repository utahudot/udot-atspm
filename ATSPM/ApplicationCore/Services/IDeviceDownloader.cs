using ATSPM.Application.Common;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Domain.Services;
using System;
using System.IO;

namespace ATSPM.Application.Services
{
    /// <summary>
    /// When executed, downloads data from device using the specified <see cref="TransportProtocols"/>
    /// </summary>
    public interface IDeviceDownloader : IExecutableServiceWithProgressAsync<Device, Tuple<Device, FileInfo>, ControllerDownloadProgress>
    {
        /// <inheritdoc>
        TransportProtocols Protocol { get; }
    }
}
