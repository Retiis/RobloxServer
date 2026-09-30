<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Landing.aspx.cs" Inherits="RobloxServer.Pages.Landing" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <meta charset="utf-8" />
    <title><%: RobloxServer.Config.SiteName %> - Join millions of builders</title>
    <link rel="icon" type="image/vnd.microsoft.icon" href="<%: ResolveUrl("~/favicon.ico") %>" />
    <link href="~/Content/Site.css" rel="stylesheet" type="text/css" runat="server" />
    <link href="~/Content/Pages/Landing.css" rel="stylesheet" type="text/css" runat="server" />
</head>
<body>
    <form id="LandingForm" runat="server">
        <div id="Container">
            <div id="Experimental" data-is-animated="True">
                <div class="Content">
                    <div id="animatedHeader">
                        <div id="headerLogo"><img src="<%: ResolveUrl("~/Images/Landing/logo.png") %>" alt="<%: RobloxServer.Config.SiteName %>" /></div>
                        <div id="headerTextTop">Join millions of builders</div>
                        <div id="headerTextBottom">and explore their creations</div>
                    </div>
                    <div id="animatedBodyWrapper">
                        <div id="animatedBody">
                            <div class="VideoContainer">
                                <a href="<%: ResolveUrl("~/Games") %>"><img src="<%: ResolveUrl("~/Images/Thumbs/Place1_420x230.jpg") %>" width="380" height="250" alt="Games" /></a>
                                <div class="slogan-container">
                                    <div id="slogan">What will you build?</div>
                                </div>
                            </div>
                            <div id="animated-wrapper" data-tab="<%: SelectedTab %>">
                                <div class="sign-up-row">
                                    <div class="sign-up-inner-row">
                                        <span id="animated-tab-signup" class="animated-tab">Sign up</span>
                                        <span class="animated-tab">|</span>
                                        <span id="animated-tab-login" class="animated-tab">Login</span>
                                    </div>
                                </div>

                                <div id="animated-login" style="display: none;">
                                    <asp:Panel ID="LoginPanel" runat="server" DefaultButton="LoginButton">
                                        <div class="sign-up-row">
                                            <div class="sign-up-inner-row">
                                                <span id="login-error" class="required-text error" <%= LoginError == null ? "style=\"display: none;\"" : "" %>><%: LoginError %></span>
                                            </div>
                                        </div>
                                        <div class="sign-up-row">
                                            <asp:TextBox ID="loginUsername" runat="server" MaxLength="20" CssClass="text-box text-box-large" TabIndex="1" placeholder="Username" />
                                        </div>
                                        <div class="sign-up-row">
                                            <asp:TextBox ID="loginPassword" runat="server" TextMode="Password" MaxLength="200" CssClass="text-box text-box-large" TabIndex="2" placeholder="Password" />
                                        </div>
                                        <div>
                                            <asp:Button ID="LoginButton" runat="server" Text="Login" CssClass="btn-large btn-primary" TabIndex="3" OnClick="LoginButton_Click" UseSubmitBehavior="true" />
                                        </div>
                                    </asp:Panel>
                                    <br />
                                    <div id="login-footer" class="sign-up-row">
                                        <div>Don't have an account? <a href="#" id="show-signup">Sign up</a></div>
                                    </div>
                                </div>

                                <div id="animated-signup" style="display: none;">
                                    <asp:Panel ID="SignupPanel" runat="server" DefaultButton="SignUpButton">
                                        <div class="sign-up-row">
                                            <span id="signup-error" class="error" <%= SignupError == null ? "style=\"display: none;\"" : "" %>><%: SignupError %></span>
                                        </div>
                                        <div class="sign-up-row">
                                            <div class="sign-up-inner-row">
                                                <span id="birthdayGood" class="good-text" style="display: none;">OK</span>
                                                <span id="birthdayError" class="required-text error" style="display: none;"></span>
                                                <span id="birthdayText">Birthday</span>
                                            </div>
                                            <div>
                                                <asp:DropDownList ID="lstMonths" runat="server" TabIndex="1" />
                                                <asp:DropDownList ID="lstDays" runat="server" TabIndex="2" />
                                                <asp:DropDownList ID="lstYears" runat="server" TabIndex="3" />
                                            </div>
                                            <div>
                                                <span class="sign-up-description">Enter your birthday for a personalized experience.<br />
                                                    It will not be given to any third party.</span>
                                            </div>
                                        </div>
                                        <div class="sign-up-row">
                                            <div class="sign-up-inner-row">
                                                <span id="genderGood" class="good-text" style="display: none;">OK</span>
                                                <span id="genderError" class="required-text error" style="display: none;"></span>
                                                <span id="genderText">Gender</span>
                                            </div>
                                            <div>
                                                <input type="radio" id="MaleBtn" name="gender" value="Male" tabindex="4" <%= SelectedGender == "Male" ? "checked=\"checked\"" : "" %> /><label for="MaleBtn">Male</label>
                                                <input type="radio" id="FemaleBtn" name="gender" value="Female" tabindex="5" <%= SelectedGender == "Female" ? "checked=\"checked\"" : "" %> /><label for="FemaleBtn">Female</label>
                                            </div>
                                        </div>
                                        <div class="sign-up-row">
                                            <div class="sign-up-inner-row">
                                                <span id="robloxConsentText">This is my real Roblox username</span>
                                            </div>
                                            <p class="sign-up-help-text" style="font-size: 11px; color: #666; margin: 4px 0;">
                                                If you say Yes, we'll look up this username on Roblox's public website
                                                (roblox.com) just to confirm it exists and copy your public avatar
                                                picture to show on your profile here. We don't get your password,
                                                e-mail, or anything private — Roblox never shares that with us, and
                                                we only do this once, right now, for the account you're creating.
                                                If you say No (or this isn't really your Roblox username), we skip
                                                this step entirely and nothing is looked up.
                                            </p>
                                            <div>
                                                <input type="radio" id="RobloxConsentYes" name="robloxConsent" value="yes" tabindex="6" <%= RobloxConsent == "yes" ? "checked=\"checked\"" : "" %> /><label for="RobloxConsentYes">Yes, link my public avatar</label>
                                                <input type="radio" id="RobloxConsentNo" name="robloxConsent" value="no" tabindex="7" <%= RobloxConsent != "yes" ? "checked=\"checked\"" : "" %> /><label for="RobloxConsentNo">No</label>
                                            </div>
                                        </div>
                                        <div class="sign-up-row">
                                            <div class="sign-up-inner-row">
                                                <span id="usernameGood" class="good-text" style="display: none;">OK</span>
                                                <span id="usernameError" class="required-text error" style="display: none;"></span>
                                                <span id="usernameText">Username</span>
                                            </div>
                                            <div><asp:TextBox ID="username" runat="server" MaxLength="20" CssClass="text-box text-box-large" TabIndex="6" /></div>
                                            <div><span class="sign-up-description">3-20 alphanumeric characters, no spaces</span></div>
                                        </div>
                                        <div class="sign-up-row">
                                            <div class="sign-up-inner-row">
                                                <span id="passwordGood" class="good-text" style="display: none;">OK</span>
                                                <span id="passwordError" class="required-text error" style="display: none;"></span>
                                                <span id="passwordText">Password</span>
                                            </div>
                                            <div><asp:TextBox ID="password" runat="server" TextMode="Password" MaxLength="200" CssClass="text-box text-box-large" TabIndex="7" /></div>
                                            <div><span class="sign-up-description">At least 6 characters, not your username</span></div>
                                        </div>
                                        <div class="sign-up-row">
                                            <div class="sign-up-inner-row">
                                                <span id="passwordConfirmGood" class="good-text" style="display: none;">OK</span>
                                                <span id="passwordConfirmError" class="required-text error" style="display: none;"></span>
                                                <span id="passwordConfirmText">Confirm Password</span>
                                            </div>
                                            <div><asp:TextBox ID="passwordConfirm" runat="server" TextMode="Password" MaxLength="200" CssClass="text-box text-box-large" TabIndex="8" /></div>
                                        </div>
                                        <div>
                                            <asp:Button ID="SignUpButton" runat="server" Text="Sign Up" CssClass="btn-large btn-primary roblox-signup" TabIndex="9" OnClick="SignUpButton_Click" />
                                        </div>
                                    </asp:Panel>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <div class="Footer Experimental">
                <div class="FooterContent">
                    <p class="FooterParagraph">
                        <a href="<%: ResolveUrl("~/Games") %>">Games</a> &nbsp;|&nbsp; <a href="<%: ResolveUrl("~/Browse.aspx") %>">People</a> &nbsp;|&nbsp;
                        <a href="<%: ResolveUrl("~/Install/Download.ashx?client=Launcher") %>">Download</a> &nbsp;|&nbsp;
                        <a href="<%: RobloxServer.Config.ClientsArchiveUrl %>" target="_blank" rel="noopener">Clients 2007-2013</a> &nbsp;|&nbsp; <a href="https://github.com/V0rtexLinux/RobloxServer">Source Code</a>
                    </p>
                    <div class="FooterLegaleseContainer">
                        <p class="Legalese">
                            <%: RobloxServer.Config.SiteName %> is a fan-made private server. ROBLOX, "Online Building Toy", characters, logos, names, and all related indicia are trademarks of ROBLOX Corporation.
                            This site is not sponsored, authorized or endorsed by ROBLOX Corporation.
                        </p>
                    </div>
                </div>
            </div>
        </div>
    </form>
    <script type="text/javascript" src="<%: ResolveUrl("~/Content/Landing.js") %>"></script>
</body>
</html>
