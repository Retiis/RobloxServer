using RobloxServer.Data;
using RobloxServer.Security;
using RobloxServer.Web;

namespace RobloxServer.Handlers.Game
{
    /// <summary>
    /// Called by the game server (RobloxServerAuth.lua addon) for every player that joins:
    /// /Game/ValidateTicket.ashx?ticket=&amp;jobId=&amp;serverKey=
    /// Plain text answer so old Lua can parse it: OK|userId|userName|superSafeChat|accountAge|isAdmin or ERROR|reason.
    /// </summary>
    public class ValidateTicket : HandlerBase
    {
        protected override void Handle()
        {
            GameServer server = GameServers.Find(Request.QueryString["jobId"]);
            if (server == null || !PasswordHasher.ConstantTimeEquals(server.ServerKey, Request.QueryString["serverKey"] ?? ""))
            {
                WriteText("ERROR|Unknown game server");
                return;
            }

            AuthTicket ticket = AuthTickets.RedeemForServer(Request.QueryString["ticket"], server.JobId, server.PlaceId);
            if (ticket == null)
            {
                Logging.Log(LogType.Security, "Invalid or reused ticket presented to job " + server.JobId);
                WriteText("ERROR|Invalid authentication ticket");
                return;
            }

            User user = Db.FindUser(ticket.UserId);
            if (user == null)
            {
                WriteText("ERROR|Unknown user");
                return;
            }
            if (user.IsCurrentlyBanned)
            {
                WriteText("ERROR|Account moderated: " + (user.BanReason ?? "").Replace("|", "/"));
                return;
            }

            GameServers.Touch(server.JobId, s => s.PlayerCount++);
            WriteText(string.Join("|", "OK", user.Id.ToString(), user.Name, user.SuperSafeChat ? "true" : "false",
                user.AccountAgeDays.ToString(), Db.IsAdmin(user) ? "true" : "false"));
        }
    }
}
