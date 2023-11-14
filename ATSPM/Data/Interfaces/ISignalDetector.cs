namespace ATSPM.Data.Interfaces
{
    /// <summary>
    /// For objects in the signal and detector layer
    /// </summary>
    public interface ISignalDetector : ISignalLayer
    {
        /// <summary>
        /// Detector channel assigned to <see cref="ISignalLayer.SignalIdentifier"/>
        /// </summary>
        int DetectorChannel { get; set; }
    }
}
