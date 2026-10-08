# Spike S4a: Domínio headless com 100 mil cidadãos (.NET 10)

- **Status:** Planejado
- **Prazo:** 2 dias · **Ordem:** 3º
- **Depende de:** .NET SDK 10 · **Bloqueia:** S4b, Fase 5 (orçamento de CPU por cidadão)
- **Código:** `spikes/S4a-headless-domain/` (descartável)
- **ADRs relacionados:** [ADR-0004](../ADR/0004-domain-architecture.md), [ADR-0008](../ADR/0008-simulation-tick.md), [ADR-0009](../ADR/0009-citizen-simulation.md) · Documentos: [`DOMAIN_MODEL.md`](../DOMAIN_MODEL.md) §4, [`SIMULATION_ARCHITECTURE.md`](../SIMULATION_ARCHITECTURE.md) §2.3, [`PERFORMANCE.md`](../PERFORMANCE.md) §3

## Pergunta
Com stores SoA, IDs inteiros e atualização em baldes, quanto custa um tick (médio e pior caso) para **100 mil cidadãos** com agenda, necessidades, felicidade e ~20 mil viagens ativas, no .NET 10, sem alocar memória por tick?

## Hipótese
≤ 1 ms por tick em média no .NET 10: por tick, a agenda processa ~830 cidadãos (100k/120), necessidades e felicidade ~210 cada (100k/480), e as viagens só avançam um contador. O pior tick fica em torno da virada de hora, quando sistemas de 480 ticks coincidem; fases defasadas devem mantê-lo ≤ 5 ms.

## Por que importa
Valida que o modelo orientado a dados escala como prometido **antes** de existirem sistemas reais. Se o custo for muito maior, a thread de simulação dedicada (planejada para a F18) ou Burst via portas precisam vir antes.

## Critério de sucesso (definido antes de começar)
| Métrica | Alvo | Resultado |
|---|---|---|
| Custo médio por tick, 100k cidadãos, 1 dia TC (11.520 ticks) | ≤ 1 ms | |
| Pior tick no mesmo dia | ≤ 5 ms | |
| Alocação por tick em regime (`[MemoryDiagnoser]`) | 0 B | |
| Memória dos stores (100k) | ≤ 40 MB | |
| Custo por tick com 1k e 10k cidadãos (curva) | registrado | |
| Hash de estado igual em 2 execuções com a mesma seed | sim | |

## Escopo
**Inclui:** `CitizenStore` com os campos do `DOMAIN_MODEL.md` §4.1; `ScheduleSystem` simplificado (máquina de estados com horários e baldes de 120); `NeedsSystem` (4 necessidades, baldes de 480); `HappinessSystem` (fórmula do `HAPPINESS_SYSTEM.md` §3 em ponto fixo, contribuições agregadas por prédio); viagens sintéticas com tempo de caminho vindo de uma tabela (sem pathfinding real); PCG32; xxHash64 para o hash.
**Não inclui:** pathfinding real, economia, energia, Unity.

**Importante:** o código do domínio do spike é escrito como biblioteca `netstandard2.1` com `LangVersion 9` (Perfil U), num projeto separado do host de benchmark (`net10.0`). Assim o **S4b reaproveita o mesmo código na Unity sem mudanças**.

## Ambiente
Máquina de desenvolvimento (registrar CPU e frequência), .NET SDK 10.0.400, BenchmarkDotNet (última estável), build Release, sem depurador.

## Plano de execução
**Dia 1:** stores e sistemas sintéticos; população inicial gerada por seed (domicílios, empregos fictícios, 20% em viagem a qualquer momento); runner que executa N ticks e mede cada tick com `Stopwatch.GetTimestamp`.
**Dia 2:** BenchmarkDotNet com cenários de 1k/10k/100k (médio por tick num dia TC; pior tick; tick de virada de hora); `[MemoryDiagnoser]`; teste de hash; relatório. Revisão da metodologia pelo `performance-analyst`.

## Método de medição
BenchmarkDotNet para média e alocação; histograma próprio por tick (percentis p50/p95/p99/máx) a partir dos timestamps; memória por `GC.GetTotalMemory(true)` antes e depois de criar os stores.

## Armadilhas conhecidas
- Medir só o caso médio esconde o pico da virada de hora: o pior tick é o que causa engasgos visíveis.
- `foreach` sobre interfaces, LINQ e *closures* alocam: o código do spike segue as regras do `PERFORMANCE.md` §5, senão o número não representa o produto.
- O .NET 10 é **mais rápido** que o Mono da Unity: estes números são um limite inferior. O fator real vem do S4b.

## Saídas esperadas
- Números para `PERFORMANCE.md` §3 e `CITIZEN_SYSTEM.md` §9.
- Código-base do benchmark para o S4b.
- Se o alvo falhar: identificar o sistema mais caro e registrar a mitigação (baldes maiores, campos mais compactos, Burst via porta).

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
