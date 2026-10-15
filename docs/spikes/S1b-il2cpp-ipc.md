# Spike S1b: Player Unity IL2CPP ↔ Code Host (processo + IPC)

- **Status:** ✅ Concluído (2026-10-10)
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
| `Process.Start` do Host a partir do player IL2CPP | funciona | ❌ **Falha** nos dois modos (`UseShellExecute` false e true); exceção só diz "Success". ✅ Resolvido por **`CreateProcess` nativo** (P/Invoke, igual ao S6/`ISandboxLauncher`) |
| Named pipe (cliente na Unity, servidor no Host) no player IL2CPP **e** no Editor (Mono) | funciona (ou registrar a falha exata) | ✅ Funciona nos dois (síncrono, `PipeOptions.None`; o assíncrono quebra no IL2CPP — ver abaixo) |
| TCP loopback + token de 256 bits no handshake | funciona | ⚠️ Transporte TCP loopback funciona; **token de 256 bits não implementado** neste eco (porta efêmera passada por arquivo) → pendência F1 |
| Ida e volta de mensagem pequena (p95, player IL2CPP) | ≤ 50 ms | ✅ Média pipe 1,38 ms / TCP 0,10 ms (muito abaixo; medida a média de 1.000, não p95) |
| Mensagem de 8 MB (limite do protocolo) | ≤ 200 ms | ✅ pipe 15,3 ms / TCP 14,5 ms (IL2CPP) |
| Newtonsoft (`com.unity.nuget.newtonsoft-json`) desserializa DTOs no IL2CPP com `link.xml` | sim | ⚠️ **Não testado** aqui (eco é `[uint32 len][bytes]` cru, sem JSON) → validar em F1-13 |
| Host morto → Unity detecta em ≤ 1 s e reinicia com *backoff* | sim | ✅ Detectado em 3 ms (falha na leitura do quadro); reiniciado e reconectado em ~121 ms (sem *backoff*, que fica para F1) |
| Unity fechada (inclusive *kill* pelo Gerenciador de Tarefas) → Host encerra em ≤ 2 s | sim | ⚠️ Fechar o pipe encerra o Host (`hostExitedOnClose=True`); o *kill* da Unity com **Job Object `KILL_ON_JOB_CLOSE`** é do S6 e a integração fica para F1-74 |
| Pipe só acessível pelo usuário atual (ACL) e com nome aleatório por sessão | sim | ⚠️ Nome aleatório por sessão (GUID) ✅; **ACL explícita** (`NamedPipeServerStreamAcl` + `PipeSecurity`) não aplicada neste spike → pendência F1 |
| Chegada de mensagens não causa picos no main thread (thread de I/O + fila) | pico < 1 ms | ⚠️ **Não medido** (probe roda em `RuntimeInitializeOnLoadMethod`, fora do *frame loop*); thread de I/O + fila é desenho de F1 |

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
*Concluído em 2026-10-10 (Unity 6000.6.5f1, IL2CPP + Mono2x, win-x64).*

## O que foi feito
- **Host de eco** .NET 10 *self-contained* (`CitySky.Spike.EchoHost`, win-x64) publicado em `Assets/StreamingAssets/echohost/`. Dois modos: `--pipe <nome>` (cliente de named pipe) e `--tcp <arquivoPorta>` (listener em `127.0.0.1:0`, grava a porta num arquivo). Enquadramento `[uint32 len LE][bytes]`, ecoando cada quadro até o fluxo fechar.
- **Probe** `IpcProbe.cs` (`RuntimeInitializeOnLoadMethod`): abre o servidor de pipe / conecta no TCP, lança o Host, mede *warm-up*, ida-e-volta de 1.000 mensagens de 64 B, uma mensagem de 8 MB e um ciclo de recuperação (mata o Host, detecta, reinicia, reconecta). Grava `s1b-<backend>.txt` e sai.
- **Build** por `IpcBuild.BuildPlayer` em batch (`-executeMethod`, sem `-quit` porque o método chama `EditorApplication.Exit`), cena vazia, backend selecionável (`-backend IL2CPP|Mono`).
- Rodado nos **dois backends** (player IL2CPP e player Mono2x), em batchmode `-nographics`.

### A descoberta central: `System.Diagnostics.Process.Start` não funciona no IL2CPP
Todas as tentativas de lançar o Host pelo `System.Diagnostics.Process` falharam no player IL2CPP, com a mensagem de exceção inútil **"Success"** (um `Win32Exception` com código 0):
1. `UseShellExecute = false` → falha ("Native error= Success"), mesmo com `WorkingDirectory` e caminho normalizado por `Path.GetFullPath`.
2. `UseShellExecute = true` + `WindowStyle = Hidden` → **também** falha com a mesma mensagem.

A solução foi lançar por **`CreateProcess` nativo (P/Invoke `kernel32`)** — `ChildProcess` em `NativeProc.cs`, com `WaitForSingleObject`/`GetExitCodeProcess`/`TerminateProcess`. Funcionou de primeira. **Esse já é o caminho de produção:** o `ISandboxLauncher` (e o S6) lançam o Code Host por `CreateProcess` nativo, não por `System.Diagnostics.Process`. Ou seja, o beco sem saída do `Process.Start` no IL2CPP **não afeta a arquitetura** — só confirma que a abstração de lançamento tem de ser nativa desde o início.

Outras armadilhas do IL2CPP encontradas no caminho (todas registradas em `CODE_EXECUTION.md` §6):
- **Pipes assíncronos quebram:** `PipeOptions.Asynchronous` no lado da Unity dispara `ThreadPoolBoundHandle::OnNativeIOCompleted` sem *callback* registrável no IL2CPP → usar **pipes síncronos** (`PipeOptions.None`) com `WaitForConnection` numa thread de fundo.
- `UnityEngine.Debug` vs `System.Diagnostics.Debug` ambíguos → qualificar.

## Resultado
Latência (média de 1.000 ida-e-volta de 64 B; 8 MB = 1 amostra; batchmode):

| Métrica | Player **IL2CPP** | Player **Mono2x** |
|---|---|---|
| Lançar + conectar (pipe, *warm*) | 4,55 ms | 1,03 ms |
| **Pipe** ida-e-volta 64 B (média) | **1,380 ms** | 0,322 ms |
| **Pipe** 8 MB | 15,3 ms | 15,7 ms |
| **TCP** ida-e-volta 64 B (média) | **0,100 ms** | 0,126 ms |
| **TCP** 8 MB | 14,5 ms | 16,6 ms |
| Recuperação: detectar morte do Host | 3 ms | 3 ms |
| Recuperação: reiniciar + reconectar | 121 ms | 129 ms |
| Host sai quando o pipe fecha | sim | sim |

**Leituras:**
- Os dois transportes funcionam nos dois backends. Todos os alvos de latência passam com folga (alvo de ida-e-volta ≤ 50 ms; medido ≤ 1,4 ms).
- **No IL2CPP o named pipe síncrono é ~14× mais lento que o TCP loopback** (1,38 ms vs 0,10 ms por ida-e-volta); no Mono a diferença some (0,32 vs 0,13 ms). É sobrecarga do pipe gerenciado no IL2CPP com `Write`+`Flush` por mensagem. Mesmo assim, 1,4 ms por ida-e-volta é **irrelevante se o protocolo agrupar uma ida-e-volta por tick** (comandos em lote + *read models* em delta), não uma por comando.
- 8 MB em ~15 ms nos dois transportes — bem dentro do alvo de 200 ms.
- Morte do Host detectada quase de imediato (3 ms) pela falha de leitura do quadro.

## Decisão
- [x] **Hipótese parcialmente refutada, arquitetura mantida.** O `System.Diagnostics.Process.Start` **não** funciona no IL2CPP (os dois modos falham). Mas o lançamento por `CreateProcess` nativo funciona e **já é o caminho de produção** (`ISandboxLauncher`/S6), então o desenho do ADR-0005 segue de pé — sem fallback para o backend Mono no player (R02 evitado).
- **Transporte recomendado: named pipe** como padrão, pela segurança (sem superfície de rede; ACL por usuário), com **uma ida-e-volta por tick** para amortizar a sobrecarga do pipe no IL2CPP. **TCP loopback + token de 256 bits** fica como *fallback* explícito (porta efêmera, handshake com token), já que a latência é ainda menor — útil se o pipe gerenciado der problema em alguma máquina.

**ADR criado ou atualizado:** ADR-0005 (lançamento nativo obrigatório; pipe padrão + TCP *fallback*); `CODE_EXECUTION.md` §6 (armadilhas IL2CPP). `IIpcTransport`/`ISandboxLauncher` nascem com lançamento nativo na Fase 1.

## Riscos que sobraram
- **Serialização JSON no IL2CPP não validada** (o eco é de bytes crus). O *stripping* do IL2CPP pode remover tipos usados por reflexão do Newtonsoft → validar `link.xml`/`[Preserve]` em **F1-13** antes de confiar no protocolo real.
- **ACL explícita do pipe** (`NamedPipeServerStreamAcl` + `PipeSecurity` do usuário atual) não aplicada neste spike → **F1**.
- **Token de 256 bits do TCP** não implementado → **F1** (só habilitar o TCP com handshake).
- **Ciclo de vida sob *kill* da Unity**: a integração com Job Object `KILL_ON_JOB_CLOSE` (S6) para garantir que o Host morre junto com o jogo travado é de **F1-74**.
- **Picos no main thread** não medidos (probe fora do *frame loop*); a thread de I/O + fila é desenho de **F1**.
- Caminho com acentos (`C:\Users\João\…`) e primeiro `Process`/`CreateProcess` sob antivírus não exercitados explicitamente → revisitar em teste de campo.
