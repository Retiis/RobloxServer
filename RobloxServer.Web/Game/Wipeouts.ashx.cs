using RobloxServer.Web;

namespace RobloxServer.Handlers.Game
{
    public class Wipeouts : HandlerBase
    {
        protected override void Handle()
        {
            Scores.Add(Request.QueryString["UserID"], 1, 0);
            WriteJson(new { status = "online" });
        }
    }
}
