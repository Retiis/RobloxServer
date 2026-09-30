using System.Collections.Generic;
using RobloxServer.Data;
using RobloxServer.Web;

namespace RobloxServer.Handlers.Game
{
    /// <summary>Studio start-up settings (was storage/game/studio.ashx).</summary>
    public class Studio : HandlerBase
    {
        protected override void Handle()
        {
            User user = CurrentUser;
            var values = new Dictionary<string, string>
            {
                { "BaseUrl", Config.BaseUrl },
                { "ApiEndPoint", Config.ApiUrl.TrimEnd('/') },
                { "PlaceId", QueryLong("PlaceID").ToString() },
                { "UserId", user != null ? user.Id.ToString() : "" }
            };
            Response.ContentType = "application/json";
            Response.Write(Templates.Render("Studio.json", values));
        }
    }
}
