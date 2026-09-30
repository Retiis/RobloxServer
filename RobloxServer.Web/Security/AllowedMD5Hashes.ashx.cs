using RobloxServer.Web;

namespace RobloxServer.Handlers.Security
{
    /// <summary>/GetAllowedMD5Hashes/?apiKey= : MD5 of the client executables that may join.</summary>
    public class AllowedMD5Hashes : HandlerBase
    {
        protected override void Handle()
        {
            if (!RequireApiKey())
            {
                return;
            }
            WriteJson(new { data = Config.AllowedMD5Hashes });
        }
    }
}
