using RobloxServer.Data;
using RobloxServer.Security;
using RobloxServer.Web;

namespace RobloxServer.Handlers.Login
{
    /// <summary>Login/Negotiate.ashx?suggest=TICKET: the client trades its ticket for a .ROBLOSECURITY cookie.</summary>
    public class Negotiate : HandlerBase
    {
        protected override void Handle()
        {
            AuthTicket ticket = AuthTickets.RedeemForNegotiate(Request.QueryString["suggest"]);
            if (ticket == null)
            {
                WriteStatus(403, "Invalid ticket");
                return;
            }

            User user = Db.FindUser(ticket.UserId);
            if (user == null || user.IsCurrentlyBanned)
            {
                WriteStatus(403, "User is moderated");
                return;
            }

            Auth.SignIn(user, Context);
            WriteText(ticket.Value);
        }
    }
}
