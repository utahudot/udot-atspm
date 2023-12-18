namespace ATSPM.Data.Interfaces
{
    /// <summary>
    /// For objects in the location and detector layer
    /// </summary>
    public interface ILocationDetectorLayer : ILocationLayer
    {
        /// <summary>
        /// Detector channel assigned to <see cref="ILocationLayer.LocationIdentifier"/>
        /// </summary>
        int DetectorChannel { get; set; }
    }
}
