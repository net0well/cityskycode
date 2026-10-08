# Editor de Código Integrado

> **Status:** v1.1 (host pendente no spike S2; só opções gratuitas, ADR-0017) · **Data:** 2026-10-08 · **ADRs:** [0007](ADR/0007-code-editor.md), [0017](ADR/0017-product-scope-classroom.md) · **MVP:** sim

---

## 1. Objetivo
Dar ao aluno uma experiência de **IDE real** dentro do jogo (realce, autocomplete, documentação contextual, erros sublinhados, console clicável), porque usar ferramentas profissionais também é parte do aprendizado.

## 2. Avaliação (§45)

| Recurso | Monaco + Roslyn (adotado) | Editor nativo em UI Toolkit | Janela externa (VS Code) |
|---|---|---|---|
| Realce de sintaxe | ✅ nativo (Monarch) + classificação semântica Roslyn | ⚠️ difícil: TextField não suporta realce durante a edição | ✅ |
| Autocomplete / IntelliSense | ✅ via `CompletionService` do Roslyn | ⚠️ implementar UI do zero | ✅ |
| Diagnósticos | ✅ marcadores (`setModelMarkers`) | ⚠️ | ✅ |
| Documentação contextual (hover) | ✅ `QuickInfoService` + XML docs pt-BR | ⚠️ | ✅ |
| Integração com o jogo (pausar, executar, navegar a partir de erros) | ✅ bridge | ✅ | ❌ quebra a imersão; difícil de controlar |
| Acessibilidade (leitor de tela, zoom, alto contraste) | ✅ madura | ❌ | ✅ |
| Custo | webview (dependência) | muito trabalho próprio | — |

**Decisão:** Monaco, com serviços de linguagem fornecidos pelo **Roslyn no Code Host**. Fallback documentado: editor nativo mínimo (realce por classificação Roslyn + lista de completions), caso o spike S2 falhe.

### Hosts candidatos (todos gratuitos, ADR-0017)
| Candidato | Como funciona | Prós | Contras |
|---|---|---|---|
| **A. UnityWebBrowser** (MIT) | Chromium (CEF) em processo próprio, renderiza numa textura dentro do painel do jogo | integrado à UI do jogo; open source | último release 2.1.1 (mar/2024, CEF 122): Chromium desatualizado; ~400 MB em repouso; manutenção incerta |
| **B. Janela PhotinoX** (Apache-2.0) | o Code Host (.NET 10) abre uma janela PhotinoX, posicionada e acoplada à janela do jogo. No Windows ela usa o **WebView2** | runtime **Evergreen** atualizado pelo Windows (segurança em dia, sem custo); IME e acessibilidade nativos; tira o Chromium do processo da Unity; portável para macOS (WKWebView) e Linux (WebKitGTK) depois (ADR-0018) | duas janelas; exige o jogo em modo janela/borderless; gestão de foco e posição; janelas separadas são ruins no Steam Deck (futuro) |
| **C. Editor nativo mínimo** | UI Toolkit + classificação e completions do Roslyn | zero dependência | realce e edição limitados; muito trabalho |

**Critério do S2:** digitação sem perda, IME/acentos, foco, memória ≤ 400 MB, segurança (versão do Chromium) e esforço de integração. **Hipótese inicial:** B tende a vencer em segurança e custo de manutenção, e A em imersão.

## 3. Arquitetura

```
┌────────────────────── Unity ─────────────────────────┐
│ CodePanel (UI Toolkit)                                │
│   ├─ ICodeEditorHost  ← implementação: UWB | janela PhotinoX | nativo   │
│   │      └─ textura da webview exibida no painel + input encaminhado    │
│   ├─ ConsoleView (UI Toolkit, erros clicáveis)       │
│   └─ EditorBroker ── CodeHostClient (IPC) ───────────┼──► Code Host (Roslyn)
└──────────────────────────────────────────────────────┘
            ▲  postMessage / JS bridge (JSON)
┌───────────┴──────────── WebView ─────────────────────┐
│ editor-web (TypeScript, Vite, Monaco)                 │
│  providers: completion, hover, signatureHelp,         │
│             diagnostics(markers), formatting          │
│  CSP restritiva, sem rede, navegação bloqueada        │
└──────────────────────────────────────────────────────┘
```

- `ICodeEditorHost` isola a escolha do host (UnityWebBrowser × janela PhotinoX × editor nativo).
- O `EditorBroker` na Unity faz *debounce*, guarda a versão do documento e descarta respostas obsoletas.

## 4. Serviços de linguagem (Code Host, Roslyn)
| Recurso | API Roslyn | Fase |
|---|---|---|
| Diagnósticos ao digitar | `Compilation.GetDiagnostics` + analyzers CSK | MVP |
| Autocomplete | `CompletionService.GetCompletionsAsync` (filtrado pela allowlist e por desbloqueios) | MVP |
| Hover | `QuickInfoService` + XML docs pt-BR da Player API | MVP |
| Ajuda de assinatura | `SignatureHelp` (Features) | MVP |
| Realce semântico | `Classifier.GetClassifiedSpansAsync` | Fase 9 |
| Formatação | `Formatter.FormatAsync` | Fase 9 |
| Ir para definição (código do aluno + "metadata as source" da Player API) | `SymbolFinder` | Fase 14 |
| Renomear | `Renamer` | Fase 15 (refatoração/SOLID) |
| Code actions pedagógicas (ex.: "Extrair interface") | `CodeFixProvider` próprios | Fase 15 |

Referência de implementação: **RoslynPad** (editor open source que usa Roslyn Features para completion, quick info e signature help).

**Filtragem pedagógica do autocomplete:** itens fora da allowlist ou ainda bloqueados **não aparecem**. Isso evita que o aluno descubra `System.IO` pelo autocomplete e receba um erro em seguida.

## 5. Experiência do usuário
- **Layout:** painel acoplável na parte inferior (expansível para tela cheia) ou modo "lado a lado" (cidade à esquerda, código à direita).
- **Arquivos:** árvore simples do projeto do aluno (`Program.cs`, `EnergyAuditor.cs`...), com abas.
- **Barra:** ▶ Executar (Ctrl+Enter) · ■ Parar · 🧪 Validar desafio · 💡 Dicas · ↺ Restaurar código inicial.
- **Pausa automática** ao executar (MVP) e opcional ao abrir o editor (configuração de acessibilidade/carga cognitiva).
- **Console estilo IDE (§62):**
```
Build succeeded.  0 errors, 1 warning
  warning CSK2003 (linha 4): a variável 'total' nunca é usada.
▶ Program.cs executado em 38 ms
  Casa construída no lote L-17 (−$12.400)
Desafio: ✔ Aprovado (3/3 critérios)
Population: 2,340 · Happiness: 78% · Treasury: $124,000
```
  Erros e avisos são **clicáveis** e levam à linha no editor (`revealLineInCenter` + seleção).
- **Teclado/IME:** acentos e cedilha em strings e comentários (pt-BR). Validar no spike S2.
- **Temas:** escuro (padrão), claro e alto contraste; tamanho de fonte ajustável.

## 6. Depuração (evolução)
| Fase | Recurso |
|---|---|
| MVP | `city.Log`/`Console.WriteLine`, mensagens pedagógicas com linha, Inspector da cidade |
| 9 | **Traço de execução** em Laboratórios: instrumentação registra valores de variáveis por linha (limitado); o aluno "passa" pela execução com um controle deslizante (*time-travel* simples) |
| 17 | Integração com testes do jogador (rodar teste individual, ver falha) |
| Futuro | Breakpoints reais exigiriam depurador no Worker: fora de escopo (custo alto) |

## 7. Segurança da webview
Conteúdo **somente local** (`StreamingAssets/editor-web`), CSP `default-src 'self'`, sem `eval` além do necessário para o Monaco (*workers* via blob controlado), navegação externa bloqueada, sem cookies/armazenamento persistente além do cache local, bridge com mensagens tipadas e validadas. Ver `SECURITY.md`.

## 8. Desempenho
- Webview criada uma vez e reutilizada (esconder ≠ destruir).
- Diagnósticos com *debounce* de 250 ms; meta de ≤ 400 ms até o sublinhado.
- Completions ≤ 150 ms (Code Host quente).

## 9. Riscos e questões em aberto
| Risco | Mitigação |
|---|---|
| Opções gratuitas menos maduras (UWB com Chromium de 2024) | `ICodeEditorHost`; candidato B (PhotinoX sobre WebView2 Evergreen); fallback nativo |
| Integração de input/foco entre UI Toolkit e webview | spike S2; painel dedicado com foco exclusivo |
| Memória (Chromium ~150–400 MB) | uma instância; liberar quando o painel fica fechado por muito tempo (configurável) |
| Teclado internacional / IME | teste obrigatório no S2 |
