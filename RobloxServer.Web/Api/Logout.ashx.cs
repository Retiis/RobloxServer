using RobloxServer.Security;
using RobloxServer.Web;

namespace RobloxServer.Handlers.Api
{
    public class Logout : HandlerBase
    {
        protected override void Handle()
        {
            if (Request.HttpMethod != "POST")
            {
                WriteStatus(405, "POST required");
                return;
            }
            if (CurrentUser != null && !Auth.ValidateCsrf(Context))
            {
                return;
            }
            Auth.SignOut(Context);
            WriteJson(new { success = true });
        }
    }
}
