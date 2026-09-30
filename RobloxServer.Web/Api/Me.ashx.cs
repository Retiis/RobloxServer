using RobloxServer.Data;
using RobloxServer.Web;

namespace RobloxServer.Handlers.Api
{
    public class Me : HandlerBase
    {
        protected override void Handle()
        {
            User user = CurrentUser;
            if (user == null)
            {
                WriteJson(new { authenticated = false });
                return;
            }

            WriteJson(new
            {
                authenticated = true,
                userId = user.Id,
                userName = user.Name,
                isAdmin = Db.IsAdmin(user),
                banned = user.IsCurrentlyBanned,
                banReason = user.IsCurrentlyBanned ? user.BanReason : null
            });
        }
    }
}
