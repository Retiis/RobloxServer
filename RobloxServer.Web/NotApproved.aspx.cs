using System;
using RobloxServer.Data;
using RobloxServer.Security;
using RobloxServer.Web;

namespace RobloxServer.Pages
{
    /// <summary>The 2015 moderation page ("Banned for 1 Day" / "Account Deleted").</summary>
    public partial class NotApproved : BasePage
    {
        protected override bool AllowBanned
        {
            get { return true; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            User user = RequireLogin();
            if (user == null)
            {
                return;
            }

            if (!user.IsBanned)
            {
                Response.Redirect("~/Default.aspx", true);
                return;
            }

            if (!user.BanExpires.HasValue)
            {
                TitleLiteral.Text = "Account Deleted";
            }
            else if (user.IsCurrentlyBanned)
            {
                TimeSpan left = user.BanExpires.Value - DateTime.UtcNow;
                TitleLiteral.Text = "Banned for " + Math.Max(1, (int)Math.Ceiling(left.TotalDays)) + " Day(s)";
            }
            else
            {
                TitleLiteral.Text = "Warning";
                ReactivatePanel.Visible = true;
            }

            ReasonLiteral.Text = Server.HtmlEncode(user.BanReason ?? "");
        }

        protected void ReactivateButton_Click(object sender, EventArgs e)
        {
            User user = CurrentUser;
            if (user == null || user.IsCurrentlyBanned || !AgreeBox.Checked)
            {
                return;
            }
            Bans.UnbanUser(user.Id);
            Response.Redirect("~/Default.aspx", false);
        }
    }
}
