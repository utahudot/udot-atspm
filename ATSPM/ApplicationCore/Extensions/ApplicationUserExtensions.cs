#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Extensions/ApplicationUserExtensions.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion
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
