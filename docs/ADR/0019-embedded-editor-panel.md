# ADR-0019: Editor de código embutido no painel lateral direito do jogo; janela e tela cheia sem bordas

- **Status:** Aceito
- **Data:** 2026-10-09
- **Decisores:** dono do produto (requisitos de 2026-10-08) + resultado do spike S2b
- **Revisa:** [ADR-0007](0007-code-editor.md) (descarta o candidato "janela PhotinoX acoplada" como janela separada)
- **Documentos afetados:** `CODE_EDITOR.md`, `UI_UX.md`, `GAMEPLAY.md`, `TECH_STACK.md`, `GRAPHICS.md`, `docs/spikes/S2a-unitywebbrowser.md`, `docs/phases/F01-foundation.md`, ADR-0018

## Contexto
Requisitos do dono do produto (2026-10-08):
1. "A janela de edição de código deve ser **dentro da janela principal do game**, como uma parte do **menu lateral direito**, não uma janela separada."
2. "O jogo deve conseguir rodar **em janela e fullscreen**."
3. "O editor deve ser possível **expandir e minimizar**" e "deve ser mais bonito".

O ADR-0007 tinha três candidatos de host gratuitos: (A) UnityWebBrowser renderizando numa textura, (B) janela PhotinoX **separada** e acoplada, (C) editor nativo em UI Toolkit. O requisito 1 elimina (B) na forma de janela separada.

O spike S2b testou (B) numa nova forma: a janela do PhotinoX (WebView2) vira **janela-filha** da janela do jogo, que pertence a outro processo, e ocupa o painel direito. Resultado: funciona em janela e em tela cheia sem bordas; latências, memória e segurança dentro das metas. O foco do teclado vem de um clique no painel; para foco programático, o PhotinoX precisaria expor o `MoveFocus` do WebView2.

## Decisão
1. **Lugar do editor:** o editor de código é uma seção do **painel lateral direito** da janela do jogo, junto com o Inspector e as Lições. Nunca uma janela separada.
2. **Três estados do painel:**

   | Estado | Largura | Uso típico |
   |---|---|---|
   | **Minimizado** | faixa de ~44 px na borda direita, com "{ } Código" e um selo com o número de erros | jogar a cidade sem distração |
   | **Normal** | ~1/3 da largura da janela | lições e scripts curtos, cidade bem visível |
   | **Expandido** | ~70% da largura ("código em foco") | Laboratórios e código longo; a cidade continua visível à esquerda |

   Atalhos: **Ctrl+`** minimiza e restaura; **Ctrl+Shift+`** expande e restaura. Botões no cabeçalho do painel. O estado é lembrado por perfil.
3. **Autoridade sobre o layout:** a Unity (UI Toolkit) é dona do painel lateral e da faixa minimizada. Ela envia ao host do editor o **retângulo do painel** sempre que muda (estado, redimensionamento, troca de modo de janela, mudança de DPI). O host posiciona a superfície do editor. No estado minimizado, a superfície do editor fica **oculta**, para não gastar GPU.
4. **Modos de janela:** **janela**, **janela maximizada** e **tela cheia sem bordas** (`FullScreenMode.FullScreenWindow`, padrão da Unity no Windows). **Tela cheia exclusiva não é oferecida.** Uma janela-filha não aparece sobre uma *swapchain* exclusiva, e no Windows 10/11 a tela cheia sem bordas com *flip model* tem latência comparável. Se o S2a escolher o host (A), que renderiza numa textura, a exclusiva volta a ser possível tecnicamente, mas o padrão continua sem bordas.
5. **Candidatos de host (substituem a lista do ADR-0007):**
   - **(A) UnityWebBrowser** renderizando numa textura dentro do painel (UI Toolkit);
   - **(B') WebView2 como janela-filha** da janela do jogo, criada pelo Code Host (via PhotinoX ou pelo SDK oficial `Microsoft.Web.WebView2`, ambos gratuitos), que o S2b validou;
   - **(C) editor nativo** em UI Toolkit (fallback).

   A escolha entre (A) e (B') sai do **S2a**, que compara os dois dentro da Unity real (foco, IME, tela cheia, DPI, memória, segurança).
6. **Foco:** clicar no painel sempre entrega o foco ao editor. Abrir o painel pelo atalho também deve deixar o editor pronto para digitar. Com (B'), isso exige `CoreWebView2Controller.MoveFocus` (decisão em F1-52: expor no PhotinoX, usar o SDK oficial ou aceitar o clique).
7. **Aparência:** tema visual próprio do jogo (azul-noite, verde-água para ação, âmbar para atenção, vermelho para erro), Inter na interface e JetBrains Mono no código, ambas com licença OFL (gratuitas). O console fica sob o editor, com erros clicáveis. Os tokens de cor são os mesmos do UI Toolkit (`UI_UX.md` §6).

## Alternativas consideradas
| Alternativa | Por que não |
|---|---|
| Janela separada acoplada (ADR-0007, candidato B original) | rejeitada pelo dono do produto: quebra a imersão; não acompanha o jogo ao mover; não funciona em tela cheia |
| Painel acoplado na parte **inferior** (desenho anterior do `UI_UX.md`) | o dono do produto pediu o painel lateral direito; o código C# é vertical (linhas curtas, muitas linhas) e combina melhor com uma coluna |
| Tela cheia exclusiva | incompatível com janela-filha; ganho de latência pequeno no Windows moderno |
| Só dois estados (aberto/fechado) | o estado "expandido" é necessário para Laboratórios e código longo sem esconder a cidade |

## Consequências
**Positivas:** experiência integrada ("a IDE faz parte do jogo"); funciona em janela e em tela cheia sem bordas; a cidade fica visível em todos os estados; a superfície web pode ser ocultada no estado minimizado.
**Negativas / custos:** o host do editor precisa reposicionar a superfície quando o jogo muda de tamanho, de modo ou de DPI; fechar ou recriar a janela do jogo destrói uma janela-filha (o host recria sem perder o texto); foco programático depende do `MoveFocus`.
**Riscos:** DPI misto entre processos (testar a 125% e 150%); input queues compartilhadas entre jogo, Code Host e WebView2 (o S2b não viu travamento, mas monitorar); comportamento sob o Steam Overlay (Fase 20).

## Gatilhos de revisão
Resultado do S2a · Unity lançar um componente de web view oficial · problema de DPI ou foco sem solução · decisão de suportar Steam Deck (Fase 21+), onde o editor dentro do jogo já era o plano (ADR-0018).

## Referências
- Relatório do S2b: [`docs/spikes/S2b-photinox-monaco.md`](../spikes/S2b-photinox-monaco.md)
- Unity `FullScreenMode`: https://docs.unity3d.com/ScriptReference/FullScreenMode.html
- WebView2 `CoreWebView2Controller.MoveFocus`: https://learn.microsoft.com/microsoft-edge/webview2/reference/win32/icorewebview2controller#movefocus
