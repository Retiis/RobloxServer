using System;
using System.Text;
using RobloxServer.Data;
using RobloxServer.Web;

namespace RobloxServer.Handlers.MasterServer
{
    /// <summary>
    /// serverlist.txt in the Novetus format, one server per line:
    /// id|base64(base64(name)|base64(ip)|base64(port)|base64(client)|base64(version))
    /// Includes servers started from the RobloxServer game browser too.
    /// </summary>
    public class ServerList : HandlerBase
    {
        static string B64(string value)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(value ?? ""));
        }

        protected override void Handle()
        {
            var builder = new StringBuilder();
            foreach (GameServer server in GameServers.All())
            {
                string inner = string.Join("|",
                    B64(server.Name),
                    B64(GameServers.AddressFor(server, Ip)),
                    B64(server.Port.ToString()),
                    B64(server.Client),
                    B64(server.Version));
                builder.Append(server.JobId).Append('|').Append(B64(inner)).Append("\r\n");
            }
            WriteText(builder.ToString());
        }
    }
}
