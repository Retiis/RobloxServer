using System;
using System.Web.UI;
using RobloxServer.Data;
using RobloxServer.Security;

namespace RobloxServer
{
    public partial class SiteMaster : MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            User user = Auth.CurrentUser;
            bool isAdmin = Db.IsAdmin(user);
            LoggedIn.Visible = user != null;
            LoggedOut.Visible = user == null;
            SubMenu.Visible = user != null;
            AdminLink.Visible = isAdmin;
            AdminSubLink.Visible = isAdmin;
            if (user != null)
            {
                UserNameLiteral.Text = Server.HtmlEncode(user.Name);
                Over13Icon.Visible = !user.SuperSafeChat;
            }

            GenreLinks.DataSource = Genres.All;
            GenreLinks.DataBind();
        }

        protected void LogoutButton_Click(object sender, EventArgs e)
        {
            Auth.SignOut(Context);
            Response.Redirect("~/", false);
        }
    }
}
