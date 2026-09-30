using RobloxServer.Data;
using RobloxServer.Web;

namespace RobloxServer.Handlers.Asset
{
    /// <summary>Player.CharacterAppearance: semicolon separated list of appearance assets.</summary>
    public class CharacterFetch : HandlerBase
    {
        protected override void Handle()
        {
            long userId = QueryLong("userId");
            string bodyColors = Config.BaseUrl + "Asset/BodyColors.ashx?userId=" + userId;
            if (Noli.Is(Db.FindUser(userId)))
            {
                // Noli is all black: body colors only, no default clothing.
                WriteText(bodyColors);
                return;
            }
            WriteText(bodyColors + ";" + Config.BaseUrl + "Asset/?versionid=21351761");
        }
    }
}
