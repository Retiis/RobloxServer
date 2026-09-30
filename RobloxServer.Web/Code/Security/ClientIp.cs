using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Web;

namespace RobloxServer.Security
{
    public static class ClientIp
    {
        /// <summary>
        /// The caller's IP. X-Forwarded-For is only honoured when the request comes from a trusted
        /// proxy (the Raspberry Pi running nginx), otherwise anyone could spoof it past IP bans.
        /// </summary>
        public static string Get(HttpRequest request)
        {
            string remote = request.UserHostAddress ?? "";
            string forwarded = request.Headers["X-Forwarded-For"];

            if (!string.IsNullOrEmpty(forwarded) && Config.TrustedProxies.Contains(remote))
            {
                string first = forwarded.Split(',').Last().Trim();
                IPAddress parsed;
                if (IPAddress.TryParse(first, out parsed))
                {
                    return parsed.ToString();
                }
            }

            return remote;
        }

        public static string Get(HttpContext context)
        {
            return Get(context.Request);
        }

        /// <summary>
        /// The 2012M/2013M clients only speak IPv4. Turns "::1" (what Windows and IIS Express report for
        /// localhost) into 127.0.0.1 and "::ffff:192.168.1.5" into 192.168.1.5; anything else is unchanged.
        /// </summary>
        public static string ToGameAddress(string ip)
        {
            IPAddress address;
            if (string.IsNullOrWhiteSpace(ip) || !IPAddress.TryParse(ip.Trim(), out address))
            {
                return ip;
            }
            if (address.AddressFamily == AddressFamily.InterNetworkV6)
            {
                if (IPAddress.IsLoopback(address))
                {
                    return IPAddress.Loopback.ToString();
                }
                if (address.IsIPv4MappedToIPv6)
                {
                    return address.MapToIPv4().ToString();
                }
            }
            return address.ToString();
        }

        public static bool IsLoopback(string ip)
        {
            IPAddress address;
            return !string.IsNullOrWhiteSpace(ip) && IPAddress.TryParse(ip.Trim(), out address) && IPAddress.IsLoopback(address);
        }

        /// <summary>RFC 1918 / loopback / link-local / CGNAT: addresses that are not reachable from the Internet.</summary>
        public static bool IsPrivate(string ip)
        {
            IPAddress address;
            if (string.IsNullOrEmpty(ip) || !IPAddress.TryParse(ip, out address))
            {
                return false;
            }

            if (IPAddress.IsLoopback(address))
            {
                return true;
            }

            if (address.AddressFamily == AddressFamily.InterNetworkV6)
            {
                if (address.IsIPv4MappedToIPv6)
                {
                    return IsPrivate(address.MapToIPv4().ToString());
                }
                return address.IsIPv6LinkLocal || address.IsIPv6SiteLocal || (address.GetAddressBytes()[0] & 0xFE) == 0xFC;
            }

            byte[] b = address.GetAddressBytes();
            return b[0] == 10
                || (b[0] == 172 && b[1] >= 16 && b[1] <= 31)
                || (b[0] == 192 && b[1] == 168)
                || (b[0] == 169 && b[1] == 254)
                || (b[0] == 100 && b[1] >= 64 && b[1] <= 127);
        }
    }
}
