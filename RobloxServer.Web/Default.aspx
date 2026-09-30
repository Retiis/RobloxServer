<%@ Page Title="Games" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="RobloxServer.Pages.Default" %>
<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">
    <div id="GamesContainer">
        <div class="GameFilter">
            <div class="GameFilterGroup">
                <div class="GameFilterTitle">Sort</div>
                <ul>
                    <asp:Repeater ID="SortRepeater" runat="server">
                        <ItemTemplate>
                            <li><a class="<%# (bool)Eval("Selected") ? "SelectedSort" : "GamesSort" %>" href="<%# Eval("Url") %>"><%# Eval("Name") %></a></li>
                        </ItemTemplate>
                    </asp:Repeater>
                </ul>
            </div>
            <div class="GameFilterGroup">
                <div class="GameFilterTitle">Genre</div>
                <ul>
                    <asp:Repeater ID="GenreRepeater" runat="server">
                        <ItemTemplate>
                            <li><a class="<%# (bool)Eval("Selected") ? "SelectedGenre" : "GamesGenre" %>" href="<%# Eval("Url") %>"><%# Eval("Name") %></a></li>
                        </ItemTemplate>
                    </asp:Repeater>
                </ul>
            </div>
        </div>

        <div id="Games">
            <div class="GamesHeader">
                <span class="GamesDisplaySet"><asp:Literal ID="HeaderLiteral" runat="server" /></span>
                <span class="GamesHint">Pick a game and click <b>Play</b>. First time? <a href="<%: ResolveUrl("~/Install/Download.ashx?client=Launcher") %>">Download ROBLOX</a></span>
            </div>
            <asp:PlaceHolder ID="EmptyMessage" runat="server" Visible="false">
                <div class="SystemAlert">No games have been published yet. <a href="Develop.aspx">Publish the first one!</a></div>
            </asp:PlaceHolder>
            <div class="GameList">
                <asp:Repeater ID="GamesRepeater" runat="server">
                    <ItemTemplate>
                        <div class="Game" title="<%# Enc("Name") %> by <%# Enc("CreatorName") %>">
                            <a class="GameThumbnail" href="PlaceItem.aspx?id=<%# Eval("Id") %>"><img src="<%# PlaceThumb(Eval("Id"), "160x100") %>" width="160" height="100" alt="<%# Enc("Name") %>" /></a>
                            <div class="GameDetails">
                                <div class="GameName"><a href="PlaceItem.aspx?id=<%# Eval("Id") %>"><%# Enc("Name") %></a></div>
                                <div class="GameInfo">
                                    <span class="PlayersOnline"><span class="PlayerCount"><%# Eval("Playing", "{0:N0}") %></span> players online</span>
                                    <span class="GameIcons">
                                        <img class="GenreIcon" src="<%# GenreIcon(Eval("Genre")) %>" alt="<%# GenreName(Eval("Genre")) %>" title="Genre: <%# GenreName(Eval("Genre")) %>" />
                                        <img class="GearIcon" src="<%# ResolveUrl("~/Images/Icons/NoSuitcase16x16.png") %>" alt="No gear" title="No gear allowed" />
                                    </span>
                                </div>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </div>
    </div>
</asp:Content>
