namespace Identity.Business.Accounts
{
    public class UserResult
    {
        public UserResult(string firstName, string lastName, string email, string agency)
        {
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Agency = agency;
        }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Agency { get; set; }
    }
}