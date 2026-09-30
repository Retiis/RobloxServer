using System;
using System.Linq;
using System.Web;
using RobloxServer.Data;
using RobloxServer.Web;

namespace RobloxServer.Pages
{
    /// <summary>2013 People search: Browse.aspx?name=. Without a name it lists who was online last.</summary>
    public partial class Browse : BasePage
    {
        const int MaxResults = 50;

        protected void Page_Load(object sender, EventArgs e)
        {
            string query = (Request.QueryString["name"] ?? "").Trim();
            if (!IsPostBack)
            {
                NameBox.Text = query;
            }

            var users = Db.Users.All()
                .Where(u => query.Length == 0 || u.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                .OrderByDescending(u => string.Equals(u.Name, query, StringComparison.OrdinalIgnoreCase))
                .ThenByDescending(u => u.LastOnline)
                .Take(MaxResults)
                .ToList();

            UsersRepeater.DataSource = users.Select(u => Noli.Is(u)
                ? new
                {
                    // The myth: a question mark, no "last online" and nothing else, only the clue in the page source.
                    u.Name,
                    Comment = "<!-- " + Noli.Clue + " -->",
                    ProfileUrl = ResolveUrl("~/User.aspx?id=" + u.Id),
                    AvatarUrl = ResolveUrl("~/Asset/Avatar.ashx?userId=" + u.Id),
                    Status = "",
                    Joined = ""
                }
                : new
                {
                    u.Name,
                    Comment = "",
                    ProfileUrl = ResolveUrl("~/User.aspx?id=" + u.Id),
                    AvatarUrl = ResolveUrl("~/Asset/Avatar.ashx?userId=" + u.Id),
                    Status = IsOnline(u)
                        ? "<img src=\"" + ResolveUrl("~/Images/Icons/online.png") + "\" alt=\"\" /> <span class=\"UserOnline\">Online: Website</span>"
                        : "<img src=\"" + ResolveUrl("~/Images/Icons/offline.png") + "\" alt=\"\" /> " + HttpUtility.HtmlEncode(Ago(u.LastOnline)),
                    Joined = u.Created.ToString("M/d/yyyy")
                });
            UsersRepeater.DataBind();

            NoResults.Visible = users.Count == 0;
            QueryLiteral.Text = Server.HtmlEncode(query);
        }

        protected void SearchButton_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Browse.aspx?name=" + Server.UrlEncode(NameBox.Text.Trim()), false);
        }
    }
}
