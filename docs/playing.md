# Jogando com o RobloxPlayerLauncher

Os jogos do RobloxServer são jogados pelo **[RobloxPlayerLauncher](https://github.com/V0rtexLinux/RobloxServerLauncher)**,
o launcher oficial do site no estilo do roblox.com de 2013: você clica em **Play** na página do jogo e ele abre.
Só os clientes **2012M** e **2013M** são suportados.

## Publicar um jogo

* Pelo site: **Build** → nome, descrição, gênero, cliente (2012M ou 2013M), jogadores por servidor, público/privado e o arquivo `.rbxl`.
* Pelo Studio 2015: *File → Publish to ROBLOX* (envia para `/Data/Upload.ashx`; usa o cliente padrão, `DefaultClient`).
* Para atualizar: página do jogo → *Configure this Place* → *Upload a new version*.

Jogos antigos publicados para outros clientes (2006S … 2011M, da época do Novetus) passam a usar o `DefaultClient`.

## Instalar o launcher

1. No site, clique em **Download ROBLOX** (menu *More*, rodapé ou página do jogo).
2. Rode o `RobloxPlayerLauncher.exe` uma vez: ele se instala em `%LocalAppData%\RobloxServer` e registra o
   protocolo `robloxserver-player:`.

O botão baixa `App_Data/Launcher/RobloxPlayerLauncher.exe` do próprio site, ou, se ele não existir, a página de
releases do GitHub (`LauncherDownloadUrl` no `Web.config`). Quando o site tem um launcher novo, os launchers
instalados se atualizam sozinhos.

## Jogar e hospedar

| Botão (página do jogo) | O que acontece |
| --- | --- |
| **Play** | o site pede um ticket (`/Game/GetAuthTicket.ashx`) e abre `robloxserver-player:1+launchmode:play+...`. O launcher troca o ticket por um cookie (`Login/Negotiate.ashx`), pede um servidor ao `PlaceLauncher.ashx`, instala/atualiza o cliente e abre o jogo com o script Lua de `Join.ashx` |
| **Host Server** | `launchmode:host`: o launcher registra um job (`POST /Game/Servers.ashx`), baixa o place (conferindo o MD5), abre o cliente como servidor com o script de `/Game/GameServer.ashx` e manda heartbeat até você fechar a janela *ROBLOX Game Server* |

Na primeira vez em cada site o launcher pergunta se você confia nele, porque é o site que decide qual cliente
é baixado e executado.

## Clientes (para o administrador)

O launcher baixa os clientes do próprio site, como o setup.roblox.com de 2013:

| Arquivo | Serve |
| --- | --- |
| `App_Data/Clients/2012M.zip` | `/install/version.ashx?client=2012M`, `/install/download.ashx?client=2012M` |
| `App_Data/Clients/2013M.zip` | idem para 2013M |
| `App_Data/Launcher/RobloxPlayerLauncher.exe` | botão *Download ROBLOX* e atualização automática do launcher |

O jeito mais fácil é a página **Admin** → *Clients*: escolha o pacote, o arquivo e clique em **Upload** (até 1 GB).
O zip deve ter o **conteúdo** da pasta do cliente (o `RobloxApp_client.exe` no topo do zip, não dentro de uma pasta).
O CI já coloca o `RobloxPlayerLauncher.exe` compilado do repositório do launcher em `App_Data/Launcher`.
A versão é o começo do SHA-256 do arquivo; trocar o zip faz todos os launchers baixarem de novo. O formato do zip (exes, argumentos e o `RobloxServerClient.json`) está no
README do launcher.

## Como o servidor de jogo confere os jogadores

1. O `Join.ashx` gera, para cada entrada, um ticket de uso único e coloca no script do jogador, que cria um
   `StringValue` `RSAuthTicket` dentro do jogador.
2. O script do servidor (`GameServer.ashx`) lê o ticket e chama `/Game/ValidateTicket.ashx?ticket=…&jobId=…&serverKey=…`.
3. Ticket inválido, reutilizado, de outro servidor, nome diferente da conta ou conta banida → o jogador é expulso.
   Com `RequireAuthTickets=false` no `Web.config`, quem entra sem ticket é aceito (só o ticket inválido expulsa).
4. A aparência vem do site (`Asset/CharacterFetch.ashx` do usuário do ticket), não do que o cliente mandou.

## Rede

* Na mesma casa: o site devolve o IP local de quem hospeda.
* Pela Internet: libere a porta UDP 53640 (ou a `HostPort` do `Settings.ini` do launcher). O Raspberry Pi encaminha
  a porta (`GAME_FORWARDS`) e o site mostra o `PublicGameAddress`. Veja [raspberry-pi.md](raspberry-pi.md).

## Radmin VPN

1. Crie uma rede no Radmin VPN e peça para o seu amigo entrar nela. Seu IP no Radmin começa com `26.`.
2. No `IniciarServidor.exe`, marque **Aceitar outros PCs (Radmin VPN / rede local)** e aceite o pedido de administrador.
   A janela mostra o endereço para mandar ao amigo, ex.: `http://26.6.234.106:8080/`.
3. O amigo abre esse endereço, clica em **Download ROBLOX**, roda o launcher e confirma que confia no site.
4. Deixe `BaseUrl` e `PublicGameAddress` vazios no `Web.config`: o site responde com o endereço por onde cada um chegou,
   então o servidor de jogo hospedado no seu PC aparece para o amigo como o seu IP do Radmin.
5. *Failed to register URL "http://localhost:8080/" ... Acesso negado (0x80070005)*: a porta ficou reservada para outros
   PCs e o site estava rodando só para este PC. O `IniciarServidor.exe` novo marca **Aceitar outros PCs** sozinho nesse
   caso; para voltar a só este PC, desmarque (o Windows pede permissão para fechar a porta). À mão, num prompt de
   administrador: `netsh http delete urlacl url=http://*:8080/`.
6. Se aparecer *Failed to connect to the Game (ID=17)*, coloque `HostAddress=` com o IP do Radmin de quem hospeda no
   `%LocalAppData%\RobloxServer\Settings.ini` dessa pessoa. Quem hospeda também precisa liberar a porta UDP 53640
   (o `IniciarServidor.exe` já libera no PC do site).
