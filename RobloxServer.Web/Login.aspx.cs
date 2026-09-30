using System;
using System.Globalization;
using System.Web.UI.WebControls;
using RobloxServer.Data;
using RobloxServer.Security;
using RobloxServer.Web;

namespace RobloxServer.Pages
{
    /// <summary>2013 NewLogin page: sign in on the left, "Not a member?" with the birthday on the right.</summary>
    public partial class Login : BasePage
    {
        protected override bool AllowBanned
        {
            get { return true; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack)
            {
                return;
            }
            FillBirthday(MonthSelect, DaySelect, YearSelect);
        }

        /// <summary>The Month / Day / Year lists of the 2013 signup forms.</summary>
        public static void FillBirthday(DropDownList months, DropDownList days, DropDownList years)
        {
            months.Items.Add(new ListItem("Month", ""));
            for (int m = 1; m <= 12; m++)
            {
                months.Items.Add(new ListItem(CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(m), m.ToString()));
            }
            days.Items.Add(new ListItem("Day", ""));
            for (int d = 1; d <= 31; d++)
            {
                days.Items.Add(new ListItem(d.ToString(), d.ToString()));
            }
            years.Items.Add(new ListItem("Year", ""));
            for (int y = DateTime.UtcNow.Year; y >= 1900; y--)
            {
                years.Items.Add(new ListItem(y.ToString(), y.ToString()));
            }
        }

        protected void LoginButton_Click(object sender, EventArgs e)
        {
            string error;
            User user = Auth.Login(UserNameBox.Text, PasswordBox.Text, Context, out error);
            if (user == null)
            {
                ErrorPanel.Visible = true;
                ErrorLiteral.Text = Server.HtmlEncode(error);
                return;
            }

            Auth.SignIn(user, Context);
            Response.Redirect(Accounts.AfterLogin(user, Request.QueryString["ReturnUrl"]), false);
        }

        protected void SignUpButton_Click(object sender, EventArgs e)
        {
            // The rest of the signup (gender, username, password) is on the landing page.
            Response.Redirect("~/Landing.aspx?tab=signup&m=" + Server.UrlEncode(MonthSelect.SelectedValue)
                + "&d=" + Server.UrlEncode(DaySelect.SelectedValue) + "&y=" + Server.UrlEncode(YearSelect.SelectedValue), false);
        }
    }
}
