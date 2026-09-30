using System;
using System.Linq;
using RobloxServer.Data;
using RobloxServer.Handlers.Asset;
using RobloxServer.Web;

namespace RobloxServer.Pages
{
    /// <summary>
    /// 2013 character page: the avatar and the body colors mannequin. Clicking a body part opens the
    /// BrickColor palette; the colors are saved in User.BodyColors, which the game reads through
    /// Asset/BodyColors.ashx.
    /// </summary>
    public partial class MyCharacter : BasePage
    {
        static readonly string[][] Regions =
        {
            new[] { "HeadColor", "Head", "Head" },
            new[] { "TorsoColor", "Torso", "Torso" },
            new[] { "RightArmColor", "RightArm", "Right Arm" },
            new[] { "LeftArmColor", "LeftArm", "Left Arm" },
            new[] { "RightLegColor", "RightLeg", "Right Leg" },
            new[] { "LeftLegColor", "LeftLeg", "Left Leg" }
        };

        User user;

        protected void Page_Load(object sender, EventArgs e)
        {
            user = RequireLogin();
            Bind();
        }

        void Bind()
        {
            AvatarImage.Src = ResolveUrl("~/Asset/Avatar.ashx?userId=" + user.Id + "&v=" + DateTime.UtcNow.Ticks);
            AvatarImage.Alt = user.Name;

            // The myth: Noli stays completely black.
            VoidPanel.Visible = Noli.Is(user);
            ColorsPanel.Visible = !VoidPanel.Visible;
            if (VoidPanel.Visible)
            {
                return;
            }

            string xml = BodyColors.XmlFor(Db.FindUser(user.Id) ?? user);
            PartsRepeater.DataSource = Regions.Select(r =>
            {
                int color = BodyColors.ColorOf(xml, r[0]);
                string rgb;
                return new
                {
                    Part = r[0],
                    Css = r[1],
                    Label = r[2],
                    Color = color,
                    Rgb = Avatar.BrickColors.TryGetValue(color, out rgb) ? rgb : "A3A2A5"
                };
            });
            PartsRepeater.DataBind();
            PaletteRepeater.DataSource = Avatar.BrickColors;
            PaletteRepeater.DataBind();
        }

        protected void SaveColorButton_Click(object sender, EventArgs e)
        {
            int color;
            if (Noli.Is(user) || !BodyColors.Parts.Contains(PartField.Value) || !int.TryParse(ColorField.Value, out color)
                || !Avatar.BrickColors.ContainsKey(color))
            {
                return;
            }
            BodyColors.SetColor(Db.FindUser(user.Id) ?? user, PartField.Value, color);
            Bind();
        }
    }
}
