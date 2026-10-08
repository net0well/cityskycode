# Spike S5: Malha de ruas e interseções em runtime

- **Status:** Planejado
- **Prazo:** 3 dias · **Ordem:** 9º
- **Depende de:** Unity 6.6 (F0-04) · **Bloqueia:** Fase 3
- **Código:** `spikes/S5-road-mesh/` (descartável)
- **ADRs relacionados:** [ADR-0011](../ADR/0011-road-system.md) · Documento: [`ROAD_SYSTEM.md`](../ROAD_SYSTEM.md) §8

## Pergunta
Conseguimos gerar em runtime a malha de **segmentos retos** (pista + meio-fio + calçada) e de **interseções de até 4 ramos** (ângulo mínimo de 30°), sem artefatos, regenerando **100 interseções em ≤ 50 ms** no player IL2CPP?

## Hipótese
Sim. Com segmentos retos, o polígono da interseção sai da interseção das bordas recuadas de ramos adjacentes, com cantos arredondados por arcos e triangulação simples. O custo é dominado por alocação: com buffers reaproveitados, 100 interseções cabem em dezenas de milissegundos.

## Por que importa
A malha de interseções é o ponto técnico de maior risco gráfico das estradas (R08). O MVP limita as ruas a retas justamente para reduzir esse risco; o spike confirma que o limite basta.

## Critério de sucesso (definido antes de começar)
| Métrica | Alvo | Resultado |
|---|---|---|
| 100 interseções + segmentos regenerados (main thread, IL2CPP) | ≤ 50 ms | |
| 12 configurações de referência sem triângulos degenerados, sobreposição ou buracos (inspeção + teste automático) | 12/12 | |
| UV contínua ao longo dos segmentos (sem costura visível) | sim | |
| Alocação por regeneração (buffers reaproveitados) | limitada e registrada | |
| Em terreno com inclinação ≤ 12%, rua flutuando ou enterrada | ≤ 5 cm | |

**Configurações de referência:** T a 90°; X a 90°; Y com ramos a 30°/150°; X assimétrico (30°, 100°, 200°, 290°); ramos de comprimentos muito diferentes; segmento curto (16 m) entre duas interseções; interseção em terreno inclinado; 3 ramos quase colineares (170°); 4 ramos com um a 30° do vizinho; nó de fim de rua (*cul-de-sac* simples); dois nós próximos (≥ 16 m); interseção na borda da área construível.

## Escopo
**Inclui:** grafo mínimo em C# puro com coordenadas em mm (como o domínio fará); extrusão de perfil; algoritmo de interseção; cantos de calçada; amostragem de altura do Terrain; harness de snapshot (captura de imagem + verificações geométricas); avaliação rápida do **Unity Splines** (`SplineExtrude`) para a ferramenta de pré-visualização.
**Não inclui:** curvas, rotatórias, faixas configuráveis (Fase 11), decals finais, combinação por *chunk* (F3-23).

## Ambiente
Windows 11, Unity 6.6, URP, pacote Splines da versão.

## Plano de execução
**Dia 1:** grafo + extrusão de segmentos com UV contínua; amostragem do terreno.
**Dia 2:** polígono de interseção (bordas recuadas, interseção de retas, arcos de canto, triangulação em leque a partir do centro ou *ear clipping*); calçadas nos cantos.
**Dia 3:** as 12 configurações + verificações automáticas (área mínima de triângulo, normais para cima, sem arestas abertas); medição de desempenho; teste do `SplineExtrude` para pré-visualização; relatório.

## Método de medição
`Stopwatch` em build IL2CPP (média de 20 regenerações completas); *GC Alloc* no Profiler; snapshots PNG comparados a um *golden* com tolerância.

## Armadilhas conhecidas
- Ângulos agudos (30°) produzem cantos longos e finos: limitar o recuo e testar o caso extremo.
- Precisão: converter mm (inteiros) para `float` só na apresentação, perto da origem do chunk, para evitar tremulação longe da origem.
- Z-fighting entre calçada e terreno: pequeno *offset* ou *decal*.
- O Splines ajuda na pré-visualização e nas curvas, mas não resolve interseções; não deve virar a fonte da verdade (ADR-0011).

## Saídas esperadas
- Algoritmo de interseção validado → reescrito em F3-21/F3-22, com as 12 configurações virando testes de snapshot.
- Decisão sobre o uso do Splines no MVP (provavelmente só na Fase 11, com curvas) → nota no ADR-0011.

---
*As seções abaixo são preenchidas ao terminar.*

## O que foi feito

## Resultado

## Decisão
- [ ] Hipótese confirmada: seguir com a arquitetura documentada
- [ ] Hipótese refutada: alternativa adotada = …
- [ ] Inconclusivo: próximo passo = …

**ADR criado ou atualizado:**

## Riscos que sobraram
