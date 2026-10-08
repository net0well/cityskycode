# Profiling

> **Status:** v1.0 · **Data:** 2026-10-08

---

## 1. Ferramentas
| Ferramenta | Uso |
|---|---|
| **Unity Profiler** (CPU, GPU, memória, render, UI) | primeira parada; marcadores customizados por sistema de simulação (`ProfilerMarker` via porta `IProfilerMarkers`) |
| **Profile Analyzer** | comparar capturas (antes/depois) |
| **Memory Profiler** | snapshots, vazamentos, texturas e malhas |
| **Frame Debugger** | ordem de draw calls, batching, passes |
| **Burst Inspector** | assembly gerado dos jobs |
| **RenderDoc / PIX / Nsight Graphics** | GPU em profundidade (sombras, overdraw, vértices) |
| **BenchmarkDotNet** | microbenchmarks do domínio no .NET 10 |
| **dotnet-counters / dotnet-trace** | Code Host e Worker (GC, CPU, latência de compilação) |
| **Painel interno de simulação** | ms por sistema por tick, itens processados, velocidade efetiva |

## 2. Procedimento
1. **Reproduzir** num cenário fixo (save de referência + câmera roteirizada), em build **de player** (IL2CPP, *Development Build* + *Autoconnect Profiler*), nunca só no Editor.
2. **Medir** a linha de base: 3 capturas de 300 frames.
3. **Identificar** o gargalo: CPU main? Render thread? GPU (sombras? vértices? overdraw?)? GC?
4. **Hipótese → mudança → medir de novo** com a mesma metodologia.
5. **Registrar** em `docs/perf/AAAA-MM-DD-<tema>.md`: cenário, números antes/depois, decisão.
6. Se a mudança for arquitetural, abrir ADR.

## 3. Cenários de referência
| Cenário | Conteúdo |
|---|---|
| `perf-mvp` | vila do MVP, 50 cidadãos, 10x por 10 min |
| `perf-render-5k` | 5.000 prédios, 20.000 árvores, câmera em 3 zooms (spike S3) |
| `perf-10k` | cidade de 10k com trânsito (Fase 11) |
| `perf-100k-headless` | domínio puro, 100k, 1 dia TC (BenchmarkDotNet + Mono) |

## 4. Armadilhas conhecidas
- Profiling no Editor superestima custos (*overhead* do editor e do Mono de debug).
- Medir com o VSync ligado esconde a folga real.
- GC do Mono/Boehm: picos de coleta aparecem como *spikes* esporádicos. Investigar alocações (*GC Alloc* por frame).
- A webview aberta consome GPU para composição. Medir com o painel aberto e fechado.
