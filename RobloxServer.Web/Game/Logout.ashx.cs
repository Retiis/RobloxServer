using RobloxServer.Security;
using RobloxServer.Web;

namespace RobloxServer.Handlers.Game
{
    public class Logout : HandlerBase
    {
        protected override void Handle()
        {
            Auth.SignOut(Context);
            Response.StatusCode = 200;
        }
    }
}
