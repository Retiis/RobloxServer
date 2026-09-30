using RobloxServer.Data;
using RobloxServer.Web;

namespace RobloxServer.Handlers.Game
{
    public class GetCurrentUser : HandlerBase
    {
        protected override void Handle()
        {
            User user = CurrentUser;
            WriteText(user != null && !user.IsCurrentlyBanned ? user.Id.ToString() : "null");
        }
    }
}
