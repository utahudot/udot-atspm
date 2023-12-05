namespace Identity.Business.Accounts
{
    public class AccountResult
    {
        public AccountResult(ApplicationUser user, List<string> roles, string token, int code, string? error)
        {
            User = new UserResult(user.FirstName, user.LastName, user.Email, user.Agency);
            Roles = roles;
            Token = token;
            Code = code;
            Error = error;
        }
        public UserResult User { get; set; }
        public List<string> Roles { get; set; }
        public string Token { get; set; }
        public int Code { get; }
        public string? Error { get; }
    }
}