namespace ATSPM.Data.Interfaces
{
    /// <summary>
    /// For objects in the location and approach layer
    /// </summary>
    public interface ILocationApproachLayer : ILocationLayer
    {
        /// <summary>
        /// Id of approach assigned to <see cref="ILocationLayer.LocationIdentifier"/>
        /// </summary>
        int ApproachId { get; set; }
    }
}
