using System;
using System.IO;
using System.Net;
using System.Web;
using RobloxServer.Data;
using RobloxServer.Security;
using RobloxServer.Web;

namespace RobloxServer
{
    public class Global : HttpApplication
    {
        static readonly string[] StaticExtensions = { ".css", ".js", ".png", ".gif", ".jpg", ".ico", ".svg", ".woff", ".ttf" };

        protected void Application_Start(object sender, EventArgs e)
        {
            // .NET 4.6 already enables TLS 1.2, this keeps Mono and older hosts in line (assetdelivery needs it).
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            foreach (string dir in new[] { "Places", "Assets", "Logs", "Keys", "Settings" })
            {
                Directory.CreateDirectory(Path.Combine(Config.DataPath, dir));
            }

            ScriptSigner.EnsureKey();
            Logging.Log(LogType.Backend, "Started RobloxServer");
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            string ip = ClientIp.Get(Context);
            string appPath = Request.ApplicationPath ?? "/";
            string path = Request.Path.Substring(appPath.TrimEnd('/').Length);
            if (path.Length == 0)
            {
                path = "/";
            }

            IpBan ban = Bans.FindIpBan(ip);
            if (ban != null)
            {
                Response.StatusCode = 403;
                Response.ContentType = "text/plain";
                Response.Write("Your IP address has been banned. Reason: " + ban.Reason);
                CompleteRequest();
                return;
            }

            string extension = Path.GetExtension(path).ToLowerInvariant();
            if (Config.RateLimitRequests > 0 && !Routing.IsRateLimitExempt(path) && Array.IndexOf(StaticExtensions, extension) < 0)
            {
                if (FloodChecker.Hit("rate:" + ip, Config.RateLimitRequests, TimeSpan.FromSeconds(Config.RateLimitWindowSeconds)))
                {
                    Response.StatusCode = 429;
                    Response.AppendHeader("Retry-After", Config.RateLimitWindowSeconds.ToString());
                    Response.ContentType = "text/plain";
                    Response.Write("Too many requests, please try again later.");
                    CompleteRequest();
                    return;
                }
            }

            string extraQuery;
            string target = Routing.Resolve(path, out extraQuery);
            if (target != null && (target != "~" + path || extraQuery != null))
            {
                string query = Request.Url.Query.TrimStart('?');
                if (!string.IsNullOrEmpty(extraQuery))
                {
                    query = string.IsNullOrEmpty(query) ? extraQuery : query + "&" + extraQuery;
                }
                Context.RewritePath(target, "", query);
            }
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            Exception ex = Server.GetLastError();
            var http = ex as HttpException;
            if (http != null && http.GetHttpCode() == 404)
            {
                return;
            }
            Logging.Log(LogType.Error, Request.RawUrl + ": " + (ex != null ? ex.GetBaseException().ToString() : "unknown error"));
        }
    }
}
