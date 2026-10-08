# Performance

> **Status:** v1.0 (orçamentos iniciais, a validar por profiling) · **Data:** 2026-10-08 · **MVP:** sim
> Ferramentas e procedimentos: `PROFILING.md`.

---

## 1. Plataformas-alvo
| Perfil | Hardware | Alvo |
|---|---|---|
| **Mínimo** | 4 núcleos, 8 GB, GPU integrada (Intel Iris Xe / Radeon 680M), SSD | 30 FPS, 1080p, preset Baixo |
| **Recomendado** | 6 núcleos, 16 GB, GTX 1660 / RX 6600 | 60 FPS, 1080p, preset Alto |
| **Alto** | 8 núcleos, 32 GB, RTX 3070+ | 60+ FPS, 1440p, Ultra |

O perfil **mínimo foi confirmado** pelo dono do produto (ADR-0017). Como o jogo será usado em **laboratórios escolares**, o piloto mede em PCs reais de escola. Se o parque for mais fraco (ex.: Intel UHD 620, 4 GB), avalia-se um preset "Muito baixo" (720p interno, sem sombras dinâmicas, agentes visíveis reduzidos).

## 2. Escala progressiva (§54)
| Marco | Cidadãos | Viagens ativas | Agentes visíveis | Fase | Mínimo / Recomendado |
|---|---:|---:|---:|---|---|
| MVP | 50 | ≤ 50 | 50 | 8 | 30 / 60 FPS |
| 1k | 1.000 | ~300 | 300 | 10 | 30 / 60 |
| 10k | 10.000 | ~2.000 | 2.000 | 11 | 30 / 60 |
| 50k | 50.000 | ~10.000 | 6.000 | 18 | 25 / 50 |
| 100k+ | 100.000+ | ~20.000 | 10.000 | 18 | — / 40 |

**Não assumir simulação individual completa para tudo** (§54): ver `SIMULATION_LOD.md` (camadas S0/S1/S2 independentes da câmera e R0–R3 por câmera).

## 3. Orçamento de frame (Recomendado, 60 FPS = 16,6 ms)
| Item | CPU main | Worker threads | GPU |
|---|---:|---:|---:|
| Simulação (média em 1x/10x) | ≤ 2 / ≤ 6 ms | (Fase 18: em thread própria) | — |
| Apresentação (sync de read models, agentes, LOD) | ≤ 3 ms | Jobs: movimento visual ≤ 2 ms | — |
| UI Toolkit | ≤ 1,5 ms | — | ≤ 1 ms |
| Renderização (submissão) | ≤ 3 ms | render thread | ≤ 12 ms |
| Webview (painel aberto) | ≤ 0,5 ms | processo próprio | composição ≤ 0,5 ms |
| Folga | ≥ 4 ms | — | ≥ 2 ms |

Simulação por tick (alvos .NET 10, a medir também em Mono/IL2CPP):
| Escala | ms/tick (médio) | Pior tick (pico de hora cheia) |
|---|---:|---:|
| MVP | ≤ 0,1 | ≤ 0,5 |
| 10k | ≤ 0,5 | ≤ 2 |
| 100k | ≤ 1,5 | ≤ 5 (fases defasadas evitam picos) |

## 4. Memória
| Item | MVP | 100k |
|---|---|---|
| Domínio (stores) | < 10 MB | ~100–200 MB |
| Caches (caminhos, campos) | < 10 MB | ~100–300 MB (LRU) |
| Assets em memória | < 1,2 GB | < 2,5 GB |
| Code Host + Worker | ~250 MB | ~300 MB |
| Webview | ~150–400 MB | idem |
| **Total do processo do jogo** | < 2 GB | < 4,5 GB |

## 5. Regras de código para desempenho
- **Zero alocação por frame/tick** nos caminhos quentes (Domain e Presentation): sem LINQ, sem *closures*, sem boxing, sem `foreach` sobre interfaces, sem formatação de strings.
- SoA, laços lineares, IDs em vez de referências, pooling.
- Trabalho caro sempre **time-sliced** (baldes) ou **event-driven** (dirty flags).
- Dados para a GPU em lote (GRD/BRG, Entities Graphics), nunca um GameObject por elemento repetitivo.
- **Medir antes de otimizar.** Otimizações sem medição não entram (`performance-analyst` do dotnet-claude-kit revisa).

## 6. Testes de desempenho automatizados
- **BenchmarkDotNet** (`dotnet/benchmarks`): sistemas do domínio em cenários de 1k/10k/100k. Histórico no CI noturno, com alerta se ficar > 10% mais lento.
- **Unity Performance Testing** (PlayMode): cenas de referência (MVP, 10k), medindo FPS, ms por marcador do Profiler e GC alloc por frame (deve ser 0 no estado estável).
- **Cenas de estresse** do spike S3 mantidas como teste de regressão de renderização.
- Lembrete: Mono/IL2CPP ≠ .NET 10. Os benchmarks .NET são **limite inferior**, e as decisões finais valem com medição no player IL2CPP.
