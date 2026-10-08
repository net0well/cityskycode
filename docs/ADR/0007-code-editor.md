# ADR-0007: Usar Monaco (num host web gratuito) com serviços de linguagem Roslyn

- **Status:** Aceito (escolha do host pendente no spike S2; candidatos revisados pelo ADR-0017: somente opções gratuitas)
- **Data:** 2026-10-08
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
