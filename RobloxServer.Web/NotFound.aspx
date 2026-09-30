<%@ Page Title="Page not found" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="NotFound.aspx.cs" Inherits="RobloxServer.Pages.NotFound" %>
<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">
    <div id="ErrorPage">
        <h1>Page not found</h1>
        <p>The page you requested does not exist.</p>
        <a class="btn-medium btn-neutral" href="<%: ResolveUrl("~/Games") %>">Go to Games</a>
    </div>
</asp:Content>
