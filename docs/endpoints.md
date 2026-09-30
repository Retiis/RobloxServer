# Referência dos endpoints

As URLs não diferenciam maiúsculas de minúsculas (o `Global.asax` reescreve para o `.ashx` certo,
inclusive no Mono), e as URLs sem extensão de 2015 também funcionam.

## Cliente 2015

| URL | Handler | Descrição |
| --- | --- | --- |
| `/asset/?id=` (`/Asset/`, `assetversionid`, `placeid`) | `Asset/Default.ashx` | place local → `App_Data/Assets/{id}` → redirecionamento para o Roblox |
| `/Asset/CharacterFetch.ashx?userId=` | `Asset/CharacterFetch.ashx` | aparência do personagem |
| `/Asset/BodyColors.ashx?userId=` | `Asset/BodyColors.ashx` | cores do corpo |
| `/Game/Visit.ashx?IsPlaySolo=1&UserID=&PlaceID=` | `Game/Visit.ashx` | script de Play Solo assinado |
| `/Game/Studio.ashx` | `Game/Studio.ashx` | configuração inicial do Studio |
| `/Game/PlaceLauncher.ashx?request=RequestGame&placeId=` | `Game/PlaceLauncher.ashx` | escolhe um servidor e emite ticket |
| `/Game/Join.ashx?jobId=` | `Game/Join.ashx` | join script Lua assinado (2012M/2013M) com ticket de uso único |
| `/game/getauthticket` | `Game/GetAuthTicket.ashx` | ticket para o usuário logado |
| `/Login/Negotiate.ashx?suggest=` | `Login/Negotiate.ashx` | troca ticket por `.ROBLOSECURITY` |
| `/Game/ClientPresence.ashx`, `/Game/GetCurrentUser.ashx`, `/Game/Logout.ashx` | `Game/*` | |
| `/Game/Wipeouts.ashx`, `/Game/Knockouts.ashx` | `Game/*` | placar |
| `/Game/BuildActionPermissionCheck.ashx`, `/Game/LuaWebService/HandleSocialRequest.ashx`, `/Game/GamePass/GamePassHandler.ashx` | `Game/*` | respostas padrão |
| `/Error/Lua.ashx`, `/Error/Dmp.ashx`, `/Error/Grid.ashx` | `Error/Lua.ashx` | "Thx" |
| `/Setting/QuietGet/{Nome}/` | `Setting/QuietGet.ashx` | `App_Data/Settings/{Nome}.json` ou `{}` |
| `/v1.1/Counters/Increment/` | `Analytics/Counters.ashx` | 204 |
| `/Data/Upload.ashx?assetid=` | `Data/Upload.ashx` | "Publish to ROBLOX" do Studio |

## Servidor de jogo (RCC / launcher)

| URL | Descrição |
| --- | --- |
| `POST /Game/Servers.ashx` `action=register` | registra um job (cookie + X-CSRF-TOKEN) → `jobId`, `serverKey` |
| `/Game/Servers.ashx?action=heartbeat|playerleft|unregister&jobId=&serverKey=` | mantém / atualiza / encerra o job |
| `/Game/ValidateTicket.ashx?ticket=&jobId=&serverKey=` | `OK|userId|nome|superSafeChat|idadeDaConta|admin` ou `ERROR|motivo` |
| `/GetAllowedMD5Hashes/?apiKey=`, `/GetAllowedSecurityVersions/?apiKey=` | listas de verificação |
| `/list.php`, `/delist.php`, `/serverlist.txt` | master server compatível com Novetus |

## RobloxPlayerLauncher

| URL | Descrição |
| --- | --- |
| `robloxserver-player:1+launchmode:play\|host+gameinfo:TICKET+placeid:ID+baseurl:URL` | link aberto pelos botões Play / Host Server (`Content/PlaceLauncher.js`) |
| `/install/version.ashx?client=2012M\|2013M\|Launcher` | versão (`version-` + SHA-256) do pacote em `App_Data/Clients` ou `App_Data/Launcher` |
| `/install/download.ashx?client=` | o zip do cliente ou o `RobloxPlayerLauncher.exe` (sem o exe: redireciona para `LauncherDownloadUrl`) |
| `/Game/GameServer.ashx?jobId=&serverKey=[&loadPlace=false]` | script Lua assinado do servidor de jogo |
| `/Game/PlaceLauncher.ashx` | agora também devolve `placeId` e `client` |

## Launcher (JSON)

| URL | Descrição |
| --- | --- |
| `/Api/Status.ashx` | `{"status":"online"}` (era o `/` do servidor Node) |
| `POST /Api/Login.ashx` (`username`, `password`) | login, define `.ROBLOSECURITY` |
| `POST /Api/Logout.ashx` | logout (X-CSRF-TOKEN) |
| `/Api/Me.ashx` | usuário atual |
| `/Api/Games.ashx[?id=]` | jogos publicados |
| `/Api/Servers.ashx[?placeId=]` | servidores rodando |
| `/Keys/PublicKey.ashx[?format=xml]` | chave pública de assinatura |
