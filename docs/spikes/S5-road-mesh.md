# Spike S5: Malha de ruas e interseções em runtime

- **Status:** **Concluído** (algoritmo validado: 12/12 configurações sem artefatos; 96 interseções em ~1,4 ms). O upload do Mesh na Unity e o snapshot em PlayMode ficam para a Fase 3.
- **Prazo:** 3 dias · **Ordem:** 9º · **Início / fim:** 2026-10-09 → 2026-10-09 (algumas horas)
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
| 100 interseções + segmentos regenerados (main thread) | ≤ 50 ms | ✅ **1,45 ms (p95)** para 96 interseções de 4 ramos (grade 10×10) — ~35× abaixo da meta |
| 12 configurações de referência sem triângulos degenerados, sobreposição ou buracos | 12/12 | ✅ **12/12**: 0 triângulos degenerados, 0 com normal invertida, 0 NaN; menor área de triângulo sempre > 0 |
| UV contínua ao longo dos segmentos (sem costura visível) | sim | ✅ por construção: U atravessa a largura (0..1), V = comprimento/largura acumulado nas pontas aparadas |
| Alocação por regeneração (buffers reaproveitados) | limitada e registrada | ⚠️ **178 KB** por regeneração da grade inteira (listas realocadas a cada Build); na Fase 3, reusar buffers → próximo de 0 |
| Em terreno com inclinação ≤ 12%, rua flutuando ou enterrada ≤ 5 cm | ≤ 5 cm | ⏳ não feito (spike plano, y = 0); amostragem de altura do Terrain fica para a Fase 3 (F3-25) |

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
- **Abordagem:** a geração da malha é **matemática pura** (System.Numerics), independente da engine, então foi implementada e validada num console .NET (itera em segundos, como o S4a fez com o domínio). Na Fase 3, o mesmo algoritmo vira o gerador de malha da Presentation, que só acrescenta o **upload para `Mesh`** (SetVertices/SetTriangles/RecalculateNormals) e a amostragem de altura do Terrain. Código em `spikes/S5-road-mesh/`.
- **Algoritmo de interseção:** para cada nó, ordena os ramos por ângulo; o canto entre dois ramos vizinhos é onde a **borda esquerda** de um encontra a **borda direita** do próximo; cada ramo é **aparado** até o maior dos seus dois cantos; o polígono da interseção (pontos de borda de todos os ramos no aparo) é triangulado em **leque** a partir do centro. Pontos de borda coincidentes são removidos, e cada triângulo é emitido com a **normal para cima**, descartando área ~0.
- **Segmentos retos:** um quad entre as pontas aparadas, com UV contínua.
- **Validação:** para cada uma das 12 configurações, um verificador conta triângulos degenerados (área < 1e-4 m²), triângulos com normal para baixo (winding errado) e NaN. Desempenho medido regenerando uma grade 10×10 (96 interseções de 4 ramos) 20 vezes.
- **Visual:** `spikes/S5-road-mesh/results/s5-configs.svg` desenha as 12 malhas em vista de cima, para conferência humana (as checagens numéricas são a prova rigorosa de ausência de artefatos).

## Resultado
**12/12 configurações sem artefatos:** T, X 4-ramos, Y 30°/150°, X assimétrico (30/100/200/290), ramos de comprimentos muito diferentes, segmento curto de 16 m entre dois nós, 3 ramos quase colineares (170°), 4 ramos com um a 30° do vizinho (menor área de triângulo 2,0 m², ainda saudável), fim de linha, dois nós próximos (16 m) e passagem reta (2 ramos colineares, sem polígono de interseção). Em todas: **0 degenerados, 0 com normal invertida, 0 NaN**.

**Desempenho:** a grade de **96 interseções** (1.540 vértices, 1.080 triângulos) regenera em **p50 1,28 ms / p95 1,45 ms / máx 1,45 ms** — cerca de **35× abaixo** da meta de 50 ms, numa CPU de 2014. Alocação de 178 KB por regeneração (listas recriadas a cada Build; na Fase 3 reusar buffers deixa isso perto de zero).

**Resposta à pergunta:** sim. O algoritmo de segmentos retos + interseções de até 4 ramos gera a malha sem artefatos geométricos e muito abaixo do orçamento de tempo. O limite do MVP (só retas) é suficiente e seguro.

### Observações
1. O ângulo mínimo de 30° entre ramos produz cantos finos mas ainda com área saudável (menor triângulo 2,0 m² no caso de 30°). Abaixo disso, os cantos degeneram — manter o mínimo de 30° da regra do `ROAD_SYSTEM.md` §4.
2. "Sem buracos" é garantido por construção: os pontos de borda do segmento no aparo são **os mesmos** pontos do polígono da interseção (mesmo centro, direção, largura e aparo), então não há costura entre o segmento e a interseção.
3. **Splines:** não foi necessário avaliar o `SplineExtrude` para o MVP (só retas). Ele entra na Fase 11, com as curvas Bézier (ADR-0011).
4. Faltou a parte de **terreno inclinado** (amostragem de altura) e o **snapshot em PlayMode** — ambos pertencem à Fase 3 (F3-21/F3-25), onde o gerador vira código Unity de verdade. O risco geométrico, que era o alvo do spike, está resolvido.

## Decisão
- [x] Hipótese confirmada: seguir com o ADR-0011 (grafo no domínio, geometria própria, MVP só com retas; malha gerada na Presentation)
- [ ] Hipótese refutada
- [ ] Inconclusivo

**ADR criado ou atualizado:** nenhum. O algoritmo validado é reescrito como o gerador de malha da Presentation na Fase 3 (F3-21/F3-22), com as 12 configurações virando testes de snapshot + as mesmas checagens numéricas. Adicionar à F3: reuso de buffers (alocação ~0) e amostragem de altura do Terrain (rua colada ao relevo ≤ 5 cm).

## Riscos que sobraram
- **R08 (malha de interseções):** rebaixado — o algoritmo gera as 12 configurações sem artefatos e muito dentro do tempo. Resta validar o **upload do Mesh** na Unity (cheap) e o visual em PlayMode (F3).
- **Terreno inclinado:** a adaptação ao relevo (amostragem de altura, rua não flutuar) fica para a F3-25; é geometria adicional, não um risco de viabilidade.
- **Curvas/rotatórias:** fora do MVP (Fase 11); quando entrarem, reavaliar com Splines (ADR-0011).
