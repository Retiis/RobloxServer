# Instalando o RobloxServer (Windows / IIS)

O site é um *Web Application* ASP.NET 4.6 (Visual Studio 2015). Ele roda no IIS, no IIS Express
ou no Mono (veja o [modo local do Raspberry Pi](raspberry-pi.md)).

## Teste rápido (só neste PC)

Dê dois cliques em `iniciar-servidor.bat` na raiz do repositório (precisa do
[IIS Express](https://www.microsoft.com/download/details.aspx?id=48264)). Ele compila o site se ainda não
estiver compilado e abre `http://localhost:8080/`. Outra porta: `iniciar-servidor.bat 80`.

O `IniciarServidor.exe` faz o mesmo numa janela. Marque **Aceitar outros PCs (Radmin VPN / rede local)** para
amigos entrarem pelo Radmin VPN ou pela rede (veja [playing.md](playing.md#radmin-vpn)).

Para a Internet e o Raspberry Pi, siga os passos abaixo com o IIS completo.

## 1. Compilar

* Instale o Visual Studio 2015 (ou mais novo) com "ASP.NET and web development" e o .NET Framework 4.6.
* Abra `RobloxServer.sln` e compile em **Release** (gera `RobloxServer.Web\bin\RobloxServer.dll`).
  * Sem Visual Studio: `msbuild RobloxServer.sln /p:Configuration=Release`.

## 2. Publicar no IIS

1. Ative o IIS com **ASP.NET 4.6** (Painel de Controle → Recursos do Windows → IIS → Recursos de Desenvolvimento de Aplicativos → ASP.NET 4.x).
2. Crie um site apontando para a pasta `RobloxServer.Web`, porta **80**, sem *host name* (os clientes podem chegar pelo IP, por `robloxserver.lan` ou pelo seu DDNS).
3. O Application Pool deve ser **.NET CLR v4.0, Integrated**.
4. Dê permissão de escrita em `RobloxServer.Web\App_Data` para `IIS AppPool\<nome do pool>` (é onde ficam usuários, places, chaves e logs).
5. Abra `http://localhost/` e crie a primeira conta: **ela vira administradora**.

> Para testar sem IIS: no Visual Studio aperte F5 (IIS Express).

## 3. Configurar (`Web.config` → `<appSettings>`)

| Chave | Para que serve |
| --- | --- |
| `BaseUrl` | URL que os clientes usam. Vazio (padrão) = o endereço por onde a requisição chegou; ou fixe, ex.: `http://robloxserver.lan/` |
| `ApiKey` | apiKey dos servidores de jogo (estilo RCC). Vazio = aberto |
| `Administrators` | nomes de administradores, separados por vírgula |
| `HostPolicy` | quem pode hospedar: `Anyone`, `Owner` ou `Admin` |
| `RequireAuthTickets` | exige ticket de autenticação de cada jogador |
| `AllowedMD5Hashes` / `AllowedSecurityVersions` | listas de verificação do cliente de 2015 |
| `TrustedProxies` | IP do Raspberry Pi, para confiar no `X-Forwarded-For` |
| `PublicGameAddress` | IP público ou nome DDNS mostrado a quem está fora da rede |
| `RateLimitRequests` / `RateLimitWindowSeconds` | limite por IP (45 / 30 s) |

## 4. Fazer os clientes acharem o servidor

O servidor **não** usa o domínio `www.roblox.com` (ele é do Roblox). Use um endereço próprio:

* **Raspberry Pi com dnsmasq** (recomendado): toda a rede passa a achar `http://robloxserver.lan/`. Veja [raspberry-pi.md](raspberry-pi.md).
* **Só o IP** (`http://192.168.1.2/`) ou um nome **DDNS** para quem joga de fora.
* **Arquivo hosts** em cada PC: `192.168.1.2 robloxserver.lan www.robloxserver.lan api.robloxserver.lan`.

Com `BaseUrl` vazio, os scripts gerados (`Visit.ashx`, `Join.ashx`, `Studio.ashx`...) usam o mesmo endereço
que o cliente usou. O RobloxPlayerLauncher recebe o endereço do próprio botão Play.

Os executáveis oficiais de 2015 (Studio/Player) têm `www.roblox.com` gravado dentro. Para usá-los, troque
esse texto no executável pelo seu domínio (um nome com o mesmo número de letras de `roblox.com` é o mais fácil
de editar, ex.: `rbxsrv.lan`) ou redirecione só na sua máquina com o Fiddler. Os clientes do launcher não precisam disso.

## 5. Assinatura de scripts

Na primeira execução o servidor cria uma chave RSA de 1024 bits em `App_Data\Keys`.
Os clientes 2015 só executam `Visit.ashx`/`Join.ashx` assinados com a chave embutida neles, então
substitua a chave pública do Roblox no executável pela sua (`App_Data\Keys\PublicKeyBlob.txt`,
ou `http://robloxserver.lan/Keys/PublicKey.ashx`). É o mesmo procedimento de qualquer revival de 2015.
