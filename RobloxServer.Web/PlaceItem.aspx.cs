using System;
using System.IO;
using System.Linq;
using RobloxServer.Data;
using RobloxServer.Web;

namespace RobloxServer.Pages
{
    public partial class PlaceItem : BasePage
    {
        Place place;

        protected void Page_Load(object sender, EventArgs e)
        {
            long id;
            long.TryParse(Request.QueryString["id"], out id);
            place = Db.FindPlace(id);
            User user = CurrentUser;

            if (place == null || !(place.IsPublic || PlaceService.CanEdit(user, place)))
            {
                Response.StatusCode = 404;
                NotFoundPanel.Visible = true;
                PlacePanel.Visible = false;
                return;
            }

            Title = place.Name;
            NameLiteral.Text = Server.HtmlEncode(place.Name);
            NameLiteral2.Text = NameLiteral.Text;
            CreatorLiteral.Text = "<a href=\"" + ResolveUrl("~/User.aspx?id=" + place.CreatorId) + "\">" + Server.HtmlEncode(place.CreatorName) + "</a>";
            ClientLiteral.Text = Server.HtmlEncode(PlaceService.ClientFor(place));
            VisitsLiteral.Text = place.Visits.ToString("N0");
            UpdatedLiteral.Text = Ago(place.Updated);
            DescriptionLiteral.Text = Server.HtmlEncode(place.Description ?? "");
            IdLiteral.Text = place.Id.ToString();
            ClientLiteral2.Text = ClientLiteral.Text;

            // Read by Content/PlaceLauncher.js to build the robloxserver-player: link.
            PlaceLauncher.Attributes["data-place-id"] = place.Id.ToString();
            PlaceLauncher.Attributes["data-base-url"] = Config.BaseUrl;
            PlaceLauncher.Attributes["data-ticket-url"] = ResolveUrl("~/Game/GetAuthTicket.ashx");
            PlaceLauncher.Attributes["data-login-url"] = ResolveUrl("~/Login.aspx?ReturnUrl=" + Server.UrlEncode(Request.RawUrl));
            PlaceLauncher.Attributes["data-logged-in"] = user != null ? "true" : "false";
            HostPanel.Visible = PlaceService.CanHost(user, place);
            DownloadLink.HRef = ResolveUrl("~/asset/?id=" + place.Id);
            ThumbnailImage.Src = PlaceThumb(place.Id, "420x230");
            ThumbnailImage.Alt = place.Name;
            MaxPlayersLiteral.Text = place.MaxPlayers.ToString();
            GenreImage.Src = GenreIcon(place.Genre);
            GenreLiteral.Text = Server.HtmlEncode(GenreName(place.Genre));

            var servers = GameServers.ForPlace(place.Id);
            ServersRepeater.DataSource = servers;
            ServersRepeater.DataBind();
            NoServers.Visible = servers.Count == 0;

            EditPanel.Visible = PlaceService.CanEdit(user, place);
            if (EditPanel.Visible && !IsPostBack)
            {
                NameBox.Text = place.Name;
                DescriptionBox.Text = place.Description;
                GenreList.DataSource = Genres.All.Select(g => g.Name);
                GenreList.DataBind();
                GenreList.SelectedValue = GenreName(place.Genre);
                ClientList.DataSource = Config.Clients;
                ClientList.DataBind();
                ClientList.SelectedValue = PlaceService.CleanClient(place.Client);
                MaxPlayersBox.Text = place.MaxPlayers.ToString();
                PublicBox.Checked = place.IsPublic;
                FilteringBox.Checked = place.FilteringEnabled;
            }
        }

        protected void SaveButton_Click(object sender, EventArgs e)
        {
            if (place == null || !PlaceService.CanEdit(CurrentUser, place))
            {
                return;
            }

            if (PlaceUpload.HasFile)
            {
                byte[] data = PlaceUpload.FileBytes;
                string error = PlaceService.Validate(data);
                if (error != null)
                {
                    ErrorLabel.Text = Server.HtmlEncode(error);
                    return;
                }
                Db.SavePlaceFile(place.Id, data, "rbxl");
            }

            int maxPlayers;
            int.TryParse(MaxPlayersBox.Text, out maxPlayers);
            Db.Places.Update(p => p.Id == place.Id, p =>
            {
                p.Name = PlaceService.CleanName(NameBox.Text);
                p.Description = PlaceService.CleanDescription(DescriptionBox.Text);
                p.Client = PlaceService.CleanClient(ClientList.SelectedValue);
                p.Genre = Genres.Clean(GenreList.SelectedValue);
                p.MaxPlayers = Math.Max(1, Math.Min(maxPlayers <= 0 ? 12 : maxPlayers, 100));
                p.IsPublic = PublicBox.Checked;
                p.FilteringEnabled = FilteringBox.Checked;
                p.Updated = DateTime.UtcNow;
            });
            Response.Redirect("~/PlaceItem.aspx?id=" + place.Id, false);
        }

        protected void DeleteButton_Click(object sender, EventArgs e)
        {
            if (place == null || !PlaceService.CanEdit(CurrentUser, place))
            {
                return;
            }

            string path = Db.PlaceFilePath(place);
            Db.Places.Delete(p => p.Id == place.Id);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            Logging.Log(LogType.Backend, CurrentUser.Name + " deleted place " + place.Id);
            Response.Redirect("~/Develop.aspx", false);
        }
    }
}
