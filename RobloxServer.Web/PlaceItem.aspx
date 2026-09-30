<%@ Page Title="Game" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="PlaceItem.aspx.cs" Inherits="RobloxServer.Pages.PlaceItem" %>
<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">
    <asp:PlaceHolder ID="NotFoundPanel" runat="server" Visible="false">
        <div class="PageBox">
            <h1>Game not found</h1>
            <p>This game does not exist or is private. <a href="<%: ResolveUrl("~/Games") %>">Back to Games</a></p>
        </div>
    </asp:PlaceHolder>
    <asp:PlaceHolder ID="PlacePanel" runat="server">
        <div id="ItemContainer" class="PlaceItem">
            <h1 class="ItemTitle"><asp:Literal ID="NameLiteral" runat="server" /></h1>
            <div class="ItemBuilder">Builder: <b><asp:Literal ID="CreatorLiteral" runat="server" /></b></div>

            <div class="ItemLeft">
                <div class="ItemThumbnail"><img id="ThumbnailImage" runat="server" width="420" height="230" alt="" /></div>
                <div class="ItemDescription">
                    <div class="StandardBoxHeader">Description</div>
                    <div class="StandardBox"><p class="DescriptionText"><asp:Literal ID="DescriptionLiteral" runat="server" /></p></div>
                </div>
            </div>

            <div class="ItemRight">
                <div id="PlaceLauncher" runat="server">
                    <a class="btn-large btn-play" href="#" data-launch-mode="play">Play</a>
                    <asp:PlaceHolder ID="HostPanel" runat="server" Visible="false">
                        <a class="btn-medium btn-neutral HostButton" href="#" data-launch-mode="host">Host Server</a>
                    </asp:PlaceHolder>
                </div>
                <div class="PlayInstructions">
                    <b><asp:Literal ID="NameLiteral2" runat="server" /></b> is played with the <b><asp:Literal ID="ClientLiteral2" runat="server" /></b> client.
                    First time? <a href="<%: ResolveUrl("~/Install/Download.ashx?client=Launcher") %>">Download ROBLOX</a>, run it once, then click Play.
                    <span class="PlaceId">(place id <asp:Literal ID="IdLiteral" runat="server" />)</span>
                </div>
                <table class="ItemDetails">
                    <tr><td class="Label">Visited:</td><td><asp:Literal ID="VisitsLiteral" runat="server" /></td></tr>
                    <tr><td class="Label">Updated:</td><td><asp:Literal ID="UpdatedLiteral" runat="server" /></td></tr>
                    <tr><td class="Label">Client:</td><td><asp:Literal ID="ClientLiteral" runat="server" /></td></tr>
                    <tr><td class="Label">Max Players:</td><td><asp:Literal ID="MaxPlayersLiteral" runat="server" /></td></tr>
                    <tr><td class="Label">Genre:</td><td><img id="GenreImage" runat="server" class="GenreIcon" alt="" /> <asp:Literal ID="GenreLiteral" runat="server" /></td></tr>
                    <tr><td class="Label">Allowed Gear:</td><td><img src="<%: ResolveUrl("~/Images/Icons/NoSuitcase16x16.png") %>" class="GearIcon" alt="No gear" title="No gear allowed" /></td></tr>
                </table>
                <div class="ItemDownload"><a id="DownloadLink" runat="server" class="btn-control btn-control-medium">Download .rbxl</a></div>
            </div>
            <div style="clear: both"></div>

            <div class="ItemTabs">
                <div class="ItemTab SelectedTab">Games</div>
            </div>
            <div class="ItemTabContent">
                <h2 class="light">Running Games</h2>
                <asp:PlaceHolder ID="NoServers" runat="server"><p>No servers are running this game right now. Click <b>Host Server</b> to start one.</p></asp:PlaceHolder>
                <table class="table grid">
                    <asp:Repeater ID="ServersRepeater" runat="server">
                        <HeaderTemplate><tr class="table-header"><th class="first">Server</th><th>Host</th><th>Players</th><th>Started</th></tr></HeaderTemplate>
                        <ItemTemplate>
                            <tr>
                                <td><img src="<%# ResolveUrl("~/Images/Icons/online.png") %>" alt="" /> <%# Enc("Name") %></td>
                                <td><%# Enc("HostUserName") %></td>
                                <td><%# Eval("PlayerCount") %> / <%# Eval("MaxPlayers") %></td>
                                <td><%# Eval("Started", "{0:u}") %></td>
                            </tr>
                        </ItemTemplate>
                    </asp:Repeater>
                </table>
            </div>

            <div id="PlaceLauncherStatusPanel" class="PlaceLauncherOverlay" style="display: none">
                <div class="PlaceLauncherModal">
                    <div class="Spinner"><img src="<%: ResolveUrl("~/Images/Icons/ProgressIndicator3.gif") %>" width="32" height="32" alt="Progress" /></div>
                    <div class="PlaceLauncherStatus" id="PlaceLauncherStatus">Starting ROBLOX...</div>
                    <div class="PlaceLauncherInstall" id="PlaceLauncherInstall" style="display: none">
                        Nothing happened? <a href="<%: ResolveUrl("~/Install/Download.ashx?client=Launcher") %>">Download ROBLOX</a>,
                        run <b>RobloxPlayerLauncher.exe</b> once to install it, then click Play again.
                    </div>
                    <input type="button" class="Button CancelPlaceLauncherButton" id="CancelPlaceLauncher" value="Cancel" />
                </div>
            </div>
            <script type="text/javascript" src="<%: ResolveUrl("~/Content/PlaceLauncher.js") %>"></script>

            <asp:PlaceHolder ID="EditPanel" runat="server" Visible="false">
                <div class="PageBox" id="Configure">
                    <div class="StandardBoxHeader">Configure this Place</div>
                    <div class="StandardBox">
                        <div class="form-row"><label>Name:</label><asp:TextBox ID="NameBox" runat="server" MaxLength="50" CssClass="text-box text-box-medium" /></div>
                        <div class="form-row"><label>Description:</label><asp:TextBox ID="DescriptionBox" runat="server" TextMode="MultiLine" CssClass="text-box" /></div>
                        <div class="form-row"><label>Genre:</label><asp:DropDownList ID="GenreList" runat="server" /></div>
                        <div class="form-row"><label>Client:</label><asp:DropDownList ID="ClientList" runat="server" /></div>
                        <div class="form-row"><label>Max players per server:</label><asp:TextBox ID="MaxPlayersBox" runat="server" MaxLength="3" CssClass="text-box text-box-medium short" /></div>
                        <div class="form-row">
                            <asp:CheckBox ID="PublicBox" runat="server" Text=" Public" />
                            &nbsp; <asp:CheckBox ID="FilteringBox" runat="server" Text=" FilteringEnabled" />
                        </div>
                        <div class="form-row"><label>Upload a new version (optional):</label><asp:FileUpload ID="PlaceUpload" runat="server" /></div>
                        <asp:Label ID="ErrorLabel" runat="server" CssClass="error" />
                        <asp:Button ID="SaveButton" runat="server" Text="Save" CssClass="btn-medium btn-primary" OnClick="SaveButton_Click" />
                        <asp:Button ID="DeleteButton" runat="server" Text="Delete" CssClass="btn-medium btn-negative" OnClick="DeleteButton_Click"
                            OnClientClick="return confirm('Delete this game forever?');" />
                    </div>
                </div>
            </asp:PlaceHolder>
        </div>
    </asp:PlaceHolder>
</asp:Content>
