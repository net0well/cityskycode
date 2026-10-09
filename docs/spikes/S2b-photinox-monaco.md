# Spike S2b: PhotinoX + Monaco + serviços de linguagem Roslyn (editor embutido no painel do jogo)

- **Status:** **Concluído** (hipótese confirmada com uma ressalva: o foco do teclado precisa vir de um clique no painel)
- **Prazo:** 2 dias · **Ordem:** 4º · **Início / fim:** 2026-10-08 → 2026-10-09
- **Depende de:** .NET SDK 10, Node.js, WebView2 Runtime (todos instalados) · **Bloqueia:** S2a (comparação), Fase 1 (E10)
- **Código:** `spikes/S2b-photinox-monaco/` (descartável) · Resultados brutos: `results/selftest-1..4.json` · Capturas: `results/screenshots/`
- **ADRs relacionados:** [ADR-0007](../ADR/0007-code-editor.md), [ADR-0017](../ADR/0017-product-scope-classroom.md), [ADR-0018](../ADR/0018-multiplatform-and-steam.md), [ADR-0019](../ADR/0019-embedded-editor-panel.md) · Documento: [`CODE_EDITOR.md`](../CODE_EDITOR.md)

## Pergunta
Uma janela **PhotinoX** (WebView2 no Windows) aberta por um processo .NET 10 hospeda o Monaco **offline**, com digitação sem perda de teclas, acentos pelo teclado ABNT2, completions e diagnósticos do Roslyn dentro das metas, memória aceitável, e se acopla de forma usável a uma janela "de jogo"?

**Pergunta revisada durante o spike** (requisito novo do dono do produto em 2026-10-08): o editor deve ficar **dentro da janela do jogo, no painel lateral direito**, poder ser **expandido e minimizado**, e o jogo deve rodar **em janela e em tela cheia**. A janela separada foi descartada. A pergunta passou a ser: o mesmo editor funciona **embutido como janela-filha** da janela do jogo, que pertence a outro processo?

## Hipótese
Sim para editor, IME, desempenho e segurança (WebView2 Evergreen é atualizado pelo Windows). O ponto fraco esperado é a **experiência de duas janelas**: foco, posição e comportamento ao minimizar. Uma janela *owned* pela janela do jogo deve resolver boa parte.

## Por que importa
É o candidato preferido do ADR-0007 em segurança, custo e portabilidade. Se falhar, o host passa a ser o UnityWebBrowser (Chromium de 2024, risco R22) ou o editor nativo (muito trabalho, experiência pior).

## Critério de sucesso (definido antes de começar)
| Métrica | Alvo | Resultado |
|---|---|---|
| Monaco carrega sem rede (nenhuma requisição externa) | sim | ✅ CSP `connect-src 'self'` barrou o `fetch` externo (evento de violação registrado); navegação externa cancelada pelo host; todos os arquivos vêm de `app://localhost` |
| Digitação automatizada a ~120 palavras/min: texto final idêntico | 0 perdas | ✅ 800/800 caracteres com acentos (400 a 10 car/s + 400 em rajada), em 2 de 3 rodadas; na 3ª o teste parou sozinho quando outra janela tomou o foco (proteção do teste) |
| Acentos por *dead keys* ABNT2 (`ação`, `você`, `pão`, `à`, `útil`) e `ç` | corretos | ⏳ **verificação manual pendente**: a máquina usa teclado US. Os acentos chegaram corretos por injeção Unicode, mas o caminho das *dead keys* não foi exercitado |
| Painel de emoji/IME do Windows (Win + .) insere texto | sim | ⏳ manual pendente |
| Completions do Roslyn (p95, Host quente) | ≤ 150 ms | ✅ **17,3 ms** de ida e volta (servidor: 16,2 ms); a 1ª completion fria custa 2,1–2,3 s (aquecer em segundo plano) |
| Diagnósticos após pausa de digitação (debounce 250 ms incluído) | ≤ 400 ms | ✅ **272 ms** (p95), ou seja, ~22 ms além do debounce |
| Memória total (processo host + processos `msedgewebview2`) com o editor aberto | ≤ 400 MB | ✅ **285 MB privados** (host com Roslyn 95 MB + WebView2 190 MB). O *working set* somado dá 615 MB, mas conta páginas compartilhadas mais de uma vez |
| ~~Janela *owned* acompanha a janela de jogo~~ → **Editor embutido no painel do jogo** | comportamento aceitável | ✅ ver "Resultado" |
| Alternar foco jogo ↔ editor | sim | ⚠️ **um clique no painel** entrega o foco; foco programático não funcionou (ver "Resultado") |
| CSP restritiva aplicada e navegação externa bloqueada | sim | ✅ CSP final abaixo |
| Versão do runtime WebView2 registrada | — | **154.0.4258.62** (Evergreen) |

## Escopo
**Inclui:** `editor-web` (TypeScript + Vite + `monaco-editor` ESM, bundle local); janela PhotinoX servindo os arquivos por esquema customizado; bridge de mensagens JSON nos dois sentidos; Roslyn `CompletionService` e diagnósticos sobre um `AdhocWorkspace` com um stub da Player API; uma janela "de jogo" falsa **em outro processo** (WinForms via PowerShell) para testar o acoplamento.
**Não inclui:** Unity (vai para o S2a), hover e ajuda de assinatura (Fase 8), realce semântico (Fase 9).

## Ambiente
Intel Xeon E5-2670 v3, 32 GB, Radeon RX 7600, Windows 11 25H2 (10.0.26200), **teclado US**, monitor 1920×1080 a 100%. .NET 10.0.11 (SDK 10.0.400), PhotinoX 5.3.5 (Apache-2.0), Microsoft.CodeAnalysis.CSharp.Features/Workspaces 5.9.0, Node 24, Vite 8.3.4, TypeScript 7.0.2, monaco-editor 0.57.0, fontes Inter e JetBrains Mono (@fontsource 5.3.0, OFL-1.1), WebView2 154.0.4258.62.

---

## O que foi feito
1. **Bundle do editor** (Vite): Monaco + linguagem C#, worker embutido como `blob:` (`?worker&inline`), fontes OFL locais. ~3 MB de JS (790 KB com gzip). A API do Monaco 0.57 mudou de caminhos (`monaco-editor/editor/editor.api`, `monaco-editor/languages/definitions/csharp/register`).
2. **Host .NET 10** com PhotinoX: `PhotinoApplication.Run`, esquema `app://` servindo só `wwwroot` (com proteção contra sair da pasta), `NavigationStarting` cancelando tudo que não é `app://`, DevTools e menu de contexto desligados, pasta de dados própria. O PhotinoX já **não cria pop-ups** por padrão.
3. **Roslyn:** `AdhocWorkspace` com `MefHostServices.DefaultAssemblies` (já traz os serviços de completions), um projeto da Player API (stub) e um projeto do aluno. Cada pedido cria um snapshot imutável do documento. As mensagens do compilador **saem em pt-BR** porque o runtime tem os recursos de idioma (ex.: "não pode ser atribuído, pois é somente leitura").
4. **Autoteste automatizado** (`--selftest`): latências medidas no navegador, CSP, navegação, memória da árvore de processos, digitação por `SendInput` Unicode **protegida** (só envia teclas enquanto a janela certa tem o foco; nunca digita em outro aplicativo) e testes de janela.
5. **Janela com dono** (o plano original): funciona (some quando o jogo minimiza, volta junto, fica acima, não trava), mas **não acompanha** quando o jogo se move. Ficou como dado, porque o requisito mudou.
6. **Editor embutido** (requisito novo): o host transforma a janela do PhotinoX em **filha** (`WS_CHILD` + `SetParent`) da janela do jogo, que é de outro processo, e a posiciona no painel direito, abaixo da barra de HUD. Testado em janela, redimensionando o jogo, em **tela cheia sem bordas** (1920×1080) e de volta para janela.
7. **Foco:** quatro estratégias testadas, cada uma com uma sonda de 30 caracteres (ver "Resultado").
8. **Aparência e estados do painel** (pedido do dono do produto): tema "City: SkyCode" (azul-noite, verde-água para ação, âmbar para atenção), Inter na interface, JetBrains Mono no código, cabeçalho com **expandir** e **minimizar**, abas de arquivo, botões Executar/Validar/Dica, console estilo IDE com erros clicáveis, barra de status. Três estados: **normal** (34% da largura), **expandido** (70%, "código em foco", a cidade continua visível à esquerda) e **minimizado** (faixa de 44 px com "{ } Código" e um selo com o número de erros). Atalhos: Ctrl+` minimiza/restaura, Ctrl+Shift+` expande/restaura. Capturas geradas com `--screenshots`.

## Resultado

### Desempenho e segurança
| Medida | Valor |
|---|---|
| Página pronta após criar a janela (WebView2 + bundle) | 1,6 s |
| 1ª completion / 1º diagnóstico com o Roslyn frio | 2,1–2,3 s / 0,27–0,30 s |
| Completions com o Roslyn quente (ida e volta, 200 pedidos) | média 12–13 ms · p95 17,3 ms · máx. 256–271 ms (1º pedido) |
| Diagnósticos do fim da digitação aos marcadores (30 rodadas, debounce de 250 ms incluído) | p50 266 ms · p95 272–277 ms |
| Memória privada (host + WebView2) | 285 MB (host 95 MB, dos quais a maior parte é o Roslyn; WebView2 190 MB em 6 processos) |
| CSP final | `default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; font-src 'self' data:; img-src 'self' data:; worker-src blob:; connect-src 'self'; frame-src 'none'; object-src 'none'; base-uri 'none'; form-action 'none'` |

### Editor embutido no painel do jogo (janela-filha entre processos)
| Teste | Resultado |
|---|---|
| Tornar filha e posicionar | 21–23 ms; `IsChild` = sim; ocupa exatamente o retângulo do painel |
| Resposta das duas janelas depois de embutir (WM_NULL) | < 1 ms (sem travamento por fila de entrada compartilhada) |
| Jogo redimensionado | o painel **não** se ajusta sozinho; depois que o host reposiciona, fica exato. No produto, a Unity informa o retângulo do painel ao host |
| **Tela cheia sem bordas** (estilo da janela do jogo trocado e 1920×1080) | painel visível, filho correto, retângulo exato |
| Voltar para janela | painel visível e exato |
| Fechar o jogo | a janela-filha é destruída junto (o host do editor precisa tratar isso e recriar) |

### Foco do teclado com o editor embutido
| Estratégia | Janela com foco depois | Caracteres recebidos (de 30) |
|---|---|---|
| `SetFocus` na janela do PhotinoX | `PhotinoX` | 0 |
| `WM_ACTIVATE` + `SetFocus` | `PhotinoX` | 0 |
| `SetFocus` na janela interna do Chromium (`Chrome_RenderWidgetHostHWND`) | `Chrome_RenderWidgetHostHWND` | 0 |
| **Clique no painel** (com checagem de que o ponto está sobre o editor) | `Chrome_WidgetWin_1` | **30** |

Com o clique, o teste completo passou em janela (**300/300** caracteres idênticos) e em tela cheia sem bordas (**200/200**). Conclusão: como filha, a janela do PhotinoX nunca recebe a ativação de janela de topo, e o WebView2 só aceita o teclado quando o controlador recebe `MoveFocus` ou quando o usuário clica dentro dele. O clique é o gesto natural ("clico no editor e digito"). Já o foco por atalho de teclado ou ao abrir o painel exige chamar `CoreWebView2Controller.MoveFocus`, que o PhotinoX 5.3.5 não expõe.

### Aparência (capturas)
`results/screenshots/panel-normal.png`, `panel-expanded.png`, `panel-minimized.png`. Uma primeira versão tinha um defeito de layout: a coluna do grid crescia até a largura antiga do editor e não encolhia. Corrigido com `grid-template-columns: minmax(0, 1fr)`.

**Resposta à pergunta:** sim. O editor Monaco com Roslyn funciona **embutido no painel direito da janela do jogo**, em janela e em tela cheia sem bordas, com as três posições do painel, dentro das metas de latência, memória e segurança. A ressalva é o foco: hoje ele vem de um clique; para foco programático é preciso acesso ao `MoveFocus` do WebView2.

## Decisão
- [x] Hipótese confirmada, com mudança de forma: **editor embutido como janela-filha**, em vez de janela separada
- [ ] Hipótese refutada
- [ ] Inconclusivo

**ADR criado ou atualizado:** [ADR-0019](../ADR/0019-embedded-editor-panel.md) (editor embutido no painel lateral direito, estados do painel, janela e tela cheia sem bordas). O ADR-0007 continua valendo para Monaco + Roslyn, e a escolha final do host sai do S2a (UnityWebBrowser como textura × WebView2 como janela-filha), que precisa da Unity com licença ativada.

## Riscos que sobraram
- **Foco programático:** necessário para abrir o editor por atalho e já digitar. Caminhos: (1) contribuir com o PhotinoX para expor `MoveFocus` (Apache-2.0, fork ativo); (2) hospedar o WebView2 diretamente pelo SDK oficial (`Microsoft.Web.WebView2`, gratuito) no Code Host, sem PhotinoX; (3) aceitar o clique. Decidir na Fase 1 (F1-52).
- **Tela cheia exclusiva:** janelas-filhas não aparecem sobre uma *swapchain* em modo exclusivo. A tela cheia do jogo será **sem bordas** (`FullScreenMode.FullScreenWindow`, padrão da Unity no Windows; ADR-0019). Confirmar com a Unity real no S2a.
- **DPI:** testado só a 100% de escala. Monitores a 125–150% (comuns em notebook) precisam de teste, porque host e jogo podem ter percepções de DPI diferentes.
- **Teclado ABNT2 e IME:** verificação manual pendente (máquina com teclado US).
- **Ciclo de vida:** fechar ou recriar a janela do jogo destrói a filha; o host precisa recriá-la sem perder o código digitado.
- **Dependência:** PhotinoX é um fork comunitário (Apache-2.0); versão 5.3.5 com API ativa e documentada. Alternativa sem ele: o SDK oficial do WebView2.
