using System;
using RobloxServer.Data;
using RobloxServer.Web;

namespace RobloxServer.Pages
{
    /// <summary>Second step of the Noli ARG (see Code/Data/Noli.cs). Reached from the base64 clue: /Void.</summary>
    public partial class VoidPage : BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            RiddleLiteral.Text = Server.HtmlEncode(Noli.Riddle);
        }

        protected void SpeakButton_Click(object sender, EventArgs e)
        {
            if (!Noli.IsAnswer(AnswerBox.Text))
            {
                AngeredLabel.Visible = true;
                AnswerBox.Text = "";
                return;
            }

            AskPanel.Visible = false;
            RevealPanel.Visible = true;
            WhisperLiteral.Text = Noli.Whisper;
        }
    }
}
