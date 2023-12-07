namespace Identity.Business.Common
{
    public class BaseApiResponse
    {
        public BaseApiResponse(int code, string? error)
        {
            Code = code;
            Error = error.Length != 0 ? error : "";
        }
        public int Code { get; set; }
        public string? Error { get; set; }

    }
}