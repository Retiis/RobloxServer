using RobloxServer.Web;

namespace RobloxServer.Handlers.Game
{
    public class Knockouts : HandlerBase
    {
        protected override void Handle()
        {
            Scores.Add(Request.QueryString["UserID"], 0, 1);
            WriteJson(new { status = "online" });
        }
    }
}
