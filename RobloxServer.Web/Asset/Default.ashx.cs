using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using RobloxServer.Data;
using RobloxServer.Security;
using RobloxServer.Web;

namespace RobloxServer.Handlers.Asset
{
    /// <summary>
    /// /asset/?id= and /Asset/?id=. Order: places uploaded to this server, admin overrides in
    /// App_Data/Assets, then (like the old Node server) a redirect to Roblox's asset delivery.
    /// </summary>
    public class Default : HandlerBase
    {
        protected override void Handle()
        {
            long id = QueryLong("id");
            if (id == 0)
            {
                id = QueryLong("assetversionid");
            }
            if (id == 0)
            {
                id = QueryLong("placeid");
            }
            if (id <= 0)
            {
                WriteStatus(400, "Invalid asset id");
                return;
            }

            Place place = Db.FindPlace(id);
            if (place != null)
            {
                ServePlace(place);
                return;
            }

            string overridePath = Path.Combine(Db.AssetsPath, id.ToString());
            if (File.Exists(overridePath))
            {
                Response.ContentType = "application/octet-stream";
                Response.TransmitFile(overridePath);
                return;
            }

            if (string.Equals(Config.AssetFallback, "Roblox", StringComparison.OrdinalIgnoreCase))
            {
                string location = ResolveRobloxAsset(id);
                if (location != null)
                {
                    Response.Redirect(location, false);
                    return;
                }
            }

            WriteStatus(404, "Asset not found");
        }

        void ServePlace(Place place)
        {
            if (!CanSee(CurrentUser, place) && !IsGameServerRequest())
            {
                WriteStatus(403, "You do not have permission to access this place.");
                return;
            }

            string path = Db.PlaceFilePath(place);
            if (!File.Exists(path))
            {
                WriteStatus(404, "Place file missing");
                return;
            }

            Response.ContentType = "application/octet-stream";
            Response.AppendHeader("Content-Disposition", "attachment; filename=\"" + place.Id + "." + (place.FileExtension ?? "rbxl") + "\"");
            Response.AppendHeader("X-Place-Version", place.Version.ToString());
            Response.AppendHeader("X-Place-Md5", place.Md5 ?? "");
            Response.TransmitFile(path);
        }

        bool IsGameServerRequest()
        {
            // A registered game server may always load its own place (like RCC loading a private place).
            GameServer server = GameServers.Find(Request.QueryString["jobId"]);
            return server != null
                && PasswordHasher.ConstantTimeEquals(server.ServerKey, Request.QueryString["serverKey"] ?? "");
        }

        /// <summary>Asks assetdelivery.roblox.com for the CDN location, like routes/assets.js did.</summary>
        static string ResolveRobloxAsset(long id)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create("https://assetdelivery.roblox.com/v2/assetId/" + id);
                request.UserAgent = "Roblox/WinInet";
                request.Timeout = 10000;
                using (var response = (HttpWebResponse)request.GetResponse())
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    string json = reader.ReadToEnd();
                    Match match = Regex.Match(json, "\"location\"\\s*:\\s*\"(?<url>[^\"]+)\"");
                    return match.Success ? Regex.Unescape(match.Groups["url"].Value) : null;
                }
            }
            catch (Exception ex)
            {
                Logging.Log(LogType.Error, "Asset " + id + " could not be resolved: " + ex.Message);
                return null;
            }
        }
    }
}
