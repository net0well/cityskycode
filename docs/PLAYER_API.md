# Player API (API Educacional)

> **Status:** v0.1 (MVP) · **Data:** 2026-10-08 · **ADRs:** 0005, 0014 · **MVP:** sim
> Projeto: `dotnet/src/CitySky.PlayerApi` (.NET 10). Superfície pública revisada com `get_public_api` (Roslyn MCP) em todo PR.
> **Implementado na Fase 1 (v0, 2026-10-12):** só o mínimo do esqueleto — `CityGame.Current`, `City.Log(string)`, `City.Log(object)` e a ponte `ICityGateway`/`LiveCityGateway` (padrão Gateway/Adapter), com XML docs pt-BR (`CS1591` como erro). O resto desta API (prédios, consultas, economia) entra nas fases seguintes.

---

## 1. Princípios (§29)
1. **API de domínio, não de engine.** Nunca `GameObject`, `Transform` ou `Find`. O aluno programa contra conceitos da cidade.
2. **Código como engenharia, não como trapaça** (CRITICAL_ANALYSIS D6): grandezas físicas são **derivadas**; toda ação **custa** como na UI.
3. **Idiomática em C#:** propriedades para estado barato, métodos para ações e consultas com custo ou parâmetros (diferença ensinada explicitamente).
4. **Inglês nos identificadores, pt-BR na documentação** (XML docs que aparecem no hover e no autocomplete).
5. **Mensagens de erro em runtime pedagógicas** (`ArgumentOutOfRangeException` com texto pt-BR explicando a regra).
6. **Progressiva:** partes da API são desbloqueadas pelo currículo (diagnóstico `CSK3001`), não removidas.
7. **Pequena e estável:** cada membro novo precisa de motivo pedagógico ou de gameplay. Mudança incompatível = nova versão da API + migração do código dos saves.

> **Diferença em relação ao briefing:** `city.GetPopulation()` e `city.GetBudget()` viraram **propriedades** (`city.Population`, `city.Budget`), seguindo a convenção .NET (CA1024). `city.GetCitizens()` e `city.GetBuildings()` continuam métodos (retornam coleções novas a cada chamada). Essa distinção é conteúdo da lição 5.

## 2. Superfície do MVP (v0.1)

```csharp
namespace CitySky
{
    /// <summary>Ponto de entrada: a cidade em que você está jogando.</summary>
    public static class CityGame
    {
        public static City Current { get; }
    }

    public sealed class City
    {
        public string Name { get; }
        public int Population { get; }
        public Budget Budget { get; }
        public EnergyReport Energy { get; }
        public Percent Happiness { get; }          // 0–100
        public CityClock Clock { get; }            // data/hora de jogo (somente leitura)
        public CityRandom Random { get; }          // aleatoriedade determinística

        public BuildResult Build(Building building);               // próximo lote livre
        public BuildResult Build(Building building, Lot lot);      // sobrecarga (lição 5)
        public IReadOnlyList<Lot> FindFreeLots();
        public IReadOnlyList<Building> GetBuildings();
        public IReadOnlyList<T> GetBuildings<T>() where T : Building;   // desbloqueado em "Coleções"
        public IReadOnlyList<Citizen> GetCitizens();
        public IReadOnlyList<Road> GetRoads();
        public void Log(string message);
        public void Log(object value);
    }

    public abstract class Building
    {
        public BuildingId Id { get; }              // vazio antes de construir
        public bool IsBuilt { get; }
        public decimal Cost { get; }               // derivado
        public decimal MaintenancePerDay { get; }  // derivado
        public Lot? Lot { get; }
        public bool IsPowered { get; }
    }

    public sealed class House : Building
    {
        /// <summary>Quantas pessoas a casa comporta (1 a 6). Mais moradores = mais consumo e custo.</summary>
        public int Residents { get; set; }                  // valida faixa, ArgumentOutOfRange pt-BR
        public Insulation Insulation { get; set; }          // Basic | Good | Excellent
        public bool HasSolarPanels { get; set; }
        /// <summary>Consumo de energia em kW. Calculado pela cidade: você não pode atribuí-lo.</summary>
        public double EnergyConsumption { get; }            // somente leitura → CS0200 (lição 4)
        public int Occupants { get; }
        public Percent Happiness { get; }
    }

    public sealed class Shop : Building { public int Jobs { get; } public int Employees { get; } }
    public sealed class PowerPlant : Building { public double CapacityKw { get; } public double OutputKw { get; } }
    public sealed class CityHall : Building { }

    public enum Insulation { Basic, Good, Excellent }

    public sealed class Lot { public LotId Id { get; } public bool IsFree { get; } public string Street { get; } }
    public sealed class Road { public string Name { get; } public double LengthMeters { get; } }

    public sealed class Citizen
    {
        public string Name { get; }
        public int Age { get; }
        public House? Home { get; }
        public Building? Workplace { get; }
        public decimal Salary { get; }               // somente leitura: salários vêm da economia
        public Percent Happiness { get; }
        public string CurrentActivity { get; }
    }

    public sealed class Budget
    {
        public decimal Treasury { get; }
        public decimal IncomePerDay { get; }
        public decimal ExpensesPerDay { get; }
        public Percent ResidentialTaxRate { get; }
        public BuildResult SetResidentialTaxRate(Percent rate);    // 0–20%
    }

    public sealed class EnergyReport
    {
        public double GenerationKw { get; }
        public double ConsumptionKw { get; }
        public double BalanceKw { get; }             // negativo = déficit
        public IReadOnlyList<Building> Unpowered { get; }
    }

    public readonly struct BuildResult
    {
        public bool Success { get; }
        public string Message { get; }               // pt-BR
        public decimal Cost { get; }
    }

    public readonly struct Percent { /* 0–100, conversões implícitas de int/double com validação */ }
}
```

### 2.1 Semântica importante
| Situação | Comportamento |
|---|---|
| `new House()` | cria um **projeto** (blueprint) que não existe no mundo (`IsBuilt == false`) |
| `city.Build(house)` | gera um comando `BuildBuilding`. Se a Application aceitar, a casa aparece no tick seguinte e `house` passa a representar o prédio real |
| Alterar `house.Insulation` **antes** de construir | só muda o projeto (custo recalculado) |
| Alterar `house.Insulation` **depois** de construir | gera um comando `ModifyBuilding` (**reforma paga**). Se a Application aceitar, a simulação muda (AC-04) |
| `house.EnergyConsumption = 0` | erro de compilação `CS0200` → lição de encapsulamento (AC-05) |
| `house.Residents = 50` | `ArgumentOutOfRangeException`: "Uma casa comporta de 1 a 6 moradores. Para mais pessoas, construa um prédio residencial (desbloqueado na Fase 10)." |
| Sem dinheiro | `BuildResult.Success == false`, `Message == "Tesouro insuficiente: faltam $3.200"`. Não lança exceção (ensina a checar resultados) |

### 2.2 Gateways (implementação interna, invisível ao aluno)
```
Player API ──► ICityGateway
                 ├─ LiveCityGateway     (modo Script: consultas por IPC, CommandBuffer)
                 └─ HeadlessCityGateway (Laboratório: Domain dentro do Worker)
```
O padrão **Strategy/Adapter** aqui é o mesmo que o jogo ensina. Ele pode virar material de "bastidores" numa lição avançada.

## 3. Extensões por fase
| Fase | Adições |
|---|---|
| 9 | `city.Policies` (registro de comportamentos), `IEnergyPolicy` |
| 10 | `Zone`, `District`, `city.Zone(area, ZoneType)` (custos), `ResidentialBuilding` como tipo-base |
| 11 | `Vehicle` (abstrata), `IRouteStrategy`, `city.Traffic` (relatórios) |
| 12–13 | `GarbageArea`, `IGarbageRoutingStrategy`, serviços (`Hospital`, `School`, `PoliceStation`, `FireStation`) como tipos-base para herança |
| 14 | tipos-base abertos para herança pedagógica (`ServiceBuilding`), composição (`Hospital.Departments`) |
| 15–16 | código legado `CityManager` (fornecido), `IMapOverlay`, eventos (`city.Events.PowerShortage += ...`) |
| 17 | `nunit.framework` referenciado; `CitySky.Testing` (fakes e builders para testes do aluno) |

## 4. Documentação XML (pt-BR)
Todo membro público tem `<summary>`, `<remarks>` com o conceito relacionado e `<example>`. Esses textos aparecem no hover do Monaco e no painel de documentação contextual. O CI falha se faltar documentação num membro público (`GenerateDocumentationFile` + `CS1591` como erro).

## 5. Versionamento
- `PlayerApiVersion` (inteiro) gravado no save junto do código do aluno.
- Ao carregar um save com versão antiga: o Code Host compila com *shims* de compatibilidade ou oferece uma migração assistida (diff mostrado ao aluno). **Nunca** quebrar o código salvo silenciosamente.
