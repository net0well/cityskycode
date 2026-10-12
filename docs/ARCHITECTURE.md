# Arquitetura

> **Status:** Aceito (v1.0) · **Data:** 2026-10-08 · **ADRs:** 0003, 0004, 0005, 0008, 0015, 0016
> Diagrama geral consolidado: [`PROJECT_ARCHITECTURE.md`](PROJECT_ARCHITECTURE.md).

---

## 1. Princípios arquiteturais

1. **O domínio é a verdade.** O estado da cidade vive no Domain (C# puro). Unity, ECS e UI são **projeções** desse estado.
2. **Dependências apontam para dentro.** Presentation → Application → Domain. Infrastructure implementa portas definidas em Application/Domain.
3. **O domínio não conhece a engine.** Sem `UnityEngine`, `Unity.Mathematics`, `GameObject`, `MonoBehaviour`, `Transform`, `Color`, `Vector3` (§30). Imposto por `noEngineReferences: true` nos asmdefs, por testes de arquitetura e pelo `/arch-check`.
4. **Toda mutação é um comando.** UI, código do jogador, eventos e ferramentas mudam a cidade **somente** por comandos validados pela Application. O código do jogador não tem poderes extras.
5. **Simulação determinística.** Tick fixo, ordem fixa de sistemas, RNG semeado, aritmética inteira no estado autoritativo. Mesmo estado inicial + mesmos comandos = mesmo estado final, em Mono, IL2CPP ou .NET 10.
6. **Código do jogador fora do processo do jogo.** Compilação e execução acontecem no Code Host / Sandbox Worker (.NET 10).
7. **Orientado a dados por dentro, orientado a objetos por fora.** O domínio armazena entidades em *stores* (arrays + IDs) para escala. A **Player API** oferece uma fachada OO amigável. Essa dualidade é explicada ao aluno mais tarde, no arco de "data-oriented design".
8. **Explicabilidade é requisito.** Todo indicador registra suas causas (§14: "o jogo deve mostrar causas").
9. **Simples primeiro.** Nada de ECS, Addressables, DI container ou MessagePack até que um problema medido justifique (ADR).

---

## 2. Camadas

```
┌──────────────────────────────────────────────────────────────────────┐
│ PRESENTATION (Unity)                                                 │
│ Views (MonoBehaviour finos) · Câmera · Input · UI Toolkit · URP      │
│ Overlays (shaders) · Agentes (GameObjects → ECS) · Code Editor Host  │
└───────────────┬──────────────────────────────────────────────────────┘
                │ Commands / Queries / ReadModels / Events (somente)
┌───────────────▼──────────────────────────────────────────────────────┐
│ APPLICATION (C# puro, netstandard2.1, C# 9)                          │
│ Command handlers · Query handlers · ReadModel builders · Portas      │
│ SimulationRunner (orquestra ticks) · Políticas de jogo · Unlocks     │
└───────────────┬──────────────────────────────────────────────────────┘
                │
┌───────────────▼──────────────────────────────────────────────────────┐
│ DOMAIN (C# puro, netstandard2.1, C# 9)                               │
│ CityState (stores) · Simulation systems · Regras · Eventos de domínio │
│ Value objects (Money, Power, Fixed, IDs) · RNG determinístico        │
└───────────────▲──────────────────────────────────────────────────────┘
                │ implementa portas
┌───────────────┴──────────────────────────────────────────────────────┐
│ INFRASTRUCTURE                                                       │
│ Unity: Saves, Settings, CodeHostClient (IPC), Logging, Pathfinder    │
│        acelerado (Burst), Localization adapter                        │
│ .NET : Code Host, Sandbox Worker, Roslyn, Content loader             │
└──────────────────────────────────────────────────────────────────────┘
```

### Responsabilidades

| Camada | Pode | Não pode |
|---|---|---|
| Domain | Regras, estado, sistemas de simulação, eventos de domínio | I/O, tempo de parede (`DateTime.Now`), `Random` não semeado, engine, threads próprias, logging direto |
| Application | Orquestrar casos de uso, validar comandos (custo, permissões, desbloqueios), montar read models, definir portas | Regras de negócio de baixo nível (pertencem ao Domain), engine |
| Infrastructure | Implementar portas (save, IPC, logging, pathfinder acelerado) | Regras de negócio |
| Presentation | Renderizar, capturar input, exibir read models, enviar comandos | Mudar estado diretamente, lógica de domínio em `MonoBehaviour` (§76) |

---

## 3. Topologia de processos

```
┌─────────────────────── CitySky.exe (Unity Player, IL2CPP) ─────────────────────┐
│  Presentation · Application · Domain (autoritativo) · Infrastructure.Unity     │
│                                                                                 │
│   CodeEditorHost ── JS bridge ──► [WebView process: Chromium + Monaco]          │
│   CodeHostClient ── IPC (named pipe; fallback TCP loopback+token) ──┐           │
└─────────────────────────────────────────────────────────────────────┼───────────┘
                                                                      │
┌─────────────────────── CitySky.CodeHost.exe (.NET 10) ──────────────▼───────────┐
│  Roslyn Workspace (serviços de linguagem) · Pipeline: Parse→Analyze→Validate→   │
│  Compile · Gerência de Workers · Validação de desafios · Mapeamento de erros    │
└───────────────────────────────┬─────────────────────────────────────────────────┘
                                │ stdin/stdout (pipes anônimos) + Job Object
┌───────────────────────────────▼─────────────────────────────────────────────────┐
│  CitySky.Sandbox.Worker.exe (.NET 10) — processo restrito, descartável          │
│  Assembly do jogador (ALC coletável) · Player API runtime · Domain headless     │
│  Orçamentos: instruções, tempo, memória, recursão, saída                        │
└─────────────────────────────────────────────────────────────────────────────────┘
```

- **Unity ↔ Code Host:** protocolo de mensagens versionado (`CitySky.Contracts`), quadros com prefixo de tamanho e JSON (Newtonsoft). O Code Host é iniciado pelo jogo, que o reinicia se ele cair.
- **Code Host ↔ Worker:** o Worker é tratado como **hostil**. Tudo o que ele devolve é validado: esquema, tamanho e tipos permitidos.
- **Webview ↔ Unity:** `postMessage` / JS bridge. O Monaco **não** fala diretamente com o Code Host; a Unity faz o papel de *broker*, o que deixa um canal só e permite navegar a partir de erros no console.

Detalhes: `CODE_EXECUTION.md`, `CODE_EXECUTION_SECURITY.md`, `CODE_EDITOR.md`.

---

## 4. Assemblies e regras de dependência

### 4.1 Lado Unity (`unity/CitySky/`)

| Assembly (asmdef) | Local | `noEngineReferences` | Referencia |
|---|---|---|---|
| `CitySky.Domain` | `Packages/com.citysky.core/Runtime/Domain` | ✅ | — |
| `CitySky.Application` | `Packages/com.citysky.core/Runtime/Application` | ✅ | Domain |
| `CitySky.Contracts` | `Packages/com.citysky.core/Runtime/Contracts` | ✅ | — (DTOs do IPC) |
| `CitySky.Education` | `Packages/com.citysky.core/Runtime/Education` | ✅ | Contracts |
| `CitySky.Infrastructure` | `Assets/Code/Infrastructure` | ❌ | Application, Domain, Contracts |
| `CitySky.Presentation` | `Assets/Code/Presentation` | ❌ | Application, Contracts (somente leitura de Domain via read models) |
| `CitySky.Agents.GameObjects` | `Assets/Code/Agents/GameObjects` | ❌ | Application |
| `CitySky.Agents.Ecs` (Fase 11) | `Assets/ECS/` | ❌ | Application, Entities |
| `CitySky.Bootstrap` | `Assets/Code/Bootstrap` | ❌ | **todos** (composition root) |
| `CitySky.*.Tests` | `Assets/Tests/...` | — | alvo + NUnit |

> **Presentation não referencia Domain diretamente.** Ela lê *read models* (Application) e envia comandos. Isso impede que um `MonoBehaviour` altere o estado do domínio.

### 4.2 Lado .NET (`dotnet/`, solução `CitySky.slnx`)

| Projeto | Target | Conteúdo |
|---|---|---|
| `CitySky.Domain` | `netstandard2.1`, `LangVersion 9` | **Linka** a fonte de `unity/CitySky/Packages/com.citysky.core/Runtime/Domain/**/*.cs` |
| `CitySky.Application` | idem | Linka `.../Runtime/Application/**` |
| `CitySky.Contracts` | idem | Linka `.../Runtime/Contracts/**` |
| `CitySky.Education` | idem | Linka `.../Runtime/Education/**` |
| `CitySky.PlayerApi` | `net10.0` | API pública do jogador (com XML docs pt-BR) + implementações de gateway |
| `CitySky.CodeAnalysis` | `net10.0` | Analyzers de allowlist, analyzers pedagógicos, verificador de IL |
| `CitySky.CodeHost` | `net10.0` (exe) | Servidor IPC, Roslyn workspace, pipeline, gerência de workers |
| `CitySky.Sandbox.Runtime` | `net10.0` | Runtime de orçamento (`__Budget`, injetado pelo rewriter) + executor em ALC coletável + canal Host↔Worker. Referenciado pelo Worker (execução) e pelo Code Host (referências de compilação). *Acrescentado na Fase 1 (F1-72).* |
| `CitySky.Sandbox.Worker` | `net10.0` (exe) | Execução isolada (lê pedidos do Host por stdin, roda via Sandbox.Runtime) |
| `tests/*` | `net10.0` | NUnit 4: Domain, Application, Contracts (golden JSON), CodeAnalysis, CodeHost, **Security**, **Content**, **Architecture** |

### 4.3 Fonte única do domínio ("shared source")

```xml
<!-- dotnet/src/CitySky.Domain/CitySky.Domain.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.1</TargetFramework>
    <LangVersion>9.0</LangVersion>
    <Nullable>enable</Nullable>
    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
  </PropertyGroup>
  <ItemGroup>
    <Compile Include="../../../unity/CitySky/Packages/com.citysky.core/Runtime/Domain/**/*.cs" />
  </ItemGroup>
</Project>
```

- **A fonte vive no pacote Unity.** Agentes de IA editam ali.
- O mesmo código compila na Unity (Mono/IL2CPP) e no .NET 10 (Code Host, testes rápidos, Roslyn MCP).
- `LangVersion 9` garante que nada de C# 10+ entre no código compartilhado.
- `Nullable` habilitado nos dois lados (na Unity via `csc.rsp` com `-nullable:enable` no diretório do asmdef).

---

## 5. Fluxos principais

### 5.1 Ação do jogador via UI
```
Input → Tool (Presentation) → BuildBuildingCommand → Application.CommandBus
     → valida (custo, lote livre, desbloqueio) → CommandQueue (aplica no próximo tick)
     → Domain muda estado → DomainEvents → ReadModels atualizados → Views reagem
```

### 5.2 Código do jogador (modo Script)
```
Monaco → (bridge) → Unity CodeEditorHost → CodeHostClient → [IPC] → Code Host
  Parse → Analyze (allowlist + pedagógico) → Compile (emit + PDB) → Worker
  Worker executa Program.cs contra a Player API (gateway "Live"):
     consultas → IPC de volta à Unity (snapshot congelado no tick T)
     mutações  → CommandBuffer
  ← resultado: comandos + saída de console + diagnósticos + métricas
Unity: Application valida cada comando (mesmas regras da UI) → aplica no tick T+1
Console/ERROR_SYSTEM exibem feedback pedagógico
```
No MVP a simulação **pausa** durante a execução de scripts, que têm orçamento de até 2 s. Assim o snapshot é consistente sem versionamento.

### 5.3 Validação de desafio (Laboratório)
```
Code Host → Worker com gateway "Headless": Domain roda DENTRO do Worker
  cenário (seed + estado inicial) → código do aluno → N ticks → asserts/testes ocultos
  → ValidationReport (níveis: sintaxe, compilação, runtime, comportamento, arquitetura)
```
O mesmo `CitySky.Domain` (fonte única) garante que o laboratório se comporta como a cidade real.

### 5.4 Comportamentos do jogador chamados pela simulação (pós-MVP)
```
Tick N: sistema precisa de decisão do jogador (ex.: IGarbageRoutingStrategy)
  → monta lote de entradas → IPC → Worker avalia (orçamento) → resultados
Tick N+1: resultados aplicados como comandos/parâmetros
Timeout/erro → fallback para regra padrão + aviso pedagógico ("seu método está lento")
```

---

## 6. Modelo de threads

| Fase | Modelo |
|---|---|
| MVP | Domínio no **main thread** da Unity, com orçamento por frame e *catch-up* limitado (ver `SIMULATION_TICK.md`). IPC em thread de I/O dedicada, com fila para o main thread. |
| Fase 18 (escala) | Domínio em **thread de simulação dedicada**. Read models com *double buffering*: a apresentação lê o buffer N-1 enquanto o tick N roda. Pathfinding e campos em Jobs/Burst através de portas. |

Regras: o domínio nunca cria threads por conta própria. O paralelismo entra por portas (`IParallelExecutor`, `IPathfinder`) injetadas no composition root, com implementações deterministicamente equivalentes.

---

## 7. Composition root e DI

- **Pure DI**: um único `CitySky.Bootstrap` monta o grafo de objetos na inicialização (um `MonoBehaviour` de entrada numa cena *Bootstrap*).
- Sem singletons globais, sem `static` mutável, sem service locator (§76). Exceções exigem ADR.
- O **Code Host** usa `Microsoft.Extensions.Hosting` e DI padrão do .NET (skills `dependency-injection` e `configuration` do dotnet-claude-kit).

## 8. Eventos

- **Eventos de domínio** são acumulados num buffer durante o tick e publicados **depois** dele, em ordem determinística. Assinantes: read models, sistema de problemas, áudio, UI.
- Proibido: callbacks do domínio direto para a Unity, ou `event` C# em entidades de domínio (ver `EVENT_SYSTEM.md`).

## 9. Erros e resultados

- O Domain e a Application retornam `Result`/`CommandResult` com códigos de erro tipados (`CommandError.InsufficientFunds`) para falhas esperadas. Exceções só para bugs (invariantes violadas).
- No Code Host, falhas de compilação e execução viram um `ValidationReport` estruturado e depois mensagens pedagógicas (`ERROR_SYSTEM.md`).

## 10. Logging e diagnóstico

- Unity: abstração `ILog` (Application) com adaptador para `Debug.Log` e arquivo rotativo. O domínio **não** loga; ele emite eventos e métricas.
- Code Host: `Microsoft.Extensions.Logging` (console + arquivo). IDs de correlação por execução de código (`runId`).
- Métricas de simulação: ms por sistema por tick, contadores (agentes, viagens, buscas) e um painel de debug interno.

## 11. Configuração e dados de jogo

- **Catálogos de balanceamento** (prédios, custos, consumo, pesos de felicidade) ficam em JSON versionado em `content/` e são carregados pelo Application. São independentes da engine porque o Code Host precisa deles nos Laboratórios.
- **Configuração visual** (materiais, prefabs, cores de overlay) fica em ScriptableObjects, só na apresentação.

## 12. Aplicação da arquitetura (fitness functions)

| Regra | Mecanismo |
|---|---|
| Domain/Application/Contracts sem Unity | `noEngineReferences` + teste de arquitetura (.NET) que inspeciona referências |
| Presentation não referencia Domain | Referências de asmdef + `/arch-check` |
| Nada de `DateTime.Now`/`Random` sem seed no Domain | Analyzer + teste de arquitetura |
| Nenhum `MonoBehaviour` com regra de negócio | Code review (`/code-review` do dotnet-claude-kit) + checklist de PR |
| Determinismo | Teste: mesma seed + comandos → mesmo hash de estado (Mono **e** .NET 10) |
| Sem dependência circular | `detect_circular_dependencies` (Roslyn MCP) |
| Segurança do sandbox | Corpus de código malicioso no CI (`CitySky.Security.Tests`) |
| Portabilidade (ADR-0018) | analisador **CA1416** (compatibilidade de plataforma) como erro nos projetos .NET; código específico de SO só em implementações de `IIpcTransport`, `ISandboxLauncher`, `ICodeEditorHost`, `IPlatformServices`, `IFileLocations`; testes .NET também num runner Linux |

---

## 13. Estrutura do repositório (planejada)

```
cityskycode/
├── CLAUDE.md                     # regras para agentes de IA (ver AI_DEVELOPMENT_GUIDE)
├── README.md
├── docs/                         # esta documentação
├── unity/CitySky/                # projeto Unity
│   ├── Assets/
│   │   ├── Art/ Audio/ Materials/ Prefabs/
│   │   ├── Code/
│   │   │   ├── Bootstrap/ Infrastructure/ Presentation/
│   │   │   └── Agents/GameObjects/
│   │   ├── ECS/                  # Fase 11+: Citizens/ Vehicles/ Traffic/
│   │   ├── City/                 # prefabs/visuais: Buildings/ Roads/ Terrain/ Utilities/ Services/
│   │   ├── UI/                   # UXML/USS: Editor/ City/ Inspector/ Overlays/
│   │   ├── StreamingAssets/editor-web/   # bundle do Monaco (gerado)
│   │   └── Tests/
│   └── Packages/
│       └── com.citysky.core/Runtime/{Domain,Application,Contracts,Education}/
├── dotnet/
│   ├── CitySky.slnx  global.json  Directory.Build.props  Directory.Packages.props
│   ├── src/  (Domain, Application, Contracts, Education [links], PlayerApi,
│   │          CodeAnalysis, CodeHost, Sandbox.Worker)
│   └── tests/
├── content/                      # dados de jogo e educação (engine-independentes)
│   ├── catalogs/                 # prédios, custos, pesos
│   └── challenges/<id>/          # challenge.json, starter/, tests/, solution/, hints.md
├── editor-web/                   # fonte TypeScript do Monaco host (Vite)
└── tools/                        # scripts de build, linter de assets, empacotamento
```

> Diferenças em relação ao §63 do briefing: `Code/Domain` e `Code/Application` foram para um **pacote UPM** (para compartilhar a fonte com o .NET). `Education/` e `CodeExecution/` foram para `content/` e `dotnet/` (rodam fora da Unity).
