namespace ATSPM.Data.Models
{
    public class UserRegion
    {
        public string UserId { get; set; }
        public int RegionId { get; set; }
        public Region Region { get; set; }
    }
}
