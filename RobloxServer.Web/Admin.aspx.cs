using System;
using System.Linq;
using System.Web.UI.WebControls;
using RobloxServer.Data;
using RobloxServer.Security;
using RobloxServer.Web;

namespace RobloxServer.Pages
{
    public partial class Admin : BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            RequireAdmin();
            PublicKeyLiteral.Text = ScriptSigner.PublicKeyBlobBase64;
            if (!IsPostBack)
            {
                PackageList.DataSource = Config.Clients.Concat(new[] { ClientPackages.Launcher });
                PackageList.DataBind();
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            UsersRepeater.DataSource = Db.Users.All()
                .OrderBy(u => u.Id)
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.LastIp,
                    u.Created,
                    Status = u.IsCurrentlyBanned
                        ? "Banned: " + u.BanReason + (u.BanExpires.HasValue ? " (until " + u.BanExpires.Value.ToString("u") + ")" : " (permanent)")
                        : (Db.IsAdmin(u) ? "Admin" : "OK")
                })
                .ToList();
            UsersRepeater.DataBind();

            IpBansRepeater.DataSource = Db.IpBans.All();
            IpBansRepeater.DataBind();

            ServersRepeater.DataSource = GameServers.All();
            ServersRepeater.DataBind();

            ClientsRepeater.DataSource = Config.Clients.Concat(new[] { ClientPackages.Launcher })
                .Select(name => new { Name = name, Package = ClientPackages.Find(name) })
                .Select(c => new
                {
                    c.Name,
                    File = c.Package != null ? c.Package.FileName : "not uploaded yet",
                    Version = c.Package != null ? c.Package.Version : "-",
                    Size = c.Package != null ? (c.Package.Size / 1048576.0).ToString("0.0") + " MB" : "-",
                    Updated = c.Package != null ? c.Package.Updated.ToString("u") : "-"
                })
                .ToList();
            ClientsRepeater.DataBind();
        }

        User TargetUser()
        {
            User user = Db.FindUser(BanUserBox.Text);
            if (user == null)
            {
                MessageLabel.Text = "User not found.";
            }
            return user;
        }

        protected void BanButton_Click(object sender, EventArgs e)
        {
            User user = TargetUser();
            if (user == null)
            {
                return;
            }

            int days;
            int? duration = int.TryParse(BanDaysBox.Text, out days) && days > 0 ? days : (int?)null;
            string reason = string.IsNullOrWhiteSpace(BanReasonBox.Text) ? "Violation of the Terms of Service" : BanReasonBox.Text.Trim();
            Bans.BanUser(user.Id, reason, duration);
            Logging.Log(LogType.Security, CurrentUser.Name + " banned " + user.Name + ": " + reason);
            MessageLabel.Text = Server.HtmlEncode(user.Name + " has been banned.");
        }

        protected void UnbanButton_Click(object sender, EventArgs e)
        {
            User user = TargetUser();
            if (user == null)
            {
                return;
            }
            Bans.UnbanUser(user.Id);
            MessageLabel.Text = Server.HtmlEncode(user.Name + " has been unbanned.");
        }

        protected void MakeAdminButton_Click(object sender, EventArgs e)
        {
            User user = TargetUser();
            if (user == null)
            {
                return;
            }
            if (user.Id == CurrentUser.Id)
            {
                MessageLabel.Text = "You cannot change your own admin status.";
                return;
            }
            Db.Users.Update(u => u.Id == user.Id, u => u.IsAdmin = !u.IsAdmin);
            MessageLabel.Text = Server.HtmlEncode(user.Name + " admin status changed.");
        }

        protected void IpBanButton_Click(object sender, EventArgs e)
        {
            string ip = (IpBox.Text ?? "").Trim();
            System.Net.IPAddress parsed;
            if (!System.Net.IPAddress.TryParse(ip, out parsed))
            {
                MessageLabel.Text = "Invalid IP address.";
                return;
            }
            if (ip == ClientIp.Get(Context))
            {
                MessageLabel.Text = "You cannot ban your own IP.";
                return;
            }
            Bans.BanIp(parsed.ToString(), string.IsNullOrWhiteSpace(IpReasonBox.Text) ? "Banned" : IpReasonBox.Text.Trim());
            Logging.Log(LogType.Security, CurrentUser.Name + " banned IP " + ip);
        }

        protected void IpUnbanButton_Click(object sender, EventArgs e)
        {
            Bans.UnbanIp((IpBox.Text ?? "").Trim());
        }

        protected void PackageUploadButton_Click(object sender, EventArgs e)
        {
            if (!PackageUpload.HasFile)
            {
                PackageMessage.Text = "Choose a file to upload.";
                return;
            }

            string name = ClientPackages.CleanName(PackageList.SelectedValue);
            string temp = System.IO.Path.Combine(Config.DataPath, "upload-" + Guid.NewGuid().ToString("N") + ".tmp");
            PackageUpload.PostedFile.SaveAs(temp);
            string error = ClientPackages.Install(name, temp);
            if (error != null)
            {
                PackageMessage.Text = Server.HtmlEncode(error);
                return;
            }

            ClientPackages.Package package = ClientPackages.Find(name);
            Logging.Log(LogType.Backend, CurrentUser.Name + " uploaded " + name + " (" + package.Version + ", " + package.Size + " bytes)");
            PackageMessage.Text = Server.HtmlEncode(name + " uploaded: " + package.Version + ".");
        }

        protected void ServersRepeater_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "Remove")
            {
                GameServers.Remove((string)e.CommandArgument);
            }
        }
    }
}
