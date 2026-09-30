using System.IO;
using System.Text.RegularExpressions;
using RobloxServer.Web;

namespace RobloxServer.Handlers.Setting
{
    /// <summary>
    /// /Setting/QuietGet/ClientAppSettings/ and friends (FFlags). Returns App_Data/Settings/{name}.json,
    /// or {} when the file does not exist (same as the Node server).
    /// </summary>
    public class QuietGet : HandlerBase
    {
        protected override void Handle()
        {
            string name = Request.QueryString["name"] ?? "";
            Response.ContentType = "application/json";

            if (Regex.IsMatch(name, "^[A-Za-z0-9_]{1,64}$"))
            {
                string path = Path.Combine(Config.DataPath, "Settings", name + ".json");
                if (File.Exists(path))
                {
                    Response.TransmitFile(path);
                    return;
                }
            }

            Response.Write("{}");
        }
    }
}
