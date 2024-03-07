namespace Identity.Business.Accounts
{
    public class AccountResult
    {
        public AccountResult(string username, int code, string token, List<string> claims, string? error)
        {
            Username = username;
            //Roles = roles;
            Claims = claims;
            Token = token;
            Code = code;
            Token = token;
            Error = error;
        }
        public string Username { get; set; }
        public List<string> Claims { get; set; }
        public string Token { get; set; }
        public int Code { get; }
        public string? Error { get; }
    }
}