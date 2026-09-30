using RobloxServer.Data;
using RobloxServer.Security;
using RobloxServer.Web;

namespace RobloxServer.Handlers.Game
{
    /// <summary>/game/getauthticket: one-time ticket for the logged in user (needs .ROBLOSECURITY).</summary>
    public class GetAuthTicket : HandlerBase
    {
        protected override void Handle()
        {
            User user = RequireUser();
            if (user == null)
            {
                return;
            }

            AuthTicket ticket = AuthTickets.Issue(user.Id, user.Name, QueryLong("placeId"), Request.QueryString["jobId"]);
            WriteText(ticket.Value);
        }
    }
}
