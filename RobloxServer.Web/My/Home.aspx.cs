using System;
using System.Linq;
using RobloxServer.Data;
using RobloxServer.Security;
using RobloxServer.Web;

namespace RobloxServer.Pages
{
    /// <summary>
    /// The 2013 logged in home page ("My ROBLOX"): your character, the people on the site, a status box
    /// with My Feed, Recently Played Games and news about the newest games.
    /// </summary>
    public partial class MyHome : BasePage
    {
        const int FeedSize = 20;

        User user;

        protected void Page_Load(object sender, EventArgs e)
        {
            user = RequireLogin();
            NameLiteral.Text = Server.HtmlEncode(user.Name);
            AvatarImage.Src = ResolveUrl("~/Asset/Avatar.ashx?userId=" + user.Id);
            AvatarImage.Alt = user.Name;
            Bind();
        }

        void Bind()
        {
            PeopleRepeater.DataSource = Db.Users.All()
                .Where(u => u.Id != user.Id && !u.IsCurrentlyBanned)
                .OrderByDescending(u => u.LastOnline)
                .Take(5)
                .Select(u => new
                {
                    u.Name,
                    Status = Noli.Is(u) ? "" : u.Status,
                    ProfileUrl = ResolveUrl("~/User.aspx?id=" + u.Id),
                    AvatarUrl = ResolveUrl("~/Asset/Avatar.ashx?userId=" + u.Id),
                    StatusIcon = ResolveUrl(!Noli.Is(u) && IsOnline(u) ? "~/Images/Icons/online.png" : "~/Images/Icons/offline.png")
                });
            PeopleRepeater.DataBind();

            var feed = Db.Feed.All().OrderByDescending(p => p.Id).Take(FeedSize).ToList();
            FeedRepeater.DataSource = feed.Select(p => new
            {
                p.UserName,
                p.Text,
                When = Ago(p.Created),
                ProfileUrl = ResolveUrl("~/User.aspx?id=" + p.UserId),
                AvatarUrl = ResolveUrl("~/Asset/Avatar.ashx?userId=" + p.UserId)
            });
            FeedRepeater.DataBind();
            EmptyFeed.Visible = feed.Count == 0;

            var servers = GameServers.All();
            var recent = Db.RecentPlaceIds(Db.FindUser(user.Id) ?? user)
                .Select(Db.FindPlace)
                .Where(p => p != null && (p.IsPublic || PlaceService.CanEdit(user, p)))
                .Select(p => new { p.Id, p.Name, Playing = servers.Where(s => s.PlaceId == p.Id).Sum(s => s.PlayerCount) })
                .ToList();
            RecentRepeater.DataSource = recent;
            RecentRepeater.DataBind();
            NoRecent.Visible = recent.Count == 0;

            var news = Db.Places.Where(p => p.IsPublic)
                .OrderByDescending(p => p.Created)
                .Take(4)
                .Select(p => new { Text = "New game: " + p.Name + " by " + p.CreatorName, Url = ResolveUrl("~/PlaceItem.aspx?id=" + p.Id) })
                .ToList();
            news.Add(new { Text = "Download ROBLOX to play the games on this site", Url = ResolveUrl("~/Install/Download.ashx?client=Launcher") });
            NewsRepeater.DataSource = news;
            NewsRepeater.DataBind();
        }

        protected void ShareButton_Click(object sender, EventArgs e)
        {
            string text = (StatusBox.Text ?? "").Trim();
            if (text.Length == 0)
            {
                return;
            }
            if (text.Length > 254)
            {
                text = text.Substring(0, 254);
            }
            if (FloodChecker.Hit("status:" + user.Id, 5, TimeSpan.FromMinutes(1)))
            {
                StatusError.Text = "You are posting too fast. Wait a minute and try again.";
                return;
            }

            Db.PostStatus(user, WordFilter.Filter(text));
            StatusBox.Text = "";
            Bind();
        }
    }
}
