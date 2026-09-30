# Raspberry Pi Zero 2W: port forwarding, UPnP e DNS

O Pi fica ligado 24 h gastando ~1 W e faz o papel de "borda" da rede:

```
Internet ──► roteador ──► Raspberry Pi Zero 2W (192.168.1.2)
                             ├─ nginx :80  ──────────► site ASP.NET (IIS no PC, ou Mono no próprio Pi)
                             ├─ iptables UDP 53640 ──► PC que hospeda o jogo pelo launcher
                             ├─ dnsmasq: robloxserver.lan → 192.168.1.2 (domínio próprio, sem hosts na rede)
                             └─ upnpc: abre as portas no roteador a cada 30 min (+ DDNS opcional)
```

Os servidores de jogo (clientes 2012M e 2013M do launcher) são executáveis Windows x86 e **não rodam no Pi**:
eles ficam nos PCs, e o Pi encaminha as portas até eles.

## 1. Preparar o Pi

1. Grave o **Raspberry Pi OS Lite (64-bit, Bookworm)** com o Raspberry Pi Imager, já com Wi-Fi e SSH.
2. Dê um **IP fixo** ao Pi (reserva de DHCP no roteador é o mais simples).
3. No Pi:
   ```bash
   sudo apt-get install -y git
   git clone https://github.com/V0rtexLinux/RobloxServer.git
   cd RobloxServer/pi
   nano robloxserver-pi.conf
   sudo ./install.sh
   ```

Para ver o que seria gerado sem instalar nada: `RENDER_ONLY=1 ./install.sh` (arquivos em `pi/out/`).

## 2. `robloxserver-pi.conf`

| Variável | Exemplo | Significado |
| --- | --- | --- |
| `PI_IP` / `PI_INTERFACE` | `192.168.1.2` / `wlan0` | endereço e interface do Pi |
| `SITE_MODE` | `remote` ou `local` | site no IIS de um PC (`remote`) ou no próprio Pi via Mono (`local`) |
| `SITE_HOST` / `SITE_PORT` | `192.168.1.10` / `80` | onde está o IIS (modo `remote`) |
| `GAME_FORWARDS` | `53640:192.168.1.10 53641:192.168.1.11:53640` | `porta_externa:ip_do_pc[:porta_interna]`, um por anfitrião |
| `GAME_PROTOCOLS` | `udp` | o Roblox usa UDP; adicione `tcp` só se precisar |
| `DNS_ENABLED` / `DNS_HOSTS` | `yes` / `robloxserver.lan www.robloxserver.lan api.robloxserver.lan` | nomes próprios do servidor que passam a apontar para o Pi (o `www.roblox.com` não é tocado) |
| `UPNP_ENABLED` | `yes` | pede ao roteador para abrir a porta 80/TCP e as portas de jogo |
| `DDNS_URL` | URL do DuckDNS | atualiza o DNS dinâmico junto com o UPnP |

## 3. Depois de instalar

* **Roteador:** se não tiver UPnP, encaminhe manualmente a porta 80/TCP e as portas de jogo (UDP) para o `PI_IP`.
  Configure o **DNS do DHCP** do roteador para o `PI_IP` (assim todo PC da casa acha `http://robloxserver.lan/`).
* **Web.config do site:** coloque o `PI_IP` em `TrustedProxies` (modo `remote`) e o seu IP público / nome
  DDNS em `PublicGameAddress`. Jogadores de fora recebem esse endereço; os da rede recebem o IP local do PC.
* **Launcher de quem hospeda:** em `%LocalAppData%\RobloxServer\Settings.ini`, use em `HostPort` a porta configurada
  em `GAME_FORWARDS` e, em `HostAddress`, o IP público/DDNS (ou deixe vazio para o RobloxServer resolver pelo `PublicGameAddress`).

## 4. Modo `local` (site no próprio Pi)

Instala `mono-complete`, `mono-fastcgi-server4` e compila o site com `xbuild` no Pi, publicando em
`SITE_DIR` e rodando pelo serviço `robloxserver-fastcgi`. Funciona (é o mesmo stack do CI), mas o
Zero 2W tem só 512 MB de RAM: a primeira página demora alguns segundos para compilar. Os dados
(`App_Data`) são preservados quando você roda `install.sh` de novo para atualizar.

> O `xsp4` do Debian/Ubuntu está quebrado com o Mono 6.8, por isso usamos nginx + FastCGI.

## 5. Comandos úteis

```bash
sudo systemctl status robloxserver-firewall robloxserver-upnp.timer nginx dnsmasq
sudo rs-firewall.sh start          # reaplica o port forwarding
sudo rs-upnp.sh                    # renova o UPnP / DDNS agora
sudo iptables -t nat -S RS_PREROUTING
journalctl -u robloxserver-fastcgi -f   # modo local
```

## Observação sobre o MASQUERADE

Para o encaminhamento funcionar com o Pi fora do caminho padrão da rede, o Pi faz *MASQUERADE*:
o PC anfitrião vê todos os jogadores de fora como vindos do IP do Pi. A identificação dos jogadores
é feita pelos tickets de autenticação do RobloxServer, não pelo IP, então isso não atrapalha.
