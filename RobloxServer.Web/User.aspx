<%@ Page Title="Profile" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="User.aspx.cs" Inherits="RobloxServer.Pages.UserProfile" %>
<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">
    <asp:PlaceHolder ID="NotFoundPanel" runat="server" Visible="false">
        <div class="PageBox">
            <h1>User not found</h1>
            <p>This user does not exist. <a href="<%: ResolveUrl("~/Browse.aspx") %>">Search for people</a></p>
        </div>
    </asp:PlaceHolder>
    <asp:PlaceHolder ID="ProfilePanel" runat="server">
        <div id="ProfileContainer">
            <div class="ProfileLeft">
                <div class="StandardBoxHeader"><asp:Literal ID="HeaderLiteral" runat="server" /></div>
                <div class="StandardBox ProfileBox">
                    <div class="ProfileStatus"><asp:Literal ID="StatusLiteral" runat="server" /></div>
                    <div class="ProfileUrl"><asp:Literal ID="UrlLiteral" runat="server" /></div>
                    <img id="AvatarImage" runat="server" class="ProfileAvatar" width="200" height="200" alt="" />
                    <asp:PlaceHolder ID="StatusPanel" runat="server" Visible="false"><div class="ProfileBlurb">"<asp:Literal ID="BlurbLiteral" runat="server" />"</div></asp:PlaceHolder>
                    <table class="ProfileDetails">
                        <tr><td class="Label">Joined:</td><td><asp:Literal ID="JoinedLiteral" runat="server" /></td></tr>
                        <tr><td class="Label">Last Online:</td><td><asp:Literal ID="LastOnlineLiteral" runat="server" /></td></tr>
                        <tr><td class="Label">Place Visits:</td><td><asp:Literal ID="VisitsLiteral" runat="server" /></td></tr>
                    </table>
                    <asp:PlaceHolder ID="AdminBadge" runat="server" Visible="false"><div class="ProfileBadge">Administrator</div></asp:PlaceHolder>
                </div>
            </div>
            <div class="ProfileRight">
                <div class="StandardBoxHeader">Active Places</div>
                <div class="StandardBox">
                    <asp:PlaceHolder ID="NoPlaces" runat="server"><p class="ProfileEmpty"><asp:Literal ID="NoPlacesLiteral" runat="server" /> has no public places.</p></asp:PlaceHolder>
                    <asp:Repeater ID="PlacesRepeater" runat="server">
                        <ItemTemplate>
                            <div class="ProfilePlace">
                                <a href="<%# ResolveUrl("~/PlaceItem.aspx?id=" + Eval("Id")) %>"><img src="<%# PlaceThumb(Eval("Id"), "160x100") %>" width="160" height="100" alt="<%# Enc("Name") %>" /></a>
                                <div class="ProfilePlaceDetails">
                                    <div class="ProfilePlaceName"><a href="<%# ResolveUrl("~/PlaceItem.aspx?id=" + Eval("Id")) %>"><%# Enc("Name") %></a></div>
                                    <div>Visited <%# Eval("Visits", "{0:N0}") %> times &middot; <%# GenreName(Eval("Genre")) %></div>
                                    <div class="ProfilePlaceDescription"><%# Enc("Description") %></div>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </div>
            <div style="clear: both"></div>
        </div>
    </asp:PlaceHolder>
</asp:Content>
