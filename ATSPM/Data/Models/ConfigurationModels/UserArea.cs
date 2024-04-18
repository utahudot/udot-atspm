namespace ATSPM.Data.Models.ConfigurationModels
{
    public class UserArea
    {
        public string UserId { get; set; }
        public int AreaId { get; set; }
        public virtual Area Area { get; set; }
    }

}
