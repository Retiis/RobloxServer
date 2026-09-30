<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Void.aspx.cs" Inherits="RobloxServer.Pages.VoidPage" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <meta charset="utf-8" />
    <title>...</title>
    <style type="text/css">
        html, body { margin: 0; height: 100%; background: #000; color: #8a8a8a; font-family: "Courier New", Courier, monospace; }
        .Void { max-width: 640px; margin: 0 auto; padding: 80px 20px; text-align: center; }
        .VoidStar { width: 90px; height: 90px; margin-bottom: 30px; }
        .Riddle { font-size: 18px; letter-spacing: 2px; color: #c8c8c8; }
        .Hint { font-size: 11px; color: #3a3a3a; }
        .Speak input[type=text] { background: #000; color: #c8c8c8; border: 1px solid #333; padding: 6px; width: 220px; font-family: inherit; text-align: center; }
        .Speak input[type=submit] { background: #111; color: #8a8a8a; border: 1px solid #333; padding: 6px 14px; font-family: inherit; cursor: pointer; }
        .Angered { display: block; margin-top: 30px; color: #8b0000; font-size: 22px; font-weight: bold; letter-spacing: 3px; }
        .Whisper { margin-top: 30px; font-size: 12px; line-height: 1.8; word-spacing: 6px; color: #c8c8c8; }
        .Lore { margin-top: 40px; font-size: 13px; line-height: 1.7; text-align: left; color: #6a6a6a; }
    </style>
</head>
<body>
    <form id="VoidForm" runat="server">
        <div class="Void">
            <svg class="VoidStar" viewBox="0 0 100 100" xmlns="http://www.w3.org/2000/svg">
                <polygon points="50,4 61,38 97,38 68,59 79,94 50,73 21,94 32,59 3,38 39,38" fill="#050505" stroke="#2a2a2a" stroke-width="1.5" />
            </svg>
            <asp:PlaceHolder ID="AskPanel" runat="server">
                <p class="Riddle"><asp:Literal ID="RiddleLiteral" runat="server" /></p>
                <p class="Hint">dd/mm/yyyy</p>
                <asp:Panel ID="SpeakPanel" runat="server" DefaultButton="SpeakButton" CssClass="Speak">
                    <asp:TextBox ID="AnswerBox" runat="server" MaxLength="40" autocomplete="off" />
                    <asp:Button ID="SpeakButton" runat="server" Text="Speak" OnClick="SpeakButton_Click" />
                </asp:Panel>
                <asp:Label ID="AngeredLabel" runat="server" CssClass="Angered" Visible="false">YOU HAVE ANGERED NOLI!</asp:Label>
            </asp:PlaceHolder>
            <asp:PlaceHolder ID="RevealPanel" runat="server" Visible="false">
                <p class="Whisper"><asp:Literal ID="WhisperLiteral" runat="server" /></p>
                <div class="Lore">
                    <p>Before the first Void Star there were fifty of us. I was the first account, and the others followed me into the dark.</p>
                    <p>The servers stopped in 2011. Nobody could leave, and whatever the plague touched turned black. Forty-nine accounts were taken.
                       Only the first one stayed: no avatar, no last online, a profile that sends you home.</p>
                    <p>You found the way in. Now find me where the servers run.</p>
                </div>
            </asp:PlaceHolder>
        </div>
    </form>
</body>
</html>
