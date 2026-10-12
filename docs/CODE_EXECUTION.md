# Execução do Código do Jogador

> **Status:** v1.0 · **Data:** 2026-10-08 · **ADRs:** [0005](ADR/0005-code-execution.md), [0006](ADR/0006-code-security.md) · **MVP:** sim
> Segurança: [`CODE_EXECUTION_SECURITY.md`](CODE_EXECUTION_SECURITY.md) · Análise: [`CODE_ANALYSIS.md`](CODE_ANALYSIS.md) · API: [`PLAYER_API.md`](PLAYER_API.md)

---

## 1. Requisitos
1. O código do jogador **realmente altera** a simulação (§1, §28).
2. Não pode derrubar o jogo: laço infinito, recursão, exceções, memória.
3. Não pode acessar o computador: arquivos, rede, processos (§47).
4. Erros devem virar **feedback pedagógico** (§51).
5. Deve funcionar com o jogo compilado em **IL2CPP**.
6. Validação determinística de desafios.

## 2. Por que fora do processo da Unity (resumo do ADR-0005)
| Opção | Problema |
|---|---|
| Roslyn + `Assembly.Load` dentro da Unity | IL2CPP não executa código carregado em runtime; o Mono não descarrega assemblies; sem fronteira de segurança; `StackOverflowException` mata o jogo |
| Interpretador C# próprio | Projeto enorme, semântica incompleta, o aluno não aprende C# "de verdade" |
| C# → WebAssembly (WASI) | Ecossistema ainda experimental para compilação dinâmica de .NET |
| **Processo .NET 10 separado (adotado)** | Custo: IPC e gerência de processos, que são problemas conhecidos e controláveis |

## 3. Componentes

### 3.1 Code Host (`CitySky.CodeHost.exe`, .NET 10, *trusted*)
- Iniciado pelo jogo na abertura (aquecido em background). Reiniciado se cair.
- **Workspace Roslyn** por projeto do jogador (`AdhocWorkspace` + documentos), com serviços de linguagem para o editor (ver `CODE_EDITOR.md`).
- **Pipeline:** Parse → Analyze → Validate → Compile.
- **Gerência de Workers:** pool com 1 worker quente; recicla após violação, timeout, crash ou N execuções.
- **Validação de desafios:** orquestra cenários *headless* nos Workers e consolida o `ValidationReport`.
- Nunca executa código do jogador no próprio processo.

### 3.2 Sandbox Worker (`CitySky.Sandbox.Worker.exe`, .NET 10, *untrusted*)
- Processo descartável, restrito pelo SO (Job Object no MVP; + token restrito/AppContainer na Fase 9).
- Carrega o assembly do jogador num `AssemblyLoadContext` **coletável** (isolamento de carga e descarregamento, **não** segurança).
- Contém a **Player API runtime**, o **Domain headless** (Laboratórios) e o **runtime de orçamento** (`__Budget`).
- Comunicação só por stdin/stdout (pipes anônimos), com quadros validados.

## 4. Pipeline detalhado

```
Editor (arquivos .cs do jogador)
   │
   ▼
[1] PARSE      CSharpSyntaxTree.ParseText(LanguageVersion.CSharp14)
   │           → erros de sintaxe (CS1002, CS1513...) → ERROR_SYSTEM
   ▼
[2] ANALYZE    CSharpCompilation (referências curadas) + modelo semântico
   │           → Allowlist analyzer (CSK1xxx): APIs proibidas
   │           → Analyzers pedagógicos (CSK2xxx): dicas de estilo/conceito
   │           → Gate de desbloqueio (CSK3xxx): recurso ainda bloqueado na progressão
   ▼
[3] VALIDATE   regras estruturais do desafio (ex.: "deve existir classe que implemente IRouteStrategy")
   │           (apenas em Laboratório; ver VALIDATION_SYSTEM.md)
   ▼
[4] REWRITE    instrumentação de orçamento (Syntax rewriter):
   │           • __Budget.Tick() no início de cada corpo de laço e de método
   │           • __Budget.Enter()/Exit() para profundidade de recursão
   │           • #line para preservar linhas originais
   ▼
[5] COMPILE    Emit → PE + Portable PDB (em memória), Optimize=Release, deterministic
   │           → Verificação de IL (System.Reflection.Metadata): toda TypeRef/MemberRef ∈ allowlist
   ▼
[6] SANDBOX    Worker do pool (Job Object: memória, CPU, sem processos filhos)
   ▼
[7] EXECUTE    conforme o modo (seção 5), com orçamentos
   ▼
[8] SIMULATION comandos → Unity → Application valida → aplica no tick seguinte
   ▼
[9] FEEDBACK   console, marcadores no editor, Inspector, mensagens pedagógicas
```

### 4.1 Referências da compilação
- **Somente** assemblies de referência curados: subconjunto da BCL (`System.Runtime`, `System.Collections`, `System.Linq`, `System.Text` etc.) + `CitySky.PlayerApi.dll` + (Fase 17) `nunit.framework.dll`.
- Como os assemblies de referência da BCL modernos concentram muitos tipos em `System.Runtime` (inclusive `System.IO.File`), **restringir referências não basta**. A allowlist por símbolo (passo 2) e por IL (passo 5) é obrigatória.
- `AllowUnsafe = false`. Sem `#r`/`#load` (não é script C#). Sem analyzers/geradores fornecidos pelo jogador.

### 4.2 Instrumentação de orçamento (exemplo)
```csharp
// Código do jogador
for (int i = 0; i < 10; i--) { city.Log(i.ToString()); }

// Após o rewriter (conceitual)
for (int i = 0; i < 10; i--) { global::CitySky.Runtime.__Budget.Tick(12); city.Log(i.ToString()); }
```
`__Budget.Tick(line)` incrementa um contador. Ao estourar o limite (ex.: 5 milhões de ticks em Script, ajustável por desafio), lança `BudgetExceededException(line)`. Essa exceção vira a mensagem pedagógica **"Laço possivelmente infinito na linha 12"**. Isso é melhor que só matar o processo: o aluno recebe a linha.

Também há **watchdog** de tempo real no Code Host (2 s no Script, configurável no Laboratório), com *kill* do Worker. É a rede de segurança caso a instrumentação seja contornada.

## 5. Modos de execução

### 5.1 Script (cidade viva) — MVP
- Arquivo `Program.cs` com **top-level statements**:
```csharp
using CitySky;

var city = CityGame.Current;          // a cidade em que você está jogando
var house = new House { Residents = 3 };
var result = city.Build(house);
city.Log(result.Message);
```
- Gateway **Live**: consultas vão por IPC até a Unity (snapshot congelado do tick T, com simulação pausada no MVP) e mutações são acumuladas num `CommandBuffer`.
- Ao final: `RunResult { Commands[], ConsoleOutput, Diagnostics, Metrics }`. A Unity valida cada comando com as **mesmas regras da UI** (custo, desbloqueio, lote livre).
- Comandos são aplicados **atomicamente**? Não: em ordem, cada um com seu resultado. Um comando inválido não desfaz os anteriores, e o console mostra quais falharam e por quê. Isso é didático: o aluno vê o efeito parcial.

### 5.2 Laboratório (desafio) — MVP
- Gateway **Headless**: o `CitySky.Domain` roda **dentro do Worker**, num cenário definido pelo desafio (seed + estado inicial).
- O runner do desafio chama o código do aluno, avança N ticks e avalia os critérios (testes ocultos, métricas da cidade, regras de arquitetura).
- 100% determinístico. Resultado: `ValidationReport` (ver `VALIDATION_SYSTEM.md`).
- **Snapshots** permitem rodar variações ("antes × depois do seu código") e comparar (§52).

### 5.3 Comportamento (pós-MVP, Fase 9)
- Classes do jogador que implementam interfaces de extensão (`IGarbageRoutingStrategy`, `IMapOverlay`, `IEnergyPolicy`) e são registradas explicitamente:
```csharp
city.Policies.Garbage.Use(new NearestDepotRouting());
```
- A simulação chama o Worker **em lote** (ex.: 1x por hora TC, todas as decisões de uma vez), com orçamento de tempo (ex.: 20 ms). Resultados entram no tick seguinte.
- Timeout ou exceção → **fallback** para a regra padrão + aviso pedagógico ("sua estratégia demorou 140 ms; a cidade usou a rota padrão").

### 5.4 Testes do jogador (Fase 17)
- Arquivos com métodos `[Test]` (API NUnit: `Assert.That`, `Is.EqualTo`), descobertos por reflexão **dentro do Worker** (o runner do NUnit não é usado). Ver `TESTING.md` §B.

## 6. Protocolo Unity ↔ Code Host (`CitySky.Contracts`)
- Transporte: **named pipe** com nome aleatório por sessão e ACL do usuário atual (padrão, confirmado no S1b pela segurança — sem superfície de rede). Fallback: **TCP loopback** em porta efêmera + token de 256 bits no handshake (habilitar só se o pipe gerenciado falhar numa máquina).
- **Lançamento do Host/Worker: `CreateProcess` nativo (P/Invoke), nunca `System.Diagnostics.Process`.** O S1b mostrou que `Process.Start` **falha no player IL2CPP** nos dois modos (`UseShellExecute` false e true; exceção só diz "Success"). O lançamento nativo (mesmo caminho do `ISandboxLauncher`/S6) funciona. `IIpcTransport`/`ISandboxLauncher` nascem nativos na Fase 1.
- **Pipes síncronos** no lado da Unity (`PipeOptions.None`): o IL2CPP quebra o *callback* de I/O assíncrono (`ThreadPoolBoundHandle::OnNativeIOCompleted`). Usar thread de I/O dedicada + fila para o main thread.
- Enquadramento: `[uint32 length][json]`, limite de 8 MB por mensagem.
- **Latência medida (S1b, player IL2CPP, win-x64):** ida-e-volta de 64 B ~1,4 ms (pipe) / ~0,1 ms (TCP); 8 MB ~15 ms; morte do Host detectada em ~3 ms; reinício+reconexão ~120 ms. O pipe no IL2CPP é mais lento que o TCP, então o protocolo **agrupa uma ida-e-volta por tick** (comandos em lote + *read models* em delta), não uma por comando. Pendências do S1b: validar Newtonsoft com `link.xml` (F1-13), ACL explícita do pipe e token do TCP (F1).
- Envelope:
```json
{ "v": 1, "id": "c3f1…", "type": "CompileAndRun", "payload": { … } }
```
| Mensagem (Unity → Host) | Uso |
|---|---|
| `Hello{protocolVersion, gameVersion}` | handshake |
| `OpenWorkspace{files[]}` / `UpdateDocument{path, changes[]}` | edição incremental |
| `GetCompletions{path, position}` / `GetQuickInfo` / `GetSignatureHelp` | serviços de linguagem |
| `GetDiagnostics{path}` | sublinhados ao digitar |
| `CompileAndRun{mode, entry, budgets}` | executar |
| `QueryResponse{queryId, data}` | resposta a consulta do Worker (modo Live) |
| `RunChallenge{challengeId, files[]}` | Laboratório |
| `Cancel{runId}` | botão "Parar" |

| Mensagem (Host → Unity) | Uso |
|---|---|
| `Diagnostics{items[]}` | erros e avisos (com códigos CS/CSK, linha, coluna, mensagem pt-BR) |
| `Query{queryId, kind, args}` | o código do jogador pediu dados (modo Live) |
| `RunResult{commands[], console, metrics, error?}` | fim de execução |
| `ValidationReport{levels[], score, feedback}` | fim de desafio |
| `Completions` / `QuickInfo` / `SignatureHelp` | editor |

- **Versionamento:** `protocolVersion` inteiro. Unity e Host são distribuídos juntos, mas o handshake rejeita versões diferentes com mensagem clara.
- **Testes de contrato:** golden files JSON em `dotnet/tests/CitySky.Contracts.Tests` (desserializar → serializar → comparar).

> **Implementado na Fase 1 (v0, 2026-10-12):** `CitySky.Contracts` tem o `Protocol` (versão, limite 8 MB), `MessageFraming` (`[uint32 len][bytes]`) e as mensagens `Hello/CompileAndRun/RunResult/Diagnostics/Cancel` (classes imutáveis C# 9) com golden JSON. O pipeline `Parse→L1→reescrita→compilação→emissão→L2` está em `CitySky.CodeAnalysis`; a execução isolada (ALC coletável + orçamentos) em `CitySky.Sandbox.Runtime`/`CitySky.Sandbox.Worker`; o Code Host (Generic Host + named pipe + `WorkerPool` com 1 worker quente) em `CitySky.CodeHost`. **Lançamento do Worker: `Process` com stdio redirecionado (v0); o Job Object (F1-51) e a ACL explícita do pipe (F1-50) ficam para a sequência da Fase 1.** Corpus de segurança em `CitySky.Security.Tests`.

## 7. Orçamentos padrão
| Recurso | Script | Laboratório | Comportamento (por lote) |
|---|---|---|---|
| Tempo real (watchdog) | 2 s | 10 s (por cenário) | 20 ms (soft) / 200 ms (hard) |
| Ticks de orçamento (instrumentação) | 5 M | definido pelo desafio | 200 k |
| Profundidade de recursão | 256 | 256 | 128 |
| Memória do Worker (Job Object) | 256 MB | 512 MB | 256 MB |
| Saída de console | 64 KB | 64 KB | 4 KB |
| Comandos por execução | 500 | — | 1.000 |

## 8. Desempenho
- Code Host aquecido na inicialização do jogo: compilação "dummy" para carregar o Roslyn (*JIT warm-up*), com ReadyToRun no publish.
- Compilação incremental: reaproveita a `Compilation` e troca só as árvores alteradas.
- Worker pré-iniciado (pool). Carregar o assembly do jogador costuma custar menos de 10 ms.
- Metas: ver `MVP.md` §6.

## 9. Falhas e recuperação
| Falha | Detecção | Ação |
|---|---|---|
| Worker morre (StackOverflow, *kill*) | pipe fechado / código de saída | recicla Worker; mensagem "o programa parou inesperadamente; recursão infinita?" + linha provável (último `__Budget.Enter` registrado) |
| Code Host morre | pipe fechado | Unity reinicia o Host (backoff); o editor mostra "reconectando…" |
| Mensagem inválida do Worker | validação de esquema | descarta, mata o Worker, registra incidente de segurança local |
| Comando inválido | Application | resultado de falha por comando no console |

## 10. Testes
- Unidade do pipeline (cada estágio).
- **Corpus de segurança** (ver documento de segurança).
- Contrato (golden JSON).
- Integração: Unity PlayMode + Code Host real (CI Windows).
- **Conteúdo:** toda solução de referência de desafio passa; soluções erradas cadastradas falham com o feedback esperado.
