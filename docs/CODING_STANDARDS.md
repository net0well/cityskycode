# Padrões de Código

> **Status:** v1.0 · **Data:** 2026-10-08 · **ADRs:** 0004, 0015, 0016
> Dois perfis de linguagem: **Perfil U** (Unity + código compartilhado, C# 9) e **Perfil N** (.NET 10, C# 14). As convenções do **dotnet-claude-kit** (skill `modern-csharp`, regra `coding-style`) valem **integralmente no Perfil N** e **parcialmente no Perfil U** (ver overrides no `CLAUDE.md`).

---

## 1. Onde vale cada perfil
| Perfil | Pastas | Linguagem |
|---|---|---|
| **U** | `unity/**`, `unity/CitySky/Packages/com.citysky.core/**` (Domain, Application, Contracts, Education) | **C# 9**, .NET Standard 2.1 |
| **N** | `dotnet/src/CitySky.{PlayerApi,CodeAnalysis,CodeHost,Sandbox.Worker}`, `dotnet/tests/**`, `dotnet/benchmarks/**` | **C# 14**, .NET 10 |

## 2. Regras comuns (U e N)
- **Nomes:** PascalCase para tipos e membros públicos; `_camelCase` para campos privados; `camelCase` para locais e parâmetros; interfaces com `I`; nomes descritivos em **inglês** (ADR-0014). Comentários e XML docs em **pt-BR** quando voltados ao jogador (Player API), e em inglês ou pt-BR no código interno (consistência por arquivo).
- **Um tipo por arquivo**, nome do arquivo = nome do tipo, **organizados em pastas por área**. Nunca vários tipos (classe/record/struct/enum/interface) no mesmo arquivo — inclusive DTOs e enums pequenos. (Exceção: tipos *private nested* de implementação, que ficam com o dono.) O dono do produto preza muito essa organização.
- **Aplicar padrões de projeto** reconhecidos onde agregam valor, nomeando-os (Strategy, Factory, Adapter/Gateway, Command, Observer, Object Pool, etc.). **Sem sobre-engenharia:** não introduzir padrão sem justificativa (regra inviolável do `CLAUDE.md`).
- **`sealed` por padrão** (exceto tipos-base da Player API desenhados para herança).
- **Imutabilidade** por padrão: `readonly` em campos, structs `readonly`, coleções expostas como `IReadOnlyList<T>`.
- **Nullable reference types habilitados** (Perfil U via `csc.rsp` `-nullable:enable`; Perfil N via `Directory.Build.props`).
- Sem `static` mutável, sem Singleton, sem service locator (§76).
- Erros esperados → `Result`/`CommandResult`. Exceções → bugs e invariantes violadas.
- Sem números mágicos em regras: constantes nomeadas ou catálogo de dados.
- Métodos curtos (≤ ~40 linhas), aninhamento ≤ 3.
- Testes seguem AAA (`TESTING.md`).

## 3. Perfil U: C# 9 (Unity e compartilhado)
**Permitido:** pattern matching do C# 9 (`is not null`, relacionais, `and/or`), `new()` target-typed, `static` lambdas, `init` e `record class` (com shim `IsExternalInit` no pacote; usar com parcimônia), `switch` expressions, tuplas, `Span<T>`/`ReadOnlySpan<T>`, interpolação.

**Proibido (C# 10+):** file-scoped namespaces · `global using` · record structs · primary constructors · collection expressions (`[1, 2]`) · `required` · raw string literals · `field` · extension members · lambdas com atributos/tipo natural · `params` collections.

**Domínio (além disso):** sem `UnityEngine`/`Unity.*`; sem `DateTime.Now`, `Stopwatch`, `Guid.NewGuid`, `Random` (usar `SimulationClock` e `DeterministicRandom`); sem `float`/`double` em estado autoritativo; sem `Dictionary`/`HashSet` iterados em ordem não determinística; sem I/O; sem threads.

**Unity (Presentation/Infrastructure):**
- `MonoBehaviour`s **finos**: recebem input, chamam Application e exibem read models. Lógica testável em classes C# comuns.
- `[SerializeField] private` em vez de campos públicos. Sem `GameObject.Find`, `FindObjectOfType`, `SendMessage`, `Resources.Load` (usar referências e, na Fase 10, Addressables).
- Cuidado com `?.` e `??` em `UnityEngine.Object` (use `== null` explícito).
- Desinscrever eventos em `OnDisable`/`OnDestroy`.
- **Zero alocação por frame** em `Update`. Cache de componentes.
- Burst: só tipos *blittable*, sem exceções no caminho quente, sem gerenciados, `[BurstCompile]` com `FloatMode` padrão (resultados que vão ao domínio devem ser inteiros).
- Analyzers: `Microsoft.Unity.Analyzers`.

## 4. Perfil N: C# 14 (.NET 10)
Seguir o skill **`modern-csharp`** do dotnet-claude-kit: file-scoped namespaces, primary constructors (para DI), collection expressions, records para DTOs, pattern matching, `required`, `field` quando útil, `TimeProvider` (nunca `DateTime.Now`), `CancellationToken` propagado em todo I/O e operação longa, `IOptions<T>` para configuração, `ILogger<T>` com *message templates*, DI do Generic Host.
- `Directory.Build.props`: `Nullable=enable`, `TreatWarningsAsErrors=true` (src), `AnalysisLevel=latest-recommended`, `GenerateDocumentationFile=true` (PlayerApi com `CS1591` como erro).
- `Directory.Packages.props`: Central Package Management (versões em um lugar só).
- `.slnx` como formato de solução.

## 5. Formatação
- `.editorconfig` na raiz (regras compartilhadas U/N, com seção específica para `unity/**` que desliga sugestões de C# 10+).
- `dotnet format` (Perfil N) e formatação da IDE (Perfil U). O hook de formatação do dotnet-claude-kit roda após edições no Perfil N.

## 6. Git e PRs
- **Conventional Commits** (`feat(energy): rotate load shedding hourly`), commits atômicos.
- PR pequeno, com: fase/MVP, docs atualizados, resultado do `/verify`, prints/vídeo para mudanças visuais.
- Revisão: `/code-review` (agente) + humano. `security-auditor` para o escopo de execução de código.

## 7. Unity: projeto e versionamento
- *Asset Serialization: Force Text*; *Version Control: Visible Meta Files*; **UnityYAMLMerge** configurado como merge tool.
- `.gitignore` padrão Unity (`Library/`, `Temp/`, `Obj/`, `Build/`, `Logs/`, `UserSettings/`), mais `dotnet/**/bin`, `dotnet/**/obj`, `editor-web/node_modules`, `TestResults/`.
- `.gitattributes` com LFS para binários (lista em `ASSET_PIPELINE.md` §4) e `*.cs text eol=lf`.
- Cenas pequenas e compostas (cena de bootstrap + cenas aditivas) para reduzir conflitos de merge.
