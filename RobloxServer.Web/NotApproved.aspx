<%@ Page Title="Not Approved" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="NotApproved.aspx.cs" Inherits="RobloxServer.Pages.NotApproved" %>
<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">
    <h1><asp:Literal ID="TitleLiteral" runat="server" /></h1>
    <p>Our content monitors have determined that your behavior at <%: RobloxServer.Config.SiteName %> has been in violation of our Terms of Service.</p>
    <p><b>Reason:</b> <asp:Literal ID="ReasonLiteral" runat="server" /></p>
    <asp:PlaceHolder ID="ReactivatePanel" runat="server" Visible="false">
        <p>Please abide by the Community Guidelines so that ROBLOX can be fun for users of all ages.</p>
        <asp:CheckBox ID="AgreeBox" runat="server" Text=" I agree" />
        <asp:Button ID="ReactivateButton" runat="server" Text="Reactivate My Account" CssClass="btn-medium btn-primary" OnClick="ReactivateButton_Click" />
    </asp:PlaceHolder>
</asp:Content>
