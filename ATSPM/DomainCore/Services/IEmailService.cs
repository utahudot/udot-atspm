using ATSPM.Domain.Extensions;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;

namespace ATSPM.Domain.Services
{
    /// <summary>
    /// <see cref="IService"/> that allows abstract sending of email messages
    /// </summary>
    public interface IEmailService : IService
    {
        /// <summary>
        /// Send email
        /// </summary>
        /// <param name="message">Composed message to send to service</param>
        /// <returns>True is message sent successfully</returns>
        Task<bool> SendEmailAsync(MailMessage message);
    }

    /// <summary>
    /// Extension methods for <see cref="IEmailService"/>
    /// </summary>
    public static class EmailServiceExtensions
    {
        /// <summary>
        /// Send email
        /// </summary>
        /// <param name="service"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="isHtml"></param>
        /// <param name="priority"></param>
        /// <param name="notificationOptions"></param>
        /// <returns></returns>
        public static async Task<bool> SendEmailAsync(this IEmailService service,
                                                MailAddress from,
                                                MailAddress to,
                                                string subject,
                                                string body,
                                                bool isHtml = false,
                                                MailPriority priority = MailPriority.Normal,
                                                DeliveryNotificationOptions notificationOptions = DeliveryNotificationOptions.None)
        {
            var message = new MailMessage()
            {
                From = from,
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml,
                Priority = priority,
                DeliveryNotificationOptions = notificationOptions
            };

            message.To.Add(to);

            return await service.SendEmailAsync(message);
        }

        /// <summary>
        /// Send email
        /// </summary>
        /// <param name="service"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="isHtml"></param>
        /// <param name="priority"></param>
        /// <param name="notificationOptions"></param>
        /// <returns></returns>
        public static async Task<bool> SendEmailAsync(this IEmailService service,
                                                MailAddress from,
                                                IEnumerable<MailAddress> to,
                                                string subject,
                                                string body,
                                                bool isHtml = false,
                                                MailPriority priority = MailPriority.Normal,
                                                DeliveryNotificationOptions notificationOptions = DeliveryNotificationOptions.None)
        {
            var message = new MailMessage()
            {
                From = from,
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml,
                Priority = priority,
                DeliveryNotificationOptions = notificationOptions
            };

            message.To.AddRange(to);

            return await service.SendEmailAsync(message);
        }
    }
}
