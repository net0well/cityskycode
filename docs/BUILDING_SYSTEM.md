# Sistema de Construções

> **Status:** v1.0 · **Data:** 2026-10-08 · **MVP:** sim (House, Shop, CityHall, PowerPlant)

---

## 1. Intenção
Prédios são o ponto de encontro entre **território** (lotes), **pessoas** (moradores, trabalhadores), **economia** (custos, receitas) e **infraestrutura** (energia, água). Também são o principal veículo pedagógico de **objetos, propriedades, herança e composição**.

## 2. Modelo
- **Catálogo de arquétipos** (`content/catalogs/buildings.json`): dados de balanceamento independentes da engine.
- **Instância** (`BuildingStore`): `Id`, `ArchetypeId`, `LotId`, `Owner`, `Params` (struct por tipo), `Derived` (recalculado), `Condition` (0–10000), `Powered`, `ConstructedAt`, `Occupancy`/`Employees`.
- **Visual** (Presentation): `BuildingVisualCatalog` (ScriptableObject) mapeia `ArchetypeId` + variação (seed do lote) → prefab/LOD group.

```json
{ "id": "house.small", "kind": "House", "footprint": [1, 1],
  "baseCost": 800000, "costPerResident": 120000,
  "insulation": { "Basic": { "cost": 0, "factor": 1.00 },
                  "Good": { "cost": 150000, "factor": 0.80 },
                  "Excellent": { "cost": 350000, "factor": 0.65 } },
  "solar": { "cost": 400000, "peakW": 1500 },
  "baseWPerResident": 450, "baseWHouse": 600,
  "maintenancePerDayPermille": 1 }
```
(valores em centavos e watts, ilustrativos e sujeitos a balanceamento)

## 3. Regras de cálculo (MVP)
```
House.EnergyConsumption(W) = round((baseWHouse + baseWPerResident × Residents) × insulationFactor) − solarOutput(horaDoDia)
House.Cost = baseCost + costPerResident × Residents + insulation.cost + (solar ? solar.cost : 0)
Maintenance/dia = Cost × 0,1%
```
Ex.: 4 moradores, Good, sem solar → (600 + 1800) × 0,8 = **1.920 W ≈ 1,9 kW** (ordem de grandeza realista para uma casa, ao contrário dos "3,2 MW" do exemplo do briefing).

| Arquétipo | Empregos | Consumo | Outros |
|---|---|---|---|
| `House` | — | 1–4 kW | 1–6 moradores |
| `Shop` | 4 | 8 kW | estoque, vendas, impostos |
| `CityHall` | 3 | 10 kW | sede; não demolível |
| `PowerPlant` (gás) | 3 | — | geração constante (~60–80 kW no MVP; valor final na tarefa F7-07), custo operacional/dia |

## 4. Ciclo de vida
```
Projeto (Player API/ferramenta) → BuildBuilding (validado) → Em obra (Fase 10: duração) → Ativo
   → ModifyBuilding (reforma paga) → Deteriorando (sem manutenção, Fase 12+) → Demolido (reembolso 25%)
```
MVP: construção instantânea no tick seguinte (feedback imediato para o aprendizado). Obras com duração e construtora entram na Fase 10.

## 5. Herança × composição (pedagogia × engine)
- **Player API** (Fase 14): hierarquia didática `Building` → `ResidentialBuilding`/`CommercialBuilding`/`ServiceBuilding` → tipos concretos, com `virtual CalculateMaintenanceCost()` (polimorfismo, §41).
- **Domain:** arquétipo + `Kind` + dados por tipo (composição orientada a dados). Comportamento por `Kind` em sistemas.
- A diferença é explicada ao aluno no arco "da POO ao orientado a dados" (`LEARNING_DESIGN.md` §7).

## 6. Inspector (§36)
```
Casa #1029                     Moradores: 4
Consumo de energia: 1,9 kW     Isolamento: Bom · Solar: não
Felicidade: ████████░░ 82%
Valor: $12.400                 Manutenção: $12/dia
Problemas: Nenhum
```

## 7. Testes
Cálculos derivados (tabela de casos), custos, reforma (diferença de custo), demolição (reembolso, realocação de moradores), invariante "um prédio por lote".
