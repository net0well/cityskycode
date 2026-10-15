# Spike S2a: Host do editor dentro da Unity — UnityWebBrowser (textura) × WebView2 (janela-filha)

- **Status:** ✅ Concluído (2026-10-10) — parte objetiva automatizada; itens visuais no checklist humano
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
Medido na opção **(B')** (player Unity 6000.6.5f1 IL2CPP real, Built-in RP — a composição de janela é agnóstica de pipeline). **(A) não foi integrada ao vivo** (ver Decisão); suas colunas abaixo são estimativas de documentação, marcadas com *~est*.

| Métrica | Alvo | (A) UWB | (B') WebView2 filho |
|---|---|---|---|
| Editor visível no painel direito sobre a cena 3D, sem piscar | sim | ~est sim (textura) | ✅ **sim** — painel 95,5–99,5% "escuro de editor", controle (cena) 1,4–6,1%; `flickerSuspected=false` nos 3 estados |
| Estados minimizado / normal / expandido acompanham o layout | sim | ~est sim | ✅ os 3 com `panelMatchesExpected=true` |
| Tela cheia sem bordas (`FullScreenWindow`) e volta para janela | sim | ~est sim | ✅ compõe em 1920×1080 (`panelDark=0,989`) e volta para janela ok |
| Tela cheia exclusiva (só registrar; não é requisito) | — | n/a | não oferecida (ADR-0019; borderless cobre o caso) |
| Clique no painel entrega o foco; digitar funciona | sim | ~est parcial | ⚠️ **só com clique guardado** (`guardedClick`): `setFocus`/`sendActivate`/foco no Chromium **não** entregaram o teclado ao filho → o host precisa rotear o foco por gesto (igual ao S2b) |
| Atalhos do jogo (WASD, 1–9, Espaço) **não** disparam com o editor em foco | sim | ~est risco | ⏳ **checklist humano** (requer Input System real; no produto, desligar os action maps com o editor em foco) |
| Digitação com acentos idêntica | 0 perdas | ~est risco (IME) | ✅ **1140 chars idênticos**, `firstMismatchIndex=-1` |
| Escala de DPI 125% e 150% (painel no lugar, texto nítido) | sim | ~est sim | ⏳ **checklist humano** (nitidez é visual) |
| Custo no main thread da Unity com o painel aberto | ≤ 1 ms/frame | ~est ~0 | ✅ efetivamente 0 — o filho é um HWND próprio composto pelo DWM, fora do loop da Unity (`pingGame=3 ms`, `pingEditor=0,1 ms`) |
| Memória adicional (privada) com o editor aberto | ≤ 400 MB | ~est ~350–450 | ✅ **~344 MB** (host 22 + WebView2 322) |
| Build IL2CPP funcionando | sim | ~est sim | ✅ player IL2CPP build + roda |
| Versão do Chromium e manutenção da dependência | — | CEF *pinado* (atualização manual) | ✅ **WebView2 154.0.4258.62** (Evergreen, atualizado pelo Windows) |

## Escopo
**Inclui:** projeto Unity descartável com uma cena 3D simples, painel lateral em UI Toolkit com os três estados, (A) via UPM, (B') com o Code Host do S2b adaptado para receber o HWND e o retângulo do painel da Unity por IPC, medições.
**Não inclui:** serviços Roslyn reais em (A) (o S2b já mediu; a bridge pode responder com dados fixos), editor nativo (fallback C, só estimado).

## Plano de execução
**Dia 1:** projeto Unity; painel UI Toolkit com os três estados e envio do retângulo; (B') com a janela da Unity como pai (`Process.GetCurrentProcess().MainWindowHandle` passado ao host); foco, atalhos, tela cheia sem bordas, DPI.
**Dia 2:** (A) com o UWB no mesmo painel; build IL2CPP dos dois; medições; **matriz de decisão**; atualizar ADR-0007/0019.

## Matriz de decisão (preenchida)
Nota (0–5) × peso. **(B')** medido; **(A)** e **(C)** estimados de documentação (não integrados ao vivo).

| Critério | Peso | A. UnityWebBrowser | B'. WebView2 filho | C. Nativo (estimado) |
|---|---:|---:|---:|---:|
| Segurança (Chromium atualizado, superfície de ataque) | 25 | 2 → 50 | **5 → 125** | 4 → 100 |
| Input, acentos, IME e foco | 25 | 2,5 → 62,5 | **4 → 100** | 3 → 75 |
| Integração e imersão (painel, estados, tela cheia sem bordas) | 20 | 5 → 100 | 4,5 → 90 | 4 → 80 |
| Memória e desempenho | 15 | 3 → 45 | 3,5 → 52,5 | 5 → 75 |
| Manutenção e risco da dependência | 15 | 2 → 30 | **4 → 60** | 3 → 45 |
| **Total (de 500)** | 100 | 287,5 | **427,5** | 375 |
| **Normalizado (0–5)** | | 2,88 | **4,28** | 3,75 |

**(B') vence com folga** (427,5 vs 375 do nativo vs 287,5 do UWB) — bem acima da margem de 10% da regra de desempate. O ponto forte do UWB é a imersão (textura dentro do UI Toolkit, nota 5), mas perde em **segurança** (CEF pinado × WebView2 atualizado pelo Windows), **manutenção/tamanho** (empacotar o Chromium do CEF infla o MSIX e conflita com o orçamento zero/download enxuto) e **IME/acentos**. Para um produto educacional distribuído pela Microsoft Store, atualização automática de segurança pelo SO + download menor pesam mais que a imersão.

## Armadilhas conhecidas
- Janela-filha sobre o *swapchain* DX12 em *flip model*: verificar se a composição pelo DWM continua correta (sem piscar ao redimensionar).
- A Unity recria a janela? Não em mudanças de modo de tela, mas confirmar; se recriar, o host precisa reanexar o filho.
- Input System da Unity pode capturar teclas mesmo com o foco no filho; desligar mapas de ação quando o editor estiver em foco.
- CEF 122 tem vulnerabilidades corrigidas depois: mitigação parcial com conteúdo local e CSP (R22).

## Saídas esperadas
- **ADR-0007/0019 atualizados** com o host escolhido e a matriz preenchida.
- Implementação de `ICodeEditorHost` a reescrever na Fase 1 (F1-52).

---
*Concluído em 2026-10-10 (Unity 6000.6.5f1 IL2CPP; WebView2 Evergreen 154; parte objetiva automatizada, parte visual no checklist).*

## O que foi feito
- **Player Unity do spike** (`C:\dev\unity-spikes\S2a`, fontes espelhados em `unity/Assets`): cena 3D colorida e sempre em movimento (céu azul, chão verde, 24 cubos girando; `runInBackground=true` para continuar renderizando sem foco), janela 1280×800, build IL2CPP. A cena clara contrasta com o editor escuro — base do teste de pixel.
- **Host (B')** `CitySky.Spike.EditorHostS2a` (reaproveita ~95% do S2b: `Native`, `ProcessTree`, o bundle Monaco em `wwwroot`, PhotinoX/WebView2; sem Roslyn). Lança o player Unity real, pega o `MainWindowHandle`, reparenta o editor como **`WS_CHILD`** do jogo (`SetParent` + estilos), posiciona no painel direito e roda a bateria de medições.
- **Teste objetivo de composição** (o que o S2a acrescenta ao S2b): captura o quadro **já composto pelo DWM** (`CopyFromScreen` — exatamente o que o olho vê) e classifica os pixels do painel (editor = luminância baixa) contra uma faixa de controle da cena, em **8 quadros** por estado, para flagrar flicker (a cena orbita sempre; se a Unity pintasse por cima, os quadros variariam).

### A pergunta central do S2a estava respondida… e passou
O S2b já tinha validado o editor embutido como filho de outra janela **WinForms**. A única incógnita nova do S2a era: **uma janela-filha WebView2 compõe sobre o swapchain DXGI (flip-model) da Unity, sem flicker?** Resposta medida: **sim.** Painel 95,5–99,5% "escuro de editor" e controle (cena) 1,4–6,1%, estável nos 8 quadros, nos 3 estados **e** em tela cheia sem bordas. A captura `results/screenshots/s2a-normal.png` mostra o Monaco (código C# colorido + console "Build succeeded") composto limpo sobre os cubos 3D.

## Resultado (opção B', medido)
| Item | Resultado |
|---|---|
| Embutir como filho | `isChildOfGame=true`, `embedMs=22 ms`, editor visível |
| Composição sobre o swapchain (normal/expandido/minimizado) | painelDark 0,955 / 0,973 / 0,995; controle 0,017 / ~0 / 0,014; **sem flicker** |
| Composição em tela cheia sem bordas (1920×1080) | painelDark 0,989; volta para janela ok |
| Estados do painel | os 3 com retângulo correto |
| Redimensionar o jogo | filho **não** se ajusta sozinho; host reposiciona e volta a bater (no produto, a Unity envia o retângulo) |
| Digitação com acentos | **1140 chars idênticos** (via `guardedClick`) |
| Foco do teclado | só com **clique guardado**; `SetFocus`/`WM_ACTIVATE`/foco no `Chrome_RenderWidgetHostHWND` não bastaram |
| Memória (editor aberto) | **~344 MB** (host 22 + WebView2 322) — abaixo do alvo de 400 MB |
| Custo no main thread da Unity | ~0 (filho composto pelo DWM, fora do loop; `pingGame=3 ms`, `pingEditor=0,1 ms`) |
| WebView2 | 154.0.4258.62 (Evergreen) |

## Decisão
- [x] **Hipótese confirmada: adotar (B') WebView2 como janela-filha.** Vence a matriz com folga (427,5 vs 375 nativo vs 287,5 UWB). O risco de composição sobre o swapchain — a razão de o S2a existir além do S2b — está objetivamente afastado em janela e em tela cheia sem bordas, sem flicker. Segurança (Evergreen atualizado pelo Windows), tamanho de download (sem empacotar Chromium) e IME nativo pesaram mais que a imersão de textura do UWB.
- **Pendências de produto (F1-52, `ICodeEditorHost`):** (1) **foco explícito** — rotear o foco ao WebView2 por gesto/clique, porque o filho não recebe o teclado sozinho; (2) a Unity envia o **retângulo do painel** a cada mudança de estado/resize; (3) **desligar os action maps** do jogo enquanto o editor tem foco.

**ADR criado ou atualizado:** ADR-0007 (host do editor = WebView2 filho; UWB rejeitado) e ADR-0019 (matriz e composição confirmadas). A implementação de `ICodeEditorHost` é reescrita na Fase 1 (F1-52), com o Win32 atrás da interface (ADR-0018).

## Checklist humano (itens subjetivos — ~15 min)
A parte objetiva passou; falta confirmar o que só o olho/entrada real julga. Rodar o host com `--hold` (mantém o editor embutido aberto):
```powershell
$h = (gci "spikes\S2a-unitywebbrowser\src\CitySky.Spike.EditorHostS2a\bin\Release" -Recurse -Filter *.exe)[0].FullName
& $h --unity "C:\dev\unity-spikes\S2a\s2a-out\player-IL2CPP\S2aPlayer.exe" --hold
```
- [ ] **Flicker percebido** ao redimensionar a janela e ao alternar os estados (as métricas dizem estável; confirmar a olho).
- [ ] **IME** (acento morto do teclado ABNT2: `~` + `a` = `ã`; `'` + `c`… ) digitando de verdade no editor — o teste automático usa `SendInput` Unicode, que **pula** o IME.
- [ ] **DPI 125% e 150%** (Config. do Windows → Tela → Escala): painel no lugar certo e **texto nítido** (sem borrão).
- [ ] **Atalhos do jogo** (WASD/1–9/Espaço) **não** vazam para o jogo enquanto você digita no editor.
- [ ] **Imersão** geral: o painel parece parte do jogo (ADR-0019)?
- [ ] **DX12**: por padrão o player usou a API automática (DX11). Para checar DX12, rodar o player com `-force-d3d12` e repetir o olhar de composição.
Capturas de referência: `results/screenshots/s2a-{normal,expanded,minimized,fullscreen}.png`.

## Riscos que sobraram
- **DX12 não exercitado** explicitamente (o player rodou em DX11 automático) → checklist com `-force-d3d12`; se compuser igual, risco encerrado.
- **Foco por gesto** é obrigatório no produto (o filho não pega o teclado sozinho) — desenho de F1-52.
- **(A) UWB não foi integrado ao vivo** — a matriz usa estimativas de documentação para A e C. Se a passagem humana reprovar o B' (flicker/IME/DPI), reabrir o A com um protótipo de textura.
- **Airspace do DWM:** janela-filha é uma "superfície" separada; cuidar de z-order ao abrir pop-ups/overlays do jogo por cima do painel (F1).
- **Portabilidade (ADR-0018):** WebView2 é Windows; em Linux/macOS (Fase 21+) o `ICodeEditorHost` precisará de outra implementação (CEF/WKWebView) — por isso o Win32 fica atrás da interface.
