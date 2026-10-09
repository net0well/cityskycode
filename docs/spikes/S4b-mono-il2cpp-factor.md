# Spike S4b: Fator de custo Mono/IL2CPP e determinismo entre runtimes

- **Status:** **Concluído** (hipótese confirmada; IL2CPP ~1,34× o .NET 10; hash idêntico nos 4 runtimes)
- **Prazo:** 1 dia · **Ordem:** 8º · **Início / fim:** 2026-10-09 → 2026-10-09 (algumas horas)
- **Depende de:** S4a (código do domínio sintético em C# 9 / `netstandard2.1`), Unity 6.6 (F0-04) · **Bloqueia:** Fase 5
- **Código:** `spikes/S4b-mono-il2cpp-factor/` (descartável)
- **ADRs relacionados:** [ADR-0004](../ADR/0004-domain-architecture.md), [ADR-0008](../ADR/0008-simulation-tick.md) · Documentos: [`PERFORMANCE.md`](../PERFORMANCE.md) §6, [`TESTING.md`](../TESTING.md) §A4

## Pergunta
Quanto mais lento é o mesmo benchmark do S4a quando executado no **Editor (Mono)**, no **Player Mono** e no **Player IL2CPP**, comparado ao **.NET 10**? E o **hash de estado** é idêntico nos quatro runtimes?

## Hipótese
IL2CPP fica entre 1,2× e 2× o custo do .NET 10; Mono no Editor fica bem pior (depuração e *overhead* do Editor). O hash é idêntico porque o estado é inteiro e a ordem de iteração é explícita.

## Por que importa
Os benchmarks .NET são **limite inferior** (`PERFORMANCE.md` §6). O fator real define os orçamentos de CPU por cidadão e se a thread dedicada ou o Burst precisam vir antes da Fase 18. O hash idêntico é a primeira verificação real do **AC-13** (determinismo entre Mono, IL2CPP e .NET 10).

## Critério de sucesso (definido antes de começar)
| Métrica | Alvo | Resultado |
|---|---|---|
| Fator IL2CPP / .NET 10 (custo médio por tick, 100k) | registrado; **alerta se > 3×** | ✅ **1,34×** (0,074 ms vs 0,055 ms) — bem abaixo do alerta |
| Fator Player Mono / .NET 10 | registrado | **2,0×** (0,110 ms) |
| Fator Editor Mono / .NET 10 | registrado (só informativo) | **1,9×** (0,103 ms) |
| Hash de estado após 1 dia TC igual nos 4 runtimes | **obrigatório** | ✅ `DC336B8F860D4770` em .NET 10, Editor Mono, Player Mono e Player IL2CPP (1 e 2 dias) |
| Alocação por tick no player (Profiler: *GC Alloc*) | 0 B | ✅ **0 B** no dia inteiro, nos 4 runtimes |

## Escopo
**Inclui:** importar o projeto `netstandard2.1` do S4a num asmdef com `noEngineReferences: true`; `MonoBehaviour` executor que roda N ticks com `Stopwatch` e grava CSV e o hash; builds Mono e IL2CPP (Release).
**Não inclui:** Burst/Jobs (fase 11+), ajustes de código para otimizar (só medir).

## Ambiente
Mesma máquina do S4a (para comparar), Unity 6.6, builds Release.

## Plano de execução
Manhã: importar o código, executor e CSV; rodar no Editor. Tarde: builds Mono e IL2CPP, rodar 3 vezes cada, comparar com o S4a, conferir hash; relatório.

## Método de medição
Média e p95 por tick sobre 1 dia TC (11.520 ticks), 3 execuções por runtime, após aquecimento de 1.000 ticks; o hash é gravado no CSV e comparado por script.

## Armadilhas conhecidas
- Hash diferente aponta para iteração não determinística (dicionário, `HashSet`), `float` escondido ou diferença de aritmética: investigar **antes** de seguir para a Fase 1 (R07).
- IL2CPP com *Script Debugging* ligado distorce o resultado: desligar.
- Thermal throttling em notebook: usar a mesma máquina e plano de energia do S4a.

## Saídas esperadas
- Fator por runtime → `PERFORMANCE.md` §3 (orçamentos ajustados).
- Confirmação ou refutação do determinismo cruzado → base do teste F1-110.

---
*As seções abaixo são preenchidas ao terminar.*

## O que foi feito
- **Ambiente:** mesma máquina do S4a (Intel Xeon E5-2670 v3, 2014; Windows 11 25H2), **Unity 6000.6.5f1**, builds **Release** (IL2CPP com `Il2CppCompilerConfiguration.Release`).
- O domínio do S4a (os mesmos `.cs`, `netstandard2.1` / C# 9) foi colocado num asmdef `CitySky.Spike.Domain` com `noEngineReferences: true` dentro de um projeto Unity (fora do repositório, em `C:\dev\unity-spikes\S4b`; os scripts estão em `spikes/S4b-mono-il2cpp-factor/unity/`). **Compilou sem alterações** sob o compilador C# 9 da Unity — confirma a estratégia de *shared source* (ADR-0004).
- Um `BenchCore` roda o mesmo benchmark (1 dia TC, 100k cidadãos, custo por tick + alocação + hash). Três caminhos:
  - **Editor (Mono):** `-executeMethod BenchBuild.RunInEditor`.
  - **Player (Mono)** e **Player (IL2CPP):** `BenchBuild.BuildPlayer` compila o player (cena vazia; o benchmark roda por `RuntimeInitializeOnLoadMethod`), depois `S4bPlayer.exe -batchmode -nographics`.
- **Observação de execução:** no modo batch, `-executeMethod` **não** deve vir com `-quit` (com `-quit`, o método de build não chegava a rodar). Registrado para a Fase 1 (CI da Unity).

## Resultado
Mesma seed (42), 100k cidadãos, 1 dia TC:

| Runtime | Média (ms) | p50 | p95 | Máx | Alocado no dia | Fator vs .NET 10 |
|---|---:|---:|---:|---:|---:|---:|
| **.NET 10** (S4a) | 0,055 | 0,018 | 0,222 | 0,52 | 0 B | 1,00× |
| **Player IL2CPP** (o que vai no jogo) | **0,074** | 0,029 | 0,271 | 0,53 | 0 B | **1,34×** |
| Editor Mono | 0,103 | 0,034 | 0,394 | 0,98 | 0 B | 1,86× |
| Player Mono | 0,110 | 0,037 | 0,420 | 1,18 | 0 B | 2,00× |

**Hash de estado:** `DC336B8F860D4770` idêntico nos quatro runtimes, após 1 dia **e** após 2 dias de jogo. Resultado bruto em `spikes/S4b-mono-il2cpp-factor/results/runtimes.txt`.

**Respostas:**
1. **IL2CPP custa ~1,34× o .NET 10** (0,074 ms/tick com 100k) — muito abaixo do alerta de 3× e ainda muito abaixo da meta de 1 ms. O backend que **vai no jogo** (IL2CPP) é quase tão rápido quanto o .NET 10 e mais rápido que o Mono.
2. **O determinismo vale entre .NET 10, Mono e IL2CPP** (mesmo hash) — a primeira comprovação real do **AC-13**. A escolha de aritmética inteira e ordem de iteração explícita (ADR-0004/0008) se sustenta.
3. **Zero alocação por tick** nos quatro runtimes.

### Observações
1. O **Mono** (Editor e Player) é o mais lento (~2×). Importa pouco: o Editor não é o produto, e o player **de produção é IL2CPP**. Benchmarks feitos no .NET 10 continuam servindo como limite inferior, e o fator real do que envia (IL2CPP) é só ~1,34×.
2. A máquina é de 2014; numa CPU moderna os números absolutos caem, mas o **fator relativo** entre runtimes tende a se manter.
3. Ainda sem pathfinding/economia/energia/tráfego no tick — a folga (0,074 ms contra 1 ms) é o espaço para eles, medido sistema a sistema (trilha contínua de performance).

## Decisão
- [x] Hipótese confirmada: seguir com a arquitetura (ADR-0004); benchmarks .NET valem como limite inferior, fator IL2CPP ~1,34×
- [ ] Hipótese refutada
- [ ] Inconclusivo

**ADR criado ou atualizado:** nenhum. Atualizar `PERFORMANCE.md` §3/§6 com o fator IL2CPP medido (~1,34×) e `TESTING.md` §A4 (o teste de determinismo cruzado .NET × Mono × IL2CPP está comprovado na prática). A thread de simulação dedicada e o Burst continuam na Fase 18 — não são necessários antes.

## Riscos que sobraram
- **R06 (Mono/IL2CPP mais lento):** rebaixado — o fator do player de produção (IL2CPP) é ~1,34×, não um problema. Reavaliar quando o tick tiver todos os sistemas.
- **R07 (determinismo entre runtimes):** rebaixado para o modelo atual — hash idêntico nos 4 runtimes. Manter o teste no CI (noturno) à medida que novos sistemas entram, porque qualquer `float`/ordem não determinística que escape quebraria isso.
- Medir de novo numa CPU moderna e, no futuro, com os sistemas de tráfego (que usam mais aritmética).
