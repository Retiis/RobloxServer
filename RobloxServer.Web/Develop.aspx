<%@ Page Title="Build" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Develop.aspx.cs" Inherits="RobloxServer.Pages.Develop" %>
<asp:Content ID="Head" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="<%: ResolveUrl("~/Content/Pages/Build.css") %>" rel="stylesheet" type="text/css" />
</asp:Content>
<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">
    <h1 class="BuildPageHeaderTopLvl">Build</h1>
    <table id="build-page">
        <tr>
            <td class="menu-area">
                <a class="tab-item tab-item-selected" href="<%: ResolveUrl("~/Develop.aspx") %>">Places</a>
                <a class="tab-item" href="<%: ResolveUrl("~/Games") %>">Games</a>
                <a class="tab-item" href="<%: ResolveUrl("~/Install/Download.ashx?client=Launcher") %>">Download</a>
            </td>
            <td class="content-area">
                <table class="section-header">
                    <tr>
                        <td class="content-title"><h2 id="MyPlaces">Places</h2></td>
                        <td class="create-new"><a class="btn-medium btn-primary" href="#" id="CreateNewPlace">Create New Place</a></td>
                    </tr>
                </table>

                <div id="NewPlacePanel" class="<%: NewPlaceOpen ? "Open" : "" %>">
                    <div class="StandardBoxHeader">Create New Place</div>
                    <div class="StandardBox">
                        <p>Upload a place file (.rbxl or XML .rbxlx). You can also publish from Studio 2015 with <i>File &rarr; Publish to ROBLOX</i>.</p>
                        <div class="form-row">
                            <label for="<%= NameBox.ClientID %>">Name:</label>
                            <asp:TextBox ID="NameBox" runat="server" MaxLength="50" CssClass="text-box text-box-medium" />
                        </div>
                        <div class="form-row">
                            <label for="<%= DescriptionBox.ClientID %>">Description:</label>
                            <asp:TextBox ID="DescriptionBox" runat="server" TextMode="MultiLine" MaxLength="1000" CssClass="text-box" />
                        </div>
                        <div class="form-row">
                            <label for="<%= GenreList.ClientID %>">Genre:</label>
                            <asp:DropDownList ID="GenreList" runat="server" />
                        </div>
                        <div class="form-row">
                            <label for="<%= ClientList.ClientID %>">Client (the launcher uses this version to play it):</label>
                            <asp:DropDownList ID="ClientList" runat="server" />
                        </div>
                        <div class="form-row">
                            <label for="<%= MaxPlayersBox.ClientID %>">Max players per server:</label>
                            <asp:TextBox ID="MaxPlayersBox" runat="server" Text="12" MaxLength="3" CssClass="text-box text-box-medium short" />
                        </div>
                        <div class="form-row">
                            <asp:CheckBox ID="PublicBox" runat="server" Checked="true" Text=" Public" />
                            &nbsp; <asp:CheckBox ID="FilteringBox" runat="server" Text=" FilteringEnabled" />
                        </div>
                        <div class="form-row">
                            <label for="<%= PlaceUpload.ClientID %>">Place file:</label>
                            <asp:FileUpload ID="PlaceUpload" runat="server" />
                        </div>
                        <asp:Label ID="ErrorLabel" runat="server" CssClass="error" />
                        <asp:Button ID="PublishButton" runat="server" Text="Publish" CssClass="btn-medium btn-primary" OnClick="PublishButton_Click" />
                    </div>
                </div>

                <asp:PlaceHolder ID="NoPlaces" runat="server" Visible="false">
                    <p class="aside-text">You have not created any places yet. Click <b>Create New Place</b> to publish your first one.</p>
                </asp:PlaceHolder>
                <asp:Repeater ID="MyPlacesRepeater" runat="server" EnableViewState="false">
                    <SeparatorTemplate><div class="separator"></div></SeparatorTemplate>
                    <ItemTemplate>
                        <table class="item-table">
                            <tr>
                                <td class="image-col"><a class="game-image" href="<%# ResolveUrl("~/PlaceItem.aspx?id=" + Eval("Id")) %>"><img src="<%# PlaceThumb(Eval("Id"), "160x100") %>" alt="<%# Enc("Name") %>" /></a></td>
                                <td class="name-col">
                                    <a class="title" href="<%# ResolveUrl("~/PlaceItem.aspx?id=" + Eval("Id")) %>"><%# Enc("Name") %></a>
                                    <table class="details-table">
                                        <tr>
                                            <td class="activate-cell"><span class="<%# (bool)Eval("IsPublic") ? "place-active" : "place-inactive" %>"><%# (bool)Eval("IsPublic") ? "Public" : "Private" %></span></td>
                                            <td><span>Updated:</span><%# Ago((DateTime)Eval("Updated")) %></td>
                                        </tr>
                                    </table>
                                    <div class="aside-text"><%# GenreName(Eval("Genre")) %> &middot; <%# Enc("Client") %> &middot; version <%# Eval("Version") %> &middot; id <%# Eval("Id") %></div>
                                </td>
                                <td class="stats-col-games">
                                    <div class="totals-label">Visits:<span><%# Eval("Visits", "{0:N0}") %></span></div>
                                </td>
                                <td class="menu-col">
                                    <a class="gear-button" title="Configure this Place" href="<%# ResolveUrl("~/PlaceItem.aspx?id=" + Eval("Id")) %>#Configure"></a>
                                </td>
                            </tr>
                        </table>
                    </ItemTemplate>
                </asp:Repeater>
            </td>
        </tr>
    </table>
    <script type="text/javascript">
        document.getElementById("CreateNewPlace").onclick = function () {
            var panel = document.getElementById("NewPlacePanel");
            panel.className = panel.className === "Open" ? "" : "Open";
            return false;
        };
    </script>
</asp:Content>
