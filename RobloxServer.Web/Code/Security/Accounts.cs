using System;
using System.Web;
using RobloxServer.Data;

namespace RobloxServer.Security
{
    /// <summary>Account creation shared by Register.aspx and the 2013 landing page.</summary>
    public static class Accounts
    {
        /// <summary>Creates the account and signs it in. Returns null and sets <paramref name="error"/> on failure.</summary>
        public static User Register(string name, string password, string confirm, bool superSafeChat, string gender, HttpContext context, out string error)
        {
            error = null;
            if (!Config.RegistrationOpen)
            {
                error = "Registration is closed on this server.";
                return null;
            }

            name = (name ?? "").Trim();
            password = password ?? "";
            error = WordFilter.ValidateUserName(name);
            if (error == null && password.Length < 6)
            {
                error = "Passwords must be at least 6 characters.";
            }
            if (error == null && password != (confirm ?? ""))
            {
                error = "Passwords do not match.";
            }
            if (error == null && string.Equals(password, name, StringComparison.OrdinalIgnoreCase))
            {
                error = "Your password cannot be your username.";
            }
            if (error == null && Db.FindUser(name) != null)
            {
                error = "This username is already in use.";
            }

            // 2015 style signup flood check: 3 accounts per IP per hour.
            string ip = ClientIp.Get(context);
            if (error == null && FloodChecker.Hit("register:" + ip, 3, TimeSpan.FromHours(1)))
            {
                error = "Too many accounts have been created from your IP. Try again later.";
            }
            if (error != null)
            {
                return null;
            }

            bool firstUser = Db.Users.All().Count == 0;
            DateTime now = DateTime.UtcNow;
            string hash = PasswordHasher.Hash(password);
            User user = Db.Users.InsertWithId(u => u.Id, 1, id => new User
            {
                Id = id,
                Name = name,
                PasswordHash = hash,
                Created = now,
                LastOnline = now,
                LastIp = ip,
                IsAdmin = firstUser,
                SuperSafeChat = superSafeChat,
                Gender = gender
            });

            Logging.Log(LogType.Success, "New account " + user.Name + " (" + user.Id + ") from " + ip + (firstUser ? " [admin]" : ""));
            Auth.SignIn(user, context);
            return user;
        }

        /// <summary>Under 13 on the day of signup (2013: under 13 accounts get SuperSafeChat).</summary>
        public static bool IsUnder13(DateTime birthday)
        {
            DateTime today = DateTime.UtcNow.Date;
            int age = today.Year - birthday.Year;
            if (birthday.Date > today.AddYears(-age))
            {
                age--;
            }
            return age < 13;
        }

        /// <summary>Where to go after logging in: a local ReturnUrl, NotApproved.aspx for banned users, or Home.</summary>
        public static string AfterLogin(User user, string returnUrl)
        {
            if (user.IsCurrentlyBanned)
            {
                return "~/NotApproved.aspx";
            }
            if (string.IsNullOrEmpty(returnUrl) || !returnUrl.StartsWith("/") || returnUrl.StartsWith("//") || returnUrl.StartsWith("/\\"))
            {
                return "~/My/Home.aspx";
            }
            return returnUrl;
        }
    }
}
