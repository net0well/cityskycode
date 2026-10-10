# ADR-0007: Usar Monaco (num host web gratuito) com serviços de linguagem Roslyn

- **Status:** Aceito; **host decidido no spike S2a (2026-10-10): opção (B') WebView2 como janela-filha** (ver nota no fim). Revisado pelo [ADR-0019](0019-embedded-editor-panel.md): o editor fica embutido no painel lateral direito do jogo, e a janela separada foi descartada.
- **Data:** 2026-10-08 (host decidido em 2026-10-10)
- **Documentos afetados:** `CODE_EDITOR.md`, `UI_UX.md`, `SECURITY.md`

## Contexto
O §45 pede um editor integrado com realce, autocomplete, diagnósticos, IntelliSense e documentação contextual. O UI Toolkit não oferece um editor de código com realce durante a edição. O Monaco (MIT) é o núcleo do VS Code.

## Decisão
1. **Monaco** empacotado localmente (`editor-web/`, Vite), exibido em **webview** dentro de um painel do jogo.
2. Serviços de linguagem fornecidos pelo **Roslyn no Code Host** (completion, quick info, signature help, diagnósticos; depois classificação, formatação, navegação, renomear, code actions pedagógicas), filtrados pela allowlist e pelos desbloqueios.
3. **Unity como broker** entre webview e Code Host (canal único).
4. Abstração **`ICodeEditorHost`**, com três candidatos **gratuitos** (ADR-0017, orçamento zero):
   - **(A) UnityWebBrowser:** MIT, Chromium/CEF em processo próprio. Último release 2.1.1 (mar/2024, CEF 122).
   - **(B) Janela PhotinoX acoplada** (Apache-2.0): uma janela aberta pelo Code Host e posicionada junto à janela do jogo. No Windows ela usa o **WebView2 Evergreen**, atualizado automaticamente, com patches de segurança do Chromium sem custo. Depois funciona em macOS e Linux sem reescrita (ADR-0018).
   - **(C) Editor nativo mínimo** em UI Toolkit.

   A Vuplex (paga) foi descartada.

## Alternativas consideradas
| Alternativa | Por que não |
|---|---|
| Editor nativo em UI Toolkit | realce e IntelliSense do zero; acessibilidade fraca |
| VS Code externo | quebra a imersão e o controle (pausa, validação, navegação a partir de erros) |
| CodeMirror 6 em webview | viável, mas o Monaco tem paridade maior com a experiência do VS Code (transferência para o mundo real) |

## Consequências
**Positivas:** experiência de IDE real; acessibilidade madura; transferência para ferramentas profissionais.
**Negativas:** dependência de webview gratuita: UWB tem manutenção incerta e Chromium desatualizado; a janela PhotinoX quebra parte da imersão (duas janelas, exige modo janela/borderless). Memória extra (~150–400 MB).
**Riscos:** input/IME/foco (spike S2); Chromium desatualizado no UWB (mitigado por conteúdo 100% local e CSP, mas é um risco de segurança, R22).

## Gatilhos de revisão
Resultado do S2 · webview escolhida descontinuada · UI Toolkit ganhar editor de código rico.

## Nota de decisão do host (spike S2a, 2026-10-10)
Host escolhido: **(B') WebView2 como janela-filha** (`WS_CHILD` do player Unity), o PhotinoX no Windows. Medido no player IL2CPP real: o editor **compõe de forma estável sobre o swapchain DXGI da Unity** (painel 95–99% vs cena 1–6%, sem flicker) em janela e em tela cheia sem bordas, nos três estados do painel; digitação com acentos idêntica (1140 chars); memória ~344 MB; WebView2 Evergreen 154 (atualizado pelo Windows). Matriz de decisão: B' 427,5 > nativo 375 > **UWB 287,5** (rejeitado por CEF pinado/segurança, tamanho do Chromium empacotado e IME). Pendências para F1-52 (`ICodeEditorHost`): foco entregue por gesto (o filho não recebe o teclado sozinho), a Unity envia o retângulo do painel, e desligar os action maps do jogo com o editor em foco. Itens visuais (IME, DPI, flicker percebido, DX12) no checklist humano do relatório `docs/spikes/S2a-unitywebbrowser.md`.
