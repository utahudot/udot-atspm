#region license
// Copyright 2024 Utah Departement of Transportation
// for DomainCore - ATSPM.Domain.Extensions/IPAddressExtensions.cs
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
using System.Net;
using System.Net.NetworkInformation;

namespace ATSPM.Domain.Extensions
{
    /// <summary>
    /// <see cref="IPAddress"/> extension helpers
    /// </summary>
    public static class IPAddressExtensions
    {
        /// <summary>
        /// Checks to see if ipaddres string is valid
        /// </summary>
        /// <param name="ipaddress">ipaddress string to validate</param>
        /// <param name="ping">True if system should validate by ping</param>
        /// <returns>True if address is valid</returns>
        public static bool IsValidIPAddress(this string ipaddress, bool ping = false)
        {
            if (ipaddress == "0" || ipaddress == "0.0.0.0")
                return false;

            if (IPAddress.TryParse(ipaddress, out IPAddress ip))
            {
                if (ping)
                    return ip.PingIPAddress();
                else
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Checks to see if <see cref="IPAddress"/> is valid
        /// </summary>
        /// <param name="ipaddress"><see cref="IPAddress"/> to validate</param>
        /// <param name="ping">True if system should validate by ping</param>
        /// <returns>True if address is valid</returns>
        public static bool IsValidIPAddress(this IPAddress ipaddress, bool ping = false)
        {
            if (IPAddress.TryParse(ipaddress.ToString(), out IPAddress ip))
            {
                if (ping)
                    return ip.PingIPAddress();
                else
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Pings ip address
        /// </summary>
        /// <param name="ipaddress">IPAddress to ping</param>
        /// <param name="timeout">Ping timeout in milliseconds</param>
        /// <returns>True if ping succeeds</returns>
        public static bool PingIPAddress(this IPAddress ipaddress, int timeout = 5000)
        {
            Ping pingSender = new();
            //byte[] buffer = new byte[32];

            try
            {
                //PingReply reply = pingSender.Send(ipaddress, timeout, buffer, new PingOptions(128, true));
                PingReply reply = pingSender.Send(ipaddress, timeout);
                return reply != null && reply.Status == IPStatus.Success;
            }
            catch
            {
                return false;
            }
        }
    }
}
