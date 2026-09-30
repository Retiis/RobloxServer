using RobloxServer.Web;

namespace RobloxServer.Handlers.Game
{
    public class ClientPresence : HandlerBase
    {
        protected override void Handle()
        {
            Response.StatusCode = 200;
        }
    }
}
