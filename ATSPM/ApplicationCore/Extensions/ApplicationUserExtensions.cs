using ATSPM.Data;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;

namespace ATSPM.Application.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="ApplicationUser"/>
    /// </summary>
    public static class ApplicationUserExtensions
    {
        /// <summary>
        /// Returns a <see cref="MailAddress"/> containing the users mailing address
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static MailAddress GetMailingAddress(this ApplicationUser user)
        {
            return new MailAddress(user.Email, user.FullName);
        }

        /// <summary>
        /// Returns a <see cref="IReadOnlyList{MailAddress}"/> containing the users mailing addresses
        /// </summary>
        /// <param name="users"></param>
        /// <returns></returns>
        public static IReadOnlyList<MailAddress> GetMailingAddresses(this IEnumerable<ApplicationUser> users)
        {
            return users.Select(s => new MailAddress(s.Email, s.FullName)).ToList();    
        }
    }
}
