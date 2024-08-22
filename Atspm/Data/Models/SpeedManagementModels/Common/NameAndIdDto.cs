namespace Utah.Udot.Atspm.Data.Models.SpeedManagementModels.Common
{
    public class NameAndIdDto : AtspmConfigModelBase<Guid>
    {
        public string Name { get; set; }
        public Guid Id { get; set; }
    }
}
