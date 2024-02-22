namespace Identity.Business.Users
{
    public class UserDTO
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Agency { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string UserId { get; set; }
        public string FullName { get { return $"{FirstName} {LastName}"; } }
        public ICollection<string> Roles { get; set; }
    }
}
