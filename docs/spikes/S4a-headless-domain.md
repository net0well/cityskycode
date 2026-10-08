# Spike S4a: Domínio headless com 100 mil cidadãos (.NET 10)

- **Status:** **Concluído** (hipótese confirmada, com folga de ~18×)
- **Prazo:** 2 dias · **Ordem:** 3º · **Início / fim:** 2026-10-08 → 2026-10-08 (meio dia)
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
| Custo médio por tick, 100k cidadãos, 1 dia TC (11.520 ticks) | ≤ 1 ms | ✅ **0,055 ms** (medidor) · 0,076 ms (BenchmarkDotNet) |
| Pior tick no mesmo dia | ≤ 5 ms | ✅ **0,52 ms** (p99 0,26 ms) |
| Alocação por tick em regime (`[MemoryDiagnoser]`) | 0 B | ✅ 0 B no dia inteiro |
| Memória dos stores (100k) | ≤ 40 MB | ✅ **8,3 MB** |
| Custo por tick com 1k e 10k cidadãos (curva) | registrado | ✅ 0,0011 ms e 0,0047 ms (≈ linear) |
| Hash de estado igual em 2 execuções com a mesma seed | sim | ✅ `DC336B8F860D4770` nas duas; seed 43 dá outro hash (controle) |

## Escopo
**Inclui:** `CitizenStore` com os campos do `DOMAIN_MODEL.md` §4.1; `ScheduleSystem` simplificado (máquina de estados com horários e baldes de 120); `NeedsSystem` (4 necessidades, baldes de 480); `HappinessSystem` (fórmula do `HAPPINESS_SYSTEM.md` §3 em ponto fixo, contribuições agregadas por prédio); viagens sintéticas com tempo de caminho vindo de uma tabela (sem pathfinding real); PCG32; hash de estado (no spike, FNV-1a 64; o produto usará xxHash64).
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
- **Ambiente medido:** Intel Xeon E5-2670 v3 (2,3 GHz, 12 núcleos / 24 lógicos, 2014), 32 GB, Windows 11 25H2, .NET 10.0.11 (RyuJIT x86-64-v3), GC Workstation, build Release, sem depurador. É uma CPU de **2014**, mais lenta por núcleo que um notebook atual: os números tendem a ser conservadores para o .NET.
- **Código:** `spikes/S4a-headless-domain/` com duas partes:
  - `CitySky.Spike.Domain` (`netstandard2.1`, C# 9, sem dependências), que o S4b importa na Unity sem mudanças. Contém `CitizenStore` (SoA, 14 arrays), `BuildingStore`, `TripStore` (lista compacta com swap-remove, progresso Q16.16), `ScheduleSystem` (baldes de 120), `TripSystem` (todo tick), `NeedsSystem` (baldes de 480, fase 0), `HappinessSystem` (baldes de 480, fase 240, contribuições agregadas por prédio de forma incremental), `Pcg32`, `StateHasher` e `PopulationGenerator`.
  - `CitySky.Spike.Bench` (`net10.0`): um medidor próprio que cronometra cada um dos 11.520 ticks do 2º dia (depois de 1 dia de aquecimento), mais um benchmark BenchmarkDotNet 0.15.8 com `[MemoryDiagnoser]`.
- **Cidade sintética:** domicílios de 1–4 pessoas, 90% empregados, turno de 8 h começando entre 07:00 e 09:00, ida ao trabalho de 30–150 min TC, ida ao mercado de 10–60 min TC, 3% das casas sem energia, alíquota de 10%.
- **Como rodar:** `dotnet run -c Release --project src/CitySky.Spike.Bench` (medidor) e `… -- bdn --filter "*" --job short` (BenchmarkDotNet).

## Resultado
**Medidor por tick (2º dia de jogo, 3 rodadas por tamanho; mostrada a rodada 2):**

| Cidadãos | Média (ms) | p50 | p95 | p99 | Máx | Máx na virada de hora | Alocado no dia | Viagens ativas (média / máx) |
|---:|---:|---:|---:|---:|---:|---:|---:|---|
| 1.000 | 0,0011 | 0,0007 | 0,0026 | 0,0031 | 0,0463 | 0,0028 | 0 B | 110 / 750 |
| 10.000 | 0,0046 | 0,0011 | 0,0203 | 0,0228 | 0,0707 | 0,0327 | 0 B | 1.070 / 7.206 |
| 100.000 | 0,0555 | 0,0180 | 0,2222 | 0,2623 | 0,3966 | 0,2568 | 0 B | 10.635 / 71.781 |

As 3 rodadas de 100k ficaram entre 0,0551 e 0,0555 ms de média; o pior tick variou entre 0,40 e 0,52 ms. Única anomalia: a 1ª rodada de 10k teve um tick de 1,13 ms (aquecimento do JIT em camadas), sem repetição nas rodadas seguintes.

**BenchmarkDotNet (ShortRun):** 1k = 396 ns · 10k = 4,0 µs · 100k = 75,7 µs por tick, **0 B alocados** em todos. O erro alto em 100k (±379 µs) é esperado: o estado avança a cada invocação e a média percorre horas com cargas muito diferentes (madrugada × pico da manhã). O medidor próprio é a referência para percentis.

**Memória dos stores:** 0,1 MB (1k) · 0,8 MB (10k) · **8,3 MB (100k)**, cerca de 87 bytes por cidadão, incluindo prédios e viagens.

**Determinismo:** mesma seed, 2 dias, 100k → `DC336B8F860D4770` nas duas execuções; seed 43 → `A50626FF146B2A71` (o hash muda quando deve mudar).

**Resposta à pergunta:** com 100 mil cidadãos, o tick médio custa **~0,06 ms** no .NET 10 (meta: 1 ms, folga de ~18×), o pior tick **~0,5 ms** (meta: 5 ms, folga de ~10×), sem alocação e com 8 MB de estado.

### Achado: o pico de viagens simultâneas é muito maior que o previsto
O `PERFORMANCE.md` §2 supõe **~20.000 viagens ativas** para 100k cidadãos (20%). A medição deu **média de 10,6k (11%), mas pico de 71,8k (72%)** no começo da manhã. Duas causas:
1. **A escala temporal do ADR-0008:** viagens longas em tempo de calendário (1 km a pé ≈ 2 h TC). Ida e volta do trabalho ocupam boa parte da manhã e do fim da tarde.
2. **Sincronização artificial:** no gerador, quem deveria sair antes das 06:00 só acorda às 06:00 e sai junto com todos os outros; além disso, a agenda decide em baldes de 15 min.

O custo de CPU não é problema: o pior tick, já com o pico, ficou em 0,5 ms. Mas o pico afeta três coisas:
- o **limite de viagens concorrentes** do `SIMULATION_LOD.md` §2 (20.000 na Fase 18);
- o orçamento de **agentes visíveis** e do **tráfego meso** (Fase 11), que foram dimensionados para ~20% de viagens ativas;
- a **sensação** da cidade: se 70% dos moradores estiverem na rua ao mesmo tempo, a manhã vira um formigueiro e a tarde fica vazia.

Calibrar a distribuição de horários (turnos variados, acordar conforme o tempo de viagem) e a escala de mobilidade é tarefa da Fase 5 (F5-30, risco R13), e o número medido entra nas premissas da Fase 11.

## Decisão
- [x] Hipótese confirmada: seguir com a arquitetura documentada
- [ ] Hipótese refutada: alternativa adotada = …
- [ ] Inconclusivo: próximo passo = …

**ADR criado ou atualizado:** nenhum. Os stores SoA com baldes (ADR-0004, ADR-0009) se sustentam com folga, e a thread dedicada continua na Fase 18. Atualizados `PERFORMANCE.md` §2–3 (números medidos e premissa de viagens ativas) e o plano da Fase 5 (F5-30).

## Riscos que sobraram
- **R06 (Mono/IL2CPP mais lento):** continua aberto até o S4b. Com folga de 18× no .NET, só uma penalidade acima de 10× no IL2CPP ameaçaria o orçamento; o S4b mede isso com este mesmo código.
- **Faltam sistemas no tick:** pathfinding real, economia, energia e tráfego meso ainda não entram neste número. A folga existe para eles, mas cada sistema novo mede seu custo (trilha contínua de performance).
- **Pico de viagens simultâneas** (achado acima): precisa de calibração na Fase 5 e revisão das premissas da Fase 11.
