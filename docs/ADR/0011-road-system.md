# ADR-0011: Rede viária no domínio, com Unity Splines só na apresentação

- **Status:** Aceito
- **Data:** 2026-10-08
- **Documentos afetados:** `ROAD_SYSTEM.md`, `MAP_SYSTEM.md`

## Contexto
O briefing sugere avaliar Unity Splines para estradas curvas. A rede precisa existir no domínio (pathfinding, lotes, energia, Laboratórios *headless*), que não pode depender da Unity.

## Decisão
1. Grafo viário (`RoadNode`, `RoadSegment`, `Lane`) e geometria **`Bezier2` própria em milímetros** no domínio.
2. **Unity Splines** na apresentação: ferramenta de desenho, pré-visualização e extrusão. Conversão domínio → Splines.
3. **MVP:** um tipo de rua, segmentos retos, interseções automáticas até 4 ramos. Curvas, rotatórias, faixas configuráveis, semáforos, pontes e túneis em fases posteriores.
4. Malha de interseção gerada por algoritmo próprio (spike S5).

## Alternativas consideradas
| Alternativa | Por que não |
|---|---|
| Splines como fonte da verdade | domínio dependeria da Unity; sem Laboratórios *headless* |
| Asset pronto de estradas | modelo de dados do asset ≠ grafo de simulação; acoplamento; licença |

## Consequências
**Positivas:** uma verdade, utilizável pela simulação e pelo Code Host.
**Negativas:** geometria de interseções feita em casa (risco técnico).

## Gatilhos de revisão
Spike S5 · necessidade de recursos avançados que justifiquem um asset (com ficha de dependência).
