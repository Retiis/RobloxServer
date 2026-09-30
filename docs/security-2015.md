# Segurança no estilo Roblox 2015

A ideia é reproduzir as medidas que o Roblox usava em 2015, **inclusive as fracas**. Esta página
lista o que existe e onde está, e o que é fraco de propósito.

| Medida (2015) | Onde | Observação |
| --- | --- | --- |
| Cookie `.ROBLOSECURITY` (ticket de Forms Authentication criptografado com a `machineKey`, em hexadecimal) | `Web.config` `<forms name=".ROBLOSECURITY">`, `Code/Security/Auth.cs` | HttpOnly, **sem HTTPS obrigatório** e válido por 30 dias com renovação — como em 2015 |
| Senhas com PBKDF2-HMAC-SHA1, 1000 iterações, salt de 128 bits | `Code/Security/PasswordHasher.cs` | formato do ASP.NET Identity 2 (VS2015); fraco hoje |
| Assinatura de scripts `--rbxsig%...%` (RSA 1024 + SHA-1 sobre `"\r\n" + script`) | `Code/Security/ScriptSigner.cs`, `Game/Visit.ashx`, `Game/Join.ashx` | chave de 1024 bits e SHA-1 são quebráveis hoje |
| Tickets de autenticação de uso único (5 min) | `Code/Security/AuthTickets.cs`, `Game/PlaceLauncher.ashx`, `Game/GetAuthTicket.ashx`, `Login/Negotiate.ashx` | um uso pelo cliente e um uso pelo servidor de jogo |
| Validação do jogador pelo servidor de jogo | `Game/ValidateTicket.ashx` + `App_Data/Templates/GameServer.lua` | o servidor chuta quem não tem ticket válido ou está banido |
| `apiKey` de servidor (RCC) na query string | `HandlerBase.RequireApiKey`, `/GetAllowedMD5Hashes/`, `/GetAllowedSecurityVersions/` | chave em texto claro na URL, como era |
| Lista de versões e MD5 de clientes permitidos | `AllowedSecurityVersions`, `AllowedMD5Hashes` no `Web.config` | o launcher confere o SHA-256 dos pacotes de cliente (`/install/`) |
| `serverKey` por job | `Game/Servers.ashx` | cada servidor hospedado recebe a sua |
| X-CSRF-TOKEN (403 "Token Validation Failed" com o token no cabeçalho) | `Auth.ValidateCsrf`, `Game/Servers.ashx`, `Api/Logout.ashx` | o launcher repete a requisição com o token |
| ViewState + EventValidation + `ViewStateUserKey` nas páginas | `Code/Web/BasePage.cs` | anti-CSRF clássico do Web Forms |
| Request validation do ASP.NET | `Web.config` `validateRequest="true"` | bloqueia `<script>` em formulários |
| Flood checker de login (5 tentativas/5 min por conta, 15 por IP) | `Auth.Login`, `FloodChecker.cs` | sem captcha |
| Limite de cadastros (3 por IP por hora) | `Code/Security/Accounts.cs` (Register.aspx e a landing) | |
| Limite de requisições por IP (45/30 s) | `Global.asax.cs` | assets e presença ficam de fora |
| Regras de nome de usuário (3–20, letras/números, um `_`) + filtro de palavras por lista negra | `WordFilter.cs`, `App_Data/FilteredWords.txt` | fácil de burlar, como o filtro da época |
| SuperSafeChat para menores de 13 | `Register.aspx` → `Visit.ashx`/`Join.ashx`/`ValidateTicket` | |
| Moderação: "Banned for N Days", "Account Deleted", reativar conta | `Admin.aspx`, `NotApproved.aspx`, `Bans.cs` | |
| Ban de IP | `Admin.aspx`, `Global.asax.cs` | `X-Forwarded-For` só é aceito do Raspberry Pi (`TrustedProxies`) |
| Places privados | `Asset/Default.ashx`, `Api/Games.ashx` | só dono/admin, ou um servidor de jogo com `serverKey` |
| Cabeçalho `X-AspNet-Version` exposto | `Web.config` `enableVersionHeader="true"` | vazamento de versão típico da época |

## Fraquezas mantidas de propósito

* HTTP puro (o cookie `.ROBLOSECURITY` pode ser capturado na rede). Se quiser, coloque TLS no nginx do Pi.
* RSA 1024 + SHA-1 na assinatura de scripts.
* `apiKey` na URL; se `ApiKey` ficar vazio, os endpoints ficam abertos.
* O master server compatível com Novetus (`list.php`) aceita qualquer anúncio, como o original em PHP.
* Sem 2FA, sem captcha, sem verificação de e-mail.
