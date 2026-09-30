using System;
using System.Linq;
using System.Web.UI.WebControls;
using RobloxServer.Data;
using RobloxServer.Security;
using RobloxServer.Web;

namespace RobloxServer.Pages
{
    /// <summary>
    /// The 2013 "BlueCity" landing page, shown at / to visitors who are not logged in:
    /// Sign up (birthday, gender, username, password) and Login tabs over the scrolling city.
    /// </summary>
    public partial class Landing : BasePage
    {
        protected string LoginError { get; private set; }
        protected string SignupError { get; private set; }

        /// <summary>"signup" on the first visit, "login" when the visitor has an account cookie or just failed to log in.</summary>
        protected string SelectedTab { get; private set; }

        protected override bool AllowBanned
        {
            get { return true; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1));

            if (CurrentUser != null)
            {
                Response.Redirect("~/My/Home.aspx", true);
                return;
            }

            SelectedTab = Request.Cookies["RBXReturning"] != null ? "login" : "signup";
            if (!IsPostBack)
            {
                Login.FillBirthday(lstMonths, lstDays, lstYears);
                // Coming from the "Sign Up" button of the login page, with the birthday already chosen.
                if (Request.QueryString["tab"] == "signup")
                {
                    SelectedTab = "signup";
                    Select(lstMonths, Request.QueryString["m"]);
                    Select(lstDays, Request.QueryString["d"]);
                    Select(lstYears, Request.QueryString["y"]);
                }
            }
        }

        static void Select(DropDownList list, string value)
        {
            if (list.Items.FindByValue(value ?? "") != null)
            {
                list.SelectedValue = value;
            }
        }

        protected void LoginButton_Click(object sender, EventArgs e)
        {
            SelectedTab = "login";
            string error;
            User user = Auth.Login(loginUsername.Text, loginPassword.Text, Context, out error);
            if (user == null)
            {
                LoginError = error;
                return;
            }
            Auth.SignIn(user, Context);
            Response.Redirect(Accounts.AfterLogin(user, null), false);
        }

        protected void SignUpButton_Click(object sender, EventArgs e)
        {
            SelectedTab = "signup";
            DateTime birthday;
            if (!TryGetBirthday(out birthday))
            {
                SignupError = "Please enter a valid birthday.";
                return;
            }
            if (!MaleBtn.Checked && !FemaleBtn.Checked)
            {
                SignupError = "Please choose Male or Female.";
                return;
            }

            string error;
            User user = Accounts.Register(username.Text, password.Text, passwordConfirm.Text, Accounts.IsUnder13(birthday),
                MaleBtn.Checked ? "Male" : "Female", Context, out error);
            if (user == null)
            {
                SignupError = error;
                return;
            }
            Response.Redirect("~/My/Home.aspx", false);
        }

        bool TryGetBirthday(out DateTime birthday)
        {
            birthday = DateTime.MinValue;
            int year, month, day;
            if (!int.TryParse(lstYears.SelectedValue, out year) || !int.TryParse(lstMonths.SelectedValue, out month)
                || !int.TryParse(lstDays.SelectedValue, out day) || year < 1900 || month < 1 || month > 12
                || day < 1 || day > DateTime.DaysInMonth(year, month))
            {
                return false;
            }
            birthday = new DateTime(year, month, day);
            return birthday <= DateTime.UtcNow.Date;
        }
    }
}
