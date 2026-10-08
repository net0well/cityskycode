# CLAUDE.md — CitySkyCode

> Instruções operacionais para agentes de IA. Fonte completa: [`docs/AI_DEVELOPMENT_GUIDE.md`](docs/AI_DEVELOPMENT_GUIDE.md).
> Se `/dotnet-init` (dotnet-claude-kit) for executado, escolher **merge**: este arquivo é a fonte de verdade.

## Projeto
Simulador de cidade 3D (Unity 6 + URP) em que o jogador **programa a cidade em C#** para aprender C#, POO, SOLID, padrões, arquitetura e testes. O código do jogador é compilado por **Roslyn** e executado num **processo separado e restrito** (.NET 10), e só altera a cidade por **comandos** validados.

**Fase atual: 0 (Pesquisa, arquitetura e spikes).** Documentação pronta; ainda não há código de produto. Próximo passo: executar os spikes na ordem de `docs/spikes/README.md` (começa pelo S6). Ver `docs/DEVELOPMENT_ROADMAP.md` e `docs/phases/`.

## Leitura obrigatória antes de qualquer mudança
1. `docs/README.md` (índice) → 2. **plano da fase atual** (`docs/phases/Fnn-*.md`; localizar a tarefa pelo ID `Fn-xx`) → 3. `docs/ARCHITECTURE.md` → 4. `docs/TECH_STACK.md` → 5. documento do sistema afetado → 6. ADRs relacionados (`docs/ADR/`) → 7. `docs/CRITICAL_ANALYSIS.md` (decisões que divergem do briefing original `main.md`) → 8. `docs/ADR/0017-product-scope-classroom.md` (decisões de produto). Em spike: o plano do spike (`docs/spikes/S*.md`) define pergunta, critério e escopo; código de spike fica em `spikes/<id>/` e é descartável.

## Decisões de produto (ADR-0017)
- **Produto educacional**, usado em **sala de aula**: perfis locais, lições ≤ 20 min, sem conta, offline, instalação sem admin, LGPD (nada sai do PC).
- **Compartilhamento por arquivo** (`.citysky`, só código-fonte), nunca executa sozinho, só depois do sandbox L5 completo (Fase 9).
- **Orçamento zero:** nenhuma dependência, asset ou ferramenta paga. Arte e áudio CC0. Distribuição pela Microsoft Store (MSIX assinado gratuitamente).
- Hardware mínimo: Intel Iris Xe, 8 GB, 30 FPS no preset Baixo.

## Plataformas (ADR-0018)
- **Windows primeiro** (decisão do dono do produto). Linux/Steam Deck e macOS só depois do lançamento no Windows (Fase 21+). **Não implementar** nada específico dessas plataformas antes disso.
- **Mas manter a arquitetura portável:** o que é específico de SO fica atrás de `IIpcTransport`, `ISandboxLauncher`, `ICodeEditorHost`, `IPlatformServices`, `IFileLocations`. Nenhuma API exclusiva do Windows fora dessas implementações; `Path.Combine` sempre. Os testes .NET também rodam num runner Linux como canário.
- Steam no futuro (Steamworks.NET atrás de `IPlatformServices`). Escolas usam Microsoft Store, itch.io ou download direto.

## Ferramental de IA para .NET: **dotnet-claude-kit (obrigatório)**
Todo trabalho em C#/.NET usa o plugin **dotnet-claude-kit** (skills, agentes, comandos) e o MCP **CWM.RoslynNavigator**.
```bash
dotnet tool install -g CWM.RoslynNavigator
/plugin marketplace add codewithmukesh/dotnet-claude-kit
/plugin install dotnet-claude-kit
```
- **MCP primeiro:** usar `find_symbol`, `find_references`, `get_file_outline`, `get_public_api`, `detect_circular_dependencies` antes de ler arquivos inteiros. Solução: `dotnet/CitySky.slnx`.
- **Fluxo:** `/spec` → `/plan` → `/tdd` → `/verify` → `/code-review`. Mais `/arch-check` (sempre), `/security-scan` (Code Host, Worker, Player API, IPC), `/health-check` (fim de fase), `/outdated` (mensal).
- **Agentes:** `dotnet-architect`, `test-engineer`, `security-auditor` (obrigatório no sandbox), `performance-analyst`, `code-reviewer`, `refactor-cleaner`, `build-error-resolver`.
- **Não usar sem ADR:** minimal-api, openapi, scalar, ef-core, aspire, docker, authentication, caching, httpclient-factory, resilience, messaging, serilog, opentelemetry, scaffold.

### Overrides sobre os padrões do dotnet-claude-kit (prevalecem)
| # | Regra do CitySky |
|---|---|
| O1 | **C# 9** em `unity/**` e em `com.citysky.core` (código compartilhado): sem file-scoped namespace, primary constructor, collection expression, `required`, record struct, `field`, extension members. C# 14 **só** em `dotnet/src/CitySky.{PlayerApi,CodeAnalysis,CodeHost,Sandbox.Worker}` e testes. |
| O2 | Testes com **NUnit** (`Assert.That`), não xUnit v3. Sem WebApplicationFactory/Testcontainers (ADR-0016). |
| O3 | Domain usa só `SimulationClock`; nunca `DateTime.Now`/`TimeProvider`. Code Host usa `TimeProvider`. |
| O4 | `CancellationToken` no Code Host/Worker; **não** no Domain (ticks síncronos). |
| O5 | Arquitetura já decidida (ADR-0003/0004). Não rodar `architecture-advisor` para rediscuti-la. Mudanças só via ADR. |
| O6 | Código compartilhado com **zero** dependências externas. Pacotes Unity só via UPM com ficha de dependência. |
| O7 | `sealed` por padrão, exceto tipos-base da Player API feitos para herança pedagógica. |
| O8 | Sem LINQ/alocações em caminhos quentes (por tick ou por frame). |

## Regras invioláveis
- Domain/Application/Contracts/Education **sem** `UnityEngine`/`Unity.*` (`noEngineReferences: true`).
- Presentation **não** referencia Domain; lê read models e envia comandos.
- Toda mutação de estado é um **comando** validado. O código do jogador **nunca** tem poder que a UI não tem.
- Simulação **determinística**: tick fixo, RNG semeado, inteiros/ponto fixo no estado autoritativo, dinheiro em centavos (`long`).
- LOD de simulação **não depende da câmera** (ADR-0009).
- Nada de lógica de domínio em `MonoBehaviour`, nem Singleton/`static` mutável sem ADR, nem dependência sem justificativa.
- Ampliar a allowlist de APIs do jogador exige `security-auditor` + caso no corpus `CitySky.Security.Tests`.
- Strings visíveis ao jogador sempre via tabelas de localização (pt-BR primeiro).
- **Antes de implementar, perguntar: "pertence ao MVP/fase atual?"** Se não pertence: documentar e colocar no roadmap, sem implementar.
- Ao mudar comportamento: atualizar o doc do sistema (e `docs/PLAYER_API.md` se a API mudou).

## Layout (planejado; criado na Fase 1)
```
unity/CitySky/                    Projeto Unity (Assets/Code/{Bootstrap,Infrastructure,Presentation}, Assets/ECS, Assets/UI)
unity/CitySky/Packages/com.citysky.core/Runtime/{Domain,Application,Contracts,Education}   ← FONTE do domínio
dotnet/CitySky.slnx               Solução .NET: linka a fonte do domínio + PlayerApi, CodeAnalysis, CodeHost, Sandbox.Worker, tests/
content/                          Catálogos (JSON) e desafios (content/challenges/<id>/)
editor-web/                       Monaco host (TypeScript + Vite)
docs/                             Documentação e ADRs
```

## Comandos (a partir da Fase 1)
```bash
dotnet build dotnet/CitySky.slnx
dotnet test  dotnet/CitySky.slnx
# Unity (batch):
Unity.exe -batchmode -projectPath unity/CitySky -runTests -testPlatform EditMode -testResults TestResults/editmode.xml
```

## Git
Conventional Commits citando o ID da tarefa (`feat(roads): split segment on crossing (F3-04)`), commits atômicos, sem force-push na `main`. Git LFS para binários. Nunca mover asset sem o `.meta`.
