using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;

namespace ControllerLogger.Helpers
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
                {
                    Ping pingSender = new Ping();
                    PingOptions pingOptions = new PingOptions();

                    // Use the default Ttl value which is 128,  
                    // but change the fragmentation behavior. 
                    pingOptions.DontFragment = true;

                    byte[] buffer = new byte[32];
                    int timeout = 120;
                    try
                    {
                        PingReply reply = pingSender.Send(ip, timeout, buffer, pingOptions);
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
