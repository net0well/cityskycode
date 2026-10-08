# Sistema de Veículos

> **Status:** v1.0 (intenção de design) · **Data:** 2026-10-08 · **Fase:** 11 · **MVP:** não

---

## 1. Tipos
| Categoria | Exemplos | Dono | Fase |
|---|---|---|---|
| Particulares | carro | domicílio | 11 |
| Carga | caminhão de entrega | empresa | 11 |
| Serviço | caminhão de lixo, ambulância, viatura, caminhão de bombeiros | prefeitura | 12–13 |
| Transporte público | ônibus, VLT/metrô | prefeitura | 19 |

## 2. Modelo (Domain)
- **Frota** por dono: `VehicleStore` (SoA). Veículo = `VehicleId`, `Kind`, `Owner`, `Home` (garagem/depósito), `Capacity`, `Condition`, `CurrentTrip`.
- **Particulares:** não são simulados como objetos persistentes. Viram atributo do domicílio (`HasCar`) e só "existem" durante uma `Trip` de carro (como em city builders clássicos). Isso reduz memória e estado.
- **Serviço e transporte:** persistentes (rotas, capacidade, manutenção).
- **Escolha de modo** (Fase 11): a pé se a distância for menor que ~1 km; senão carro (se houver) ou, na Fase 19, transporte público, por custo generalizado (tempo + dinheiro + conforto).

## 3. Veículos de serviço
Rotas planejadas por **estratégias** (`IRouteStrategy`): mais próximo primeiro, varredura por setores, prioridade por urgência. São o cenário pedagógico central de herança → interface → Strategy (§74).

## 4. Apresentação
ECS + Entities Graphics. Modelos com LOD0–2 + impostor, cores e variantes por instância (sem materiais únicos), luzes de freio e de sirene como emissivos (Forward+ só para luzes próximas).

## 5. Player API (Fase 11/14)
```csharp
public abstract class Vehicle { public string Plate { get; } public VehicleKind Kind { get; } public Building Depot { get; } }
public sealed class GarbageTruck : Vehicle { public int CapacityKg { get; } public int LoadKg { get; } }
public interface IRouteStrategy { Route Plan(Vehicle vehicle, IReadOnlyList<Stop> stops, RoadMap map); }
```

## 6. Testes
Capacidade e carga, escolha de modo (tabela), rotas por estratégia (cenários pequenos com resposta conhecida).
