using RobloxServer.Data;
using RobloxServer.Web;

namespace RobloxServer.Handlers.MasterServer
{
    /// <summary>delist.php?id= : removes a legacy listing.</summary>
    public class Delist : HandlerBase
    {
        protected override void Handle()
        {
            GameServer server = GameServers.Find(Request.QueryString["id"]);
            if (server != null && server.IsLegacy)
            {
                GameServers.Remove(server.JobId);
            }
            WriteText("");
        }
    }
}
