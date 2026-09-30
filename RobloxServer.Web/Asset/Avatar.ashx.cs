using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using RobloxServer.Data;
using RobloxServer.Web;

namespace RobloxServer.Handlers.Asset
{
    /// <summary>
    /// Avatar thumbnail (SVG) for profiles and the People search. There is no thumbnail renderer, so the
    /// Robloxian is drawn flat with the user's body colors. Unknown users get the 2010 question mark,
    /// and so does Noli: the myth says its avatar never loaded.
    /// </summary>
    public class Avatar : HandlerBase
    {
        /// <summary>RGB of the BrickColors a 2012/2013 body can have (also the palette of My/Character.aspx).</summary>
        public static readonly Dictionary<int, string> BrickColors = new Dictionary<int, string>
        {
            { 1, "F2F3F3" }, { 2, "A1A5A2" }, { 3, "F9E999" }, { 5, "D7C59A" }, { 6, "C2DAB8" }, { 9, "E8BAC8" },
            { 11, "80BBDB" }, { 12, "CB8442" }, { 18, "CC8E69" }, { 21, "C4281C" }, { 22, "C470A0" }, { 23, "0D69AC" },
            { 24, "F5CD30" }, { 25, "624732" }, { 26, "1B2A35" }, { 27, "6D6E6C" }, { 28, "287F47" }, { 29, "A1C48C" },
            { 36, "F3CF9B" }, { 37, "4B974B" }, { 38, "A05F35" }, { 45, "B4D2E4" }, { 101, "DA867A" }, { 102, "6E99CA" },
            { 104, "6B327C" }, { 105, "E29B40" }, { 106, "DA8541" }, { 107, "008F9C" }, { 119, "A4BD47" }, { 125, "EAB892" },
            { 135, "74869D" }, { 141, "27462D" }, { 151, "789082" }, { 153, "957977" }, { 192, "694028" }, { 194, "A3A2A5" },
            { 199, "635F62" }, { 208, "E5E4DF" }, { 217, "7C5C46" }, { 226, "FDEA8D" }, { 1001, "F8F8F8" }, { 1002, "CDCDCD" },
            { 1003, "111111" }, { 1004, "FF0000" }, { 1009, "FFFF00" }, { 1010, "0000FF" }
        };

        protected override void Handle()
        {
            User user = Db.FindUser(QueryLong("userId"));
            Response.ContentType = "image/svg+xml";
            Response.Write(user == null || Noli.Is(user) ? QuestionMark(Noli.Is(user)) : Robloxian(BodyColors.XmlFor(user)));
        }

        static string QuestionMark(bool noli)
        {
            var svg = new StringBuilder();
            svg.Append("<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 100 100\" width=\"100\" height=\"100\">");
            if (noli)
            {
                svg.Append("<desc>").Append(Noli.Clue).Append("</desc>");
            }
            svg.Append("<rect width=\"100\" height=\"100\" fill=\"#e9e9e9\" stroke=\"#bcbcbc\"/>");
            svg.Append("<text x=\"50\" y=\"72\" text-anchor=\"middle\" font-family=\"Arial, sans-serif\" font-size=\"64\" font-weight=\"bold\" fill=\"#9a9a9a\">?</text>");
            svg.Append("</svg>");
            return svg.ToString();
        }

        static string Robloxian(string bodyColorsXml)
        {
            string head = Color(bodyColorsXml, "HeadColor");
            string torso = Color(bodyColorsXml, "TorsoColor");
            string leftArm = Color(bodyColorsXml, "LeftArmColor");
            string rightArm = Color(bodyColorsXml, "RightArmColor");
            string leftLeg = Color(bodyColorsXml, "LeftLegColor");
            string rightLeg = Color(bodyColorsXml, "RightLegColor");

            // Seen from the front, the right arm and leg are on the left of the picture.
            var svg = new StringBuilder();
            svg.Append("<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 100 100\" width=\"100\" height=\"100\">");
            svg.Append("<g stroke=\"#000\" stroke-opacity=\"0.25\" stroke-width=\"0.8\">");
            svg.AppendFormat("<rect x=\"38\" y=\"4\" width=\"24\" height=\"22\" rx=\"5\" fill=\"#{0}\"/>", head);
            svg.AppendFormat("<rect x=\"30\" y=\"27\" width=\"40\" height=\"36\" fill=\"#{0}\"/>", torso);
            svg.AppendFormat("<rect x=\"12\" y=\"27\" width=\"17\" height=\"36\" fill=\"#{0}\"/>", rightArm);
            svg.AppendFormat("<rect x=\"71\" y=\"27\" width=\"17\" height=\"36\" fill=\"#{0}\"/>", leftArm);
            svg.AppendFormat("<rect x=\"30\" y=\"63\" width=\"19.5\" height=\"34\" fill=\"#{0}\"/>", rightLeg);
            svg.AppendFormat("<rect x=\"50.5\" y=\"63\" width=\"19.5\" height=\"34\" fill=\"#{0}\"/>", leftLeg);
            svg.Append("</g>");
            // The classic smile.
            svg.Append("<circle cx=\"45\" cy=\"13\" r=\"1.8\" fill=\"#000\"/><circle cx=\"55\" cy=\"13\" r=\"1.8\" fill=\"#000\"/>");
            svg.Append("<path d=\"M43.5 18.5 Q50 24 56.5 18.5\" fill=\"none\" stroke=\"#000\" stroke-width=\"1.4\" stroke-linecap=\"round\"/>");
            svg.Append("</svg>");
            return svg.ToString();
        }

        static string Color(string xml, string part)
        {
            Match match = Regex.Match(xml ?? "", "<int name=\"" + part + "\">(\\d+)</int>");
            int number;
            string rgb;
            if (match.Success && int.TryParse(match.Groups[1].Value, out number) && BrickColors.TryGetValue(number, out rgb))
            {
                return rgb;
            }
            return "A3A2A5";
        }
    }
}
