# A conta Noli

Noli é um dos mitos mais antigos do Roblox. Em 26 de fevereiro de 2010, o usuário **YaleUniversity** postou no
fórum que a conta "noli" aparecia sem avatar e sem "last online", e que abrir o perfil levava de volta à página
inicial. Depois vieram as histórias: pele toda preta, uma aura escura, cerca de 50 contas (Noli foi a primeira) e,
mais tarde, a **Void Cult**, que passou a se chamar de ARG, com mensagens codificadas que diziam
*"You have angered Noli!"*.

O site não cria a conta. Quem se registrar com o nome **Noli** (maiúsculas ou minúsculas, tanto faz) ganha o
comportamento do mito. A lógica fica em `Code/Data/Noli.cs`.

| Onde | O que acontece |
| --- | --- |
| No jogo (`Asset/BodyColors.ashx`, `Asset/CharacterFetch.ashx`) | pele toda *Really black* (1003) e sem roupa |
| People (`Browse.aspx`, `/People`) | um **?** no lugar do avatar, sem "last online" e sem data de entrada |
| Perfil (`User.aspx?id=`) | não abre: manda de volta para a página inicial |
| No jogo (`App_Data/Templates/GameServer.lua`) | aura de fumaça preta, e tudo em que Noli pisa fica preto |
| Chat do jogo | quem digita `noli` deixa o servidor escuro por alguns segundos, com a mensagem *"You have angered Noli!"* (uma vez por minuto) |

Os outros usuários agora também têm perfil (`User.aspx`, estilo 2013, com os places públicos) e aparecem em
**People** (menu *More* e rodapé). Os avatares são desenhados com as cores do corpo (`Asset/Avatar.ashx`).

## O ARG

<details>
<summary>Spoilers</summary>

1. O perfil quebrado deixa uma mensagem em base64: no cabeçalho `X-Void` do redirecionamento, no código-fonte da
   linha de Noli em People e no `<desc>` do avatar "?".
   Ela diz `YOU HAVE ANGERED NOLI! FIFTY SLEEP IN THE VOID. /VOID`.
2. `/Void` é uma página preta com uma pergunta em ROT13: `WHEN DID YALEUNIVERSITY FIRST SEE ME?`
   A resposta é **26/02/2010**. Outros formatos de data também valem, e a resposta errada mostra *YOU HAVE ANGERED NOLI!*.
3. A resposta certa revela uma mensagem em binário (`SAY MY NAME WHERE THE SERVERS RUN`) e um pedaço da história.
4. Em um servidor rodando, digitar `noli` no chat faz Noli responder.

</details>
