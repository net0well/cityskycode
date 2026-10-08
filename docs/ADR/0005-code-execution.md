# ADR-0005: Executar o código do jogador fora do processo do jogo (Code Host + Worker)

- **Status:** Aceito
- **Data:** 2026-10-08
- **Documentos afetados:** `CODE_EXECUTION.md`, `PLAYER_API.md`, `ARCHITECTURE.md`

## Contexto
- **IL2CPP** (AOT) não executa assemblies carregados em runtime.
- O Mono da Unity não descarrega assemblies sem AppDomains, e `AssemblyLoadContext`/AppDomain **não são fronteiras de segurança** (orientação da Microsoft: isolamento por SO/processo para código não confiável).
- `StackOverflowException` encerra o processo e não é capturável.
- A Unity 6 está limitada a C# 9. A Unity 7 alpha (out/2026) ainda mira C# 9.

## Decisão
1. **Code Host** (.NET 10, Roslyn 5.x), processo confiável: workspace, serviços de linguagem, pipeline (Parse → Analyze → Validate → Rewrite → Compile), validação de desafios, gerência de Workers.
2. **Sandbox Worker** (.NET 10), processo descartável e restrito: carrega o assembly do aluno em ALC coletável e executa com orçamentos.
3. **Player API** gera **comandos**, validados pela Application com as mesmas regras da UI. Gateways: *Live* (cidade, consultas por IPC) e *Headless* (Laboratório, domínio dentro do Worker).
4. Protocolo próprio versionado (`CitySky.Contracts`): JSON com prefixo de tamanho, named pipe (fallback TCP loopback + token).
5. O aluno escreve **C# 14**.

## Alternativas consideradas
| Alternativa | Por que não |
|---|---|
| Roslyn + Assembly.Load na Unity (Mono) | sem IL2CPP; sem segurança; StackOverflow mata o jogo; C# 9 |
| Interpretador C# próprio | escopo enorme; semântica incompleta |
| C# → WASM/WASI | imaturo para compilação dinâmica .NET em 2026 |
| Simulação inteira no processo .NET (Unity só renderiza) | replicação de estado complexa; contraria a arquitetura híbrida; o código do aluno ficaria no mesmo processo da simulação autoritativa |

## Consequências
**Positivas:** isolamento real; IL2CPP no jogo; C# moderno para o aluno; validação *headless* com o mesmo domínio.
**Negativas:** IPC, gerência de processos, distribuição de um runtime .NET self-contained (~70–100 MB).
**Riscos:** latência de IPC em comportamentos por tick (mitigação: lote, baixa frequência, fallback).

## Gatilhos de revisão
Spike S1 falhar · Unity CoreCLR estável **e** surgimento de isolamento in-process confiável (improvável).
