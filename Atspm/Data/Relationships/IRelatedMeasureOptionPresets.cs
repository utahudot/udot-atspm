using Utah.Udot.Atspm.Data.Interfaces;
using Utah.Udot.Atspm.Data.Models.MeasureOptions;

namespace Utah.Udot.Atspm.Data.Relationships
{
    /// <summary>
    /// Related option presets
    /// </summary>
    public interface IRelatedMeasureOptionPresets
    {
        /// <summary>
        /// Collection of measure option presets
        /// </summary>
        ICollection<MeasureOptionPreset> MeasureOptionPresets { get; set; }
    }
}
