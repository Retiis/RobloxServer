<%@ Page Title="Sign Up" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Register.aspx.cs" Inherits="RobloxServer.Pages.Register" %>
<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">
    <div class="SignupPage">
        <h1>Sign Up and Start Having Fun!</h1>
        <asp:Panel ID="RegisterPanel" runat="server" DefaultButton="RegisterButton">
            <div class="StandardBox">
                <div class="form-row">
                    <label for="<%= UserNameBox.ClientID %>">Username:</label>
                    <asp:TextBox ID="UserNameBox" runat="server" MaxLength="20" CssClass="text-box text-box-large" />
                    <span class="tip-text">3-20 characters: letters, numbers and one underscore. Don't use your real name.</span>
                </div>
                <div class="form-row">
                    <label for="<%= PasswordBox.ClientID %>">Password:</label>
                    <asp:TextBox ID="PasswordBox" runat="server" TextMode="Password" MaxLength="200" CssClass="text-box text-box-large" />
                </div>
                <div class="form-row">
                    <label for="<%= ConfirmBox.ClientID %>">Confirm Password:</label>
                    <asp:TextBox ID="ConfirmBox" runat="server" TextMode="Password" MaxLength="200" CssClass="text-box text-box-large" />
                </div>
                <div class="form-row">
                    <asp:CheckBox ID="Under13Box" runat="server" Text=" I am under 13 (enables SuperSafeChat)" />
                </div>
                <asp:Label ID="ErrorLabel" runat="server" CssClass="error" />
                <asp:Button ID="RegisterButton" runat="server" Text="Sign Up" CssClass="btn-large btn-primary" OnClick="RegisterButton_Click" />
                <p class="SignupLogin">Already a member? <a href="Login.aspx">Log in</a></p>
            </div>
        </asp:Panel>
        <asp:PlaceHolder ID="ClosedMessage" runat="server" Visible="false">
            <div class="SystemAlert">Registration is closed on this server.</div>
        </asp:PlaceHolder>
    </div>
</asp:Content>
