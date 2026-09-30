<%@ Page Title="Home" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Home.aspx.cs" Inherits="RobloxServer.Pages.MyHome" %>
<asp:Content ID="Head" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="<%: ResolveUrl("~/Content/Pages/Home.css") %>" rel="stylesheet" type="text/css" />
</asp:Content>
<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">
    <div class="home-container">
        <h1>Hello, <asp:Literal ID="NameLiteral" runat="server" />!</h1>

        <div class="left-column">
            <div class="left-column-boxes">
                <div class="user-avatar-container">
                    <div class="user-avatar-holder">
                        <a class="user-avatar" href="<%: ResolveUrl("~/User.aspx") %>"><img id="AvatarImage" runat="server" class="user-avatar-image" alt="" /></a>
                    </div>
                    <div class="user-avatar-text"><a href="<%: ResolveUrl("~/My/Character.aspx") %>">Change Character</a></div>
                </div>
            </div>
            <div class="left-column-boxes">
                <h2>People</h2>
                <div class="best-friends">
                    <asp:Repeater ID="PeopleRepeater" runat="server" EnableViewState="false">
                        <ItemTemplate>
                            <div class="user">
                                <a class="avatar-image-link" href="<%# Eval("ProfileUrl") %>"><img src="<%# Eval("AvatarUrl") %>" alt="<%# Enc("Name") %>" /></a>
                                <div class="info">
                                    <div class="name"><img src="<%# Eval("StatusIcon") %>" alt="" /><a href="<%# Eval("ProfileUrl") %>"><%# Enc("Name") %></a></div>
                                    <div class="status"><%# Enc("Status") %></div>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                    <a href="<%: ResolveUrl("~/Browse.aspx") %>">See all people</a>
                </div>
            </div>
        </div>

        <div class="middle-column">
            <asp:Panel ID="StatusPanel" runat="server" DefaultButton="ShareButton" CssClass="status-update">
                <asp:TextBox ID="StatusBox" runat="server" MaxLength="254" CssClass="text-box text-box-medium status-textbox" placeholder="What are you up to?" />
                <asp:Button ID="ShareButton" runat="server" Text="Share" CssClass="btn-control btn-control-medium share-button" OnClick="ShareButton_Click" />
            </asp:Panel>
            <asp:Label ID="StatusError" runat="server" CssClass="error" />
            <h2>My Feed</h2>
            <div class="middle-column-box">
                <asp:PlaceHolder ID="EmptyFeed" runat="server" Visible="false">
                    <div class="default-roblox-feed">
                        <div class="default-feed-content">Nobody has posted yet. Tell everyone what you are up to, then check back here to see what the other builders are doing.</div>
                    </div>
                </asp:PlaceHolder>
                <asp:Repeater ID="FeedRepeater" runat="server" EnableViewState="false">
                    <ItemTemplate>
                        <div class="feed-container">
                            <div class="feed-image-container"><a href="<%# Eval("ProfileUrl") %>"><img src="<%# Eval("AvatarUrl") %>" alt="<%# Enc("UserName") %>" /></a></div>
                            <div class="feed-text-container">
                                <a class="feed-name" href="<%# Eval("ProfileUrl") %>"><%# Enc("UserName") %></a>
                                <div>"<%# Enc("Text") %>"</div>
                                <div class="feed-date"><%# Eval("When") %></div>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </div>

        <div class="right-column">
            <div class="right-column-box">
                <h2>Recently Played Games</h2>
                <asp:PlaceHolder ID="NoRecent" runat="server"><p class="home-empty">You have not played any games yet. <a href="<%: ResolveUrl("~/Games") %>">Find a game</a></p></asp:PlaceHolder>
                <asp:Repeater ID="RecentRepeater" runat="server" EnableViewState="false">
                    <ItemTemplate>
                        <div class="recent-place-container">
                            <a class="recent-place-thumb" href="<%# ResolveUrl("~/PlaceItem.aspx?id=" + Eval("Id")) %>"><img class="recent-place-thumb-img" src="<%# PlaceThumb(Eval("Id"), "160x100") %>" alt="" /></a>
                            <a class="recent-place-name" href="<%# ResolveUrl("~/PlaceItem.aspx?id=" + Eval("Id")) %>"><%# Enc("Name") %></a>
                            <div class="recent-place-players-online"><%# Eval("Playing", "{0:N0}") %> players online</div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
            <div class="right-column-box">
                <h2><%: RobloxServer.Config.SiteName %> News</h2>
                <div class="roblox-news-feed">
                    <asp:Repeater ID="NewsRepeater" runat="server" EnableViewState="false">
                        <ItemTemplate><div class="roblox-news-feed-item"><a href="<%# Eval("Url") %>"><%# Enc("Text") %></a></div></ItemTemplate>
                    </asp:Repeater>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
