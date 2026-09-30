using System;
using System.Linq;
using RobloxServer.Data;
using RobloxServer.Web;

namespace RobloxServer.Pages
{
    /// <summary>2013 build page: the Places tab with your places and "Create New Place".</summary>
    public partial class Develop : BasePage
    {
        /// <summary>Shows the Create New Place form (after a failed publish, or with ?View=publish).</summary>
        protected bool NewPlaceOpen { get; private set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            User user = RequireLogin();
            if (user == null)
            {
                return;
            }

            NewPlaceOpen = string.Equals(Request.QueryString["View"], "publish", StringComparison.OrdinalIgnoreCase);
            if (!IsPostBack)
            {
                GenreList.DataSource = Genres.All.Select(g => g.Name);
                GenreList.DataBind();
                ClientList.DataSource = Config.Clients;
                ClientList.DataBind();
                ClientList.SelectedValue = Config.DefaultClient;
            }

            BindPlaces(user);
        }

        void BindPlaces(User user)
        {
            var places = Db.Places.Where(p => p.CreatorId == user.Id).OrderByDescending(p => p.Updated).ToList();
            MyPlacesRepeater.DataSource = places;
            MyPlacesRepeater.DataBind();
            NoPlaces.Visible = places.Count == 0;
        }

        protected void PublishButton_Click(object sender, EventArgs e)
        {
            NewPlaceOpen = true;
            User user = CurrentUser;
            if (user == null)
            {
                return;
            }

            if (!PlaceUpload.HasFile)
            {
                ErrorLabel.Text = "Choose a place file to upload.";
                return;
            }

            byte[] data = PlaceUpload.FileBytes;
            string error = PlaceService.Validate(data);
            if (error != null)
            {
                ErrorLabel.Text = Server.HtmlEncode(error);
                return;
            }

            int maxPlayers;
            int.TryParse(MaxPlayersBox.Text, out maxPlayers);

            Place place = PlaceService.Create(user, NameBox.Text, DescriptionBox.Text, ClientList.SelectedValue,
                PublicBox.Checked, FilteringBox.Checked, maxPlayers, data, GenreList.SelectedValue);
            Response.Redirect("~/PlaceItem.aspx?id=" + place.Id, false);
        }
    }
}
