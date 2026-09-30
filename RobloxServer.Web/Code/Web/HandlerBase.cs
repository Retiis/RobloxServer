using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using RobloxServer.Data;
using RobloxServer.Security;

namespace RobloxServer.Web
{
    /// <summary>Base class for every .ashx handler.</summary>
    public abstract class HandlerBase : IHttpHandler
    {
        protected HttpContext Context { get; private set; }
        protected HttpRequest Request { get { return Context.Request; } }
        protected HttpResponse Response { get { return Context.Response; } }

        public virtual bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            Context = context;
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Handle();
        }

        protected abstract void Handle();

        protected User CurrentUser
        {
            get { return Auth.CurrentUser; }
        }

        protected string Ip
        {
            get { return ClientIp.Get(Context); }
        }

        public static string Serialize(object value)
        {
            var serializer = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
            return serializer.Serialize(value);
        }

        protected void WriteJson(object value)
        {
            Response.ContentType = "application/json";
            Response.Write(Serialize(value));
        }

        protected void WriteText(string text)
        {
            Response.ContentType = "text/plain";
            Response.Write(text);
        }

        protected void WriteStatus(int status, string text)
        {
            Response.StatusCode = status;
            // Handlers answer 401 themselves instead of being redirected to Login.aspx.
            Response.SuppressFormsAuthenticationRedirect = true;
            if (text != null)
            {
                WriteText(text);
            }
        }

        protected void WriteSignedScript(string script)
        {
            Response.ContentType = "text/plain";
            Response.Write(ScriptSigner.Sign(script));
        }

        protected long QueryLong(string name)
        {
            long value;
            return long.TryParse(Request.QueryString[name], out value) ? value : 0;
        }

        protected int QueryInt(string name, int fallback)
        {
            int value;
            return int.TryParse(Request.QueryString[name], out value) ? value : fallback;
        }

        protected string Param(string name)
        {
            return Request.Form[name] ?? Request.QueryString[name];
        }

        /// <summary>RCC style ?apiKey= check used by game servers. Writes a 403 and returns false on failure.</summary>
        protected bool RequireApiKey()
        {
            string key = Config.ApiKey;
            if (string.IsNullOrEmpty(key))
            {
                // No ApiKey configured: endpoint stays open, like a dev environment in 2015.
                return true;
            }

            string sent = Request.QueryString["apiKey"] ?? Request.QueryString["apikey"] ?? Request.Headers["X-Api-Key"];
            if (PasswordHasher.ConstantTimeEquals(key, sent ?? ""))
            {
                return true;
            }

            Logging.Log(LogType.Security, "Rejected request to " + Request.Path + " with a bad apiKey from " + Ip);
            WriteStatus(403, "Invalid apiKey");
            return false;
        }

        /// <summary>Writes 401 and returns null when nobody is logged in or the account is banned.</summary>
        protected User RequireUser()
        {
            User user = CurrentUser;
            if (user == null)
            {
                WriteStatus(401, "User is not authorized.");
                return null;
            }
            if (user.IsCurrentlyBanned)
            {
                WriteStatus(403, "User is moderated: " + user.BanReason);
                return null;
            }
            return user;
        }

        protected byte[] ReadBody()
        {
            Stream input = Request.InputStream;
            string encoding = Request.Headers["Content-Encoding"] ?? "";
            if (encoding.IndexOf("gzip", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                input = new System.IO.Compression.GZipStream(input, System.IO.Compression.CompressionMode.Decompress);
            }

            using (var memory = new MemoryStream())
            {
                input.CopyTo(memory);
                return memory.ToArray();
            }
        }

        protected static string ToIso(DateTime value)
        {
            return DateTime.SpecifyKind(value, DateTimeKind.Utc).ToString("yyyy-MM-ddTHH:mm:ssZ");
        }

        protected static bool CanHost(User user, Place place)
        {
            return PlaceService.CanHost(user, place);
        }

        protected static bool CanSee(User user, Place place)
        {
            return place != null && (place.IsPublic || (user != null && (place.CreatorId == user.Id || Db.IsAdmin(user))));
        }

        protected static string[] SplitList(string value)
        {
            return (value ?? "").Split(',').Select(s => s.Trim()).Where(s => s.Length > 0).ToArray();
        }
    }
}
