# Fase 1 — Fundação (walking skeleton)

> **Status:** Planejada · **Bloco:** A · **Atualizado:** 2026-10-08
> **Entrada:** gate da Fase 0 (S1, S2, S6, S7 resolvidos; transporte IPC, mecanismo de sandbox e host do editor decididos) · **Saída:** `city.Log("Olá")` digitado no jogo aparece no console **e** o teste de hash passa em Mono e .NET 10 · **Estimativa:** 6 semanas

---

## 1. Objetivo
Ter, de ponta a ponta e com qualidade de produção, o caminho **editor → Code Host → Worker → jogo → console**, sobre uma simulação determinística vazia, com CI, arquitetura e segurança já impostas por ferramentas.

## 2. Por que agora
É o *walking skeleton*: cada camada existe, ainda que fina, e todas as fases seguintes só acrescentam carne. Ele também transforma regras de documento em regras de compilador (`noEngineReferences`, `LangVersion 9`, CA1416, testes de arquitetura), antes que exista código para violá-las.

## 3. Escopo
**Inclui:** repositório, CI, solução .NET, projeto Unity, pacote `com.citysky.core`, núcleo do domínio (tempo, IDs, RNG, comandos, eventos, hash), runner de simulação, interfaces de plataforma com implementação Windows, Contracts v1, Code Host v0, Worker v0 (Job Object), Player API v0 (`city.Log`), editor Monaco no host escolhido, console, localização, save/load do esqueleto.
**Não inclui:** terreno e câmera (F2), qualquer sistema de cidade (F3+), analyzers pedagógicos (F8), AppContainer (F9).

## 4. Entregáveis
- Repositório com CI verde em Windows (dotnet + Unity) e canário Linux (dotnet).
- `dotnet/CitySky.slnx` navegável pelo Roslyn MCP, incluindo a fonte compartilhada.
- Cena *Bootstrap* que sobe o composition root, o Code Host e o editor.
- Demonstração gravada do marco `city.Log("Olá")`.

## 5. Backlog

### E1 — Repositório e CI
| ID | Tarefa | Onde | Depende de | Pronto quando |
|---|---|---|---|---|
| F1-01 | `.editorconfig` com seção do Perfil N e seção `unity/**` (Perfil U, sem sugestões de C# 10+) | raiz | — | `dotnet format --verify-no-changes` passa |
| F1-02 | `.gitignore` (Unity + dotnet + node + `TestResults/`) e `.gitattributes` completo (`ASSET_PIPELINE.md` §4) | raiz | — | arquivo binário de teste vai para o LFS |
| F1-03 | `dotnet/global.json` (SDK 10.0.x), `Directory.Build.props` (Nullable, `TreatWarningsAsErrors` em src, `AnalysisLevel=latest-recommended`, **CA1416 como erro**), `Directory.Packages.props` (CPM) | `dotnet/` | — | build falha ao usar API exclusiva do Windows fora das implementações de plataforma |
| F1-04 | `CitySky.slnx` com os projetos da `ARCHITECTURE.md` §4.2 (vazios onde ainda não há código) | `dotnet/` | F1-03 | `dotnet build` verde; MCP `get_project_graph` mostra o grafo esperado |
| F1-05 | Workflow `dotnet-windows.yml`: build + test em todo PR | `.github/workflows/` | F1-04 | PR de teste mostra o job verde |
| F1-06 | Workflow `dotnet-linux.yml` (canário de portabilidade, ADR-0018): build + testes de domínio e Contracts | idem | F1-04 | job verde em `ubuntu-latest` |
| F1-07 | Workflow `unity-tests.yml` (GameCI, licença Personal em *secrets*): EditMode + PlayMode em PRs que tocam `unity/**` | idem | F1-10 | job verde |
| F1-08 | Template de PR ("Pertence à fase? ID da tarefa", docs atualizados, `/verify`) | `.github/` | — | template aparece ao abrir PR |

### E2 — Projeto Unity
| ID | Tarefa | Onde | Depende de | Pronto quando |
|---|---|---|---|---|
| F1-10 | Projeto Unity (6.7 LTS se lançada; senão 6.6) com template URP; *Force Text*, *Visible Meta Files*, IL2CPP, Input System, UnityYAMLMerge; versão fixada em `ProjectVersion.txt` | `unity/CitySky/` | — | player IL2CPP abre a cena vazia |
| F1-11 | Pacote `com.citysky.core` com asmdefs `CitySky.Domain`, `.Application`, `.Contracts`, `.Education` (`noEngineReferences: true`) e `csc.rsp` com `-nullable:enable` | `Packages/com.citysky.core/Runtime/` | F1-10 | usar `UnityEngine` no Domain não compila |
| F1-12 | asmdefs `CitySky.Infrastructure`, `.Presentation`, `.Agents.GameObjects`, `.Bootstrap` e de testes, com as referências da `ARCHITECTURE.md` §4.1 (Presentation **não** referencia Domain) | `Assets/Code/` | F1-11 | referência proibida não compila |
| F1-13 | Pacotes UPM: Localization, Input System, `com.unity.nuget.newtonsoft-json`, Test Framework, Performance Testing; `link.xml` para tipos desserializados | `Packages/manifest.json` | F1-10 | player IL2CPP desserializa um DTO de teste |

### E3 — Fonte compartilhada (.NET)
| ID | Tarefa | Onde | Depende de | Pronto quando |
|---|---|---|---|---|
| F1-20 | Projetos `CitySky.Domain/Application/Contracts/Education` com `<Compile Include>` apontando para o pacote Unity, `netstandard2.1`, `LangVersion 9` | `dotnet/src/` | F1-04, F1-11 | arquivo `.cs` criado no pacote compila nos dois lados |
| F1-21 | Shim `IsExternalInit` (para `init` em C# 9) | `Runtime/Domain/Internal/` | F1-20 | `init` compila na Unity e no .NET |
| F1-22 | `CitySky.Architecture.Tests`: nenhum assembly compartilhado referencia `UnityEngine`/`Unity.*`; Domain sem `DateTime.Now`, `Random`, `Stopwatch`, `Guid.NewGuid`, `float`/`double` em campos de store; dependências entre projetos conforme `PROJECT_ARCHITECTURE.md` §5 | `dotnet/tests/` | F1-20 | teste falha ao introduzir violação proposital |

### E4 — Núcleo do domínio
| ID | Tarefa | Onde | Depende de | Pronto quando |
|---|---|---|---|---|
| F1-30 | IDs fortemente tipados (`DOMAIN_MODEL.md` §3.1) e value objects `Money` (centavos `long`, `checked`), `Fixed` (Q16.16), `Percent`, `Power` | Domain | F1-20 | testes de unidade de aritmética, overflow e igualdade |
| F1-31 | `SimulationClock`: tick ↔ TM ↔ TC, fases do dia, calendário (`SIMULATION_TICK.md` §2, §7) | Domain | F1-30 | testes de borda (meia-noite, 100 anos de jogo sem overflow) |
| F1-32 | `DeterministicRandom` (PCG32) com fluxo por `(worldSeed, systemId, tick)` | Domain | — | sequência dourada fixa nos dois runtimes |
| F1-33 | `ISimulationSystem`, `TickSchedule` (período + fase), `SystemSchedule`, `SimulationContext`, `Simulation.RunTick` (`SIMULATION_ARCHITECTURE.md` §3) | Domain | F1-31 | sistema com período P executa `floor(T/P)` vezes em T ticks |
| F1-34 | `ICityCommand`, `CommandQueue` ordenada por `(targetTick, seq)`, `CommandResult` com erros tipados | Domain/Application | F1-33 | testes de ordem e de comando inválido |
| F1-35 | `DomainEventBuffer` (structs, despacho após o tick, em ordem) | Domain | F1-33 | assinante recebe eventos na ordem de emissão |
| F1-36 | `StateHasher` (xxHash64 próprio) e `CityState` mínimo (relógio + 1 contador) | Domain | F1-30 | hash estável entre execuções |
| F1-37 | `CalendarSystem` (primeiro sistema real) e `HeartbeatSystem` (contador para testes) | Domain | F1-33 | testes de fase do dia |
| F1-38 | `HeadlessSimulation` (Domain + Application sem Unity) | Application | F1-33..37 | roda 1 dia TC em teste |

### E5 — Application e runner
| ID | Tarefa | Onde | Depende de | Pronto quando |
|---|---|---|---|---|
| F1-40 | `SimulationRunner` + `TickAccumulator`: velocidades Pausa/1x/2x/3x/5x/10x, `MaxTicksPerFrame = 12`, `SimBudgetMs = 6`, velocidade efetiva, `alpha` (`SIMULATION_TICK.md` §4) | Application | F1-33 | teste: com orçamento estourado nenhum tick é pulado |
| F1-41 | `CommandBus` (validação → fila) e read model `CitySummary` (data, hora, velocidade efetiva) com *dirty tracking* | Application | F1-34 | read model só republica quando muda |
| F1-42 | Portas `ILog`, `IProfilerMarkers` e adaptadores Unity | Application / Infrastructure | — | log aparece no console da Unity e em arquivo |
| F1-43 | Composition root (Pure DI) na cena *Bootstrap*; sem singletons nem `static` mutável | Bootstrap | F1-40..42 | `/arch-check` limpo |

### E6 — Interfaces de plataforma (ADR-0018)
| ID | Tarefa | Onde | Depende de | Pronto quando |
|---|---|---|---|---|
| F1-50 | `IIpcTransport` + implementação escolhida no S1b (named pipe com ACL do usuário ou TCP loopback com token de 256 bits) | Contracts (interface) / Infrastructure + Code Host | S1b | teste de integração de ida e volta |
| F1-51 | `ISandboxLauncher` + implementação Windows com **Job Object** (limites do `CODE_EXECUTION_SECURITY.md` §3 L5) | Code Host | S6 | Worker morre ao fechar o Host; filho proibido |
| F1-52 | `ICodeEditorHost` + implementação escolhida no S2 | Presentation | S2a/S2b | editor abre e recebe foco |
| F1-53 | `IPlatformServices` (implementação *Standalone*) e `IFileLocations` (pastas de saves, perfis, logs; empacotado ou não, conforme o S7) | Infrastructure | S7 | caminhos montados com `Path.Combine`; teste por plataforma |

### E7 — Contracts v1
| ID | Tarefa | Onde | Depende de | Pronto quando |
|---|---|---|---|---|
| F1-60 | Envelope `{v, id, type, payload}`, quadro `[uint32 length][json]`, limite de 8 MB | Contracts | F1-20 | testes de enquadramento (mensagem truncada, tamanho excedido) |
| F1-61 | Mensagens `Hello`, `CompileAndRun`, `RunResult`, `Diagnostics`, `Cancel` (classes imutáveis C# 9, Newtonsoft com `TypeNameHandling.None`) | Contracts | F1-60 | golden JSON em `CitySky.Contracts.Tests` |

### E8 — Code Host e Worker v0
| ID | Tarefa | Onde | Depende de | Pronto quando |
|---|---|---|---|---|
| F1-70 | Code Host com Generic Host, DI, `TimeProvider`, `ILogger`, `runId` por execução | `CitySky.CodeHost` | F1-04 | sobe e responde `Hello` |
| F1-71 | Pipeline v0: parse (C# 14) → compilação contra referências curadas + `CitySky.PlayerApi` → **allowlist v0** (nega por padrão; permite só o necessário para `city.Log` e literais) → rewriter de orçamento → emit + PDB → verificação de IL | `CitySky.CodeAnalysis`, `CitySky.CodeHost` | S1a | `System.IO.File` rejeitado em L1 **e** em L2 (teste isolado de cada camada) |
| F1-72 | Worker v0: ALC coletável, `Console` redirecionado, orçamentos (2 s, 5 M ticks, recursão 256, 256 MB), resposta validada pelo Host | `CitySky.Sandbox.Worker` | F1-51 | laço infinito → mensagem com linha; recursão infinita não derruba o Host |
| F1-73 | Pool com 1 Worker quente; reciclagem após violação, timeout, crash ou N execuções | Code Host | F1-72 | 200 execuções seguidas sem crescimento de memória > 10% |
| F1-74 | Code Host iniciado pela Unity (aquecido em background, compilação "dummy"), reiniciado com *backoff* se cair; encerrado com o jogo | Infrastructure (`CodeHostClient`) | F1-50 | matar o Host → "reconectando…" → volta em ≤ 5 s |
| F1-75 | `CitySky.Security.Tests` (corpus inicial: arquivo, rede, processo, reflexão, laço, recursão, memória) rodando no CI Windows | `dotnet/tests/` | F1-71, F1-72 | corpus verde |

### E9 — Player API v0
| ID | Tarefa | Onde | Depende de | Pronto quando |
|---|---|---|---|---|
| F1-80 | `CityGame.Current`, `City.Log(string)`, `City.Log(object)`; `ICityGateway` com `LiveCityGateway` mínimo; XML docs pt-BR; `CS1591` como erro | `CitySky.PlayerApi` | F1-70 | hover mostra a documentação pt-BR |
| F1-81 | Revisão com `get_public_api` e `security-auditor` | — | F1-80 | relatório anexado ao PR |

### E10 — Editor e console
| ID | Tarefa | Onde | Depende de | Pronto quando |
|---|---|---|---|---|
| F1-90 | `editor-web/` (TypeScript + Vite + Monaco, bundle local), bridge tipada, CSP do S2 | `editor-web/` | S2 | build gera `StreamingAssets/editor-web` |
| F1-91 | `CodePanel` e `ConsoleView` (UI Toolkit), botão ▶ e Ctrl+Enter, erro clicável → linha no editor | Presentation | F1-52, F1-90 | clicar no erro posiciona o cursor |
| F1-92 | `EditorBroker`: debounce de 250 ms, versão do documento, descarte de respostas obsoletas | Presentation | F1-91 | resposta antiga não sobrescreve a nova |
| F1-93 | Diagnósticos ao digitar (`GetDiagnostics`) | Code Host + Presentation | F1-71 | sublinhado em ≤ 400 ms (medido) |

### E11 — Localização e persistência
| ID | Tarefa | Onde | Depende de | Pronto quando |
|---|---|---|---|---|
| F1-100 | Tabelas de strings pt-BR (Unity Localization); chaves para tudo que o esqueleto mostra; mensagens do Code Host vindas de recursos pt-BR | Presentation / Code Host | F1-13 | teste "nenhuma string sem chave" passa (AC-15 começa aqui) |
| F1-101 | Save/Load do esqueleto: `manifest.json` (`schemaVersion: 1`, `tick`, `seed`, `hash`), `city.json.gz`, escrita atômica com `.bak` | Application / Infrastructure | F1-36, F1-53 | round-trip com hash igual |

### E12 — Determinismo e marco
| ID | Tarefa | Onde | Depende de | Pronto quando |
|---|---|---|---|---|
| F1-110 | Teste de determinismo: mesma seed + mesmos comandos → mesmo hash, em `dotnet test` **e** em Unity EditMode, contra o mesmo *golden* | testes | F1-38 | os dois verdes no CI |
| F1-111 | Teste de independência de velocidade: 2 dias TC em 1x e em 10x → hashes iguais | testes | F1-40 | verde |
| F1-112 | **Marco:** `city.Log("Olá")` no editor do jogo aparece no console (vídeo curto em `docs/media/`, se o repositório permitir) | todos | E8–E10 | demonstração feita no player IL2CPP |

## 6. Conteúdo pedagógico
Ainda não há lição, mas o marco é a semente da Lição 1 ("Bom dia, prefeito(a)"). Primeiras mensagens pt-BR entram no catálogo: `CS1002`, `CS0103`, `CSK1001` e "laço possivelmente infinito na linha N".

## 7. Testes e qualidade
Unidade (núcleo), determinismo (2 runtimes), contrato (golden JSON), arquitetura, corpus de segurança inicial, integração Unity + Code Host (PlayMode). Nenhuma alocação por frame no runner em regime (Performance Testing).

## 8. Gate de saída
- [ ] F1-112 demonstrado em build IL2CPP.
- [ ] F1-110 e F1-111 verdes no CI.
- [ ] CI verde: dotnet Windows, canário Linux e Unity.
- [ ] `/verify` e `/arch-check` sem violações; `/security-scan` sem achados críticos no Code Host, Worker e Contracts.
- [ ] Ida e volta de script simples ≤ 1,5 s quente e ≤ 4 s frio (`MVP.md` §6), medidas no hardware recomendado.

## 9. Riscos
| Risco | Ref. | Mitigação nesta fase |
|---|---|---|
| Agentes aplicarem C# 10+ no código Unity | R19 | `LangVersion 9` no csproj compartilhado; CI Unity |
| Dívida de portabilidade | R26 | CA1416 como erro; canário Linux desde o primeiro PR |
| Determinismo quebrar entre Mono e .NET | R07 | F1-110 desde o primeiro sistema |
| Escopo do esqueleto inchar | R01 | nada de sistema de cidade nesta fase |

## 10. Documentos e ADRs a atualizar
`ARCHITECTURE.md` (detalhes reais de asmdefs e projetos) · `CODE_EXECUTION.md` §6 (protocolo v1 real) · `TECH_STACK.md` §8 (versões fixadas) · `CODING_STANDARDS.md` (se o `.editorconfig` mudar regras) · `PLAYER_API.md` (v0) · `TESTING.md` §A5 (workflows reais) · status "Implementado (esqueleto)" em `SIMULATION_TICK.md` e `SIMULATION_ARCHITECTURE.md`.

## 11. Ferramental
`dotnet-architect` (estrutura da solução) · `/spec` + `/plan` para E4, E8 · `/tdd` em todo o E4 · `test-engineer` (determinismo, golden files) · `security-auditor` + `/security-scan` (E8, E9) · `/verify`, `/arch-check` em todo PR · `/health-check` no encerramento.

## 12. Encerramento
- [ ] Gate verificado e registrado
- [ ] `/health-check` com nota registrada
- [ ] `RISKS.md` revisado
- [ ] Velocidade real registrada; estimativas das Fases 2–8 recalibradas
