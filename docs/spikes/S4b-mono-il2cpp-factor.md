# Spike S4b: Fator de custo Mono/IL2CPP e determinismo entre runtimes

- **Status:** Planejado
- **Prazo:** 1 dia · **Ordem:** 8º
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
| Fator IL2CPP / .NET 10 (custo médio por tick, 100k) | registrado; **alerta se > 3×** | |
| Fator Player Mono / .NET 10 | registrado | |
| Fator Editor Mono / .NET 10 | registrado (só informativo) | |
| Hash de estado após 1 dia TC igual nos 4 runtimes | **obrigatório** | |
| Alocação por tick no player (Profiler: *GC Alloc*) | 0 B | |

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

## Resultado

## Decisão
- [ ] Hipótese confirmada: seguir com a arquitetura documentada
- [ ] Hipótese refutada: alternativa adotada = …
- [ ] Inconclusivo: próximo passo = …

**ADR criado ou atualizado:**

## Riscos que sobraram
