using System;
using RobloxServer.Data;
using RobloxServer.Security;
using RobloxServer.Web;

namespace RobloxServer.Handlers.MasterServer
{
    /// <summary>
    /// Novetus master server compatibility (list.php): ?name=&amp;ip=&amp;port=&amp;client=&amp;version=&amp;id=[&amp;placeid=].
    /// Legacy servers have no server key, so they cannot validate authentication tickets.
    /// </summary>
    public class List : HandlerBase
    {
        protected override void Handle()
        {
            string name = Request.QueryString["name"];
            string ip = Request.QueryString["ip"];
            string client = Request.QueryString["client"];
            string version = Request.QueryString["version"];
            string id = Request.QueryString["id"];
            int port;

            if (!int.TryParse(Request.QueryString["port"], out port) || port <= 0 || port >= 65535
                || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(client)
                || string.IsNullOrWhiteSpace(version) || string.IsNullOrWhiteSpace(id) || id.Length > 64)
            {
                WriteText("ERROR: Invalid server information.");
                return;
            }

            if (string.IsNullOrWhiteSpace(ip) || ip.Length > 255)
            {
                ip = Ip;
            }

            GameServers.Add(new GameServer
            {
                JobId = id,
                PlaceId = QueryLong("placeid"),
                Name = WordFilter.Filter(name.Length > 60 ? name.Substring(0, 60) : name),
                Address = ip.Trim(),
                SourceIp = Ip,
                Port = port,
                Client = client,
                Version = version,
                ServerKey = "",
                MaxPlayers = 0,
                Started = DateTime.UtcNow,
                LastHeartbeat = DateTime.UtcNow,
                IsLegacy = true
            });

            Logging.Log(LogType.Backend, "Novetus server '" + name + "' listed at " + ip + ":" + port);
            WriteText("");
        }
    }
}
