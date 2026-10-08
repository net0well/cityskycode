# Spike S1b: Player Unity IL2CPP ↔ Code Host (processo + IPC)

- **Status:** Planejado
- **Prazo:** 2 dias · **Ordem:** 5º
- **Depende de:** Unity 6.6 com IL2CPP instalada (F0-04), S1a · **Bloqueia:** S7, Fase 1 (`IIpcTransport`, `CodeHostClient`)
- **Código:** `spikes/S1b-il2cpp-ipc/` (descartável)
- **ADRs relacionados:** [ADR-0005](../ADR/0005-code-execution.md), [ADR-0001](../ADR/0001-engine-selection.md) · Documento: [`CODE_EXECUTION.md`](../CODE_EXECUTION.md) §6

## Pergunta
Um player Windows **IL2CPP** da Unity 6.6 consegue iniciar o Code Host (.NET 10 *self-contained*, em `StreamingAssets`), conectar por **named pipe** (`System.IO.Pipes`) — ou por **TCP loopback com token** como alternativa —, trocar mensagens do protocolo com baixa latência, e se recuperar quando o Host cai?

## Hipótese
`System.Diagnostics.Process.Start` funciona no IL2CPP para Windows desktop. O suporte a `NamedPipeClientStream` na biblioteca de classes da Unity é o ponto incerto; se falhar ou se comportar mal, TCP loopback com token de 256 bits resolve com latência equivalente para mensagens pequenas.

## Por que importa
Se a Unity IL2CPP não conseguir iniciar e falar com um processo externo, o desenho do ADR-0005 cai, e o fallback seria usar o backend Mono no player (pior desempenho, R02).

## Critério de sucesso (definido antes de começar)
| Métrica | Alvo | Resultado |
|---|---|---|
| `Process.Start` do Host a partir do player IL2CPP | funciona | |
| Named pipe (cliente na Unity, servidor no Host) no player IL2CPP **e** no Editor (Mono) | funciona (ou registrar a falha exata) | |
| TCP loopback + token de 256 bits no handshake | funciona | |
| Ida e volta de mensagem pequena (p95, player IL2CPP) | ≤ 50 ms | |
| Mensagem de 8 MB (limite do protocolo) | ≤ 200 ms | |
| Newtonsoft (`com.unity.nuget.newtonsoft-json`) desserializa DTOs no IL2CPP com `link.xml` | sim | |
| Host morto → Unity detecta em ≤ 1 s e reinicia com *backoff* | sim | |
| Unity fechada (inclusive *kill* pelo Gerenciador de Tarefas) → Host encerra em ≤ 2 s | sim | |
| Pipe só acessível pelo usuário atual (ACL) e com nome aleatório por sessão | sim | |
| Chegada de mensagens não causa picos no main thread (thread de I/O + fila) | pico < 1 ms | |

## Escopo
**Inclui:** projeto Unity descartável com cena mínima; `MonoBehaviour` que inicia o Host e uma thread de I/O; Host de eco em .NET 10 (`NamedPipeServerStreamAcl.Create` com `PipeSecurity` do usuário atual; listener TCP em porta efêmera); enquadramento `[uint32 length][json]`; cenários de falha.
**Não inclui:** Roslyn (já validado no S1a; aqui o Host só ecoa), editor, sandbox.

## Ambiente
Windows 11; Unity 6.6 (registrar a versão exata) com Windows Build Support (IL2CPP); Visual Studio com C++; .NET SDK 10.0.400.

## Plano de execução
**Dia 1:** projeto Unity com IL2CPP; Host de eco publicado *self-contained* em `StreamingAssets/CodeHost/`; `Process.Start` com caminho via `Application.streamingAssetsPath` e `Path.Combine`; cliente de named pipe na thread de I/O; fila para o main thread; `link.xml` para os DTOs. Build de player e teste.
**Dia 2:** caminho TCP com token; medições de latência e tamanho; cenários de falha (matar o Host; matar a Unity: testar tanto *parent PID watch* no Host quanto a Unity colocando o Host num **Job Object com `KILL_ON_JOB_CLOSE`**, que garante o encerramento mesmo se o jogo travar); relatório.

## Método de medição
Timestamps nas duas pontas (`Stopwatch` no Host, `Time.realtimeSinceStartupAsDouble` + `Stopwatch` na Unity) para 1.000 mensagens; Unity Profiler (Development Build) para picos no main thread.

## Armadilhas conhecidas
- A biblioteca de classes do IL2CPP vem do Mono, e o suporte a `System.IO.Pipes` no Windows já teve lacunas: por isso o fallback TCP existe desde o início.
- *Stripping* do IL2CPP remove tipos usados só por reflexão (Newtonsoft): `link.xml` ou `[Preserve]`.
- Caminho com espaços ou acentos (ex.: `C:\Users\João\…`): testar explicitamente.
- O antivírus pode bloquear ou atrasar o primeiro `Process.Start` de um executável novo; registrar.

## Saídas esperadas
- **Transporte escolhido** (named pipe ou TCP) → ADR-0005 e `CODE_EXECUTION.md` §6.
- Estratégia de ciclo de vida do Host (Job Object no lado do jogo ou *parent watch*) → F1-74.
- `link.xml` de referência → F1-13.

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
