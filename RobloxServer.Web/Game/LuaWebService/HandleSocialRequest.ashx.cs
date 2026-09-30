using RobloxServer.Web;

namespace RobloxServer.Handlers.Game
{
    /// <summary>SocialService URLs. There are no friends or groups here yet, so everything answers "no".</summary>
    public class HandleSocialRequest : HandlerBase
    {
        protected override void Handle()
        {
            Response.ContentType = "text/xml";
            switch ((Request.QueryString["method"] ?? "").ToLowerInvariant())
            {
                case "getgrouprank":
                    Response.Write("<Value Type=\"integer\">0</Value>");
                    break;
                case "getgrouprole":
                    Response.Write("Guest");
                    break;
                default:
                    Response.Write("<Value Type=\"boolean\">false</Value>");
                    break;
            }
        }
    }
}
