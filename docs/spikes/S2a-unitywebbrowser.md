# Spike S2a: Host do editor dentro da Unity — UnityWebBrowser (textura) × WebView2 (janela-filha)

- **Status:** Planejado · **bloqueado até a licença Unity Personal ser ativada** no Unity Hub
- **Prazo:** 2 dias · **Ordem:** 6º
- **Depende de:** Unity 6000.6.5f1 com IL2CPP (instalada em `C:\Unity\Editor`), licença ativada, S2b (bundle do editor e protótipo de janela-filha) · **Bloqueia:** Fase 1 (`ICodeEditorHost`, F1-52), ADR-0007/0019
- **Código:** `spikes/S2a-unitywebbrowser/` (descartável)
- **ADRs relacionados:** [ADR-0007](../ADR/0007-code-editor.md), [ADR-0017](../ADR/0017-product-scope-classroom.md), [ADR-0019](../ADR/0019-embedded-editor-panel.md) · Documento: [`CODE_EDITOR.md`](../CODE_EDITOR.md) §2

## Pergunta
Dentro de um player Unity 6.6 IL2CPP real, qual host entrega o editor (o mesmo bundle do S2b) **no painel lateral direito**, com os estados minimizado/normal/expandido, em **janela** e em **tela cheia sem bordas**, com o melhor equilíbrio entre segurança, input, imersão, memória e manutenção?

- **(A) UnityWebBrowser** (MIT, Chromium/CEF em processo próprio) renderizando numa textura exibida num `VisualElement` do UI Toolkit;
- **(B') WebView2 como janela-filha** da janela da Unity, criada pelo Code Host (o S2b validou com uma janela WinForms; aqui com a janela real da Unity e o seu *swapchain*).

## Hipótese
(B') vence em segurança (WebView2 Evergreen × CEF 122 de mar/2024) e em input/IME (controle nativo). (A) vence em imersão e na tela cheia exclusiva, mas a exclusiva não é oferecida (ADR-0019). Esperamos escolher (B'), desde que a janela-filha componha bem sobre o *swapchain* da Unity (DX11/DX12) e que o foco funcione com clique e com o atalho.

## Por que importa
Fecha a escolha do host do editor, condição do gate da Fase 0 (S2 = S2a + S2b).

## Critério de sucesso (definido antes de começar)
| Métrica | Alvo | (A) UWB | (B') WebView2 filho |
|---|---|---|---|
| Editor visível no painel direito sobre a cena 3D (DX11 e DX12) | sim, sem piscar | | |
| Estados minimizado / normal / expandido acompanham o layout do UI Toolkit | sim | | |
| Tela cheia sem bordas (`FullScreenWindow`) e volta para janela | sim | | |
| Tela cheia exclusiva (só registrar; não é requisito) | — | | |
| Clique no painel entrega o foco; atalho Ctrl+` abre e já permite digitar | sim | | |
| Atalhos do jogo (WASD, 1–9, Espaço) **não** disparam com o editor em foco | sim | | |
| Digitação de 800 caracteres com acentos idêntica | 0 perdas | | |
| Escala de DPI 125% e 150% (painel no lugar certo, texto nítido) | sim | | |
| Custo no main thread da Unity com o painel aberto | ≤ 1 ms/frame | | |
| Memória adicional (privada) com o editor aberto | ≤ 400 MB | | |
| Build IL2CPP funcionando | sim | | |
| Versão do Chromium e manutenção da dependência registradas | — | | |

## Escopo
**Inclui:** projeto Unity descartável com uma cena 3D simples, painel lateral em UI Toolkit com os três estados, (A) via UPM, (B') com o Code Host do S2b adaptado para receber o HWND e o retângulo do painel da Unity por IPC, medições.
**Não inclui:** serviços Roslyn reais em (A) (o S2b já mediu; a bridge pode responder com dados fixos), editor nativo (fallback C, só estimado).

## Plano de execução
**Dia 1:** projeto Unity; painel UI Toolkit com os três estados e envio do retângulo; (B') com a janela da Unity como pai (`Process.GetCurrentProcess().MainWindowHandle` passado ao host); foco, atalhos, tela cheia sem bordas, DPI.
**Dia 2:** (A) com o UWB no mesmo painel; build IL2CPP dos dois; medições; **matriz de decisão**; atualizar ADR-0007/0019.

## Matriz de decisão (preencher)
| Critério | Peso | A. UnityWebBrowser | B'. WebView2 filho | C. Nativo (estimado) |
|---|---:|---:|---:|---:|
| Segurança (Chromium atualizado, superfície de ataque) | 25 | | | |
| Input, acentos, IME e foco | 25 | | | |
| Integração e imersão (painel, estados, tela cheia sem bordas) | 20 | | | |
| Memória e desempenho | 15 | | | |
| Manutenção e risco da dependência | 15 | | | |
| **Total (0–5 × peso)** | 100 | | | |

Regra: se A e B' ficarem a menos de 10% um do outro, vence **B'** (segurança atualizada pelo Windows e portabilidade, ADR-0018).

## Armadilhas conhecidas
- Janela-filha sobre o *swapchain* DX12 em *flip model*: verificar se a composição pelo DWM continua correta (sem piscar ao redimensionar).
- A Unity recria a janela? Não em mudanças de modo de tela, mas confirmar; se recriar, o host precisa reanexar o filho.
- Input System da Unity pode capturar teclas mesmo com o foco no filho; desligar mapas de ação quando o editor estiver em foco.
- CEF 122 tem vulnerabilidades corrigidas depois: mitigação parcial com conteúdo local e CSP (R22).

## Saídas esperadas
- **ADR-0007/0019 atualizados** com o host escolhido e a matriz preenchida.
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
