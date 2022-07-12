using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;

namespace ATSPM.Domain.Extensions
{
    public static class IPAddressExtensions
    {
        public static bool IsValidIPAddress(this string ipaddress, bool ping = false, int timeout = 4000)
        {
            if (ipaddress == "0" || ipaddress == "0.0.0.0")
                return false;

            if (IPAddress.TryParse(ipaddress, out IPAddress ip))
            {
                if (ping)
                {
                    Ping pingSender = new Ping();
                    byte[] buffer = new byte[32];

                    try
                    {
                        PingReply reply = pingSender.Send(ip, timeout, buffer, new PingOptions(128, true));
                        if (reply != null && reply.Status == IPStatus.Success)
                        {
                            return true;
                        }
                    }
                    catch
                    {
                        return false;
                    }
                }
                else
                    return true;
            }

            return false;
        }
    }
}
