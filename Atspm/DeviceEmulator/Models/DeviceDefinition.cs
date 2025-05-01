namespace DeviceEmulator.Models
{
    public class DeviceDefinition
    {
        public string DeviceIdentifier { get; set; } = default!;
        public string Protocol { get; set; } = default!; // ftp, sftp, http, https
        public string IpAddress { get; set; } = default!;
        public int Port { get; set; }
        public string LogDirectory { get; set; } = "/data"; // Where to store device logs
        public bool UseCompression { get; set; } = false;

    }
}
