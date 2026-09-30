<%@ Page Title="Error" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Error.aspx.cs" Inherits="RobloxServer.Pages.Error" %>
<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">
    <div id="ErrorPage">
        <h1>Oops! We're sorry, but an error has occurred.</h1>
        <p>An unexpected error happened. It has been logged.</p>
        <a class="btn-medium btn-neutral" href="<%: ResolveUrl("~/Games") %>">Go to Games</a>
    </div>
</asp:Content>
