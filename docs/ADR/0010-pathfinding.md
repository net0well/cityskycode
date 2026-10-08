# ADR-0010: Pathfinding em grafo próprio e tráfego mesoscópico autoritativo

- **Status:** Aceito (o modelo de tráfego tem **revisão obrigatória** no spike da Fase 11)
- **Data:** 2026-10-08
- **Documentos afetados:** `PATHFINDING.md`, `TRAFFIC_SYSTEM.md`, `VEHICLE_SYSTEM.md`

## Contexto
O briefing pede para não usar milhares de `NavMeshAgent` e para criar uma camada própria (Graph → Pathfinding → Cache → Agent Simulation → Movement → Rendering). O tráfego microscópico autoritativo é caro e difícil de tornar determinístico entre runtimes, e precisa rodar *headless* em Laboratórios.

## Decisão
1. **Grafos próprios** (pedestres, viário por faixa, transporte) em CSR, com custos **inteiros**.
2. **A*** (desempate por ID), **Dijkstra multi-fonte** para "mais próximo"/cobertura, **cache** versionado por região, orçamento de buscas por tick, hierarquia por bairro na escala 50k+.
3. Porta `IPathfinder` com implementação gerenciada e implementação Burst **idênticas**.
4. **Tráfego autoritativo mesoscópico** (link: densidade → velocidade BPR, capacidade de interseção, *spillback*). **Microssimulação apenas visual** (ECS).
5. **NavMesh** só para gerar arestas em áreas livres (pós-MVP), nunca agentes por cidadão.

## Alternativas consideradas
| Alternativa | Por que não |
|---|---|
| NavMeshAgent por cidadão | não escala; não determinístico |
| Micro autoritativo | custo, determinismo, Laboratórios *headless* (revisar no spike) |
| Macro (só fluxos) | não dá para seguir cidadãos nem "ver" o trânsito |

## Consequências
**Positivas:** escala, determinismo, mesma lógica *headless* e no jogo, algoritmos ensináveis (Dijkstra).
**Negativas:** menos emergência microscópica.

## Gatilhos de revisão
Spike comparativo da Fase 11 · playtest dizendo que o trânsito "não parece real".
