using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Security;
using RobloxServer.Data;

namespace RobloxServer.Security
{
    /// <summary>
    /// Authentication through the .ROBLOSECURITY cookie. Just like 2015 it is an ASP.NET Forms
    /// Authentication ticket (encrypted with the machineKey and hex encoded), which is why the
    /// real cookie looked like a very long hexadecimal string.
    /// </summary>
    public static class Auth
    {
        const string ItemKey = "RobloxServer.CurrentUser";

        public static string CookieName
        {
            get { return FormsAuthentication.FormsCookieName; }
        }

        /// <summary>The logged in user, or null. Banned users are returned too; check IsCurrentlyBanned.</summary>
        public static User CurrentUser
        {
            get
            {
                HttpContext context = HttpContext.Current;
                if (context == null)
                {
                    return null;
                }

                if (context.Items.Contains(ItemKey))
                {
                    return context.Items[ItemKey] as User;
                }

                User user = null;
                if (context.User != null && context.User.Identity != null && context.User.Identity.IsAuthenticated)
                {
                    long id;
                    if (long.TryParse(context.User.Identity.Name, out id))
                    {
                        user = Db.FindUser(id);
                    }
                }

                context.Items[ItemKey] = user;
                return user;
            }
        }

        public static void SignIn(User user, HttpContext context)
        {
            FormsAuthentication.SetAuthCookie(user.Id.ToString(), true);
            context.Items[ItemKey] = user;
            // The landing page opens on the Login tab for people who already have an account here.
            context.Response.Cookies.Add(new HttpCookie("RBXReturning", "1") { Expires = DateTime.UtcNow.AddYears(1), HttpOnly = true });

            string ip = ClientIp.Get(context);
            Db.Users.Update(u => u.Id == user.Id, u =>
            {
                u.LastIp = ip;
                u.LastOnline = DateTime.UtcNow;
            });
        }

        /// <summary>Checks the credentials with 2015 style flood checking (per IP and per account).</summary>
        public static User Login(string userName, string password, HttpContext context, out string error)
        {
            error = null;
            string ip = ClientIp.Get(context);
            string ipKey = "login-ip:" + ip;
            string userKey = "login-user:" + (userName ?? "").Trim().ToLowerInvariant();
            TimeSpan window = TimeSpan.FromMinutes(Config.LoginFloodMinutes);

            if (FloodChecker.IsFlooded(ipKey, Config.LoginFloodAttempts * 3) || FloodChecker.IsFlooded(userKey, Config.LoginFloodAttempts))
            {
                error = "Too many attempts. Please wait a bit.";
                Logging.Log(LogType.Security, "Login flood check tripped for '" + userName + "' from " + ip);
                return null;
            }

            User user = Db.FindUser(userName);
            if (user == null || !PasswordHasher.Verify(user.PasswordHash, password ?? ""))
            {
                FloodChecker.Increment(ipKey, window);
                FloodChecker.Increment(userKey, window);
                error = "Incorrect username or password.";
                return null;
            }

            FloodChecker.Reset(userKey);
            return user;
        }

        public static void SignOut(HttpContext context)
        {
            FormsAuthentication.SignOut();
            context.Items[ItemKey] = null;
        }

        static byte[] csrfSecret;
        static readonly object CsrfSync = new object();

        static byte[] CsrfSecret
        {
            get
            {
                lock (CsrfSync)
                {
                    if (csrfSecret == null)
                    {
                        string dir = Path.Combine(Config.DataPath, "Keys");
                        string path = Path.Combine(dir, "CsrfSecret.txt");
                        Directory.CreateDirectory(dir);
                        RobloxServer.Data.RemoteBlobStore.Pull("Keys/CsrfSecret.txt", path);
                        if (!File.Exists(path))
                        {
                            byte[] secret = new byte[32];
                            using (var rng = new RNGCryptoServiceProvider())
                            {
                                rng.GetBytes(secret);
                            }
                            File.WriteAllText(path, Convert.ToBase64String(secret));
                            RobloxServer.Data.RemoteBlobStore.Push("Keys/CsrfSecret.txt", path);
                        }
                        csrfSecret = Convert.FromBase64String(File.ReadAllText(path).Trim());
                    }
                    return csrfSecret;
                }
            }
        }

        /// <summary>X-CSRF-TOKEN bound to the current .ROBLOSECURITY cookie.</summary>
        public static string CsrfToken(HttpContext context)
        {
            HttpCookie cookie = context.Request.Cookies[CookieName];
            string seed = cookie != null ? cookie.Value : "anonymous";
            using (var hmac = new HMACSHA256(CsrfSecret))
            {
                byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(seed));
                return Convert.ToBase64String(hash, 0, 9);
            }
        }

        /// <summary>
        /// Validates the X-CSRF-TOKEN header. On failure writes the 2015 style 403 "Token Validation
        /// Failed" response with the correct token in the header so the caller can retry.
        /// </summary>
        public static bool ValidateCsrf(HttpContext context)
        {
            string expected = CsrfToken(context);
            string sent = context.Request.Headers["X-CSRF-TOKEN"];
            if (PasswordHasher.ConstantTimeEquals(expected, sent ?? ""))
            {
                return true;
            }

            context.Response.StatusCode = 403;
            context.Response.StatusDescription = "Token Validation Failed";
            context.Response.AppendHeader("X-CSRF-TOKEN", expected);
            context.Response.ContentType = "application/json";
            context.Response.Write("{\"errors\":[{\"code\":0,\"message\":\"Token Validation Failed\"}]}");
            return false;
        }
    }
}
