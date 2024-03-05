namespace WatchDog.Models
{
    public interface IMailService
    {
        void ConnectAndAuthenticate();
        Task<bool> SendEmailAsync(string from, List<ApplicationUser> users, string subject, string body);
    }
}