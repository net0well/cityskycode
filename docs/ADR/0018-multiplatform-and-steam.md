# ADR-0018: Windows primeiro, arquitetura portável, outras plataformas e Steam depois

- **Status:** Aceito
- **Data:** 2026-10-08
- **Decisores:** dono do produto ("o jogo deve rodar em todos os computadores"; "publicar na Steam futuramente"; "primeiro vamos fazer funcionar em Windows, depois veremos as outras plataformas")
- **Complementa:** ADR-0001 (plataformas), ADR-0007/0017 (host do editor)
- **Documentos afetados:** `ENGINE_DECISION.md`, `TECH_STACK.md`, `CODE_EDITOR.md`, `CODE_EXECUTION_SECURITY.md`, `DEVELOPMENT_ROADMAP.md`, `CLASSROOM.md`, `SHARING_SYSTEM.md`, `SAVE_SYSTEM.md`, `SECURITY.md`, `RISKS.md`, `FUTURE_FEATURES.md`, `/CLAUDE.md`

## Contexto
O objetivo de longo prazo é rodar "em todos os computadores" e publicar na Steam. A prioridade imediata é **fazer funcionar no Windows**. O que "todos os computadores" significa na prática (pesquisa de 2026-10-08):

| Plataforma | Viável? | Observação |
|---|---|---|
| Windows 10/11 x64 | ✅ | **foco atual** |
| Windows ARM64 (Snapdragon X etc.) | ✅ | a Unity gera player ARM64; o .NET 10 tem `win-arm64`. Barato de incluir no próprio ciclo Windows |
| Linux x64 (Ubuntu, Fedora, SteamOS / Steam Deck) | ✅ depois | player Unity Linux (Vulkan); .NET 10 `linux-x64`; sandbox próprio (seccomp + Landlock). Steam Deck exige **build nativo** (o sandbox do Windows provavelmente não funciona sob Proton/Wine) |
| macOS (Apple Silicon e Intel) | ✅ depois, com custo | players Unity e .NET existem. **Distribuir (inclusive pela Steam) exige notarização → Apple Developer Program, ~US$ 99/ano** |
| ChromeOS / Chromebook | ❌ | não roda apps nativos. A Steam para Chromebook foi encerrada em 1º/jan/2026. Só uma futura versão web atenderia |
| Windows 7/8, 32 bits | ❌ | fora do suporte da Unity 6 e do .NET 10 |

## Decisão
1. **Windows primeiro.** MVP, piloto escolar e primeiro lançamento (Microsoft Store, itch.io e, depois, Steam) são **só Windows**: x64, mais ARM64 se o custo de teste for baixo. Toda a qualidade (sandbox L5, desempenho, acessibilidade) é garantida primeiro no Windows.
2. **Outras plataformas depois:** Linux/Steam Deck e macOS entram numa fase **pós-lançamento Windows** (Fase 21+), reavaliada pelo dono do produto. Chromebook fica fora, com a versão web como pesquisa futura.
3. **Arquitetura portável desde já** (custo baixo, evita reescrita):
   - Tudo que é específico de sistema fica atrás de interfaces: `IIpcTransport` (named pipe hoje; Unix domain socket depois), `ISandboxLauncher` (Job Object/AppContainer hoje; seccomp+Landlock ou Seatbelt depois), `ICodeEditorHost`, `IPlatformServices` (Standalone hoje; Steam depois), `IFileLocations`.
   - **Nenhuma API exclusiva do Windows** fora dessas implementações. Caminhos com `Path.Combine`.
   - **Host do editor:** a janela WebView2 pura é trocada por uma **janela PhotinoX** (Apache-2.0, fork mantido do Photino.NET, `net10.0`). No Windows ela **usa o próprio WebView2**, o mesmo motor e o mesmo custo de hoje, e depois funciona em macOS (WKWebView) e Linux (WebKitGTK) sem reescrita. O UnityWebBrowser continua candidato, e o editor nativo é o fallback.
   - **CI:** builds e testes Unity só no Windows. Os testes .NET (domínio, Code Host) também rodam num runner Linux como **canário de portabilidade**, o que é barato e pega dependências acidentais do Windows. Builds Unity para Linux/macOS só na Fase 21+.
4. **Steam (futuro, Fase 20, Windows):**
   - **Steamworks.NET** (MIT) atrás de `IPlatformServices`.
   - **Steam Cloud** para saves e conquistas.
   - **Steam Workshop** como canal de compartilhamento online, o que dispensa servidor próprio. O conteúdo é código de terceiros, portanto exige **L5 completo** na plataforma publicada.
   - Contas Steam têm idade mínima, então **escolas continuam pela Microsoft Store, itch.io ou download direto**.
5. **Steam Deck e macOS** (quando vierem, Fase 21+): build Linux nativo; alvo "Playable" no Deck (camada de cidade com controle, programação com teclado, texto ≥ 12 px a 1280×800); sandbox Linux sem depender de *user namespaces* (restritos pelo AppArmor no Ubuntu 24.04+); macOS com Seatbelt + hardened runtime + notarização.

## Custos futuros (exceções ao orçamento zero do ADR-0017, a decidir quando chegar a hora)
| Item | Custo | Quando | Alternativa sem custo |
|---|---|---|---|
| Steam Direct | US$ 100 por jogo (devolvido após US$ 1.000 em vendas) | lançamento na Steam | itch.io, Microsoft Store |
| Apple Developer Program (notarização) | ~US$ 99/ano | só se/quando houver macOS | nenhuma para distribuir a terceiros |
| Hardware de teste (Linux/Steam Deck, Mac) | variável | Fase 21+ | runners de CI, testers voluntários |

## Alternativas consideradas
| Alternativa | Por que não |
|---|---|
| Três plataformas desde o MVP | triplica o sandbox, os testes e os builds antes de validar o jogo; contraria a prioridade do dono do produto |
| Windows sem preocupação de portabilidade | portar depois exigiria reescrever IPC, sandbox e editor; as interfaces custam pouco agora |
| Windows + Proton para Linux/Deck | sandbox do Windows provavelmente não funciona sob Wine |

## Consequências
**Positivas:** foco total no Windows; portar depois fica possível sem reescrita; o Steam Workshop resolve o compartilhamento online sem servidor.
**Negativas:** usuários de Linux/Mac/Chromebook esperam; as interfaces de plataforma são um pequeno custo extra de abstração.
**Riscos:** ver `RISKS.md` R21, R26–R28.

## Gatilhos de revisão
Lançamento no Windows concluído (abrir a Fase 21) · demanda de escolas por Linux/Chromebook · orçamento para macOS · mudanças da Valve ou da Apple.

## Referências
- Steam Direct fee: https://partner.steamgames.com/doc/gettingstarted/appfee
- Notarização de builds macOS na Steam: https://techraptor.net/content/apple-notarization-mac-gaming
- Fim da Steam para Chromebook (jan/2026): https://www.androidauthority.com/steam-chromebook-3585553/
- PhotinoX (NuGet): https://packages.nuget.org/packages/PhotinoX
- Restrição de user namespaces no Ubuntu: https://discourse.ubuntu.com/t/understanding-apparmor-user-namespace-restriction/58007
- Steam Deck: compatibilidade: https://partner.steamgames.com/doc/steamdeck/compat
