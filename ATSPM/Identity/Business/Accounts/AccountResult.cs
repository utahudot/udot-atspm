namespace Identity.Business.Accounts
{
    public class AccountResult
    {
        public AccountResult(int code, string token, List<string> claims, string? message)
        {
            Claims = claims;
            Token = token;
            Code = code;
            Token = token;
            Message = message;
        }
        public List<string> Claims { get; set; }
        public string Token { get; set; }
        public int Code { get; }
        public string? Message { get; set; }
    }
}