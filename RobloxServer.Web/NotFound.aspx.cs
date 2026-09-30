using System;
using RobloxServer.Web;

namespace RobloxServer.Pages
{
    public partial class NotFound : BasePage
    {
        protected override bool AllowBanned
        {
            get { return true; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.StatusCode = 404;
            Response.TrySkipIisCustomErrors = true;
        }
    }
}
