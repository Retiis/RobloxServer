using System.Collections.Generic;
using RobloxServer.Data;
using RobloxServer.Security;
using RobloxServer.Web;

namespace RobloxServer.Handlers.Game
{
    /// <summary>
    /// /Game/Join.ashx?jobId=: signed Lua join script for the 2012M/2013M clients, like the 2012
    /// join.ashx. RobloxPlayerLauncher downloads it with the player's cookie and starts the client with it.
    /// </summary>
    public class Join : HandlerBase
    {
        protected override void Handle()
        {
            User user = RequireUser();
            if (user == null)
            {
                return;
            }

            GameServer server = GameServers.Find(Request.QueryString["jobId"]);
            if (server == null)
            {
                WriteStatus(404, "Game not found");
                return;
            }

            Place place = Db.FindPlace(server.PlaceId);
            if (server.PlaceId != 0 && !CanSee(user, place))
            {
                WriteStatus(403, "Place is private");
                return;
            }

            if (place != null)
            {
                Db.Places.Update(p => p.Id == place.Id, p => p.Visits++);
                Db.AddRecentPlace(user.Id, place.Id);
            }

            AuthTicket ticket = AuthTickets.Issue(user.Id, user.Name, server.PlaceId, server.JobId);
            var values = new Dictionary<string, string>
            {
                { "BaseUrl", Templates.LuaLongString(Config.BaseUrl) },
                { "ServerAddress", Templates.LuaLongString(GameServers.AddressFor(server, Ip, Request.ServerVariables["LOCAL_ADDR"])) },
                { "ServerPort", server.Port.ToString() },
                { "PlaceId", server.PlaceId.ToString() },
                { "JobId", Templates.LuaLongString(server.JobId) },
                { "UserId", user.Id.ToString() },
                { "UserName", Templates.LuaLongString(user.Name) },
                { "AuthTicket", ticket.Value },
                { "SuperSafeChat", user.SuperSafeChat ? "true" : "false" },
                { "AccountAge", user.AccountAgeDays.ToString() },
                { "CharacterAppearance", Templates.LuaLongString(Config.BaseUrl + "Asset/CharacterFetch.ashx?userId=" + user.Id + "&placeId=" + server.PlaceId) },
                { "PingUrl", Templates.LuaLongString(Config.BaseUrl + "Game/ClientPresence.ashx?version=old&PlaceID=" + server.PlaceId) },
                { "PingInterval", "120" }
            };

            Response.AppendHeader("X-Client", PlaceService.CleanClient(server.Client));
            WriteSignedScript(Templates.Render("Join.lua", values));
        }
    }
}
