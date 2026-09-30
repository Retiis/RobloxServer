# Páginas no estilo 2013

O site imita o roblox.com de 2012–2013. O CSS e as imagens das páginas abaixo vêm da recriação do site de 2013
feita pela comunidade [RobloxLabs](https://github.com/RobloxLabs/web) (licença MIT, veja `RobloxServer.Web/Content/Pages/LICENSE-RobloxLabs.txt`), adaptados para Web Forms.

| Página | Endereço | O que tem |
| --- | --- | --- |
| Landing (cidade azul animada) | `/` sem login, `Landing.aspx` | abas *Sign up* (aniversário, gênero, usuário, senha) e *Login*. Menores de 13 anos entram com SuperSafeChat |
| Login | `Login.aspx` | layout do *NewLogin*: entrar à esquerda, "Not a member?" com o aniversário à direita |
| My ROBLOX | `/` com login, `/Home`, `My/Home.aspx` | "Hello, nome!", avatar, pessoas, caixa *What are you up to?* com **My Feed**, **Recently Played Games** e notícias (jogos novos) |
| Character | `My/Character.aspx` | manequim do *Avatar Body Colors*: clique numa parte do corpo e escolha a cor na paleta. As cores vão para o jogo (`Asset/BodyColors.ashx`) |
| Perfil | `User.aspx?id=` (sem id: o seu) | status, avatar, lugares ativos |
| Build | `Develop.aspx` | aba *Places* com a lista no estilo 2013 (Public/Private, visitas, engrenagem) e *Create New Place* |
| Games | `/Games` | lista de jogos com filtros de ordem e gênero |
| People | `Browse.aspx`, `/People` | busca de usuários |

O submenu de quem está logado segue o de 2013: Home, Profile, Character, Places, People, Download.

Ainda não existem (precisam de sistemas novos no servidor): Catalog, Inventory, Friends, Groups, Forum, Messages,
Trade e Builders Club.
