using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.Common;

namespace Utah.Udot.Atspm.Data.Models.SpeedManagementModels.SpeedVariability
{
    public class SpeedVariabilityDto : BaseSpeedResultDto
    {
        public List<SpeedVariabilityDataDto> Data {  get; set; }
    }

    public class SpeedVariabilityDataDto
    {
        public DateTime Date { get; set; }
        public double MinSpeed { get; set; }
        public double MaxSpeed { get; set; }
        public double AvgSpeed { get; set; }
        public double SpeedVariability { get; set; }

    }
}
