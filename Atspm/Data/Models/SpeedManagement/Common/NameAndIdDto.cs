using ATSPM.Data.Models.ConfigurationModels;

namespace ATSPM.Data.Models.SpeedManagement.Common
{
    public class NameAndIdDto : AtspmConfigModelBase<Guid>
    {
        public string Name { get; set; }
        public Guid Id { get; set; }
    }
}
