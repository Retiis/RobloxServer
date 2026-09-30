using System;
using RobloxServer.Web;

namespace RobloxServer.Pages
{
    public partial class Error : BasePage
    {
        protected override bool AllowBanned
        {
            get { return true; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.StatusCode = 500;
            Response.TrySkipIisCustomErrors = true;
        }
    }
}
