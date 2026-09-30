using System;
using System.Linq;
using System.Web;
using RobloxServer.Data;
using RobloxServer.Web;

namespace RobloxServer.Pages
{
    public partial class Default : BasePage
    {
        static readonly string[][] Sorts =
        {
            new[] { "Popular", "Most Popular" },
            new[] { "Visits", "Most Visited" },
            new[] { "Updated", "Recently Updated" },
            new[] { "Newest", "Newest" }
        };

        string sort;
        Genres.Genre genre;

        protected void Page_Load(object sender, EventArgs e)
        {
            // "/" is the 2013 landing page for visitors and My ROBLOX for members; the games list is /Games.
            string path = (Request.RawUrl ?? "/").Split('?')[0];
            if (path == "/" || path == Request.ApplicationPath.TrimEnd('/') + "/")
            {
                if (CurrentUser == null)
                {
                    Server.Transfer("~/Landing.aspx");
                }
                else
                {
                    Response.Redirect("~/My/Home.aspx", true);
                }
                return;
            }

            sort = Sorts.Select(s => s[0]).FirstOrDefault(s => string.Equals(s, Request.QueryString["sort"], StringComparison.OrdinalIgnoreCase)) ?? Sorts[0][0];
            genre = Genres.Find(Request.QueryString["genre"]);

            User user = CurrentUser;
            var servers = GameServers.All();
            var games = Db.Places.All()
                .Where(p => p.IsPublic || (user != null && (p.CreatorId == user.Id || IsAdmin)))
                .Where(p => genre.Name == Genres.Default || Genres.Clean(p.Genre) == genre.Name)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.CreatorName,
                    p.Client,
                    p.Genre,
                    p.Visits,
                    p.Created,
                    p.Updated,
                    Playing = servers.Where(s => s.PlaceId == p.Id).Sum(s => s.PlayerCount)
                });

            switch (sort)
            {
                case "Visits":
                    games = games.OrderByDescending(g => g.Visits).ThenByDescending(g => g.Playing);
                    break;
                case "Updated":
                    games = games.OrderByDescending(g => g.Updated);
                    break;
                case "Newest":
                    games = games.OrderByDescending(g => g.Created);
                    break;
                default:
                    games = games.OrderByDescending(g => g.Playing).ThenByDescending(g => g.Visits);
                    break;
            }

            var list = games.ToList();
            GamesRepeater.DataSource = list;
            GamesRepeater.DataBind();
            EmptyMessage.Visible = list.Count == 0;
            HeaderLiteral.Text = Server.HtmlEncode(Sorts.First(s => s[0] == sort)[1] + (genre.Name == Genres.Default ? "" : " " + genre.Name) + " Games");

            SortRepeater.DataSource = Sorts.Select(s => new { Name = s[1], Url = Link(s[0], genre.Key), Selected = s[0] == sort });
            SortRepeater.DataBind();
            GenreRepeater.DataSource = Genres.All.Select(g => new { g.Name, Url = Link(sort, g.Key), Selected = g == genre });
            GenreRepeater.DataBind();
        }

        string Link(string sortKey, string genreKey)
        {
            return HttpUtility.HtmlAttributeEncode(ResolveUrl("~/Games?sort=" + sortKey + "&genre=" + genreKey));
        }
    }
}
