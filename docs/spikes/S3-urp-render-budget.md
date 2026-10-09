# Spike S3: Orçamento de renderização do URP em GPU integrada

- **Status:** Planejado
- **Prazo:** 3 dias · **Ordem:** 7º
- **Depende de:** Unity 6.6 (F0-04), **PC com Intel Iris Xe ou similar** (F0-06) · **Bloqueia:** gate da Fase 2
- **Código:** `spikes/S3-urp-render-budget/` (descartável; a cena vira `perf-render` reescrita na F2)
- **ADRs relacionados:** [ADR-0002](../ADR/0002-render-pipeline.md), [ADR-0017](../ADR/0017-product-scope-classroom.md) · Documentos: [`GRAPHICS.md`](../GRAPHICS.md), [`PERFORMANCE.md`](../PERFORMANCE.md), [`PROFILING.md`](../PROFILING.md)

## Pergunta
O URP (Forward+, GPU Resident Drawer, GPU Occlusion Culling, STP) mantém **≥ 30 FPS a 1080p no preset Baixo** num PC com **Intel Iris Xe e 8 GB**, com **5.000 prédios** low-poly (kits Kenney), **20.000 árvores** e **1.000 agentes** animados, de dia e de noite?

## Hipótese
Sim, desde que: o GRD faça o *instancing* dos prédios; a distância de sombra seja curta (1 cascata, 400 m); props pequenos não projetem sombra; o STP renderize a 67% da resolução. Os 1.000 agentes animados (o MVP tem só 50) são o item mais arriscado no CPU; o número serve para medir a folga.

## Por que importa
A GPU é o gargalo mais provável de um city builder (R05; lição de *Cities: Skylines II*). O hardware mínimo (Iris Xe) foi confirmado pelo dono do produto. Se a meta falhar, mudam os presets, a direção de arte ou o hardware mínimo (revisão do ADR-0017).

## Critério de sucesso (definido antes de começar)
| Métrica (Iris Xe, 1080p, preset Baixo, build IL2CPP) | Alvo | Resultado |
|---|---|---|
| FPS p5 em zoom próximo (~50 m), médio (~300 m) e distante (~1.200 m), de dia | ≥ 30 | |
| Idem à noite (Forward+, até 64 luzes reais + emissivos) | ≥ 30 | |
| Tempo de GPU por frame (p95) | ≤ 30 ms | |
| Tempo de CPU no main thread (p95) | ≤ 20 ms | |
| Memória do processo | ≤ 2 GB | |
| Draw calls / batches (registrar com e sem GRD) | registrado | |
| Mesma cena no PC de desenvolvimento (RX 7600), preset Alto | ≥ 60 FPS | |

## Escopo
**Inclui:** gerador procedural da cena (grade de quarteirões com prédios Kenney variados, árvores, 1.000 agentes Quaternius com Animator e culling), URP configurado nos presets Baixo e Alto, câmera roteirizada nos 3 zooms, ciclo dia/noite simplificado, captura de métricas em build de player.
**Não inclui:** simulação (os agentes andam em trajetos fixos), ruas com malha final (S5), overlays.

## Ambiente
PC alvo: registrar modelo, CPU, GPU (Iris Xe / driver), RAM, resolução, se está na tomada (notebook em bateria reduz desempenho). PC de desenvolvimento: Radeon RX 7600, 32 GB. Unity 6.6, URP da versão.

## Plano de execução
**Dia 1:** importar kits; verificar polígonos (os kits Kenney já são low-poly: medir se LOD ajuda ou se o gargalo é draw call/sombra); gerar LODs com o **Mesh LOD** da Unity 6.2+ na importação (Blender só onde falhar); gerador da cena; configuração do URP.
**Dia 2:** câmera roteirizada; captura por `FrameTimingManager` gravando CSV (funciona sem o Editor conectado, útil no PC emprestado); build IL2CPP *Development* e *Release*; primeira rodada no Iris Xe.
**Dia 3:** variações (GRD ligado/desligado, Mesh LOD ligado/desligado, distância de sombra, SSAO, STP 67% × 100%, densidade de árvores, número de agentes 50/300/1.000) e, se der tempo, custo dos complementos gratuitos de `docs/art/ART_SOURCES.md` §3 (fog volumétrico, nuvens em domo); relatório com a configuração recomendada do preset Baixo.

## Método de medição
3 capturas de 300 frames por zoom e por período (dia/noite), VSync desligado, após 30 s de aquecimento (`PROFILING.md` §2). Percentis calculados do CSV. Memória por `Profiler.GetTotalReservedMemoryLong` e Gerenciador de Tarefas.

## Armadilhas conhecidas
- Medir no Editor superestima custos: só vale build de player.
- Notebook em bateria ou com plano de energia "Equilibrado" derruba a GPU integrada: registrar e medir na tomada, em "Melhor desempenho".
- Driver antigo de GPU integrada (comum em escola): registrar a versão e, se possível, medir também com driver atualizado.
- O GRD tem requisitos de configuração do URP (Forward+, variantes do BatchRendererGroup); conferir na documentação da versão.

## Saídas esperadas
- Configuração do preset Baixo validada → `GRAPHICS.md` §3 e `PERFORMANCE.md` §3.
- Orçamento de agentes visíveis por preset → `SIMULATION_LOD.md` §3.
- Cena e câmera de referência → `perf-render` (F2-50).
- Se falhar: proposta de preset "Muito baixo" ou revisão do hardware mínimo com o dono do produto.

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
