# Spike S2a: UnityWebBrowser no painel do jogo (e decisão do host do editor)

- **Status:** Planejado
- **Prazo:** 2 dias · **Ordem:** 6º
- **Depende de:** Unity 6.6 (F0-04), S2b (mesmo bundle do Monaco e mesmos critérios) · **Bloqueia:** Fase 1 (`ICodeEditorHost`), ADR-0007
- **Código:** `spikes/S2a-unitywebbrowser/` (descartável)
- **ADRs relacionados:** [ADR-0007](../ADR/0007-code-editor.md), [ADR-0017](../ADR/0017-product-scope-classroom.md) · Documento: [`CODE_EDITOR.md`](../CODE_EDITOR.md) §2

## Pergunta
O **UnityWebBrowser** (MIT, Chromium/CEF em processo próprio) renderiza o mesmo Monaco do S2b dentro de um painel UI Toolkit da Unity 6.6, com input, acentos, foco e memória aceitáveis, funcionando em build IL2CPP? Comparado ao S2b, qual host o projeto adota?

## Hipótese
Funciona e é mais imersivo (tudo dentro do jogo), mas perde em segurança e manutenção: o último release documentado é o **2.1.1 (22/mar/2024, CEF 122)**, sem release posterior encontrado em out/2026. A expectativa é que o S2b vença no critério de segurança.

## Por que importa
Fecha a escolha do host do editor (ADR-0007), uma das condições do gate da Fase 0.

## Critério de sucesso (definido antes de começar)
Os mesmos critérios do S2b, mais:

| Métrica | Alvo | Resultado |
|---|---|---|
| Monaco renderizado numa textura exibida num `VisualElement` | sim | |
| Atalhos do jogo (WASD, 1–9, Espaço) **não** disparam enquanto o editor tem foco | sim | |
| Custo no main thread da Unity com o painel aberto | ≤ 1 ms/frame | |
| Build IL2CPP com o processo do motor do UWB funcionando | sim | |
| Versão do Chromium/CEF e exposição a CVEs conhecidas registradas | — | |
| Atividade do projeto (último commit, issues abertas, mantenedores) registrada | — | |

## Escopo
**Inclui:** pacote do UWB via UPM (registrar a versão exata e a origem), painel UI Toolkit com a textura, encaminhamento de input, bridge JS ↔ C# do UWB com o mesmo protocolo do S2b, medições.
**Não inclui:** serviços Roslyn reais (o S2b já mediu; aqui a bridge pode responder com dados fixos), editor nativo (fallback C, só estimado).

## Ambiente
Windows 11, Unity 6.6, UnityWebBrowser (versão mais recente disponível), teclado ABNT2.

## Plano de execução
**Dia 1:** instalar o UWB; painel com o Monaco; input e foco; *dead keys* e `ç`; bridge.
**Dia 2:** build IL2CPP; medições (memória, custo no main thread, latência da bridge); preencher a **matriz de decisão** abaixo com os dados do S2a e do S2b; atualizar o ADR-0007.

## Matriz de decisão (preencher com os dois spikes)
| Critério | Peso | A. UnityWebBrowser | B. Janela PhotinoX | C. Editor nativo (estimado) |
|---|---:|---:|---:|---:|
| Segurança (Chromium atualizado, superfície de ataque) | 25 | | | |
| Input, acentos, IME e foco | 25 | | | |
| Integração e imersão | 20 | | | |
| Memória e desempenho | 15 | | | |
| Manutenção e risco da dependência | 15 | | | |
| **Total (0–5 × peso)** | 100 | | | |

Regra: se A e B ficarem a menos de 10% um do outro, vence **B** (segurança atualizada pelo Windows e portabilidade, ADR-0018).

## Armadilhas conhecidas
- Conflito de input entre o UI Toolkit, o Input System e a textura do navegador.
- Processos do motor "órfãos" ao fechar o jogo (corrigido no 2.1.1 segundo o changelog; verificar).
- CEF 122 tem vulnerabilidades corrigidas em versões posteriores: mitigação parcial com conteúdo 100% local e CSP (R22).

## Saídas esperadas
- **ADR-0007 atualizado** com o host escolhido e a matriz preenchida.
- Implementação de `ICodeEditorHost` a reescrever na Fase 1 (F1-52).

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
