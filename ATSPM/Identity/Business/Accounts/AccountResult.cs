namespace Identity.Business.Accounts
{
    public class AccountResult
    {
        public AccountResult(string username, int code, string token, string? error)
        {
            Username = username;
            Code = code;
            Token = token;
            Error = error;
        }
        public string Username { get; set; }
        //public List<string> Roles { get; set; }
        public string Token { get; set; }
        public int Code { get; }
        public string? Error { get; }
    }
}