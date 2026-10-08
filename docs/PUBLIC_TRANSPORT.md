# Transporte Público

> **Status:** v1.0 (intenção de design) · **Data:** 2026-10-08 · **Fase:** 19 · **MVP:** não

---

## 1. Escopo (§21)
Ônibus (Fase 19), depois metrô/trem (pós-lançamento). Paradas, estações, linhas, rotas e passageiros. O jogador **cria linhas** (UI e código).

## 2. Modelo
| Entidade | Campos |
|---|---|
| `TransitLine` | Id, Mode, Stops (ordenadas), Frequency (min TC), Vehicles, Fare, Color |
| `TransitStop` | Id, Position (nó do grafo de pedestres e do viário), Lines |
| `TransitVehicle` | Id, LineId, Capacity, Load, PositionOnLine |

- **Grafo de transporte** (ver `PATHFINDING.md`): arestas de linha (tempo de viagem), de espera (meia frequência) e de transbordo.
- **Escolha modal:** custo generalizado = tempo a pé + espera + viagem + tarifa × valor do tempo (por renda) + penalidade de transbordo.
- Ônibus usam o **viário** e sofrem trânsito (meso).

## 3. Métricas calculadas (§21)
| Métrica | Definição |
|---|---|
| Passageiros | embarques por dia por linha/parada |
| Tempo | tempo médio porta a porta × alternativa de carro |
| Cobertura | % da população a ≤ 400 m de uma parada (Dijkstra multi-fonte no grafo de pedestres) |
| Custo | operação (veículos, motoristas) − receita de tarifas |
| Eficiência | passageiros·km / custo; ocupação média |

## 4. Pedagogia
Otimização de linhas como desafio (algoritmos, trade-offs), `ITransitScheduler` (Strategy) e relatórios com LINQ (Módulo 4).

## 5. Riscos
Complexidade da escolha modal e da calibração. Mitigação: começar só com ônibus, uma tarifa e frequência fixa.
