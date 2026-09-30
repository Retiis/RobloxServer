using System.IO;
using RobloxServer.Web;

namespace RobloxServer.Handlers.Error
{
    /// <summary>Error/Lua.ashx, Error/Dmp.ashx and Error/Grid.ashx: log whatever the client reports and thank it.</summary>
    public class Lua : HandlerBase
    {
        protected override void Handle()
        {
            if (Request.HttpMethod == "POST" && Request.ContentLength > 0 && Request.ContentLength < 64 * 1024)
            {
                using (var reader = new StreamReader(Request.InputStream))
                {
                    Logging.Log(LogType.Error, "Client error report from " + Ip + " (" + Request.Path + "): " + reader.ReadToEnd());
                }
            }
            WriteText("Thx");
        }
    }
}
