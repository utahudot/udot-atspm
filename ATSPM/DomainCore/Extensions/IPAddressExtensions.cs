using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace ATSPM.Domain.Extensions
{
    public static class IPAddressExtensions
    {
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

        public static bool PingIPAddress(this IPAddress ipaddress, int timeout = 4000)
        {
            Ping pingSender = new Ping();
            byte[] buffer = new byte[32];

            try
            {
                PingReply reply = pingSender.Send(ipaddress, timeout, buffer, new PingOptions(128, true));
                return reply != null && reply.Status == IPStatus.Success;
            }
            catch
            {
                return false;
            }
        }
    }
}
