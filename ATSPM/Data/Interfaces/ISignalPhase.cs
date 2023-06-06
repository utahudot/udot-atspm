namespace ATSPM.Data.Interfaces
{
    /// <summary>
    /// For objects in the signal and phase layer
    /// </summary>
    public interface ISignalPhaseLayer : ISignalLayer
    {
        /// <summary>
        /// Phase number assigned to <see cref="ISignalLayer.SignalIdentifier"/>
        /// </summary>
        int PhaseNumber { get; set; }
    }
}
