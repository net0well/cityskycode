# Spike S2b: Janela PhotinoX + Monaco + serviços de linguagem Roslyn

- **Status:** **Em andamento** (testes executados em 2026-10-08; relatório completo pendente). Resultados brutos em `spikes/S2b-photinox-monaco/results/selftest-1..4.json`.

> **Resumo parcial (2026-10-08):** completions p95 17 ms (meta 150); diagnósticos p95 272 ms com o debounce de 250 ms incluído (meta 400); digitação de 800 caracteres com acentos idêntica; memória 285 MB privados (host com Roslyn 95 + WebView2 190; meta 400); WebView2 154.0.4258.62; CSP e bloqueio de navegação externa confirmados; página pronta 1,6 s após criar a janela; primeira completion fria 2,1–2,3 s (aquecer em segundo plano).
> **Mudança de requisito do dono do produto:** o editor fica **dentro da janela do jogo, no painel lateral direito**, e o jogo roda em janela e em tela cheia. A janela separada foi descartada. Testado como **janela-filha (WS_CHILD) de um "jogo" em outro processo**: embute em ~22 ms, ocupa o painel exato, funciona em janela e em tela cheia sem bordas (1920×1080), as duas janelas respondem em < 1 ms; ao redimensionar o jogo, o host precisa reposicionar o painel. **Foco:** SetFocus, WM_ACTIVATE e SetFocus no Chromium não entregam o teclado ao WebView2 embutido; **um clique no painel entrega** (300/300 e 200/200 caracteres idênticos). Pendências: foco programático (exige `CoreWebView2Controller.MoveFocus`, que o PhotinoX não expõe), tela cheia exclusiva (testar com a Unity no S2a), ADR do editor embutido, acentos por teclado ABNT2 (a máquina usa layout US; verificação manual).
- **Prazo:** 2 dias · **Ordem:** 4º
- **Depende de:** .NET SDK 10, Node.js, WebView2 Runtime (todos instalados) · **Bloqueia:** S2a (comparação), Fase 1 (E10)
- **Código:** `spikes/S2b-photinox-monaco/` (descartável)
- **ADRs relacionados:** [ADR-0007](../ADR/0007-code-editor.md), [ADR-0017](../ADR/0017-product-scope-classroom.md), [ADR-0018](../ADR/0018-multiplatform-and-steam.md) · Documento: [`CODE_EDITOR.md`](../CODE_EDITOR.md)

## Pergunta
Uma janela **PhotinoX** (WebView2 no Windows) aberta por um processo .NET 10 hospeda o Monaco **offline**, com digitação sem perda de teclas, acentos pelo teclado ABNT2, completions e diagnósticos do Roslyn dentro das metas, memória aceitável, e se acopla de forma usável a uma janela "de jogo"?

## Hipótese
Sim para editor, IME, desempenho e segurança (WebView2 Evergreen é atualizado pelo Windows). O ponto fraco esperado é a **experiência de duas janelas**: foco, posição e comportamento ao minimizar. Uma janela *owned* pela janela do jogo deve resolver boa parte.

## Por que importa
É o candidato preferido do ADR-0007 em segurança, custo e portabilidade. Se falhar, o host passa a ser o UnityWebBrowser (Chromium de 2024, risco R22) ou o editor nativo (muito trabalho, experiência pior).

## Critério de sucesso (definido antes de começar)
| Métrica | Alvo | Resultado |
|---|---|---|
| Monaco carrega sem rede (rede desligada; nenhuma requisição externa no DevTools) | sim | |
| Digitação automatizada de 2.000 caracteres a ~120 palavras/min: texto final idêntico | 0 perdas | |
| Acentos por *dead keys* ABNT2 (`ação`, `você`, `pão`, `à`, `útil`) e `ç` | corretos | |
| Painel de emoji/IME do Windows (Win + .) insere texto | sim | |
| Completions do Roslyn (p95, Host quente) | ≤ 150 ms | |
| Diagnósticos após pausa de digitação (debounce 250 ms incluído) | ≤ 400 ms | |
| Memória total (processo host + processos `msedgewebview2`) com o editor aberto | ≤ 400 MB | |
| Janela *owned* acompanha a janela de jogo (mover, minimizar, restaurar, Alt+Tab) | comportamento aceitável (descrever) | |
| Alternar foco jogo ↔ editor por atalho (crase) | sim | |
| CSP restritiva aplicada e navegação externa bloqueada | sim (registrar a CSP final) | |
| Versão do runtime WebView2 registrada | — | |

## Escopo
**Inclui:** `editor-web` mínimo (TypeScript + Vite + `monaco-editor` ESM, bundle local); janela PhotinoX servindo os arquivos por esquema customizado ou arquivos locais; bridge de mensagens tipadas (JSON) nos dois sentidos; Roslyn `CompletionService` e diagnósticos sobre um `AdhocWorkspace` com um stub da Player API (`City.Log`, `House`); uma janela "de jogo" falsa (Win32/WinForms) para testar o acoplamento.
**Não inclui:** Unity (o acoplamento real com a janela da Unity fica para a Fase 1), hover e ajuda de assinatura (Fase 8), realce semântico (Fase 9).

## Ambiente
Windows 11 com teclado **ABNT2** e layout pt-BR; .NET SDK 10.0.400; PhotinoX (última estável no NuGet); Node 24; `monaco-editor` (última estável); WebView2 Runtime instalado (registrar versão).

## Plano de execução
**Dia 1 — Editor na janela**
1. Bundle do Monaco com Vite (workers do Monaco empacotados localmente).
2. Janela PhotinoX carregando o bundle; CSP `default-src 'self'` com o mínimo que o Monaco exigir (provavelmente `worker-src 'self' blob:` e `style-src 'self' 'unsafe-inline'`); bloquear navegação externa.
3. Bridge: `sendMessage`/`receiveMessage` do Photino, mensagens `{type, payload}` validadas dos dois lados.

**Dia 2 — Roslyn, acoplamento e medições**
1. Completions e diagnósticos com debounce e descarte de respostas obsoletas por versão do documento.
2. Janela de jogo falsa; tornar a janela do editor *owned* (`SetWindowLongPtr` com `GWLP_HWNDPARENT`) e posicioná-la sobre a área do painel; atalho de foco.
3. Rodar a tabela; script de digitação com `SendInput` (incluindo sequências de *dead keys*); relatório.

## Método de medição
Latências com timestamps nas duas pontas da bridge (média e p95 de 200 requisições); memória somando o *working set* de todos os processos da árvore; digitação comparando o texto enviado com `editor.getValue()`.

## Armadilhas conhecidas
- *Dead keys* chegam como sequências de eventos de teclado; perder o evento de composição quebra acentos. Testar com o layout ABNT2 real, não só US-Internacional.
- Janelas *owned* não podem ficar atrás da dona, mas podem sair da área visível dela; tratar movimento e redimensionamento da janela de jogo.
- Em tela cheia exclusiva, a janela separada não aparece: o jogo precisará de modo janela ou *borderless* (registrar o impacto).
- O PhotinoX é um fork comunitário: registrar data do último release, atividade e licença (ficha de dependência).

## Saídas esperadas
- Dados para a decisão do host no ADR-0007 (junto com o S2a).
- CSP final e formato da bridge → `CODE_EDITOR.md` §7 e F1-90.
- Medidas reais de completions e diagnósticos → `CODE_EDITOR.md` §8.

---
*As seções abaixo são preenchidas ao terminar.*

## O que foi feito

## Resultado

## Decisão
- [ ] Hipótese confirmada: seguir com a arquitetura documentada
- [ ] Hipótese refutada: alternativa adotada = …
- [ ] Inconclusivo: próximo passo = …

**ADR criado ou atualizado:**

## Riscos que sobraram
