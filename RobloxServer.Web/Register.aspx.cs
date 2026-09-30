using System;
using RobloxServer.Data;
using RobloxServer.Security;
using RobloxServer.Web;

namespace RobloxServer.Pages
{
    public partial class Register : BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            RegisterPanel.Visible = Config.RegistrationOpen;
            ClosedMessage.Visible = !Config.RegistrationOpen;
        }

        protected void RegisterButton_Click(object sender, EventArgs e)
        {
            string error;
            User user = Accounts.Register(UserNameBox.Text, PasswordBox.Text, ConfirmBox.Text, Under13Box.Checked, null, Context, out error);
            if (user == null)
            {
                ErrorLabel.Text = Server.HtmlEncode(error);
                return;
            }
            Response.Redirect("~/My/Home.aspx", false);
        }
    }
}
