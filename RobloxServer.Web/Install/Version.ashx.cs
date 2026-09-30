using RobloxServer.Data;
using RobloxServer.Web;

namespace RobloxServer.Handlers.Install
{
    /// <summary>
    /// /install/version.ashx?client=2012M|2013M|Launcher: what RobloxPlayerLauncher should have
    /// installed, like setup.roblox.com/version in 2013.
    /// </summary>
    public class Version : HandlerBase
    {
        protected override void Handle()
        {
            string name = ClientPackages.CleanName(Request.QueryString["client"]);
            if (name == null)
            {
                Response.StatusCode = 404;
                WriteJson(new { available = false, message = "Unknown client. This site supports: " + string.Join(", ", Config.Clients) + "." });
                return;
            }

            ClientPackages.Package package = ClientPackages.Find(name);
            if (package == null)
            {
                Response.StatusCode = 404;
                WriteJson(new
                {
                    available = false,
                    client = name,
                    message = name == ClientPackages.Launcher
                        ? "This site does not host the launcher."
                        : "The " + name + " client has not been installed on this site yet. Ask the administrator to upload it."
                });
                return;
            }

            WriteJson(new
            {
                available = true,
                client = package.Name,
                version = package.Version,
                sha256 = package.Sha256,
                size = package.Size,
                url = Config.BaseUrl + "Install/Download.ashx?client=" + package.Name
            });
        }
    }
}
