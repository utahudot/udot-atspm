namespace Utah.Udot.Atspm.Infrastructure.Configuration
{
    /// <summary>
    /// Options pattern model for services that implement <see cref="IDeviceDownloader"/>
    /// </summary>
    public class DeviceDownloaderConfiguration
    {
        /// <summary>
        /// Base path to store downloaded event logs
        /// </summary>
        public string BasePath { get; set; }

        /// <summary>
        /// Flag for deleting remote file after downloading
        /// </summary>
        public bool DeleteRemoteFile { get; set; }

        /// <summary>
        /// Flag to ping <see cref="Device"/> to verify <see cref="Device.Ipaddress"/> before downloading
        /// </summary>
        public bool Ping { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{BasePath} - {DeleteRemoteFile} - {Ping}";
        }
    }
}
