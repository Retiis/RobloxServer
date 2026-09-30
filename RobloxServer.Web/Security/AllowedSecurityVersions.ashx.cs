using RobloxServer.Web;

namespace RobloxServer.Handlers.Security
{
    /// <summary>
    /// /GetAllowedSecurityVersions/?apiKey= : versions (e.g. "0.235.0pcplayer") the game server accepts.
    /// This was the core of 2015 client verification, together with GetAllowedMD5Hashes.
    /// </summary>
    public class AllowedSecurityVersions : HandlerBase
    {
        protected override void Handle()
        {
            if (!RequireApiKey())
            {
                return;
            }
            WriteJson(new { data = Config.AllowedSecurityVersions });
        }
    }
}
