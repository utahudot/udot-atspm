namespace ATSPM.Data.Enums
{
    /// <summary>
    /// Transport protocols used for connecting to devices
    /// </summary>
    public enum TransportProtocols
    {
        /// <summary>
        /// Unknown protocol
        /// </summary>
        Unknown,
        
        /// <summary>
        /// Supports FTP protocol
        /// </summary>
        Ftp,
        
        /// <summary>
        /// Supports SFTP Protocol
        /// </summary>
        Sftp,
        
        /// <summary>
        /// Supports SNMP Protocol
        /// </summary>
        Snmp,
        
        /// <summary>
        /// Supports HTTP Protocol
        /// </summary>
        Http
    }
}
