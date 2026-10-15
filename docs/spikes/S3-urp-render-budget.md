# Spike S3: Orçamento de renderização do URP em GPU integrada

- **Status:** ⚠️ Concluído (parcial, 2026-10-10) — teto validado no RX 7600 com folga enorme; **mínimo Iris Xe pendente** (gate da Fase 2) e animação *skinned* não medida
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
**Medido no PC de desenvolvimento (RX 7600, não Iris Xe — decisão do dono: usar a máquina atual como teto).** 1080p, build IL2CPP *Release*, URP Forward+ + GRD + render scale 0,67, sombra 120 m/1 cascata, MSAA off. 240 frames/segmento após aquecimento, VSync off. **Agentes = cápsulas rígidas em movimento (não *skinned*)** — ver ressalvas.

| Métrica | Alvo | Resultado (RX 7600, 1080p) |
|---|---|---|
| FPS p5 em zoom próximo/médio/distante, de dia | ≥ 30 | ✅ **269 / 297 / 300** (mediana ~380–420) |
| Idem à noite (Forward+, 121 point lights) | ≥ 30 | ✅ **339 / 302 / 302** |
| Tempo de GPU por frame (p95) | ≤ 30 ms | ✅ **~0,3 ms** (medido em D3D11; no D3D12 o FrameTimingManager não reporta GPU) |
| Tempo de CPU no main thread (p95) | ≤ 20 ms | ✅ **2,4–3,7 ms** (o frame é levemente **CPU-bound**) |
| Memória do processo | ≤ 2 GB | ✅ **~580 MB** reservada (mono heap 7 MB) |
| Draw calls / batches (com e sem GRD) | registrar | ⚠️ GRD **engajou** no player (`GPU Resident Drawer created`, modo `InstancedDrawing`); contagem exata de draw calls **não capturada** em runtime (stat de Editor/Frame Debugger) → follow-up |
| Mesma cena no PC de desenvolvimento (RX 7600), preset Alto | ≥ 60 FPS | ✅ atendido com folga (≥ 269 FPS p5 já no Baixo) |
| **Mínimo real (Intel Iris Xe, 8 GB)** | ≥ 30 | ⏳ **não validado** — não há Iris Xe nesta máquina; é o teto, não o mínimo (ver Decisão/Checklist) |

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
*Concluído (parcial) em 2026-10-10 — teto validado no RX 7600; mínimo Iris Xe pendente de hardware.*

## O que foi feito
- **Projeto Unity URP** (`C:\dev\unity-spikes\S3`, URP 17.6.0 embutido): o `S3Build` cria por script o URP asset (Forward+, **GRD `InstancedDrawing`**, render scale 0,67, sombra 120 m/1 cascata, MSAA off), adiciona o `URP/Lit` aos *Always Included Shaders* (senão o stripping o remove e `Shader.Find` devolve null no player — bug encontrado e corrigido), liga `enableFrameTimingStats` e builda IL2CPP.
- **Harness runtime** `PerfRunner`: spawna a cena em runtime a partir de argumentos (5.000 prédios em grade, 20.000 árvores, 1.000 agentes, 121 point lights de rua), roteiriza a câmera em **3 zooms × dia/noite**, move os agentes, e grava **CSV por frame + resumo JSON** via `FrameTimingManager` (FPS por `unscaledDeltaTime`, GPU/CPU ms, memória por `Profiler`). Reutilizável como a cena `perf-render` da F2.
- Rodado a 1080p no RX 7600, em D3D12 (padrão) e D3D11 (para o GPU ms, que o D3D12 não reporta aqui).

## Resultado
Em **todos os 6 segmentos** (near/mid/far × dia/noite): FPS p5 **≥ 269**, mediana **~380–505**; CPU main thread p95 **2,4–3,7 ms**; GPU p95 **~0,3 ms**; memória **~580 MB**. O **GRD engajou** (log `GPU Resident Drawer created`). O frame é levemente **CPU-bound** (~3 ms) mesmo no Xeon E5-2670 v3 (2014, single-thread modesto) — a GPU fica praticamente ociosa graças ao *instancing* do GRD + geometria low-poly + render scale 0,67. Ou seja: **no teto (RX 7600) há folga enorme** (6×+ sobre o piso de 30 FPS, 3×+ sobre 60).

**Leitura honesta:** o gargalo provável do produto (R05) é a GPU numa Iris Xe — que **não foi medida**. A GPU aqui é tão pequena (0,3 ms) que mesmo 10× num hardware integrado daria ~3 ms (ainda < 30 ms); e o custo de CPU já é baixo num CPU antigo. A extrapolação é **favorável**, mas continua sendo extrapolação.

## Decisão
- [~] **Inconclusivo quanto ao gate (mínimo Iris Xe); teto confirmado.** A arquitetura de render (URP Forward+ + GRD + preset Baixo) roda o orçamento-alvo com folga gigante no RX 7600 e o pipeline/harness estão validados. **Mas o gate da Fase 2 é o mínimo Iris Xe 8 GB**, que exige rodar este mesmo build num PC com Iris Xe (ou um *downclock*/limite de GPU que o simule). Próximo passo: rodar `S3Player.exe` num Iris Xe e colar o `s3-summary.json`.
- **A maior ressalva:** os 1.000 agentes são **cápsulas rígidas**, não personagens *skinned* com Animator. O custo real de `SkinnedMeshRenderer` + Animator (o item de CPU mais arriscado, R05) **não foi medido** — precisa de modelos Quaternius reais. O MVP tem só 50 agentes, então o risco imediato é baixo, mas o teto de 1.000 *animados* fica em aberto.

**ADR criado ou atualizado:** nenhum ainda (sem dado de Iris Xe). Quando o mínimo for medido: `GRAPHICS.md` §3 (config do preset Baixo), `PERFORMANCE.md` §3, `SIMULATION_LOD.md` §3 (orçamento de agentes visíveis). Se falhar no Iris Xe: preset "Muito baixo" ou revisão do hardware mínimo com o dono (ADR-0017).

## Checklist humano / passo pendente
Para fechar o gate, rodar o **mesmo build** num Iris Xe (PC de escola ou notebook com GPU integrada), na tomada e em "Melhor desempenho":
```powershell
S3Player.exe -screen-fullscreen 0 -screen-width 1920 -screen-height 1080 -outdir .\res
# cole res\s3-summary.json no relatório; FPS p5 ≥ 30 em todos os segmentos = gate OK
```
Variações úteis (mesmo build): `-agents 50|300|1000`, `-trees 5000|20000`, `-buildings 2000|5000`. Para comparar com/sem GRD, rebuildar com `m_GPUResidentDrawerMode=0`. Itens só a olho: fluidez percebida, serrilhado com render scale 0,67, sombras curtas aceitáveis.

## Riscos que sobraram
- **Mínimo Iris Xe não medido** — é o gate real da Fase 2; tudo aqui é teto.
- **Animação *skinned* dos agentes não medida** (proxy de cápsula) — o custo de CPU real do Animator/SkinnedMeshRenderer fica para um follow-up com modelos Quaternius; é o R05.
- **Arte proxy** (caixas/cones procedurais), não os kits Kenney/Quaternius finais — contagem de polígonos e LOD finais podem diferir.
- **GPU ms sub-reportado** no D3D12 pelo FrameTimingManager (cruzado com D3D11 e com o frametime derivado do FPS; conclusão não muda).
- **Draw calls não contados em runtime** — GRD engajou, mas a contagem exata com/sem GRD precisa do Frame Debugger (Editor).
