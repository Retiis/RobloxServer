// Play / Host Server buttons of PlaceItem.aspx. Like the 2013 site: get a one-time ticket for the
// logged in user, then hand it to RobloxPlayerLauncher through the robloxserver-player: protocol.
(function () {
    var root = document.getElementById("MainContent_PlaceLauncher") || document.querySelector("[data-place-id]");
    var panel = document.getElementById("PlaceLauncherStatusPanel");
    if (!root || !panel) {
        return;
    }

    var statusText = document.getElementById("PlaceLauncherStatus");
    var install = document.getElementById("PlaceLauncherInstall");
    var words = ["Bloxxing", "Bootstrapping", "Calibrating", "De-noobing", "Energizing", "Generating",
                 "Loading", "Optimizing", "Reticulating", "Synchronizing", "Uploading", "Aggregating"];
    var timers = [];

    function setStatus(text) {
        statusText.textContent = text;
    }

    function clearTimers() {
        while (timers.length) {
            clearTimeout(timers.pop());
        }
    }

    function hide() {
        clearTimers();
        panel.style.display = "none";
    }

    function openUri(uri) {
        // A hidden iframe keeps the page when the protocol is not registered.
        var frame = document.createElement("iframe");
        frame.style.display = "none";
        frame.src = uri;
        document.body.appendChild(frame);
        timers.push(setTimeout(function () { frame.parentNode && frame.parentNode.removeChild(frame); }, 30000));
    }

    function launch(mode) {
        if (root.getAttribute("data-logged-in") !== "true") {
            window.location.href = root.getAttribute("data-login-url");
            return;
        }

        var placeId = root.getAttribute("data-place-id");
        var baseUrl = root.getAttribute("data-base-url");
        clearTimers();
        install.style.display = "none";
        panel.style.display = "block";
        setStatus(mode === "host" ? "Starting the game server..." : "Starting ROBLOX...");

        var request = new XMLHttpRequest();
        request.open("GET", root.getAttribute("data-ticket-url") + "?placeId=" + encodeURIComponent(placeId), true);
        request.onload = function () {
            if (request.status !== 200) {
                setStatus(request.status === 401 ? "Please log in again." : "Could not start the game (" + request.status + ").");
                return;
            }

            var uri = "robloxserver-player:1"
                + "+launchmode:" + mode
                + "+gameinfo:" + encodeURIComponent(request.responseText)
                + "+placeid:" + encodeURIComponent(placeId)
                + "+baseurl:" + encodeURIComponent(baseUrl)
                + "+launchtime:" + new Date().getTime();
            if (mode === "play") {
                uri += "+placelauncherurl:" + encodeURIComponent(baseUrl + "Game/PlaceLauncher.ashx?request=RequestGame&placeId=" + placeId);
            }
            openUri(uri);

            var i = 0;
            (function cycle() {
                setStatus(words[i % words.length] + "...");
                i++;
                timers.push(setTimeout(cycle, 1500));
            })();
            timers.push(setTimeout(function () { install.style.display = "block"; }, 8000));
        };
        request.onerror = function () {
            setStatus("Could not reach the website.");
        };
        request.send();
    }

    var buttons = root.querySelectorAll("[data-launch-mode]");
    for (var b = 0; b < buttons.length; b++) {
        buttons[b].addEventListener("click", function (e) {
            e.preventDefault();
            launch(this.getAttribute("data-launch-mode"));
        });
    }
    document.getElementById("CancelPlaceLauncher").addEventListener("click", hide);
})();
