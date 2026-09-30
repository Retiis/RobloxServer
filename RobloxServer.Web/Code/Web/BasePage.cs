using System;
using System.Web;
using System.Web.UI;
using RobloxServer.Data;
using RobloxServer.Security;

namespace RobloxServer.Web
{
    /// <summary>Base class for every .aspx page.</summary>
    public class BasePage : Page
    {
        /// <summary>Pages a banned user may still open.</summary>
        protected virtual bool AllowBanned
        {
            get { return false; }
        }

        protected User CurrentUser
        {
            get { return Auth.CurrentUser; }
        }

        protected bool IsAdmin
        {
            get { return Db.IsAdmin(CurrentUser); }
        }

        protected override void OnInit(EventArgs e)
        {
            // Anti-CSRF for postbacks: ViewState is bound to the .ROBLOSECURITY cookie (or the IP when logged out).
            HttpCookie cookie = Request.Cookies[Auth.CookieName];
            ViewStateUserKey = cookie != null && !string.IsNullOrEmpty(cookie.Value) ? cookie.Value : ClientIp.Get(Context);

            User user = CurrentUser;
            if (user != null && user.IsCurrentlyBanned && !AllowBanned)
            {
                Response.Redirect("~/NotApproved.aspx", true);
            }
            if (user != null && DateTime.UtcNow - user.LastOnline > TimeSpan.FromMinutes(1))
            {
                // "Last Online" on profiles and in the People search.
                Db.Users.Update(u => u.Id == user.Id, u => u.LastOnline = DateTime.UtcNow);
            }

            base.OnInit(e);
        }

        protected User RequireLogin()
        {
            User user = CurrentUser;
            if (user == null)
            {
                Response.Redirect("~/Login.aspx?ReturnUrl=" + HttpUtility.UrlEncode(Request.RawUrl), true);
            }
            return user;
        }

        protected void RequireAdmin()
        {
            if (!IsAdmin)
            {
                Response.Redirect("~/Default.aspx", true);
            }
        }

        /// <summary>HTML encoded Eval() for data binding (Mono does not support the &lt;%#: %&gt; syntax).</summary>
        protected string Enc(string field)
        {
            return HttpUtility.HtmlEncode(Convert.ToString(Eval(field)));
        }

        /// <summary>
        /// Place thumbnail. There is no thumbnail renderer, so every place gets one of the 2013
        /// front page screenshots, picked by id so a place always shows the same picture.
        /// </summary>
        protected string PlaceThumb(object placeId, string size)
        {
            long id = Convert.ToInt64(placeId);
            return ResolveUrl("~/Images/Thumbs/Place" + (Math.Abs(id % 5) + 1) + "_" + size + ".jpg");
        }

        protected string GenreIcon(object genre)
        {
            return ResolveUrl("~/Images/GenreIcons/" + Genres.Find(Convert.ToString(genre)).Icon);
        }

        protected static string GenreName(object genre)
        {
            return Genres.Clean(Convert.ToString(genre));
        }

        /// <summary>Opened a page in the last 5 minutes (shown as "Online: Website").</summary>
        protected static bool IsOnline(User user)
        {
            return DateTime.UtcNow - user.LastOnline < TimeSpan.FromMinutes(5);
        }

        protected static string Ago(DateTime utc)
        {
            TimeSpan span = DateTime.UtcNow - utc;
            if (span.TotalMinutes < 1) return "just now";
            if (span.TotalHours < 1) return (int)span.TotalMinutes + " minutes ago";
            if (span.TotalDays < 1) return (int)span.TotalHours + " hours ago";
            if (span.TotalDays < 30) return (int)span.TotalDays + " days ago";
            return utc.ToString("MMM d, yyyy");
        }
    }
}
