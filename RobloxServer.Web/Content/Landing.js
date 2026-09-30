// 2013 landing page (Landing/BlueCity/Animated.js and the Animated*FormValidator scripts),
// rewritten without jQuery. The two city layers scroll at different speeds; the tabs switch
// between Sign up and Login, and the forms are checked before they are posted.
(function () {
    var messages = {
        requiredField: "Required",
        doesntMatch: "Doesn't match",
        tooShort: "Too short",
        tooLong: "Too long",
        noSpaces: "No spaces allowed",
        invalidBirthday: "Invalid birthday",
        loginFieldsRequired: "Username and Password are required."
    };

    function $(id) { return document.getElementById(id); }

    function scroll(element, speed) {
        var x = 0;
        setInterval(function () {
            x -= speed;
            element.style.backgroundPosition = x + "px 0";
        }, 1000 / 60);
    }

    function select(tab) {
        var signup = tab === "signup";
        $("animated-signup").style.display = signup ? "" : "none";
        $("animated-login").style.display = signup ? "none" : "";
        $("animated-tab-signup").className = "animated-tab" + (signup ? " animated-tab-selected" : "");
        $("animated-tab-login").className = "animated-tab" + (signup ? "" : " animated-tab-selected");
    }

    function mark(name, error) {
        var good = $(name + "Good"), bad = $(name + "Error");
        if (bad) { bad.innerHTML = error || ""; bad.style.display = error ? "" : "none"; }
        if (good) { good.style.display = error ? "none" : ""; }
        return !error;
    }

    function checkBirthday() {
        var ok = $("lstMonths").value && $("lstDays").value && $("lstYears").value;
        return mark("birthday", ok ? null : messages.invalidBirthday);
    }

    function checkGender() {
        return mark("gender", $("MaleBtn").checked || $("FemaleBtn").checked ? null : messages.requiredField);
    }

    function checkUsername() {
        var name = $("username").value;
        return mark("username", name.length === 0 ? messages.requiredField : /\s/.test(name) ? messages.noSpaces
            : name.length < 3 ? messages.tooShort : name.length > 20 ? messages.tooLong : null);
    }

    function checkPassword() {
        var password = $("password").value;
        return mark("password", password.length === 0 ? messages.requiredField : password.length < 6 ? messages.tooShort : null);
    }

    function checkPasswordConfirm() {
        var confirm = $("passwordConfirm").value;
        return mark("passwordConfirm", confirm.length === 0 ? messages.requiredField : confirm !== $("password").value ? messages.doesntMatch : null);
    }

    function validateSignup() {
        var results = [checkBirthday(), checkGender(), checkUsername(), checkPassword(), checkPasswordConfirm()];
        return results.indexOf(false) < 0;
    }

    function validateLogin() {
        var ok = $("loginUsername").value.length > 0 && $("loginPassword").value.length > 0;
        var error = $("login-error");
        if (!ok) { error.innerHTML = messages.loginFieldsRequired; error.style.display = ""; }
        return ok;
    }

    window.addEventListener("load", function () {
        var experimental = $("Experimental");
        if (experimental.getAttribute("data-is-animated") === "True") {
            scroll($("Container"), 0.71);
            scroll(document.body, 0.35);
        }

        $("animated-tab-signup").onclick = function () { select("signup"); };
        $("animated-tab-login").onclick = function () { select("login"); };
        $("show-signup").onclick = function () { select("signup"); return false; };
        select($("animated-wrapper").getAttribute("data-tab"));

        $("lstMonths").onchange = $("lstDays").onchange = $("lstYears").onchange = checkBirthday;
        $("MaleBtn").onclick = $("FemaleBtn").onclick = checkGender;
        $("username").onblur = checkUsername;
        $("password").onkeyup = checkPassword;
        $("passwordConfirm").onkeyup = checkPasswordConfirm;
        $("SignUpButton").onclick = validateSignup;
        $("login-button").onclick = validateLogin;
    });
})();
