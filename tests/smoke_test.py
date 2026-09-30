#!/usr/bin/env python3
"""
End-to-end smoke test for RobloxServer (ASP.NET) running behind nginx + fastcgi-mono-server4
(or IIS). Only uses the Python standard library.

    python3 tests/smoke_test.py http://127.0.0.1:8081 [path/to/deployed/site]

When the deployed site path is given, the test finishes by lowering RateLimitRequests in its
Web.config (which restarts the app) and checks that the limiter answers 429.
"""
import base64
import gzip
import hashlib
import html
import http.cookiejar
import json
import re
import sys
import time
import urllib.error
import urllib.parse
import urllib.request
import uuid
import xml.etree.ElementTree as ET

BASE = sys.argv[1].rstrip("/") if len(sys.argv) > 1 else "http://127.0.0.1:8081"
SITE_DIR = sys.argv[2] if len(sys.argv) > 2 else None

PLACE = (b'<roblox xmlns:xmime="http://www.w3.org/2005/05/xmlmime" version="4">'
         b'<Item class="Workspace"><Properties><string name="Name">Workspace</string></Properties></Item></roblox>')

failures = []


def check(condition, message):
    print(("  ok   " if condition else "  FAIL ") + message)
    if not condition:
        failures.append(message)


class Client:
    def __init__(self):
        self.jar = http.cookiejar.CookieJar()
        self.opener = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(self.jar),
                                                  NoRedirect())

    def request(self, path, data=None, headers=None, method=None):
        url = path if path.startswith("http") else BASE + path
        req = urllib.request.Request(url, data=data, headers=headers or {}, method=method)
        try:
            with self.opener.open(req, timeout=30) as response:
                return response.status, dict(response.headers), response.read()
        except urllib.error.HTTPError as error:
            return error.code, dict(error.headers), error.read()

    def get(self, path):
        return self.request(path)

    def post_form(self, path, fields, headers=None):
        body = urllib.parse.urlencode(fields).encode()
        h = {"Content-Type": "application/x-www-form-urlencoded"}
        h.update(headers or {})
        return self.request(path, body, h, "POST")

    def postback(self, path, fields, button, files=None):
        """Submits an ASP.NET Web Forms page: copies hidden fields (__VIEWSTATE...) then posts."""
        status, _, body = self.get(path)
        page = body.decode("utf-8", "replace")
        form = {}
        for match in re.finditer(r'<input[^>]+type="hidden"[^>]*>', page):
            tag = match.group(0)
            name = re.search(r'name="([^"]+)"', tag)
            value = re.search(r'value="([^"]*)"', tag)
            if name:
                form[html.unescape(name.group(1))] = html.unescape(value.group(1)) if value else ""
        controls = {}
        for match in re.finditer(r'name="([^"]*\$([A-Za-z0-9_]+))"', page):
            controls[match.group(2)] = match.group(1)
        for key, value in fields.items():
            form[controls.get(key, key)] = value
        form[controls.get(button, button)] = button
        if files:
            boundary = uuid.uuid4().hex
            parts = []
            for key, value in form.items():
                parts.append(("--%s\r\nContent-Disposition: form-data; name=\"%s\"\r\n\r\n%s\r\n"
                              % (boundary, key, value)).encode())
            for key, (filename, content) in files.items():
                parts.append(("--%s\r\nContent-Disposition: form-data; name=\"%s\"; filename=\"%s\"\r\n"
                              "Content-Type: application/octet-stream\r\n\r\n"
                              % (boundary, controls.get(key, key), filename)).encode() + content + b"\r\n")
            parts.append(("--%s--\r\n" % boundary).encode())
            return self.request(path, b"".join(parts),
                                {"Content-Type": "multipart/form-data; boundary=" + boundary}, "POST")
        return self.post_form(path, form)


class NoRedirect(urllib.request.HTTPRedirectHandler):
    def redirect_request(self, req, fp, code, msg, headers, newurl):
        return None


def verify_rbxsig(text, key_xml):
    """Verifies --rbxsig%SIG%body (RSA PKCS#1 v1.5 + SHA-1) with plain big-int math."""
    match = re.match(r"--rbxsig%([^%]+)%(.*)", text, re.S)
    if not match:
        return False
    root = ET.fromstring(key_xml)
    n = int.from_bytes(base64.b64decode(root.find("Modulus").text), "big")
    e = int.from_bytes(base64.b64decode(root.find("Exponent").text), "big")
    signature = int.from_bytes(base64.b64decode(match.group(1)), "big")
    size = (n.bit_length() + 7) // 8
    decoded = pow(signature, e, n).to_bytes(size, "big")
    digest_info = bytes.fromhex("3021300906052b0e03021a05000414") + hashlib.sha1(match.group(2).encode("utf-8")).digest()
    expected = b"\x00\x01" + b"\xff" * (size - len(digest_info) - 3) + b"\x00" + digest_info
    return decoded == expected


def main():
    anon = Client()
    admin = Client()
    player = Client()

    print("Status and routing")
    status, _, body = anon.get("/Api/Status.ashx")
    check(status == 200 and json.loads(body)["status"] == "online", "Api/Status.ashx is online")
    for path in ["/api/status.ashx", "/API/STATUS.ASHX", "/status", "/Status/"]:
        status, _, _ = anon.get(path)
        check(status == 200, "case-insensitive route " + path)
    status, _, body = anon.get("/")
    check(status == 200 and b"Join millions of builders" in body and b"animated-signup" in body, "/ is the 2013 landing page for visitors")
    status, _, body = anon.get("/Games")
    check(status == 200 and b"Games" in body and b"GamesContainer" in body, "games page renders at /Games")
    status, _, body = anon.get("/Login.aspx")
    check(status == 200 and b"Login to" in body and b"signUpPanel" in body, "2013 login page renders")
    status, _, _ = anon.get("/this/does/not/exist.aspx")
    check(status == 404, "unknown page is 404")

    print("Legacy client endpoints")
    status, _, body = anon.get("/Setting/QuietGet/ClientAppSettings/")
    check(status == 200 and body.strip() == b"{}", "Setting/QuietGet/ClientAppSettings")
    status, _, _ = anon.get("/v1.1/Counters/Increment/?apiKey=x&counterName=y&amount=1")
    check(status == 204, "v1.1/Counters/Increment is 204")
    status, _, body = anon.get("/GetAllowedMD5Hashes/?apiKey=x")
    check(status == 200 and json.loads(body) == {"data": []}, "GetAllowedMD5Hashes")
    status, _, body = anon.get("/GetAllowedSecurityVersions/")
    check(status == 200 and "0.235.0pcplayer" in json.loads(body)["data"], "GetAllowedSecurityVersions")
    status, _, body = anon.get("/Game/Wipeouts.ashx?UserID=1")
    check(status == 200 and b"online" in body, "Game/Wipeouts.ashx")
    status, _, body = anon.get("/Error/Dmp.ashx")
    check(body == b"Thx", "Error/Dmp.ashx says Thx")
    status, _, body = anon.get("/game/GetCurrentUser.ashx")
    check(body == b"null", "GetCurrentUser is null when logged out")
    status, _, body = anon.get("/Asset/CharacterFetch.ashx?userId=1")
    check(b"BodyColors.ashx?userId=1" in body, "CharacterFetch")
    status, _, body = anon.get("/Asset/BodyColors.ashx?userId=1")
    check(b'class="BodyColors"' in body, "BodyColors")
    status, _, key_xml = anon.get("/Keys/PublicKey.ashx?format=xml")
    check(status == 200 and b"<Modulus>" in key_xml, "public key (xml)")
    status, _, blob = anon.get("/Keys/PublicKey.ashx")
    check(len(base64.b64decode(blob)) == 148, "PUBLICKEYBLOB is 148 bytes (1024-bit)")
    key_xml = key_xml.decode()

    print("Accounts")
    status, headers, _ = admin.postback("/Register.aspx", {"UserNameBox": "Builderman", "PasswordBox": "hunter22",
                                                            "ConfirmBox": "hunter22"}, "RegisterButton")
    check(status == 302, "register Builderman (first user)")
    status, _, body = admin.get("/Api/Me.ashx")
    me = json.loads(body)
    check(me.get("authenticated") and me.get("isAdmin"), "first user is logged in and admin")
    check(any(c.name == ".ROBLOSECURITY" for c in admin.jar), ".ROBLOSECURITY cookie is set")
    status, _, body = player.postback("/Register.aspx", {"UserNameBox": "bad name!", "PasswordBox": "hunter22",
                                                        "ConfirmBox": "hunter22"}, "RegisterButton")
    check(status == 200 and b"Only letters" in body, "invalid username rejected")
    status, _, _ = player.postback("/Register.aspx", {"UserNameBox": "Noob_1", "PasswordBox": "password1",
                                                      "ConfirmBox": "password1", "Under13Box": "on"}, "RegisterButton")
    check(status == 302, "register Noob_1 (under 13)")
    status, _, body = Client().postback("/Landing.aspx", {"username": "Noli", "password": "void1234", "passwordConfirm": "void1234",
                                                          "lstMonths": "2", "lstDays": "30", "lstYears": "2010",
                                                          "gender": "FemaleBtn"}, "SignUpButton")
    check(status == 200 and b"valid birthday" in body, "landing signup rejects 30 February")
    noli = Client()
    status, headers, _ = noli.postback("/Landing.aspx", {"username": "Noli", "password": "void1234", "passwordConfirm": "void1234",
                                                         "lstMonths": "11", "lstDays": "1", "lstYears": "2009",
                                                         "gender": "FemaleBtn"}, "SignUpButton")
    check(status == 302 and headers.get("Location", "").endswith("/My/Home.aspx"), "register Noli from the landing page")
    status, _, body = noli.get("/Api/Me.ashx")
    check(json.loads(body).get("userName") == "Noli", "the landing page signs the new account in")
    status, _, body = anon.get("/Asset/BodyColors.ashx?userId=3")
    check(body.count(b">1003</int>") == 6, "Noli is completely black (Really black)")
    status, _, body = anon.get("/Asset/CharacterFetch.ashx?userId=3")
    check(b"BodyColors.ashx?userId=3" in body and b"versionid" not in body, "Noli has no default clothing")
    clue = base64.b64encode(b"YOU HAVE ANGERED NOLI! FIFTY SLEEP IN THE VOID. /VOID").decode()
    status, headers, _ = anon.get("/User.aspx?id=3")
    check(status == 302 and headers.get("Location", "") in ("/", BASE + "/")
          and headers.get("X-Void") == clue, "Noli's profile sends you home and leaves the clue")
    status, _, body = anon.get("/User.aspx?username=noli")
    check(status == 302, "Noli's profile by name sends you home too")
    status, _, body = anon.get("/User.aspx?id=1")
    check(status == 200 and b"Builderman" in body and b"Avatar.ashx?userId=1" in body, "Builderman's profile")
    status, _, _ = anon.get("/User.aspx?id=999")
    check(status == 404, "unknown profile is 404")
    status, headers, body = anon.get("/Asset/Avatar.ashx?userId=1")
    check(status == 200 and b"<svg" in body and b"?</text>" not in body, "avatar thumbnail (svg)")
    status, _, body = anon.get("/Asset/Avatar.ashx?userId=3")
    check(b"?</text>" in body and clue.encode() in body, "Noli's avatar is a question mark with the clue")
    status, _, body = anon.get("/Browse.aspx?name=noli")
    row = re.search(rb"<tr>\s*<!-- ([^ ]+) -->(.*?)</tr>", body, re.S)
    check(status == 200 and row and row.group(1).decode() == clue and b"Avatar.ashx?userId=3" in row.group(2)
          and b"line.png" not in row.group(2) and b"ago" not in row.group(2), "People search: Noli has no avatar or last online")
    status, _, body = anon.get("/People")
    check(status == 200 and b"Builderman" in body and b"Noob_1" in body, "People lists everyone")
    status, _, body = anon.get("/Void")
    check(status == 200 and b"JURA QVQ LNYRHAVIREFVGL SVEFG FRR ZR?" in body, "the Void asks its question (ROT13)")
    status, _, body = Client().postback("/Void", {"AnswerBox": "01/01/2011"}, "SpeakButton")
    check(status == 200 and b"YOU HAVE ANGERED NOLI!" in body, "a wrong answer angers Noli")
    status, _, body = Client().postback("/Void", {"AnswerBox": "26/02/2010"}, "SpeakButton")
    whisper = " ".join(format(c, "08b") for c in b"SAY MY NAME WHERE THE SERVERS RUN")
    check(status == 200 and whisper.encode() in body and b"ANGERED" not in body, "the right answer reveals the whisper (binary)")
    status, _, body = player.post_form("/Api/Logout.ashx", {})
    check(status == 403, "Api/Logout without X-CSRF-TOKEN is 403")
    fresh = Client()
    status, _, body = fresh.post_form("/Api/Login.ashx", {"username": "noob_1", "password": "password1"})
    check(json.loads(body).get("success"), "Api/Login.ashx works (case-insensitive name)")

    print("Publishing")
    status, _, body = anon.request("/Data/Upload.ashx?assetid=0&type=Place&name=Crossroads", PLACE,
                                   {"Content-Type": "application/octet-stream"}, "POST")
    check(status == 401, "Data/Upload.ashx needs .ROBLOSECURITY")
    status, _, body = admin.request("/Data/Upload.ashx?assetid=0&type=Place&name=Crossroads&description=Classic&ispublic=true",
                                    gzip.compress(PLACE), {"Content-Type": "application/octet-stream",
                                                           "Content-Encoding": "gzip"}, "POST")
    check(status == 200 and body.isdigit(), "Studio publish (gzip) returns asset id")
    place_id = int(body)
    check(place_id >= 1900000000, "local asset ids start at 1900000000")
    status, _, _ = admin.postback("/Develop.aspx", {"NameBox": "Private Place", "DescriptionBox": "secret",
                                                    "ClientList": "2013M", "MaxPlayersBox": "8", "PublicBox": ""},
                                  "PublishButton", files={"PlaceUpload": ("private.rbxl", PLACE)})
    check(status == 302, "publish from Develop.aspx (private)")
    status, _, body = player.get("/Api/Games.ashx")
    games = json.loads(body)["data"]
    check(any(g["id"] == place_id and g["name"] == "Crossroads" for g in games), "Api/Games lists the public game")
    check(not any(g["name"] == "Private Place" for g in games), "private game hidden from others")
    status, _, body = admin.get("/Api/Games.ashx")
    private = [g for g in json.loads(body)["data"] if g["name"] == "Private Place"]
    check(len(private) == 1 and private[0]["client"] == "2013M", "owner sees private game with its client")
    check(any(g["id"] == place_id and g["client"] == "2012M" for g in games), "Studio publish uses the default client (2012M)")
    status, _, body = admin.request("/Data/Upload.ashx?assetid=0&type=Place&name=Old&client=2009E&ispublic=false", PLACE,
                                    {"Content-Type": "application/octet-stream"}, "POST")
    status, _, body = admin.get("/Api/Games.ashx?id=%d" % int(body))
    check(json.loads(body)["data"][0]["client"] == "2012M", "unsupported clients (2009E) become 2012M")
    status, _, body = player.get("/asset/?id=%d" % private[0]["id"])
    check(status == 403, "private place file is protected")
    status, _, body = player.get("/Asset/?ID=%d" % place_id)
    check(status == 200 and body == PLACE, "place file downloads through /Asset/?ID=")
    status, _, body = anon.get("/PlaceItem.aspx?id=%d" % place_id)
    check(status == 200 and b"Crossroads" in body, "PlaceItem.aspx renders")
    check(b'data-launch-mode="play"' in body and b"PlaceLauncher.js" in body, "PlaceItem.aspx has the Play button")

    print("Signed scripts")
    status, _, body = player.get("/Game/Visit.ashx?IsPlaySolo=1&UserID=2&PlaceID=%d" % place_id)
    text = body.decode()
    check(text.startswith("--rbxsig%"), "Visit.ashx is signed")
    check(verify_rbxsig(text, key_xml), "Visit.ashx signature verifies with the public key")
    check("[====[Noob_1]====]" in text and "game:SetPlaceID(%d)" % place_id in text, "Visit.ashx personalised")
    check("player:SetSuperSafeChat(true)" in text, "under 13 account gets SuperSafeChat")
    status, _, body = anon.get("/game/studio.ashx")
    check(json.loads(body)["BaseUrl"] == BASE + "/", "Game/Studio.ashx uses the address the request came in on")
    check("roblox.com" not in text.replace("roblox.xsd", ""), "Visit.ashx does not point at roblox.com")

    print("Game servers, tickets and join")
    status, headers, body = admin.post_form("/Game/Servers.ashx", {"action": "register", "placeId": place_id,
                                                                   "port": 53640, "client": "2012M",
                                                                   "address": "192.168.1.50", "maxPlayers": 2})
    token = headers.get("X-CSRF-TOKEN") or headers.get("x-csrf-token")
    check(status == 403 and token, "register without X-CSRF-TOKEN is 403 and returns a token")
    status, _, body = admin.post_form("/Game/Servers.ashx", {"action": "register", "placeId": place_id,
                                                             "port": 53640, "client": "2012M",
                                                             "address": "192.168.1.50", "maxPlayers": 2},
                                      {"X-CSRF-TOKEN": token})
    job = json.loads(body)
    check(job.get("success") and job.get("serverKey"), "game server registered")
    status, _, body = player.get("/Game/PlaceLauncher.ashx?request=RequestGame&placeId=%d" % place_id)
    launch = json.loads(body)
    check(launch["status"] == 2 and launch["jobId"] == job["jobId"], "PlaceLauncher finds the job")
    check(launch["joinScriptUrl"].startswith(BASE + "/Game/Join.ashx"), "joinScriptUrl uses the request address")
    check(launch["client"] == "2012M" and launch["placeId"] == place_id, "PlaceLauncher names the client to install")
    status, _, body = player.get(launch["joinScriptUrl"])
    text = body.decode()
    check(verify_rbxsig(text, key_xml), "Join.ashx signature verifies")
    check("local ServerAddress = [====[192.168.1.50]====]" in text and "local ServerPort = 53640" in text,
          "join script has LAN address")
    check("local UserName = [====[Noob_1]====]" in text and "local SuperSafeChat = true" in text
          and "NetworkClient:PlayerConnect" in text, "join script has the user")
    client_ticket = re.search(r"local AuthTicket = \[====\[([0-9A-F]+)\]====\]", text).group(1)
    status, _, body = anon.get("/Game/GameServer.ashx?jobId=%s&serverKey=wrong" % job["jobId"])
    check(status == 404, "GameServer.ashx needs the serverKey")
    status, _, body = anon.get("/Game/GameServer.ashx?jobId=%s&serverKey=%s&loadPlace=false" % (job["jobId"], job["serverKey"]))
    text = body.decode()
    check(verify_rbxsig(text, key_xml) and "local Port = 53640" in text and "local LoadPlace = false" in text
          and "NetworkServer:Start(Port)" in text, "GameServer.ashx script for the job")
    validate = "/Game/ValidateTicket.ashx?ticket=%s&jobId=%s&serverKey=%s"
    status, _, body = anon.get(validate % (client_ticket, job["jobId"], "wrong"))
    check(body.startswith(b"ERROR"), "ValidateTicket rejects a bad serverKey")
    status, _, body = anon.get(validate % (client_ticket, job["jobId"], job["serverKey"]))
    check(body.startswith(b"OK|2|Noob_1|true"), "ValidateTicket accepts the ticket once")
    status, _, body = anon.get(validate % (client_ticket, job["jobId"], job["serverKey"]))
    check(body.startswith(b"ERROR"), "ticket cannot be replayed")
    status, _, body = anon.get("/Game/Servers.ashx?action=heartbeat&players=2&jobId=%s&serverKey=%s"
                               % (job["jobId"], job["serverKey"]))
    check(body == b"OK", "heartbeat")
    status, _, body = player.get("/Game/PlaceLauncher.ashx?request=RequestGame&placeId=%d" % place_id)
    check(json.loads(body)["status"] == 6, "PlaceLauncher reports a full game")
    status, _, body = anon.get("/Api/Servers.ashx?placeId=%d" % place_id)
    check(json.loads(body)["data"][0]["players"] == 2, "Api/Servers shows the player count")
    for sent, expected in [("::1", "127.0.0.1"), ("::ffff:192.168.1.60", "192.168.1.60")]:
        status, _, body = admin.post_form("/Game/Servers.ashx", {"action": "register", "placeId": place_id, "port": 53641,
                                                                "address": sent}, {"X-CSRF-TOKEN": token})
        extra = json.loads(body)
        servers = json.loads(anon.get("/Api/Servers.ashx?placeId=%d" % place_id)[2])["data"]
        check(any(s["jobId"] == extra["jobId"] and s["address"] == expected for s in servers),
              "IPv6 host address %s is stored as IPv4 %s" % (sent, expected))
        anon.get("/Game/Servers.ashx?action=unregister&jobId=%s&serverKey=%s" % (extra["jobId"], extra["serverKey"]))

    print("2013 pages")
    status, headers, _ = player.get("/")
    check(status == 302 and headers.get("Location", "").endswith("/My/Home.aspx"), "/ sends members to My ROBLOX")
    status, _, body = player.get("/My/Home.aspx")
    check(status == 200 and b"Hello, Noob_1!" in body, "home page greets the user")
    check(b"recent-place-container" in body and b"PlaceItem.aspx?id=%d" % place_id in body, "Recently Played Games lists the joined place")
    status, _, body = player.postback("/My/Home.aspx", {"StatusBox": "Building a castle"}, "ShareButton")
    check(status == 200 and b"Building a castle" in body and b"feed-container" in body, "status update shows in My Feed")
    status, _, body = anon.get("/User.aspx?id=2")
    check(b"Building a castle" in body, "the status shows on the profile")
    status, _, body = player.postback("/My/Character.aspx", {"PartField": "HeadColor", "ColorField": "21"}, "SaveColorButton")
    page_title = re.search(rb"<title>\s*(.*?)\s*</title>", body, re.S)
    check(status == 200, "character page saves a body color (HTTP %d, %s)"
          % (status, page_title.group(1).decode("utf-8", "replace") if page_title else body[:200]))
    status, _, body = anon.get("/Asset/BodyColors.ashx?userId=2")
    check(b'<int name="HeadColor">21</int>' in body, "the new head color reaches BodyColors.ashx")
    status, _, body = player.postback("/My/Character.aspx", {"PartField": "HeadColor", "ColorField": "99999"}, "SaveColorButton")
    status, _, body = anon.get("/Asset/BodyColors.ashx?userId=2")
    check(b'<int name="HeadColor">21</int>' in body, "a color outside the palette is ignored")
    status, _, body = noli.postback("/My/Character.aspx", {"PartField": "HeadColor", "ColorField": "21"}, "SaveColorButton")
    status, _, body = anon.get("/Asset/BodyColors.ashx?userId=3")
    check(body.count(b">1003</int>") == 6, "Noli stays completely black")
    status, _, body = admin.get("/Develop.aspx")
    check(status == 200 and b"item-table" in body and b"Crossroads" in body and b"Create New Place" in body, "2013 build page lists the places")

    print("RobloxPlayerLauncher")
    status, _, body = player.get("/Game/GetAuthTicket.ashx?placeId=%d" % place_id)
    check(status == 200 and re.match(rb"^[0-9A-F]{96}$", body), "Play button gets a one-time ticket")
    launcher = Client()
    status, _, _ = launcher.get("/Login/Negotiate.ashx?suggest=" + body.decode())
    status2, _, me = launcher.get("/Api/Me.ashx")
    check(status == 200 and json.loads(me).get("userName") == "Noob_1", "launcher trades the ticket for a cookie")
    status, _, _ = Client().get("/Login/Negotiate.ashx?suggest=" + body.decode())
    check(status == 403, "the ticket works once")
    status, _, body = anon.get("/install/version.ashx?client=2009E")
    check(status == 404 and not json.loads(body)["available"], "install: unsupported client")
    status, _, body = anon.get("/install/version.ashx?client=2013m")
    check(status == 404 and json.loads(body)["client"] == "2013M", "install: 2013M not uploaded yet")
    import io, os, zipfile
    bundled = SITE_DIR and os.path.exists(SITE_DIR.rstrip("/") + "/App_Data/Launcher/RobloxPlayerLauncher.exe")
    status, headers, body = anon.get("/Install/Download.ashx?client=Launcher")
    if bundled:
        check(status == 200 and body[:2] == b"MZ", "launcher download serves the bundled RobloxPlayerLauncher.exe")
        status, _, body = anon.get("/install/version.ashx?client=Launcher")
        check(json.loads(body)["available"], "install: launcher version (self update)")
    else:
        check(status == 302 and "github.com" in headers.get("Location", ""), "launcher download falls back to GitHub")

    def make_zip(files):
        data = io.BytesIO()
        with zipfile.ZipFile(data, "w") as z:
            for name, content in files.items():
                z.writestr(name, content)
        return data.getvalue()

    def upload(package, filename, content):
        _, _, page = admin.postback("/Admin.aspx", {"PackageList": package}, "PackageUploadButton",
                                    files={"PackageUpload": (filename, content)})
        return html.unescape(page.decode("utf-8", "replace"))

    page = upload("2013M", "2013M.zip", b"not a zip")
    check("That is not a .zip file." in page, "Admin upload rejects a file that is not a zip")
    page = upload("2013M", "2013M.zip", make_zip({"2013M/RobloxApp_client.exe": b"MZ", "2013M/content/x.txt": b"x"}))
    check('inside the folder "2013M"' in page, "Admin upload explains a zipped folder")
    page = upload("Launcher", "RobloxPlayerLauncher.exe", b"#!/bin/sh")
    check("That is not a Windows program." in page, "Admin upload rejects a launcher that is not an exe")
    client_zip = make_zip({"RobloxApp_client.exe": b"MZ fake", "content/scripts/cores/StarterScript.lua": b"--"})
    page = upload("2013M", "2013M.zip", client_zip)
    status, _, body = anon.get("/install/version.ashx?client=2013M")
    check("2013M uploaded: version-" in page and status == 200
          and json.loads(body)["sha256"] == hashlib.sha256(client_zip).hexdigest(), "Admin uploads the 2013M client")
    status, _, body = player.get("/Admin.aspx")
    check(b"PackageUpload" not in body, "only admins see the package upload")
    if SITE_DIR:
        package = b"PK fake 2012M client package"
        os.makedirs(SITE_DIR.rstrip("/") + "/App_Data/Clients", exist_ok=True)
        with open(SITE_DIR.rstrip("/") + "/App_Data/Clients/2012M.zip", "wb") as f:
            f.write(package)
        status, _, body = anon.get("/install/version.ashx?client=2012M")
        info = json.loads(body)
        check(status == 200 and info["available"] and info["sha256"] == hashlib.sha256(package).hexdigest()
              and info["version"] == "version-" + info["sha256"][:16], "install: 2012M version")
        status, _, body = anon.get("/install/download.ashx?client=2012M")
        check(status == 200 and body == package, "install: 2012M download")
        os.remove(SITE_DIR.rstrip("/") + "/App_Data/Clients/2012M.zip")

    print("Novetus master server")
    status, _, body = anon.get("/list.php?name=Legacy&ip=1.2.3.4&port=53640&client=2009E&version=1.3&id=abc123")
    check(status == 200 and b"ERROR" not in body, "list.php accepted")
    status, _, body = anon.get("/serverlist.txt")
    lines = [l for l in body.decode().split("\r\n") if l]
    names = []
    for line in lines:
        inner = base64.b64decode(line.split("|", 1)[1]).decode()
        names.append(base64.b64decode(inner.split("|")[0]).decode())
    check("Legacy" in names and "Crossroads" in names, "serverlist.txt has legacy and new servers")
    anon.get("/delist.php?id=abc123")
    status, _, body = anon.get("/serverlist.txt")
    check(body.count(b"\r\n") == 1, "delist.php removed the legacy server")
    status, _, body = anon.get("/Game/Servers.ashx?action=unregister&jobId=%s&serverKey=%s" % (job["jobId"], job["serverKey"]))
    check(body == b"OK", "unregister")

    print("Moderation")
    bad = Client()
    for _ in range(5):
        bad.post_form("/Api/Login.ashx", {"username": "Builderman", "password": "wrong"})
    status, _, body = bad.post_form("/Api/Login.ashx", {"username": "Builderman", "password": "hunter22"})
    check("Too many attempts" in json.loads(body).get("message", ""), "login flood check")
    status, _, body = admin.postback("/Admin.aspx", {"BanUserBox": "Noob_1", "BanReasonBox": "Exploiting",
                                                     "BanDaysBox": "1"}, "BanButton")
    check(status == 200 and b"has been banned" in body, "admin bans Noob_1 for 1 day")
    status, headers, _ = player.get("/Develop.aspx")
    check(status == 302 and "NotApproved" in headers.get("Location", ""), "banned user is sent to NotApproved.aspx")
    status, _, body = player.get("/NotApproved.aspx")
    check(b"Banned for 1 Day" in body and b"Exploiting" in body, "NotApproved.aspx shows the ban")
    status, _, body = player.get("/Game/PlaceLauncher.ashx?request=RequestGame&placeId=%d" % place_id)
    check(json.loads(body)["status"] == 12, "banned user cannot join")
    status, _, body = player.get("/Admin.aspx")
    check(status in (302, 200) and b"Moderate a user" not in body, "non admin cannot open Admin.aspx")

    if SITE_DIR:
        print("Rate limit")
        path = SITE_DIR.rstrip("/") + "/Web.config"
        config = open(path).read()
        open(path, "w").write(re.sub(r'key="RateLimitRequests" value="\d+"', 'key="RateLimitRequests" value="5"', config))
        time.sleep(3)
        codes = [anon.get("/Api/Status.ashx")[0] for _ in range(12)]
        check(429 in codes, "rate limiter returns 429 (codes: %s)" % codes)
        open(path, "w").write(config)

    print()
    if failures:
        print("%d check(s) failed" % len(failures))
        sys.exit(1)
    print("All checks passed")


if __name__ == "__main__":
    main()
