<%@ Page Title="People" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Browse.aspx.cs" Inherits="RobloxServer.Pages.Browse" %>
<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">
    <div id="BrowseContainer">
        <h1>People</h1>
        <asp:Panel ID="SearchPanel" runat="server" DefaultButton="SearchButton" CssClass="BrowseSearch">
            <asp:TextBox ID="NameBox" runat="server" MaxLength="20" CssClass="text-box text-box-medium" />
            <asp:Button ID="SearchButton" runat="server" Text="Search Users" CssClass="btn-control btn-control-medium" OnClick="SearchButton_Click" />
        </asp:Panel>
        <asp:PlaceHolder ID="NoResults" runat="server" Visible="false"><p>No results for "<asp:Literal ID="QueryLiteral" runat="server" />".</p></asp:PlaceHolder>
        <table class="table grid BrowseTable">
            <asp:Repeater ID="UsersRepeater" runat="server">
                <HeaderTemplate><tr class="table-header"><th class="first">Avatar</th><th>Name</th><th>Location / Last Seen</th><th>Joined</th></tr></HeaderTemplate>
                <ItemTemplate>
                    <tr>
                        <%# Eval("Comment") %>
                        <td class="BrowseAvatar"><a href="<%# Eval("ProfileUrl") %>"><img src="<%# Eval("AvatarUrl") %>" width="48" height="48" alt="<%# Enc("Name") %>" /></a></td>
                        <td><a href="<%# Eval("ProfileUrl") %>"><b><%# Enc("Name") %></b></a></td>
                        <td><%# Eval("Status") %></td>
                        <td><%# Eval("Joined") %></td>
                    </tr>
                </ItemTemplate>
            </asp:Repeater>
        </table>
    </div>
</asp:Content>
