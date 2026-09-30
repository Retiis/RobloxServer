using RobloxServer.Data;
using RobloxServer.Security;
using RobloxServer.Web;

namespace RobloxServer.Handlers.Api
{
    /// <summary>POST username + password. Sets the .ROBLOSECURITY cookie (used by the launcher).</summary>
    public class Login : HandlerBase
    {
        protected override void Handle()
        {
            if (Request.HttpMethod != "POST")
            {
                WriteStatus(405, "POST required");
                return;
            }

            string error;
            User user = Auth.Login(Request.Form["username"], Request.Form["password"], Context, out error);
            if (user == null)
            {
                WriteJson(new { success = false, message = error });
                return;
            }

            if (user.IsCurrentlyBanned)
            {
                WriteJson(new
                {
                    success = false,
                    banned = true,
                    message = "Your account has been moderated. Reason: " + user.BanReason
                        + (user.BanExpires.HasValue ? " (until " + ToIso(user.BanExpires.Value) + ")" : " (permanent)")
                });
                return;
            }

            Auth.SignIn(user, Context);
            WriteJson(new { success = true, userId = user.Id, userName = user.Name, isAdmin = Db.IsAdmin(user) });
        }
    }
}
