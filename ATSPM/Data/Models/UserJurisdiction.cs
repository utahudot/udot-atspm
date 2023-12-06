namespace ATSPM.Data.Models
{
    public class UserJurisdiction
    {
        public string UserId { get; set; }
        public int JurisdictionId { get; set; }
        public Jurisdiction Jurisdiction { get; set; }
    }
}
