# Modelo de Domínio

> **Status:** v1.0 · **Data:** 2026-10-08 · **ADRs:** 0004, 0008, 0009 · **MVP:** parcial (marcado)
> Linguagem: código em inglês, explicações em pt-BR (ADR-0014). Glossário: `GLOSSARY.md`.

---

## 1. Visão geral

O domínio é **C# 9 puro (`netstandard2.1`)**, sem dependências externas, compilado pela Unity **e** pelo SDK .NET 10 (shared source). Ele contém:

- **Estado** (`CityState`): *stores* orientados a dados, com entidades referenciadas por **IDs fortemente tipados**.
- **Sistemas de simulação** (`ISimulationSystem`): funções do estado executadas no tick.
- **Comandos** (`ICityCommand`): a única forma de mutação externa.
- **Eventos de domínio**: fatos publicados ao final do tick.
- **Value objects**: `Money`, `Power`, `WaterVolume`, `Fixed`, `Percent`, `GameTime`, coordenadas.

### Por que orientado a dados e não "um objeto por cidadão"?
| Aspecto | Objeto por entidade (`class Citizen`) | Stores + IDs (adotado) |
|---|---|---|
| 100k cidadãos | 100k objetos no heap; GC do Mono (Boehm, não geracional) varre tudo | Arrays de structs contíguos, pressão de GC ≈ 0 |
| Atualização em lote | Chamadas virtuais e cache misses | Laços lineares, prontos para Burst/Jobs |
| Serialização (save/replay) | Grafo de objetos, ciclos | Dump de arrays versionado |
| Ponte com ECS | Conversão cara | IDs mapeiam para entidades ECS |
| Determinismo | Ordem de iteração de coleções frágil | Ordem por ID, explícita |

> **Pedagogia:** a Player API oferece uma fachada **orientada a objetos** (`citizen.Home.Residents`) sobre esses stores. Mais adiante no currículo, o jogo revela por que a engine é orientada a dados (arco "data-oriented design").

---

## 2. Contextos delimitados (DDD tático, enxuto)

```
┌─────────────┐  ┌───────────────┐  ┌────────────┐  ┌──────────────┐
│ Território  │  │ Infraestrutura│  │ Construções│  │  População   │
│ Terrain,Map │  │ RoadNetwork,  │  │ Buildings, │  │ Citizens,    │
│ Zones, Lots │  │ PowerGrid,    │  │ Catalog    │  │ Households   │
│             │  │ WaterNetwork  │  │            │  │ Schedules    │
└─────────────┘  └───────────────┘  └────────────┘  └──────────────┘
┌─────────────┐  ┌───────────────┐  ┌────────────┐  ┌──────────────┐
│  Economia   │  │  Mobilidade   │  │  Serviços  │  │  Bem-estar   │
│ Ledger,     │  │ Trips, Paths, │  │ Health,Edu,│  │ Needs,       │
│ Companies,  │  │ Traffic(meso),│  │ Police,    │  │ Happiness,   │
│ Budget,     │  │ Transit       │  │ Fire,      │  │ CityHealth   │
│ Market      │  │               │  │ Garbage    │  │              │
└─────────────┘  └───────────────┘  └────────────┘  └──────────────┘
┌─────────────┐  ┌───────────────┐
│  Ambiente   │  │ Ocorrências   │      Fora do domínio da cidade:
│ Pollution,  │  │ Problems,     │      • Educação do jogador (Learning) → CitySky.Education
│ Weather,    │  │ Events        │      • Execução de código → Code Host
│ DayNight    │  │               │
└─────────────┘  └───────────────┘
```

Todos compartilham o mesmo `CityState` (monólito modular *in-process*). Os contextos se comunicam por **eventos de domínio** e por **leituras de stores públicos de outros contextos**, nunca escrevendo no store alheio.

---

## 3. Tipos fundamentais

### 3.1 IDs fortemente tipados (C# 9: `readonly struct`, sem `record struct`)
```csharp
namespace CitySky.Domain
{
    public readonly struct CitizenId : IEquatable<CitizenId>
    {
        public readonly int Value;
        public CitizenId(int value) { Value = value; }
        public bool IsValid => Value > 0;
        public bool Equals(CitizenId other) => Value == other.Value;
        public override bool Equals(object? obj) => obj is CitizenId o && Equals(o);
        public override int GetHashCode() => Value;
        public override string ToString() => "C" + Value;
        public static readonly CitizenId None = default;
    }
    // Idem: HouseholdId, BuildingId, LotId, RoadNodeId, RoadSegmentId, CompanyId,
    //       AccountId, TripId, DistrictId, ProblemId, EventId
}
```
- IDs começam em 1 (`0` = inválido). **Reuso** de IDs liberados com **geração** (`Value` = índice + 24 bits, `Generation` = 8 bits) a partir da Fase 18. No MVP: IDs monotônicos.

### 3.2 Valores e unidades
| Tipo | Representação | Unidade | Observação |
|---|---|---|---|
| `Money` | `long` centavos | $ | Operadores checados (`checked`). Player API expõe `decimal` |
| `Power` | `int` watts | W (exibir kW/MW) | Consumidores em kW, usinas em MW |
| `WaterVolume` | `int` litros | L (exibir m³) | — |
| `Fixed` | `int` Q16.16 | — | Ponto fixo para frações autoritativas |
| `Percent` | `short` em centésimos (0–10000) | % | Felicidade, satisfação, alíquotas |
| `GameTime` | `long` ticks | — | Conversões em `SimulationClock` |
| `Position` | `int` milímetros (x, z) + `int` elevação mm | m | Determinístico entre runtimes |
| `Bezier2` | 4 × `Position` | — | Geometria de estrada |

> **Regra (ADR-0004):** estado autoritativo **sem `float`/`double`**. Valores derivados só para exibição podem usar `double` na Application/Presentation.

### 3.3 RNG determinístico
`DeterministicRandom` (PCG32), com semente derivada de `(worldSeed, systemId, tick)`. Cada sistema tem um fluxo próprio, então adicionar um sistema não altera o RNG dos outros.

---

## 4. `CityState` e stores

```csharp
public sealed class CityState
{
    public SimulationClock Clock { get; }
    public TerrainInfo Terrain { get; }
    public LotStore Lots { get; }
    public RoadNetwork Roads { get; }
    public BuildingStore Buildings { get; }
    public HouseholdStore Households { get; }
    public CitizenStore Citizens { get; }
    public CompanyStore Companies { get; }
    public Ledger Ledger { get; }
    public CityBudget Budget { get; }
    public PowerGrid Power { get; }
    public TripStore Trips { get; }
    public NeedsState Needs { get; }          // por cidadão (SoA)
    public HappinessState Happiness { get; }  // por cidadão + agregados
    public CityHealthState Health { get; }
    public ProblemStore Problems { get; }
    public ModifierStack Modifiers { get; }   // efeitos de eventos/clima/políticas
    // ... água, lixo, poluição, serviços (pós-MVP)
}
```

### 4.1 Padrão de store (Structure of Arrays)
```csharp
public sealed class CitizenStore
{
    // índice = CitizenId.Value
    private CitizenId[] _ids;
    private HouseholdId[] _household;
    private BuildingId[] _workplace;      // None = desempregado
    private byte[] _ageGroup;             // Adult no MVP
    private byte[] _educationLevel;
    private CitizenActivity[] _activity;  // enum: AtHome, Working, Shopping, Traveling...
    private TripId[] _currentTrip;
    private int _count;

    public int Count => _count;
    public CitizenId Add(HouseholdId household) { /* ... */ }
    public void Remove(CitizenId id) { /* swap-remove + índice reverso */ }
    public BuildingId GetWorkplace(CitizenId id) => _workplace[id.Value];
    public void SetWorkplace(CitizenId id, BuildingId b) { /* só sistemas do contexto */ }
    // Iteração: for (int i = 1; i <= _count; i++) ...  (sem LINQ, sem alocação)
}
```
- **Escrita** só por sistemas do próprio contexto ou por handlers de comando (métodos `internal` + `InternalsVisibleTo` controlado).
- **Leitura** pública e barata (sem cópias).

---

## 5. Entidades por contexto (campos principais)

### 5.1 Território
| Entidade | Campos | Invariantes |
|---|---|---|
| `Lot` | Id, SegmentId (frente), Side, Index, Polygon (4 `Position`), ZoneType, BuildingId, Slope (Percent), Buildable | Um prédio por lote (MVP); inclinação > 25% → não construível |
| `District` (Fase 10) | Id, Name, Polygon, Policies | Não sobrepostos |
| `AnalysisGrid` | célula 32 m, campos escalares (poluição, valor do terreno, cobertura) | Dimensão fixa por mapa |

### 5.2 Infraestrutura: rede viária (ver `ROAD_SYSTEM.md`)
| Entidade | Campos |
|---|---|
| `RoadNode` | Id, Position, Control (`None`/`Stop`/`Signal`/`Roundabout`), SegmentIds |
| `RoadSegment` | Id, StartNode, EndNode, Geometry (`Bezier2`), RoadTypeId, Length (mm), LaneIds |
| `RoadType` (catálogo) | Lanes (sentido, largura), SpeedLimit, Sidewalks, Capacity/lane/h, CostPerMeter, Maintenance |
| `Lane` | Id, SegmentId, Direction, Index, Connections (lanes de saída por nó) |

### 5.3 Construções (ver `BUILDING_SYSTEM.md`)
| Entidade | Campos |
|---|---|
| `Building` | Id, ArchetypeId, LotId, Owner (`City`/`Company`/`Household`), Condition, Params (struct por arquétipo), Derived (consumo, capacidade), Powered, ConstructedAt |
| `BuildingArchetype` (catálogo) | Kind (`House`, `Shop`, `CityHall`, `PowerPlant`...), Footprint, BaseCost, Jobs, Capacity, regras de cálculo |

Parâmetros e derivados de uma **House**:
```
Params:  Residents (1–6), Insulation (Basic|Good|Excellent), HasSolarPanels
Derived: EnergyConsumption = f(Residents, Insulation) − solar(time-of-day)
         Cost = base + Residents·k1 + insulation[k] + solar
         MaintenancePerDay = Cost · 0,1%
```
`Derived` é **sempre recalculado** pelo domínio. Nunca é atribuído de fora (CRITICAL_ANALYSIS D6).

### 5.4 População (ver `CITIZEN_SYSTEM.md`, `HOUSEHOLD_SYSTEM.md`)
| Entidade | Campos |
|---|---|
| `Household` | Id, HomeBuildingId, Members (≤ 6), AccountId, MoveInTick, UnhappyDays |
| `Citizen` | Id, HouseholdId, AgeGroup, Education, Workplace, Activity, CurrentTrip, ScheduleTemplateId, NameSeed (nome gerado de forma determinística, não armazenado) |

### 5.5 Economia (ver `ECONOMY_SYSTEM.md`, `MONEY_FLOW.md`, `BUDGET_SYSTEM.md`)
| Entidade | Campos |
|---|---|
| `Account` | Id, OwnerKind (`Household`/`Company`/`Treasury`/`External`), OwnerId, Balance (`Money`) |
| `Transaction` | Tick, From, To, Amount, Category (`Wage`, `Purchase`, `IncomeTax`, `BusinessTax`, `EnergyBill`, `Construction`, `Maintenance`, `Import`, `Export`...), Ref |
| `Company` | Id, BuildingId, Kind, AccountId, Employees, Inventory, Revenue/Expenses (dia) |
| `CityBudget` | Alíquotas, receitas/despesas por categoria (dia/mês), histórico |

**Invariante de conservação:** `Σ saldo(contas) = constante + Σ fluxos com External`. É testado após **todo** tick nos testes de domínio.

### 5.6 Mobilidade (ver `PATHFINDING.md`, `TRAFFIC_SYSTEM.md`)
| Entidade | Campos |
|---|---|
| `Trip` | Id, CitizenId, Mode (`Walk` no MVP; `Car`, `Transit` depois), Origin, Destination, PathRef, DepartTick, ExpectedArriveTick, Progress (Fixed), State |
| `PathRef` | handle para o `PathCache` (lista de arestas) |
| `LinkState` (Fase 11) | por lane/segmento: Density, Flow, Speed, Queue |

### 5.7 Bem-estar (ver `HAPPINESS_SYSTEM.md`, `CITY_HEALTH_SYSTEM.md`)
| Entidade | Campos |
|---|---|
| `NeedsState` | por cidadão: satisfação 0–10000 por necessidade (SoA) |
| `HappinessState` | por cidadão: valor suavizado + **contribuições por fator** (para explicação); agregados por prédio/bairro/cidade |
| `CityHealthState` | índice, nível, indicadores normalizados, tendência, histerese |

### 5.8 Ocorrências (ver `EVENT_SYSTEM.md`)
| Entidade | Campos |
|---|---|
| `Problem` | Id, Kind, Severity, Location (lote/segmento/bairro), DetectedTick, Evidence (lista de causas), SuggestedActions, LinkedConcepts (pedagogia) |
| `CityEvent` | Id, DefinitionId, StartTick, Duration, Modifiers, Area |

---

## 6. Comandos

```csharp
public interface ICityCommand
{
    CommandSource Source { get; }   // UI, PlayerCode, System, Event
}

public sealed class BuildBuildingCommand : ICityCommand
{
    public CommandSource Source { get; }
    public BuildingArchetypeId Archetype { get; }
    public BuildingParams Params { get; }
    public LotId? Lot { get; }        // null → próximo lote livre (regra didática da lição 3)
    // construtor...
}
```

| Comando (MVP) | Validações (Application) |
|---|---|
| `BuildRoadSegment` | geometria válida, sem sobreposição, custo, área construível |
| `DemolishRoadSegment` / `DemolishBuilding` | existe, não é a prefeitura, reembolso |
| `BuildBuilding` | desbloqueado, lote livre e compatível, saldo suficiente, parâmetros na faixa |
| `ModifyBuilding` | existe, construído pelo jogador, custo de reforma |
| `SetTaxRate` | categoria válida, 0–20% |

**Ordem determinística:** os comandos chegam numa `CommandQueue`, recebem `(targetTick, sequence)` e são aplicados no início do tick-alvo, nessa ordem. O resultado (`CommandResult`) volta para quem enviou (UI ou Code Host).

---

## 7. Eventos de domínio (exemplos)
`BuildingConstructed`, `BuildingDemolished`, `HouseholdMovedIn`, `HouseholdMovedOut`, `CitizenHired`, `CitizenFired`, `TripCompleted`, `PowerShortageStarted`, `PowerShortageEnded`, `BudgetDayClosed`, `ProblemDetected`, `ProblemResolved`, `CityHealthLevelChanged`.

Os eventos são **structs** acumulados num `DomainEventBuffer` por tick e despachados em ordem após o tick. Assinantes nunca alteram o estado diretamente: se precisarem, emitem comandos.

---

## 8. Explicabilidade (requisito transversal)
Todo indicador calculado guarda **contribuições** num formato comum:
```csharp
public readonly struct Contribution
{
    public readonly FactorId Factor;   // ex.: Needs.Energy
    public readonly int Delta;         // em centésimos de ponto
    public readonly CauseRef Cause;    // ex.: PowerShortage(gridId) / Building(id)
}
```
- O Inspector e o painel "Por que?" percorrem essas contribuições. Exemplo: Felicidade −18 ← Energia (−12) ← Prédio sem energia ← Déficit da rede de 40 kW ← Usina sobrecarregada.
- As contribuições são guardadas de forma **agregada** (por prédio/bairro) e **amostrada** (por cidadão, só sob demanda, recalculando para o cidadão inspecionado) para não explodir a memória.

---

## 9. Linguagem ubíqua (trecho; completo em `GLOSSARY.md`)
| pt-BR (jogo/aulas) | Código |
|---|---|
| Cidade | `City` / `CityState` |
| Cidadão / Domicílio | `Citizen` / `Household` |
| Lote | `Lot` |
| Prédio / Casa / Mercado / Prefeitura / Usina | `Building` / `House` / `Shop` / `CityHall` / `PowerPlant` |
| Tesouro municipal | `Treasury` |
| Livro-razão | `Ledger` |
| Viagem | `Trip` |
| Necessidade / Satisfação | `Need` / `Satisfaction` |
| Termômetro da cidade | `CityHealth` |
| Problema | `Problem` |

---

## 10. Testes do domínio
- Unidade (NUnit) por regra, com construtores de cenário (`CityBuilder` de teste — Test Data Builder).
- **Propriedades/invariantes** a cada tick: conservação de dinheiro, nenhum cidadão sem domicílio ativo, consumo ≥ 0, IDs válidos.
- **Determinismo:** hash do estado após N ticks, executado em .NET 10 e no Mono (UTF).
- **Balanceamento:** cidades roteirizadas por 1–10 anos de jogo (*headless*), com faixas aceitáveis por indicador.
