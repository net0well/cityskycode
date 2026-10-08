# Guia de Desenvolvimento para IAs

> **Status:** Aceito (v1.0) · **Data:** 2026-10-08 · **ADR:** [ADR-0015](ADR/0015-ai-tooling-dotnet-claude-kit.md)
> Vale para **qualquer agente de IA** (Claude Code, Copilot, Codex etc.) e para humanos. O resumo operacional está no [`/CLAUDE.md`](../CLAUDE.md).

---

## 1. Ferramental obrigatório: **dotnet-claude-kit**

Todo trabalho de **.NET/C#** neste projeto (domínio, Application, Code Host, Sandbox Worker, analyzers, testes, CI) **deve** usar o plugin **dotnet-claude-kit** (codewithmukesh, MIT, v0.11+), com seus skills, agentes, comandos e o servidor MCP **CWM.RoslynNavigator**.

### 1.1 Instalação (uma vez por máquina)
```bash
dotnet tool install -g CWM.RoslynNavigator
```
Dentro do Claude Code:
```
/plugin marketplace add codewithmukesh/dotnet-claude-kit
/plugin install dotnet-claude-kit
```
Atualizar: `/plugin marketplace update dotnet-claude-kit` → `/reload-plugins`, e `dotnet tool update -g CWM.RoslynNavigator`.

### 1.2 Por que usar
- **Roslyn MCP primeiro:** consultar o compilador (`find_symbol`, `find_references`, `get_file_outline`, `get_public_api`, `detect_circular_dependencies`...) custa muito menos tokens do que ler arquivos inteiros, e é mais preciso.
- Fluxos de qualidade prontos (`/verify`, `/code-review`, `/arch-check`, `/security-scan`) aplicados de forma consistente por qualquer agente.
- Conhecimento de .NET 10 / C# 14 atualizado para o Code Host.

### 1.3 Onde o MCP funciona
O RoslynNavigator descobre `.sln`/`.slnx`. Use a solução **`dotnet/CitySky.slnx`**: ela compila a mesma fonte do domínio que vive no pacote Unity (shared source, ver `ARCHITECTURE.md` §4.3), então **todo o código compartilhado é navegável pelo MCP**. Para código exclusivo da Unity (Presentation, Infrastructure.Unity), a `.sln` gerada pela IDE da Unity pode ser usada em caráter experimental.

---

## 2. Mapa: tarefa → recurso do dotnet-claude-kit

### 2.1 Comandos (workflows)
| Tarefa | Comando | Observação CitySky |
|---|---|---|
| Especificar funcionalidade | `/spec` | Gera `docs/specs/NNN-*.md`. Toda funcionalidade com 3+ passos começa aqui |
| Planejar implementação | `/plan` | Deve citar docs e ADRs afetados e responder "pertence ao MVP?" |
| Desenvolver regra de domínio | `/tdd` | Vermelho → verde → refatora, com **NUnit** (ver overrides) |
| Verificar antes do PR | `/verify` | 7 fases (build, analyzers, antipatterns, testes, segurança, formatação, diff) |
| Revisar código | `/code-review` | Prioriza blast radius: Domain, segurança do sandbox, determinismo |
| Conformidade arquitetural | `/arch-check` | Domain/Application/Contracts sem engine; Presentation não referencia Domain |
| Auditoria de segurança | `/security-scan` | Obrigatório em PRs que tocam Code Host, Worker, Player API ou IPC |
| Saúde do projeto | `/health-check` | Ao fim de cada fase |
| Corrigir build quebrado | `/build-fix` | Loop limitado |
| Limpeza | `/de-sloppify` | Ver overrides (CancellationToken só no Code Host) |
| Dependências / CVEs | `/outdated` | Mensal e antes de releases |
| Upgrade de .NET / pacotes | `/migrate` | Só a parte NuGet/.NET (não há EF Core) |
| Gerar CLAUDE.md | `/dotnet-init` | **Usar a opção "merge"**: o `CLAUDE.md` atual é a fonte de verdade |

### 2.2 Skills de conhecimento
| Aplicáveis | Uso no CitySky |
|---|---|
| `modern-csharp` | **Integral** no Code Host/Worker/PlayerApi (.NET 10, C# 14). **Subconjunto C# 9** no código compartilhado/Unity |
| `project-structure` | `dotnet/`: `.slnx`, `Directory.Build.props`, `Directory.Packages.props` (CPM), `global.json` |
| `testing`, `tdd` | Padrão AAA, testes de comportamento. Framework **NUnit** (override) |
| `clean-architecture`, `ddd` | Camadas, value objects, **IDs fortemente tipados**, eventos de domínio. Adaptar *aggregates* ao modelo orientado a dados (stores) |
| `error-handling` | Result pattern no Domain/Application (sem ProblemDetails, que é HTTP) |
| `dependency-injection`, `configuration`, `logging` | Code Host (Generic Host) |
| `ci-cd` | GitHub Actions |
| `code-review`, `verify`, `arch-check`, `security-scan`, `health-check`, `de-sloppify` | Qualidade contínua |
| `architecture-advisor` | **Apenas** para novos contextos delimitados. A arquitetura principal já está decidida (ADR-0004), não rediscutir |

| **Não aplicáveis** (exigem ADR para uso) | Motivo |
|---|---|
| `minimal-api`, `openapi`, `scalar`, `api-versioning`, `authentication` | Não há API HTTP |
| `ef-core`, `migrate` (parte EF) | Não há banco de dados |
| `aspire`, `docker`, `container-publish` | App desktop (exceção: imagens de CI) |
| `caching`, `httpclient-factory`, `resilience`, `messaging` | Sem serviços de rede |
| `serilog`, `opentelemetry` | Adiados (MEL basta). Reavaliar em profiling avançado |
| `scaffold`, `vertical-slice` | Templates de API web; o scaffold de funcionalidades usa os modelos de `docs/` |

### 2.3 Agentes especialistas
| Agente | Quando |
|---|---|
| `dotnet-architect` | Estrutura da solução `dotnet/`, limites entre projetos |
| `test-engineer` | Estratégia e infraestrutura de testes (NUnit, testes de determinismo, golden files) |
| `security-auditor` | **Sandbox, allowlist, IPC, Player API**: revisão obrigatória |
| `performance-analyst` | Laços quentes do domínio, alocações, BenchmarkDotNet |
| `code-reviewer` | Revisão de PR (somente leitura) |
| `refactor-cleaner` | Código morto, limpeza (worktree isolado) |
| `build-error-resolver` | Builds quebrados após refatorações grandes |
| ~~`api-designer`, `ef-core-specialist`~~ | Não aplicáveis |
| `devops-engineer` | Somente CI (GitHub Actions); Docker não se aplica ao produto |

### 2.4 Ferramentas MCP mais úteis
`find_symbol`, `find_references`, `find_callers`, `get_file_outline`, `get_symbol_source`, `get_type_hierarchy`, **`get_public_api`** (revisar mudanças na superfície da **Player API**), `get_project_graph`, **`detect_circular_dependencies`**, `detect_antipatterns`, `get_diagnostics`, `find_dead_code`, `get_test_coverage_map`.

---

## 3. Overrides: onde o CitySky diverge dos padrões do dotnet-claude-kit

O dotnet-claude-kit mira **aplicações .NET 10 / ASP.NET Core**. Este projeto tem uma parte Unity (C# 9). As regras abaixo **prevalecem** sobre as do plugin:

| # | Regra do plugin | Override no CitySky | Motivo |
|---|---|---|---|
| O1 | C# 14: file-scoped namespaces, primary constructors, collection expressions, `required`, `field`, extension members, record structs | **Proibidos** em `unity/**` e no código compartilhado (`com.citysky.core`). Permitidos em `dotnet/src/CitySky.{PlayerApi,CodeAnalysis,CodeHost,Sandbox.Worker}` e nos testes | Unity 6 = C# 9 (`LangVersion 9` fixado no csproj de shared source) |
| O2 | Testes com xUnit v3 + WebApplicationFactory + Testcontainers | **NUnit** (UTF na Unity; NUnit 4 no .NET), modelo `Assert.That`. Sem WAF/Testcontainers | ADR-0016: um só vocabulário de testes (Unity, .NET e código do jogador) |
| O3 | `TimeProvider` em vez de `DateTime.Now` | Domain usa **somente** `SimulationClock` (ticks). Code Host usa `TimeProvider` | Determinismo |
| O4 | Propagar `CancellationToken` | Code Host/Worker: sim. Domain: **não** (ticks síncronos e determinísticos) | Simplicidade e determinismo |
| O5 | `architecture`: perguntar antes de recomendar | Arquitetura já decidida (ADRs 0003/0004). Mudanças só via ADR | Evitar rediscussão |
| O6 | `packages`: sempre a última versão estável | Mantido para `dotnet/`. Código compartilhado: **zero** dependências externas. Unity: só pacotes via UPM com ficha de dependência | Portabilidade e §65 |
| O7 | `sealed` por padrão | Mantido. Exceção: tipos-base da Player API desenhados para herança pedagógica | POO é conteúdo |
| O8 | Records para DTOs | Em C# 9 (shared): `record class` exige o shim `IsExternalInit`; preferir classes imutáveis simples nos Contracts (serialização Newtonsoft) | Compatibilidade Unity |
| O9 | LINQ livre | Proibido em caminhos quentes (por tick/frame) do Domain e da Presentation | GC do Mono/Boehm não é geracional |

---

## 4. Processo obrigatório (§76 do briefing, expandido)

1. **Ler** [`/README.md`](../README.md) e [`docs/README.md`](README.md).
1a. **Ler o plano da fase atual** ([`docs/phases/`](phases/README.md)) e localizar a tarefa pelo ID (`F3-04`). Tarefa fora do plano → aplicar a pergunta do passo 8 antes de qualquer coisa.
2. **Ler** [`ARCHITECTURE.md`](ARCHITECTURE.md).
3. **Ler** [`TECH_STACK.md`](TECH_STACK.md).
4. **Ler** o documento do sistema que será alterado (ex.: `ENERGY_SYSTEM.md`).
5. **Ler** os ADRs relacionados (listados no cabeçalho do documento do sistema).
6. **Inspecionar o código existente**, com **Roslyn MCP primeiro** (`find_symbol`, `get_file_outline`) e leitura de arquivos depois.
7. **Identificar dependências** (`find_references`, `get_project_graph`).
8. **Propor a alteração** (`/spec` e `/plan`). Responder explicitamente: **"Isso pertence ao MVP/fase atual?"** Se não pertence: documentar, colocar no roadmap e **não implementar** (§75).
9. **Implementar** (`/tdd` para regras de domínio).
10. **Criar testes** (unidade, determinismo, golden files, segurança, conforme o caso).
11. **Executar testes** (`dotnet test` em `dotnet/`; Unity Test Runner em modo batch quando tocar código Unity).
12. **Atualizar a documentação** (documento do sistema, `PLAYER_API.md` se a API mudou, ADR se a decisão é relevante).
13. **Verificar regressões** (`/verify`, `/arch-check`; `/security-scan` se tocou execução de código).

### Definition of Done de qualquer PR
- [ ] Pertence à fase atual (ou é correção).
- [ ] Testes novos/alterados passando; determinismo intacto.
- [ ] `/verify` verde; `/arch-check` sem violações.
- [ ] Documentação e (se aplicável) ADR atualizados.
- [ ] Nenhuma string visível ao jogador fixa no código.
- [ ] Sem alocação por frame nova em caminhos quentes (ou justificada).

---

## 5. A IA **nunca** deve (§76 + regras do projeto)
- Criar arquitetura paralela ou duplicar sistemas existentes.
- Adicionar dependências sem a ficha de dependência (`TECH_STACK.md` §6) e sem ADR quando relevante.
- Colocar lógica de domínio em `MonoBehaviour`.
- Referenciar `UnityEngine`/`Unity.*` no Domain, Application, Contracts ou Education.
- Ignorar ECS quando a escala exigir **ou** transformar tudo em ECS sem necessidade (ADR-0003).
- Criar Singleton global, `static` mutável ou service locator sem ADR.
- Adicionar abstrações "para o futuro" sem um problema concreto.
- Usar `DateTime.Now`, `Random` sem seed, `float`/`double` em estado autoritativo do domínio, ou `float` para dinheiro.
- Dar ao código do jogador qualquer poder que a UI não tenha.
- Ampliar a allowlist da Player API/BCL sem `security-auditor` + teste no corpus de segurança.
- Executar, durante o desenvolvimento, código vindo de saves ou de terceiros fora do sandbox.
- Editar arquivos `.meta`, cenas ou prefabs da Unity "na mão" quando a operação exige o Editor. Nunca apagar `.meta` órfãos sem verificar o GUID.
- Usar C# 10+ em código compilado pela Unity.

---

## 6. Armadilhas específicas para IAs em projetos Unity
- **`.meta` e GUIDs:** mover ou renomear arquivos de asset sem o `.meta` quebra referências. Mover sempre o par.
- **asmdefs:** criar um `.cs` numa pasta sem asmdef coloca o código no `Assembly-CSharp`, o que quebra as regras de dependência.
- **Recompilação:** mudanças em C# exigem que a Unity recompile. Testes de Unity rodam por CLI: `Unity.exe -batchmode -projectPath unity/CitySky -runTests -testPlatform EditMode -testResults results.xml`.
- **`UnityEngine.Object` e null:** `?.` e `??` ignoram o operador `==` sobrecarregado. Usar comparação explícita com `null` em tipos da Unity.
- **Serialização Unity:** campos `[SerializeField] private`. Propriedades não são serializadas.
- **Burst:** sem tipos gerenciados, sem exceções em caminho quente, sem `static` mutável.
- **IL2CPP stripping:** tipos usados só via reflexão (ex.: desserialização Newtonsoft) precisam de `link.xml`/`[Preserve]`.

---

## 7. Como propor uma decisão (ADR)
1. Copiar `docs/ADR/TEMPLATE.md` → `docs/ADR/NNNN-titulo-curto.md`.
2. Status `Proposto`, com contexto, alternativas (no mínimo 2), consequências e gatilhos de revisão.
3. Linkar o ADR nos documentos afetados.
4. Só implementar após o status `Aceito`.

## 8. Checklists por tipo de mudança
| Mudança | Itens extras |
|---|---|
| Regra de domínio | teste de unidade + teste de determinismo + explicabilidade (causas registradas) + doc do sistema |
| Novo sistema de simulação | período em ticks + orçamento de ms + read model + overlay/inspector + doc + entrada em `CITY_SIMULATION.md` |
| Player API | XML docs pt-BR + allowlist + teste de segurança + revisão `get_public_api` + `PLAYER_API.md` + impacto pedagógico |
| Novo desafio | `challenge.json` + solução de referência passando no CI + soluções erradas com feedback esperado + dicas 1–3 + explicação |
| Execução/sandbox | `security-auditor` + `/security-scan` + corpus ampliado |
| Performance sensível | benchmark antes e depois (BenchmarkDotNet/Unity Performance Testing) + `performance-analyst` |
| Asset 3D | LODs + orçamento de triângulos + linter de assets |

## 9. Convenções de comunicação dos agentes
- Commits: **Conventional Commits** citando o ID da tarefa do plano da fase (`feat(energy): rotate load shedding hourly (F7-03)`, `fix(codehost): ...`), atômicos. Nunca dar force-push na `main`.
- Branches: `feat/<area>-<resumo>`, `fix/...`, `spike/...`, `docs/...`.
- PRs: template com "Pertence ao MVP/fase?", docs atualizados, resultado do `/verify`.
- No meio de uma sessão longa: `/checkpoint` (commit + nota de passagem). Ao terminar: `/wrap-up` (ritual de fim de sessão do dotnet-claude-kit; a próxima sessão carrega o contexto).
- `/tdd` do plugin assume xUnit + Testcontainers: ao invocá-lo, lembrar o override O2 (NUnit, sem Testcontainers).
