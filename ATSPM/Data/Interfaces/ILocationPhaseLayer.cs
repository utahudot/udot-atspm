namespace ATSPM.Data.Interfaces
{
    /// <summary>
    /// For objects in the location and phase layer
    /// </summary>
    public interface ILocationPhaseLayer : ILocationLayer
    {
        /// <summary>
        /// Phase number assigned to <see cref="ILocationLayer.LocationIdentifier"/>
        /// </summary>
        int PhaseNumber { get; set; }
    }
}
