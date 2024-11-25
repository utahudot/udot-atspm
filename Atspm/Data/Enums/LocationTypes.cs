using System.ComponentModel.DataAnnotations;

namespace Utah.Udot.Atspm.Data.Enums
{
    public enum LocationTypes
    {
        /// <summary>
        /// Intersection
        /// </summary>
        [Display(Name = "Intersection", Order = 0)]
        I = 1,

        /// <summary>
        /// Ramp Meter
        /// </summary>
        [Display(Name = "Ramp Meter")]
        RM = 2,
    }
}
