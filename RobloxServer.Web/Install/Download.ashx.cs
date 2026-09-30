using RobloxServer.Data;
using RobloxServer.Web;

namespace RobloxServer.Handlers.Install
{
    /// <summary>
    /// /install/download.ashx?client=2012M|2013M|Launcher: the client zip or the launcher. Without a
    /// launcher in App_Data/Launcher the launcher download goes to Config.LauncherDownloadUrl.
    /// </summary>
    public class Download : HandlerBase
    {
        protected override void Handle()
        {
            string name = ClientPackages.CleanName(Request.QueryString["client"]);
            ClientPackages.Package package = ClientPackages.Find(name);
            if (package == null)
            {
                if (name == ClientPackages.Launcher)
                {
                    Response.Redirect(Config.LauncherDownloadUrl, false);
                    return;
                }
                WriteStatus(404, "Not found");
                return;
            }

            Response.ContentType = name == ClientPackages.Launcher ? "application/octet-stream" : "application/zip";
            Response.AppendHeader("Content-Disposition", "attachment; filename=\"" + package.FileName + "\"");
            Response.AppendHeader("X-Package-Version", package.Version);
            Response.AppendHeader("X-Package-Sha256", package.Sha256);
            Response.TransmitFile(package.Path);
        }
    }
}
