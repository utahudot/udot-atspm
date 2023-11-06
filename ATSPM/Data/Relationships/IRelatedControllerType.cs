using ATSPM.Data.Models;

namespace ATSPM.Data.Relationships
{
    /// <summary>
    /// Related controller type
    /// </summary>
    public interface IRelatedControllerType
    {
        /// <summary>
        /// Related controller type
        /// </summary>
        int ControllerTypeId { get; set; }
        
        /// <summary>
        /// Controller type
        /// </summary>
        ControllerType ControllerType { get; set; }
    }
}
