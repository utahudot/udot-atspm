namespace WatchDog.Models
{
    public interface IMailService
    {
        Task<bool> SendEmailAsync(string from, List<ApplicationUser> users, string subject, string body);
    }
}