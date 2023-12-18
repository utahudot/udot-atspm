namespace Identity.Business.Accounts
{
    public class AccountResult
    {
        public AccountResult(ApplicationUser user, int code, string? error)
        {
            if(user != null)
            {
                User = new UserResult(user.FirstName, user.LastName, user.Email, user.Agency);
            }
            Code = code;
            Error = error;
        }
        public UserResult User { get; set; }
        //public List<string> Roles { get; set; }
        //public string Token { get; set; }
        public int Code { get; }
        public string? Error { get; }
    }
}