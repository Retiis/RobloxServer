using RobloxServer.Web;

namespace RobloxServer.Handlers.Game
{
    public class GamePassHandler : HandlerBase
    {
        protected override void Handle()
        {
            Response.ContentType = "text/xml";
            Response.Write("<Value Type=\"boolean\">false</Value>");
        }
    }
}
