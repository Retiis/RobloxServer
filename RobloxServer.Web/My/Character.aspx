<%@ Page Title="Character" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Character.aspx.cs" Inherits="RobloxServer.Pages.MyCharacter" %>
<asp:Content ID="Head" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="<%: ResolveUrl("~/Content/Pages/Character.css") %>" rel="stylesheet" type="text/css" />
</asp:Content>
<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">
    <div class="CharacterPage">
        <h1>Character Customizer</h1>
        <div class="Column1f">
            <div class="StandardBoxHeader">Avatar</div>
            <div class="StandardBox">
                <img id="AvatarImage" runat="server" class="AvatarImage" alt="" />
            </div>
        </div>
        <div class="Column2f">
            <div class="StandardBoxHeader">Avatar Body Colors</div>
            <div class="StandardBox CustomizeCharacterContainer">
                <asp:PlaceHolder ID="VoidPanel" runat="server" Visible="false">
                    <p class="NoResults">The void does not let go of its color.</p>
                </asp:PlaceHolder>
                <asp:PlaceHolder ID="ColorsPanel" runat="server">
                    <p class="CharacterHint">Click a body part to change its color.</p>
                    <div class="Mannequin" style="margin: 0 auto;">
                        <div class="ColorChooserFrame">
                            <asp:Repeater ID="PartsRepeater" runat="server" EnableViewState="false">
                                <ItemTemplate>
                                    <div class="ColorChooserRegion <%# Eval("Css") %>" data-part="<%# Eval("Part") %>" data-color="<%# Eval("Color") %>"
                                        title="<%# Eval("Label") %>" style="background-color: #<%# Eval("Rgb") %>;"></div>
                                </ItemTemplate>
                            </asp:Repeater>
                        </div>
                    </div>
                    <div id="ColorPickerModal" class="ColorPickerModal">
                        <h3>Choose a <span id="ColorPickerPart">body part</span> color</h3>
                        <asp:Repeater ID="PaletteRepeater" runat="server" EnableViewState="false">
                            <ItemTemplate><div class="ColorPickerItem" data-color="<%# Eval("Key") %>" title="BrickColor <%# Eval("Key") %>" style="background-color: #<%# Eval("Value") %>;"></div></ItemTemplate>
                        </asp:Repeater>
                    </div>
                    <asp:HiddenField ID="PartField" runat="server" />
                    <asp:HiddenField ID="ColorField" runat="server" />
                    <asp:Button ID="SaveColorButton" runat="server" OnClick="SaveColorButton_Click" style="display: none;" />
                </asp:PlaceHolder>
            </div>
        </div>
    </div>
    <script type="text/javascript">
        (function () {
            var modal = document.getElementById("ColorPickerModal");
            if (!modal) { return; }
            var selected = null;
            var regions = document.querySelectorAll(".ColorChooserRegion");
            var items = document.querySelectorAll(".ColorPickerItem");
            Array.prototype.forEach.call(regions, function (region) {
                region.onclick = function () {
                    if (selected) { selected.className = selected.className.replace(" Selected", ""); }
                    selected = region;
                    region.className += " Selected";
                    document.getElementById("ColorPickerPart").innerHTML = region.title.toLowerCase();
                    Array.prototype.forEach.call(items, function (item) {
                        item.className = "ColorPickerItem" + (item.getAttribute("data-color") === region.getAttribute("data-color") ? " Current" : "");
                    });
                    modal.className = "ColorPickerModal Open";
                };
            });
            Array.prototype.forEach.call(items, function (item) {
                item.onclick = function () {
                    if (!selected) { return; }
                    document.getElementById("<%= PartField.ClientID %>").value = selected.getAttribute("data-part");
                    document.getElementById("<%= ColorField.ClientID %>").value = item.getAttribute("data-color");
                    document.getElementById("<%= SaveColorButton.ClientID %>").click();
                };
            });
        })();
    </script>
</asp:Content>
