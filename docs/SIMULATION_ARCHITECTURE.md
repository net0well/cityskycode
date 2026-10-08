# Arquitetura da Simulação

> **Status:** v1.0 · **Data:** 2026-10-08 · **ADRs:** 0003, 0004, 0008, 0009 · **MVP:** sim
> Relacionados: `SIMULATION_TICK.md` (tempo e frequências), `SIMULATION_LOD.md` (níveis), `CITY_SIMULATION.md` (interações entre sistemas).

---

## 1. Objetivos
1. **Determinismo:** mesmo estado + mesmos comandos → mesmo resultado, em Mono, IL2CPP e .NET 10.
2. **Independência de engine:** a simulação roda *headless* (testes, Code Host, benchmarks).
3. **Escalabilidade progressiva:** de 50 a 100k+ cidadãos sem reescrever a arquitetura.
4. **Explicabilidade:** cada resultado tem causas consultáveis.
5. **Desacoplamento do frame rate:** a velocidade muda só a quantidade de ticks por segundo real.

## 2. Componentes

```
┌──────────────────────────── Application ─────────────────────────────┐
│ SimulationRunner                                                      │
│  ├─ TickAccumulator (tempo real × velocidade → ticks devidos)         │
│  ├─ CommandQueue  (comandos ordenados por (tick, seq))                │
│  ├─ Simulation (Domain) ───────────────────────────────┐             │
│  ├─ ReadModelPublisher (dirty tracking)                  │             │
│  └─ Metrics (ms por sistema)                             │             │
└──────────────────────────────────────────────────────────┼────────────┘
┌──────────────────────────── Domain ──────────────────────▼────────────┐
│ Simulation                                                             │
│  ├─ CityState (stores)                                                 │
│  ├─ SystemSchedule: lista ordenada de (ISimulationSystem, Period, Phase)│
│  ├─ DomainEventBuffer                                                  │
│  ├─ RandomStreams (PCG32 por sistema)                                  │
│  └─ Ports: IPathfinder, IParallelExecutor, IBehaviorEvaluator          │
└────────────────────────────────────────────────────────────────────────┘
```

### 2.1 Contratos
```csharp
public interface ISimulationSystem
{
    SystemId Id { get; }
    /// Período em ticks (1 = todo tick) e fase (deslocamento) para espalhar a carga.
    TickSchedule Schedule { get; }
    void Execute(SimulationContext ctx);
}

public sealed class SimulationContext
{
    public CityState State { get; }
    public long Tick { get; }
    public SimulationClock Clock { get; }
    public DeterministicRandom Random { get; }        // fluxo do sistema
    public DomainEventBuffer Events { get; }
    public ContributionRecorder Explain { get; }
    public SimulationPorts Ports { get; }
}
```

### 2.2 Ordem fixa dos sistemas (MVP em **negrito**)
| # | Sistema | Período (ticks) | Contexto |
|---|---|---|---|
| 0 | **CommandApplySystem** | 1 | Application → Domain |
| 1 | **CalendarSystem** (dia/noite, fases do dia) | 1 | Ambiente |
| 2 | WeatherSystem | 480 | Ambiente |
| 3 | **ScheduleSystem** (decide a próxima atividade, em baldes) | 1 (baldes de 120) | População |
| 4 | **TripPlanningSystem** (pede caminhos, orçamento por tick) | 1 | Mobilidade |
| 5 | **TripProgressSystem** (avança viagens, chegadas) | 1 | Mobilidade |
| 6 | TrafficMesoSystem | 4 | Mobilidade |
| 7 | **PowerGridSystem** | 480 ou *dirty* | Infraestrutura |
| 8 | WaterSystem / SewageSystem | 480 ou *dirty* | Infraestrutura |
| 9 | **EmploymentSystem** (vagas ↔ desempregados) | 480 | Economia |
| 10 | **CommerceSystem** (compras ao chegar ao mercado, estoques) | evento + 480 | Economia |
| 11 | **LedgerSettlementSystem** (salários, impostos, contas) | 11.520 (diário) + 480 | Economia |
| 12 | ServiceCoverageSystem | 480 | Serviços |
| 13 | PollutionSystem | 480 | Ambiente |
| 14 | **NeedsSystem** (baldes) | 1 (baldes de 480) | Bem-estar |
| 15 | **HappinessSystem** (baldes) | 1 (baldes de 480) | Bem-estar |
| 16 | **MigrationSystem** (chegada/saída) | 11.520 | População |
| 17 | **ProblemDetectionSystem** | 480 | Ocorrências |
| 18 | EventSystem | 480 | Ocorrências |
| 19 | **CityHealthSystem** | 480 | Bem-estar |
| 20 | **BudgetCloseSystem** | 11.520 | Economia |

> **Fase** (deslocamento): sistemas com o mesmo período recebem fases diferentes (ex.: Power na fase 0, Employment na 40, Pollution na 80) para que um único tick não acumule toda a carga.

### 2.3 Baldes (time-slicing)
Sistemas por cidadão processam só uma fatia por tick:
```csharp
// NeedsSystem: cada cidadão é atualizado 1x por hora de jogo (480 ticks)
int bucket = (int)(ctx.Tick % 480);
for (int i = bucket + 1; i <= citizens.Count; i += 480) { Update(i, ctx); }
```
O custo por tick fica proporcional a `N/480`. Com 100k cidadãos são ~209 atualizações por tick.

## 3. O tick (algoritmo)
```
RunTick(t):
  1. ApplyCommands(t)            // ordem (t, seq), cada um gera CommandResult
  2. for system in Schedule:
        if system.Schedule.IsDue(t): system.Execute(ctx(t, system))
  3. Events.Flush()  → assinantes (ReadModels, Problems, Áudio, UI)
  4. ReadModels.PublishDirty()
  5. if (t % HashInterval == 0 && DebugDeterminism) StateHasher.Record(t)
```
- **Nenhum sistema lê tempo real.** Só `ctx.Tick` e `ctx.Clock`.
- **Nenhum sistema altera stores de outro contexto.** Para isso, emite eventos ou chama serviços de domínio do contexto dono.

## 4. Portas de aceleração
| Porta | Implementação padrão (Domain) | Implementação acelerada (Unity, Fase 11+) | Requisito |
|---|---|---|---|
| `IPathfinder` | A* gerenciado, custos inteiros | Jobs + Burst em lote | **Resultados idênticos** (custos inteiros, *tie-breaking* por ID) |
| `IParallelExecutor` | sequencial | `IJobParallelFor` / `Parallel.For` no .NET | Trabalho particionado sem estado compartilhado mutável |
| `IFieldSolver` (difusão de poluição, cobertura) | sequencial | Burst | Inteiros/ponto fixo |
| `IBehaviorEvaluator` (código do jogador, Fase 9) | regra padrão | IPC em lote para o Worker | Latência de 1 tick, *fallback* |

O composition root escolhe a implementação. Os testes de determinismo comparam padrão × acelerada.

## 5. Read models
- Construídos a partir do estado e dos eventos, com **dirty tracking**: só o que mudou é republicado.
- Exemplos: `CitySummary` (HUD), `BuildingDetails(id)` (Inspector), `OverlayField(kind)`, `AgentSnapshot` (posições para a apresentação), `BudgetReport`, `ProblemList`.
- **Imutáveis do ponto de vista do consumidor.** Na Fase 18, ficam em *double buffer* entre a thread de simulação e a principal.

## 6. Ponte com a apresentação de agentes
```
Domain (Trip: path, depart, expectedArrive, progress)
   │  AgentSnapshot (por frame, só viagens ativas)
   ▼
IAgentPresentation
   ├─ GameObjectAgentPresenter (MVP): pool de GameObjects
   └─ EcsAgentPresenter (Fase 11): entidades ECS, Entities Graphics
```
A apresentação **interpola** a posição no caminho com `progress(t + alpha)`. Ela **nunca** escreve no domínio. Ver `SIMULATION_LOD.md`.

## 7. Runner *headless*
`HeadlessSimulation` (Domain + Application, sem Unity) é usado por:
- testes de unidade, determinismo e balanceamento (`dotnet test`);
- Code Host / Worker nos **Laboratórios** (validação de desafios);
- benchmarks (BenchmarkDotNet);
- ferramenta de linha de comando para simular N dias e exportar CSV (balanceamento).

## 8. Hash de estado e determinismo
- `StateHasher`: hash incremental (xxHash64 implementado no domínio, sem dependência) sobre os stores em ordem de ID.
- Em debug, guarda hashes a cada 480 ticks. O teste de determinismo compara execuções em runtimes diferentes.
- **Regras:** sem `Dictionary` iterado em ordem não determinística (usar arrays/`SortedList` ou ordenar por ID), sem `HashSet` iterado, sem `float` autoritativo, sem tempo real, sem RNG compartilhado.

## 9. Desempenho
- Métricas por sistema (ms, itens processados) expostas num painel de debug e no Unity Profiler (marcadores via porta `IProfilerMarkers`).
- Orçamento por tick: ver `PERFORMANCE.md`. Sistemas acima do orçamento por 3 ticks seguidos geram aviso em desenvolvimento.
- Proibido nos caminhos por tick: LINQ, `foreach` sobre interfaces (boxing), *closures*, `string` formatting, alocações.

## 10. Evolução planejada
| Fase | Mudança |
|---|---|
| MVP | main thread, ~12 sistemas, 50 cidadãos |
| 11 | tráfego meso, pathfinding acelerado, agentes ECS |
| 18 | thread de simulação dedicada + double buffer de read models; IDs com geração; hierarquia de pathfinding |
