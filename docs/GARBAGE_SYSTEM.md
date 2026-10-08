# Sistema de Lixo

> **Status:** v1.0 (intenção de design) · **Data:** 2026-10-08 · **Fase:** 12 · **MVP:** não
> Cenário pedagógico central da missão completa do briefing (§74).

---

## 1. Componentes (§24)
Produção de lixo · coleta · caminhões · aterros · reciclagem · capacidade · rotas.

## 2. Modelo
| Elemento | Regra |
|---|---|
| Produção | kg/dia por prédio = f(moradores ou funcionários, tipo, renda) |
| Acúmulo | por prédio. Acima do limiar, aparece o ícone e caem *Health*/*Environment* do lote e dos vizinhos |
| Depósitos | `Landfill` (capacidade total em t; enche) · `RecyclingCenter` (converte X% e gera receita) · `Incinerator` (Fase 19: energia + poluição) |
| Caminhões | frota por depósito, capacidade em kg, turno |
| Coleta | o caminhão visita uma sequência de paradas (prédios com lixo) planejada por **estratégia de rota**, volta ao depósito quando cheio |

## 3. Planejamento de rotas (Strategy)
- **Padrão da engine:** varredura gulosa "mais próximo com mais lixo" a partir do depósito, com Dijkstra multi-fonte para "depósito mais próximo".
- **Do aluno:** `IGarbageRoutingStrategy`, avaliada no Worker em lote (Fase 9, IPC) e comparada com a padrão em métricas: kg coletados/dia, km rodados, lixo acumulado máximo.

## 4. Problemas (§24)
> "O bairro norte está acumulando lixo."

Detecção: média de acúmulo no bairro > limiar por 2 dias. Evidências: caminhões insuficientes? Depósito longe? Aterro cheio? Rota ineficiente? O jogador **investiga** com overlay de lixo, "Show Garbage Routes" e o Inspector do caminhão (carga, rota, tempo ocioso).

## 5. Pedagogia (sequência do §74)
1. `GarbageCollector.Collect(areas)`: classe, método, parâmetros, coleções, laços.
2. "Depósito mais próximo": algoritmo de mínimo → BFS/Dijkstra.
3. Tipos de veículos (lixo, ônibus, polícia, bombeiros): herança.
4. Rotas diferentes por tipo: interface + polimorfismo.
5. "O sistema ficou difícil de manter": Strategy + SOLID.
6. Bug de rota: teste que reproduz e corrige.

## 6. Testes
Produção por tipo, capacidade dos caminhões (nunca excede), estratégia padrão em grafos pequenos com resposta conhecida, aterro cheio → problema.
