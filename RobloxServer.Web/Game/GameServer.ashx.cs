using System.Collections.Generic;
using RobloxServer.Data;
using RobloxServer.Security;
using RobloxServer.Web;

namespace RobloxServer.Handlers.Game
{
    /// <summary>
    /// /Game/GameServer.ashx?jobId=&amp;serverKey=[&amp;loadPlace=false]: signed Lua script that turns a
    /// 2012M/2013M client into the game server of a job registered with Game/Servers.ashx (the 2012
    /// gameserver.ashx). Only the host that registered the job knows the serverKey.
    /// </summary>
    public class GameServerScript : HandlerBase
    {
        protected override void Handle()
        {
            GameServer server = GameServers.Find(Request.QueryString["jobId"]);
            if (server == null || !PasswordHasher.ConstantTimeEquals(server.ServerKey, Request.QueryString["serverKey"] ?? ""))
            {
                WriteStatus(404, "Unknown game server");
                return;
            }

            Place place = Db.FindPlace(server.PlaceId);
            bool loadPlace = !string.Equals(Request.QueryString["loadPlace"], "false", System.StringComparison.OrdinalIgnoreCase);
            var values = new Dictionary<string, string>
            {
                { "BaseUrl", Templates.LuaLongString(Config.BaseUrl) },
                { "JobId", Templates.LuaLongString(server.JobId) },
                { "ServerKey", Templates.LuaLongString(server.ServerKey) },
                { "PlaceId", server.PlaceId.ToString() },
                { "Port", server.Port.ToString() },
                { "MaxPlayers", (server.MaxPlayers > 0 ? server.MaxPlayers : 12).ToString() },
                { "RequireAuth", Config.RequireAuthTickets ? "true" : "false" },
                { "FilteringEnabled", place != null && place.FilteringEnabled ? "true" : "false" },
                { "LoadPlace", loadPlace ? "true" : "false" },
                { "PlaceUrl", Templates.LuaLongString(Config.BaseUrl + "asset/?id=" + server.PlaceId) }
            };

            WriteSignedScript(Templates.Render("GameServer.lua", values));
        }
    }
}
