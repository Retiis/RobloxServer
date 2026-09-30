using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Hosting;

namespace RobloxServer.Web
{
    /// <summary>
    /// The 2015 client asks for /Game/Visit.ashx, /game/visit.ashx, /asset/?id=, /GetAllowedMD5Hashes/ ...
    /// IIS is case insensitive but Mono/XSP on the Raspberry Pi is not, and some URLs have no
    /// extension at all. Global.asax asks this class where a request should really go and uses
    /// Context.RewritePath, so every handler still lives in a normal .ashx/.aspx file.
    /// </summary>
    public static class Routing
    {
        static readonly object Sync = new object();
        static Dictionary<string, string> routes;

        /// <summary>Legacy and extensionless URLs. Keys are lower case, without trailing slash.</summary>
        static readonly Dictionary<string, string> Aliases = new Dictionary<string, string>
        {
            { "/asset", "~/Asset/Default.ashx" },
            { "/asset/default.aspx", "~/Asset/Default.ashx" },
            { "/game/getauthticket", "~/Game/GetAuthTicket.ashx" },
            { "/game/logout.aspx", "~/Game/Logout.ashx" },
            { "/game/join.ashx", "~/Game/Join.ashx" },
            { "/game/placelauncher.ashx", "~/Game/PlaceLauncher.ashx" },
            { "/error/grid.ashx", "~/Error/Lua.ashx" },
            { "/error/dmp.ashx", "~/Error/Lua.ashx" },
            { "/v1.1/counters/increment", "~/Analytics/Counters.ashx" },
            { "/v1.0/multiincrement", "~/Analytics/Counters.ashx" },
            { "/getallowedsecurityversions", "~/Security/AllowedSecurityVersions.ashx" },
            { "/getallowedmd5hashes", "~/Security/AllowedMD5Hashes.ashx" },
            { "/list.php", "~/MasterServer/List.ashx" },
            { "/delist.php", "~/MasterServer/Delist.ashx" },
            { "/serverlist.txt", "~/MasterServer/ServerList.ashx" },
            { "/games", "~/Default.aspx" },
            { "/home", "~/My/Home.aspx" },
            { "/my/character", "~/My/Character.aspx" },
            { "/landing", "~/Landing.aspx" },
            { "/newlogin", "~/Login.aspx" },
            { "/login/default.aspx", "~/Login.aspx" },
            { "/ide/publish", "~/Develop.aspx" },
            { "/develop", "~/Develop.aspx" },
            { "/people", "~/Browse.aspx" },
            { "/void", "~/Void.aspx" },
            { "/status", "~/Api/Status.ashx" },
            { "/install/version", "~/Install/Version.ashx" },
            { "/install/download", "~/Install/Download.ashx" }
        };

        static Dictionary<string, string> Routes
        {
            get
            {
                lock (Sync)
                {
                    if (routes == null)
                    {
                        routes = Build();
                    }
                    return routes;
                }
            }
        }

        static Dictionary<string, string> Build()
        {
            var table = new Dictionary<string, string>(StringComparer.Ordinal);
            string root = HostingEnvironment.ApplicationPhysicalPath;

            // Every handler and page, reachable in any letter case.
            foreach (string file in Directory.GetFiles(root, "*.*", SearchOption.AllDirectories))
            {
                string extension = Path.GetExtension(file).ToLowerInvariant();
                if (extension != ".ashx" && extension != ".aspx")
                {
                    continue;
                }

                string relative = file.Substring(root.Length).Replace('\\', '/').TrimStart('/');
                if (relative.StartsWith("bin/", StringComparison.OrdinalIgnoreCase) || relative.StartsWith("obj/", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                table["/" + relative.ToLowerInvariant()] = "~/" + relative;
            }

            foreach (var alias in Aliases)
            {
                table[alias.Key] = alias.Value;
            }

            return table;
        }

        /// <summary>
        /// Returns the app relative virtual path (~/...) the request should be rewritten to, or null
        /// to leave the request alone. <paramref name="extraQuery"/> is appended to the query string.
        /// </summary>
        public static string Resolve(string appRelativePath, out string extraQuery)
        {
            extraQuery = null;

            appRelativePath = Regex.Replace(appRelativePath ?? "/", "/{2,}", "/");
            string path = appRelativePath.ToLowerInvariant();

            // Mono's FastCGI server turns "/GetAllowedMD5Hashes/" into "/GetAllowedMD5Hashes/Default.aspx".
            const string defaultDocument = "/default.aspx";
            if (path.EndsWith(defaultDocument) && path.Length > defaultDocument.Length && !Routes.ContainsKey(path))
            {
                path = path.Substring(0, path.Length - defaultDocument.Length);
                appRelativePath = appRelativePath.Substring(0, appRelativePath.Length - defaultDocument.Length);
            }
            if (path.Length > 1 && path.EndsWith("/"))
            {
                path = path.TrimEnd('/');
            }

            // /Setting/QuietGet/ClientAppSettings/ -> QuietGet.ashx?name=ClientAppSettings
            const string settingPrefix = "/setting/quietget/";
            if (path.StartsWith(settingPrefix) || path.StartsWith("/setting/get/"))
            {
                string name = path.Substring(path.IndexOf('/', 9) + 1);
                string original = appRelativePath.TrimEnd('/');
                extraQuery = "name=" + HttpUtility.UrlEncode(original.Substring(original.Length - name.Length));
                return "~/Setting/QuietGet.ashx";
            }

            string target;
            return Routes.TryGetValue(path, out target) ? target : null;
        }

        /// <summary>Paths that are never rate limited (asset downloads happen in bursts while a place loads).</summary>
        public static bool IsRateLimitExempt(string appRelativePath)
        {
            string path = (appRelativePath ?? "").ToLowerInvariant();
            string[] exempt = { "/asset", "/content/", "/error/", "/v1.1/counters", "/v1.0/multiincrement", "/game/clientpresence", "/game/validateticket", "/game/servers", "/install/download", "/favicon.ico" };
            return exempt.Any(e => path.StartsWith(e));
        }
    }
}
