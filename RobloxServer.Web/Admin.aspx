<%@ Page Title="Admin" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Admin.aspx.cs" Inherits="RobloxServer.Pages.Admin" %>
<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">
    <h1>Admin</h1>
    <asp:Label ID="MessageLabel" runat="server" CssClass="error" />

    <h2>Moderate a user</h2>
    <div class="form-row"><label>Username</label><asp:TextBox ID="BanUserBox" runat="server" MaxLength="20" CssClass="text-box text-box-medium" /></div>
    <div class="form-row"><label>Reason</label><asp:TextBox ID="BanReasonBox" runat="server" MaxLength="200" CssClass="text-box text-box-medium" /></div>
    <div class="form-row"><label>Days (empty = Account Deleted / permanent)</label><asp:TextBox ID="BanDaysBox" runat="server" MaxLength="4" CssClass="text-box text-box-medium" /></div>
    <asp:Button ID="BanButton" runat="server" Text="Ban" CssClass="btn-medium btn-negative" OnClick="BanButton_Click" />
    <asp:Button ID="UnbanButton" runat="server" Text="Unban" CssClass="btn-medium btn-neutral" OnClick="UnbanButton_Click" />
    <asp:Button ID="MakeAdminButton" runat="server" Text="Toggle admin" CssClass="btn-medium btn-neutral" OnClick="MakeAdminButton_Click" />

    <h2>IP bans</h2>
    <div class="form-row"><label>IP address</label><asp:TextBox ID="IpBox" runat="server" MaxLength="45" CssClass="text-box text-box-medium" /></div>
    <div class="form-row"><label>Reason</label><asp:TextBox ID="IpReasonBox" runat="server" MaxLength="200" CssClass="text-box text-box-medium" /></div>
    <asp:Button ID="IpBanButton" runat="server" Text="Ban IP" CssClass="btn-medium btn-negative" OnClick="IpBanButton_Click" />
    <asp:Button ID="IpUnbanButton" runat="server" Text="Unban IP" CssClass="btn-medium btn-neutral" OnClick="IpUnbanButton_Click" />
    <table class="table grid">
        <asp:Repeater ID="IpBansRepeater" runat="server">
            <HeaderTemplate><tr class="table-header"><th class="first">IP</th><th>Reason</th><th>Date</th></tr></HeaderTemplate>
            <ItemTemplate><tr><td><%# Enc("Ip") %></td><td><%# Enc("Reason") %></td><td><%# Eval("Created", "{0:u}") %></td></tr></ItemTemplate>
        </asp:Repeater>
    </table>

    <h2>Users</h2>
    <table class="table grid">
        <asp:Repeater ID="UsersRepeater" runat="server">
            <HeaderTemplate><tr class="table-header"><th class="first">Id</th><th>Name</th><th>Last IP</th><th>Created</th><th>Status</th></tr></HeaderTemplate>
            <ItemTemplate>
                <tr>
                    <td><%# Eval("Id") %></td><td><%# Enc("Name") %></td><td><%# Enc("LastIp") %></td>
                    <td><%# Eval("Created", "{0:u}") %></td><td><%# Enc("Status") %></td>
                </tr>
            </ItemTemplate>
        </asp:Repeater>
    </table>

    <h2>Running servers</h2>
    <table class="table grid">
        <asp:Repeater ID="ServersRepeater" runat="server" OnItemCommand="ServersRepeater_ItemCommand">
            <HeaderTemplate><tr class="table-header"><th class="first">Name</th><th>Place</th><th>Address</th><th>Client</th><th>Host</th><th>Players</th><th></th></tr></HeaderTemplate>
            <ItemTemplate>
                <tr>
                    <td><%# Enc("Name") %></td><td><%# Eval("PlaceId") %></td><td><%# Enc("Address") %>:<%# Eval("Port") %></td>
                    <td><%# Enc("Client") %></td><td><%# Enc("HostUserName") %></td><td><%# Eval("PlayerCount") %></td>
                    <td><asp:LinkButton runat="server" CommandName="Remove" CommandArgument='<%# Eval("JobId") %>'>Remove</asp:LinkButton></td>
                </tr>
            </ItemTemplate>
        </asp:Repeater>
    </table>

    <h2>Clients</h2>
    <p>RobloxPlayerLauncher downloads the clients from this site. Upload one zip per client (the contents of the client
       folder, with <code>RobloxApp_client.exe</code> at the top of the zip). The files go to <code>App_Data/Clients</code>
       and <code>App_Data/Launcher</code>; you can also copy them there by hand or by FTP. Uploading a new file makes every
       launcher update.</p>
    <div class="form-row">
        <label>Package</label>
        <asp:DropDownList ID="PackageList" runat="server" />
    </div>
    <div class="form-row">
        <label>File (.zip for a client, RobloxPlayerLauncher.exe for the launcher)</label>
        <asp:FileUpload ID="PackageUpload" runat="server" />
    </div>
    <asp:Button ID="PackageUploadButton" runat="server" Text="Upload" CssClass="btn-medium btn-primary" OnClick="PackageUploadButton_Click" />
    <asp:Label ID="PackageMessage" runat="server" CssClass="error" />
    <table class="table grid" style="margin-top: 12px">
        <asp:Repeater ID="ClientsRepeater" runat="server">
            <HeaderTemplate><tr class="table-header"><th class="first">Package</th><th>File</th><th>Version</th><th>Size</th><th>Updated</th></tr></HeaderTemplate>
            <ItemTemplate>
                <tr>
                    <td><%# Enc("Name") %></td><td><%# Enc("File") %></td><td><%# Enc("Version") %></td>
                    <td><%# Enc("Size") %></td><td><%# Enc("Updated") %></td>
                </tr>
            </ItemTemplate>
        </asp:Repeater>
    </table>

    <h2>Script signing key</h2>
    <p>Patch this PUBLICKEYBLOB into the 2015 clients so they accept scripts signed by this server
       (also available at <a href="Keys/PublicKey.ashx">Keys/PublicKey.ashx</a>).</p>
    <p class="mono"><asp:Literal ID="PublicKeyLiteral" runat="server" /></p>
</asp:Content>
