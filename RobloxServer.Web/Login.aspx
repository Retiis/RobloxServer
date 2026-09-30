<%@ Page Title="Login" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="RobloxServer.Pages.Login" %>
<asp:Content ID="Head" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="<%: ResolveUrl("~/Content/Pages/Login.css") %>" rel="stylesheet" type="text/css" />
</asp:Content>
<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">
    <div class="NewLogin">
        <h1>Login to <%: RobloxServer.Config.SiteName %></h1>
        <div id="loginarea" class="divider-bottom">
            <div id="leftArea">
                <asp:Panel ID="LoginPanel" runat="server" DefaultButton="LoginButton">
                    <div id="loginPanel">
                        <asp:PlaceHolder ID="ErrorPanel" runat="server" Visible="false">
                            <div class="validation-summary-errors"><ul><li><asp:Literal ID="ErrorLiteral" runat="server" /></li></ul></div>
                        </asp:PlaceHolder>
                        <table id="logintable">
                            <tr id="username">
                                <td><label for="<%= UserNameBox.ClientID %>" class="form-label">Username:</label></td>
                                <td><asp:TextBox ID="UserNameBox" runat="server" MaxLength="20" CssClass="text-box text-box-medium" /></td>
                            </tr>
                            <tr id="password">
                                <td><label for="<%= PasswordBox.ClientID %>" class="form-label">Password:</label></td>
                                <td><asp:TextBox ID="PasswordBox" runat="server" TextMode="Password" MaxLength="200" CssClass="text-box text-box-medium" /></td>
                            </tr>
                        </table>
                        <div>
                            <div id="forgotPasswordPanel">Forgot your password? Ask an administrator.</div>
                            <div id="signInButtonPanel">
                                <asp:Button ID="LoginButton" runat="server" Text="Sign In" CssClass="btn-medium btn-neutral" OnClick="LoginButton_Click" />
                            </div>
                            <div class="clearFloats"></div>
                        </div>
                    </div>
                </asp:Panel>
            </div>
            <div id="rightArea" class="divider-left">
                <asp:Panel ID="SignupPanel" runat="server" DefaultButton="SignUpButton">
                    <div id="signUpPanel" class="FrontPageLoginBox">
                        <p class="text">Not a member?</p>
                        <h2>Sign up to build &amp; make friends</h2>
                        <p class="text">What is your birthday?</p>
                        <asp:DropDownList ID="MonthSelect" runat="server" CssClass="form-select" />
                        <asp:DropDownList ID="DaySelect" runat="server" CssClass="form-select" />
                        <asp:DropDownList ID="YearSelect" runat="server" CssClass="form-select" />
                        <p class="footnote" id="disclaimer">Your birthday will not be given out to any third party!</p>
                        <asp:Button ID="SignUpButton" runat="server" Text="Sign Up" CssClass="btn-medium btn-primary" OnClick="SignUpButton_Click" />
                    </div>
                </asp:Panel>
            </div>
        </div>
        <div id="guestarea">
            <h2>Just looking?</h2>
            <p class="text">See what people are building on <%: RobloxServer.Config.SiteName %>.</p>
            <a href="<%: ResolveUrl("~/Games") %>" class="btn-small btn-neutral" id="guestButton">Browse Games</a>
        </div>
    </div>
</asp:Content>
