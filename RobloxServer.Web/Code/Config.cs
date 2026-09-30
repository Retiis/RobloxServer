using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Hosting;

namespace RobloxServer
{
    /// <summary>
    /// Typed access to the &lt;appSettings&gt; section of Web.config.
    /// Every value has a default so the site boots with an empty appSettings block.
    /// </summary>
    public static class Config
    {
        static string Get(string key, string fallback)
        {
            string value = ConfigurationManager.AppSettings[key];
            return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        }

        static int GetInt(string key, int fallback)
        {
            int value;
            return int.TryParse(Get(key, ""), out value) ? value : fallback;
        }

        static bool GetBool(string key, bool fallback)
        {
            bool value;
            return bool.TryParse(Get(key, ""), out value) ? value : fallback;
        }

        static string[] GetList(string key)
        {
            return Get(key, "")
                .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => s.Length > 0)
                .ToArray();
        }

        public static string SiteName { get { return Get("SiteName", "ROBLOX"); } }

        /// <summary>
        /// The URL clients use, with a trailing slash. Empty (the default) means "the address this
        /// request came in on", so the site works as http://192.168.1.2/, http://robloxserver.lan/
        /// or a DDNS name without configuration. Set it to pin one address in every script.
        /// </summary>
        public static string BaseUrl
        {
            get
            {
                string url = Get("BaseUrl", "");
                if (url.Length == 0)
                {
                    url = RequestBaseUrl();
                }
                return url.EndsWith("/") ? url : url + "/";
            }
        }

        /// <summary>Base for the api.roblox.com style endpoints. Defaults to BaseUrl.</summary>
        public static string ApiUrl
        {
            get
            {
                string url = Get("ApiUrl", "");
                if (url.Length == 0)
                {
                    return BaseUrl;
                }
                return url.EndsWith("/") ? url : url + "/";
            }
        }

        static string RequestBaseUrl()
        {
            HttpContext context = HttpContext.Current;
            if (context == null)
            {
                return "http://localhost/";
            }

            HttpRequest request;
            try
            {
                request = context.Request;
            }
            catch (HttpException)
            {
                // Application_Start has no request.
                return "http://localhost/";
            }

            string host = request.Headers["Host"];
            if (string.IsNullOrEmpty(host) || !Regex.IsMatch(host, @"^[A-Za-z0-9.\-]+(:[0-9]{1,5})?$|^\[[0-9A-Fa-f:.]+\](:[0-9]{1,5})?$"))
            {
                host = request.Url.Authority;
            }

            string scheme = request.Url.Scheme;
            string forwardedProto = request.Headers["X-Forwarded-Proto"];
            if ((forwardedProto == "https" || forwardedProto == "http") && TrustedProxies.Contains(request.UserHostAddress ?? ""))
            {
                scheme = forwardedProto;
            }

            return scheme + "://" + host + "/";
        }

        /// <summary>RCC-style shared secret passed as ?apiKey= by game servers (2015 behaviour).</summary>
        public static string ApiKey { get { return Get("ApiKey", ""); } }

        public static bool RegistrationOpen { get { return GetBool("RegistrationOpen", true); } }

        /// <summary>User names that are site administrators ("Roblox" staff).</summary>
        public static string[] Administrators { get { return GetList("Administrators"); } }

        /// <summary>Who may host a game server: Anyone, Owner or Admin.</summary>
        public static string HostPolicy { get { return Get("HostPolicy", "Anyone"); } }

        /// <summary>When true a game server must receive a valid authentication ticket from every player.</summary>
        public static bool RequireAuthTickets { get { return GetBool("RequireAuthTickets", true); } }

        public static int AuthTicketMinutes { get { return GetInt("AuthTicketMinutes", 5); } }

        public static string[] AllowedSecurityVersions { get { return GetList("AllowedSecurityVersions"); } }

        public static string[] AllowedMD5Hashes { get { return GetList("AllowedMD5Hashes"); } }

        public static int RateLimitRequests { get { return GetInt("RateLimitRequests", 45); } }

        public static int RateLimitWindowSeconds { get { return GetInt("RateLimitWindowSeconds", 30); } }

        public static int LoginFloodAttempts { get { return GetInt("LoginFloodAttempts", 5); } }

        public static int LoginFloodMinutes { get { return GetInt("LoginFloodMinutes", 5); } }

        /// <summary>Reverse proxies (the Raspberry Pi) whose X-Forwarded-For header is trusted.</summary>
        public static string[] TrustedProxies
        {
            get
            {
                var list = new List<string>(GetList("TrustedProxies"));
                list.Add("127.0.0.1");
                list.Add("::1");
                return list.ToArray();
            }
        }

        /// <summary>
        /// Public IP or DNS name of the network (the Pi's WAN address). Game servers hosted on the
        /// LAN are shown with this address to players outside the LAN.
        /// </summary>
        public static string PublicGameAddress { get { return Get("PublicGameAddress", ""); } }

        public static int MaxPlaceSizeMegabytes { get { return GetInt("MaxPlaceSizeMegabytes", 50); } }

        /// <summary>Asset ids handed out to places uploaded here. Starts high to avoid colliding with real Roblox assets.</summary>
        public static long FirstLocalAssetId { get { return GetInt("FirstLocalAssetId", 1900000000); } }

        /// <summary>None or Roblox. Roblox resolves unknown asset ids through assetdelivery.roblox.com like the old Node server.</summary>
        public static string AssetFallback { get { return Get("AssetFallback", "Roblox"); } }

        /// <summary>The only clients RobloxPlayerLauncher knows how to install and start.</summary>
        public static readonly string[] SupportedClients = { "2012M", "2013M" };

        /// <summary>Clients games can be published for: the "Clients" setting, limited to SupportedClients.</summary>
        public static string[] Clients
        {
            get
            {
                string[] clients = GetList("Clients")
                    .Select(c => SupportedClients.FirstOrDefault(s => string.Equals(s, c, StringComparison.OrdinalIgnoreCase)))
                    .Where(c => c != null)
                    .Distinct()
                    .ToArray();
                return clients.Length > 0 ? clients : SupportedClients.ToArray();
            }
        }

        public static string DefaultClient
        {
            get
            {
                string client = Get("DefaultClient", "2012M");
                return Clients.FirstOrDefault(c => string.Equals(c, client, StringComparison.OrdinalIgnoreCase)) ?? Clients[0];
            }
        }

        /// <summary>Where "Download ROBLOX" goes when App_Data/Launcher/RobloxPlayerLauncher.exe does not exist.</summary>
        public static string LauncherDownloadUrl
        {
            get { return Get("LauncherDownloadUrl", "https://github.com/V0rtexLinux/RobloxServerLauncher/releases/latest"); }
        }

        public static string DataPath
        {
            get { return HostingEnvironment.MapPath("~/App_Data") ?? AppDomain.CurrentDomain.BaseDirectory; }
        }
    }
}
