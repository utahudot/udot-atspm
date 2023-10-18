namespace ATSPM.Data.Models
{
    public class UserArea
    {
        public string UserId { get; set; }
        //public ApplicationUser User { get; set; }

        public int AreaId { get; set; }
        public Area Area { get; set; }
    }


}
