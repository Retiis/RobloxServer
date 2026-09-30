using RobloxServer.Web;

namespace RobloxServer.Handlers.Api
{
    public class Status : HandlerBase
    {
        protected override void Handle()
        {
            WriteJson(new { status = "online", name = Config.SiteName, baseUrl = Config.BaseUrl });
        }
    }
}
