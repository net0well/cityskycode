# Pathfinding

> **Status:** v1.0 · **Data:** 2026-10-08 · **ADR:** [0010](ADR/0010-pathfinding.md) · **MVP:** A* em grafo de calçadas

---

## 1. Decisão (§6 do briefing)
> "Não assumir que milhares de cidadãos devem ser implementados como milhares de `NavMeshAgent`."

Concordamos. Em city builders, a navegação acontece sobre **redes** (ruas, faixas, calçadas, linhas de transporte), que são **grafos**. Adotamos a arquitetura sugerida pelo briefing, com camadas próprias:

```
City Navigation Graph  (grafos: viário por faixa, pedestres, transporte)
        ↓
Pathfinding            (A* ponto-a-ponto; Dijkstra multi-fonte para "mais próximo")
        ↓
Path Cache             (por origem/destino/modo/versão de custo)
        ↓
Agent Simulation       (Trips no Domain: progresso autoritativo)
        ↓
Movement               (Presentation: interpolação/microssimulação visual)
        ↓
Rendering
```
**NavMesh (AI Navigation):** opcional, só para áreas livres (praças, parques) e depois do MVP. Mesmo assim, é usado para **gerar** arestas do grafo de pedestres nessas áreas (offline/no build), não como agentes.

## 2. Grafos
| Grafo | Nós | Arestas | Custo | Fase |
|---|---|---|---|---|
| Pedestres | esquinas de calçada, entradas de lote, travessias | calçadas, faixas de pedestre | tempo a pé (ms) | MVP |
| Viário | conectores de faixa nos nós | faixas | tempo de travessia (meso, ms) | 11 |
| Transporte | paradas/estações | trechos de linha + transbordos | tempo + espera + tarifa | 19 |

**Representação:** CSR (*compressed sparse row*): arrays de offsets, destinos e custos `int`. É compacta, cache-friendly e pronta para Burst. É reconstruída de forma incremental por região quando o jogador edita a rede.

## 3. Algoritmos
| Pergunta | Algoritmo |
|---|---|
| "Caminho de A para B" | **A*** com heurística admissível (distância euclidiana / velocidade máxima), custos **inteiros**, desempate por ID do nó (determinismo) |
| "Serviço mais próximo de cada lote" (cobertura, escolha de mercado, depósito de lixo) | **Dijkstra multi-fonte**: uma execução por tipo de serviço, com todas as instalações como fontes, dá a distância e a instalação mais próxima para **todos** os nós de uma vez |
| "Muitos caminhos para o mesmo destino" | árvore de caminhos reversa (Dijkstra a partir do destino), reaproveitada por várias viagens |
| Escala 50k+ | **hierarquia por bairro** (HPA*-like): grafo abstrato entre "portais" dos bairros + refinamento local |

> Pedagogia: o desafio "encontre o centro de coleta mais próximo" (§27) começa com busca linear por distância em linha reta e evolui para BFS/Dijkstra no grafo (Módulo 13). O jogador implementa, em pequena escala, o mesmo algoritmo que a engine usa.

## 4. Cache de caminhos
- Chave: `(origemNó, destinoNó, modo, versãoDeCusto)`.
- **Versionamento:** cada edição na rede incrementa a versão da **região** afetada. Caminhos que cruzam a região ficam inválidos de forma preguiçosa (verificação ao usar).
- Custos dinâmicos (trânsito) atualizam a versão de custo por janela (1 h TC). Viagens em andamento **não** recalculam a rota (a não ser em bloqueio), o que evita oscilação.
- Limite de memória (LRU), com métricas de acerto expostas no debug.

## 5. Orçamento e assincronia
- `TripPlanningSystem` processa até N pedidos por tick (MVP: 32; Fase 18: adaptativo por ms). Pedidos excedentes esperam o tick seguinte. O cidadão "decide" um pouco mais tarde, sem impacto visível.
- Porta `IPathfinder`: implementação padrão gerenciada (Domain) e implementação Burst em lote (Fase 11), com **resultado idêntico** (custos inteiros, mesmo desempate). Testes comparam as duas.

## 6. Metas de desempenho
| Escala | Grafo | Buscas/h TC | Custo médio por busca |
|---|---|---:|---|
| MVP | ~500 nós | ~100 | < 0,05 ms |
| 10k | ~20k nós | ~10k | < 0,2 ms (com cache: 70%+ acerto) |
| 100k | ~150k nós | ~100k | hierárquico < 0,3 ms |

## 7. Testes
Caminho ótimo em grafos de referência, desempate determinístico, invalidação de cache por edição, Dijkstra multi-fonte × N buscas A* (mesmo resultado), padrão × Burst (idênticos), sem caminho (ilha desconectada → problema "bairro isolado").
