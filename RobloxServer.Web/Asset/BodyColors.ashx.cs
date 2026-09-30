using System;
using System.Collections.Generic;
using RobloxServer.Data;
using RobloxServer.Web;

namespace RobloxServer.Handlers.Asset
{
    public class BodyColors : HandlerBase
    {
        protected override void Handle()
        {
            Response.ContentType = "text/xml";
            Response.Write(XmlFor(Db.FindUser(QueryLong("userId"))));
        }

        public static readonly string[] Parts = { "HeadColor", "TorsoColor", "LeftArmColor", "RightArmColor", "LeftLegColor", "RightLegColor" };

        /// <summary>The BrickColor number of one body part in a BodyColors XML.</summary>
        public static int ColorOf(string xml, string part)
        {
            var match = System.Text.RegularExpressions.Regex.Match(xml ?? "", "<int name=\"" + part + "\">(\\d+)</int>");
            int number;
            return match.Success && int.TryParse(match.Groups[1].Value, out number) ? number : 194;
        }

        /// <summary>Changes one body part's color and saves the user's BodyColors.</summary>
        public static void SetColor(User user, string part, int brickColor)
        {
            string xml = System.Text.RegularExpressions.Regex.Replace(XmlFor(user), "(<int name=\"" + part + "\">)\\d+(</int>)", "${1}" + brickColor + "${2}");
            Db.Users.Update(u => u.Id == user.Id, u => u.BodyColors = xml);
        }

        /// <summary>The user's BodyColors, or the default ones (all "Really black" for Noli).</summary>
        public static string XmlFor(User user)
        {
            // Noli is always completely black, whatever the character page says.
            if (user != null && !string.IsNullOrEmpty(user.BodyColors) && !Noli.Is(user))
            {
                return user.BodyColors;
            }

            string xml = Templates.Render("BodyColors.xml", new Dictionary<string, string>());
            if (Noli.Is(user))
            {
                xml = System.Text.RegularExpressions.Regex.Replace(xml, @"(<int name=""\w+Color"">)\d+(</int>)", "${1}" + Noli.ReallyBlack + "${2}");
            }
            return xml;
        }
    }
}
