# Segurança da Execução de Código

> **Status:** v1.0 · **Data:** 2026-10-08 · **ADR:** [0006](ADR/0006-code-security.md) · **MVP:** camadas 1–4 e 6 · **Fase 9:** camada 5 completa
> **Revisão obrigatória** do agente `security-auditor` (dotnet-claude-kit) e `/security-scan` em toda mudança neste escopo.

---

## 1. Premissa central
> **`AssemblyLoadContext` (ou AppDomain no Mono) não é sandbox.**

- O .NET moderno **não tem** Code Access Security. A documentação legada da própria Microsoft diz que CAS e *Security-Transparent Code* não servem como fronteira de segurança para código parcialmente confiável.
- A orientação para código não confiável é usar **isolamento do sistema operacional ou virtualização**, tipicamente um processo separado com privilégios mínimos.
- Por isso, a segurança aqui é **em camadas**, e a camada que de fato contém um atacante é o **processo restrito pelo SO**. As camadas de linguagem existem para dar **feedback pedagógico** e **reduzir a superfície de ataque**.

## 2. Modelo de ameaças

### 2.1 Atores
| Ator | Motivação | Probabilidade (MVP) | Probabilidade (com compartilhamento) |
|---|---|---|---|
| Aluno (o próprio jogador) | curiosidade, erro acidental | Alta (erros) / Baixa (ataque) | idem |
| Autor de código compartilhado (solução, pacote `.citysky`, save, pacote de desafio de professor) | malícia ou brincadeira entre colegas | **Nula** (sem compartilhamento no MVP) | **Alta**: compartilhamento **confirmado** pelo dono do produto, na Fase 9 (`SHARING_SYSTEM.md`) |
| Trapaceiro | ranking/conquistas | Baixa (offline) | Média |

> Observação honesta: num jogo **offline e single-player**, o aluno pode executar qualquer programa fora do jogo. O risco relevante no MVP é **estabilidade** (travar ou derrubar o jogo, corromper o save). O risco de **segurança** cresce quando o código passa a vir de outras pessoas. Isso **vai acontecer**: o compartilhamento entre alunos foi confirmado (ADR-0017), e em sala de aula um pacote malicioso pode circular por uma turma inteira num PC compartilhado. Por isso o L5 completo é **pré-requisito** da Fase 9.

### 2.2 Ativos
Arquivos do usuário · credenciais/tokens do sistema · rede local/internet · integridade dos saves · estabilidade do jogo · privacidade (telemetria, nomes) · reputação do produto (uso em escolas).

### 2.3 Cenários (STRIDE resumido)
| # | Cenário | Categoria | Camadas que mitigam |
|---|---|---|---|
| T1 | Ler/escrever arquivos (`File`, `Directory`, `FileStream`) | Information disclosure / Tampering | L1, L2, **L5** |
| T2 | Acesso à rede (`HttpClient`, `Socket`, `Dns`) | Exfiltração | L1, L2, **L5** |
| T3 | Iniciar processos (`Process.Start`) | Elevation | L1, L2, **L5** (Job: 0 processos filhos) |
| T4 | Reflexão para chamar APIs proibidas (`Type.GetType("System.IO.File")`, `MethodInfo.Invoke`) | Bypass | L1, L2 |
| T5 | Geração de código (`Reflection.Emit`, `Expression.Compile`, `DynamicMethod`) | Bypass | L1, L2 |
| T6 | Interop nativo (`DllImport`, `Marshal`, ponteiros de função, `unsafe`) | Elevation | L1 (`AllowUnsafe=false`), L2, **L5** |
| T7 | Negação de serviço: laço infinito, recursão, alocação massiva, threads | DoS | L3, L4, **L5** (limites do Job) |
| T8 | Código em finalizador, construtor estático ou `[ModuleInitializer]` rodando fora do orçamento | Bypass do orçamento | L1 (proíbe finalizadores e ModuleInitializer), L3 |
| T9 | Ataque ao Code Host via mensagens malformadas do Worker | Tampering/Elevation | **L6** |
| T10 | Ataque ao jogo via comandos abusivos (spam, valores extremos) | DoS/Tampering | **L6** (Application valida) |
| T11 | Save/solução baixada executando código automaticamente | Spoofing | **L7** |
| T12 | `Environment.Exit`, `GC`, `AppDomain`, variáveis de ambiente | DoS/Disclosure | L1, L2 |
| T13 | Monaco/webview carregando conteúdo remoto | Spoofing/XSS | `SECURITY.md` (CSP, navegação bloqueada) |

## 3. Camadas de defesa

### L1 — Linguagem/compilação (Roslyn, no Code Host)
- `AllowUnsafe = false`. Sem diretivas de script. Sem analyzers/geradores do jogador.
- **Allowlist por símbolo**: todo símbolo referenciado (tipo, membro, construtor, operador, conversão definida pelo usuário, atributo) precisa estar na allowlist. Violações geram `CSK1001`: "Este recurso não está disponível na cidade" + explicação. O código não chega a ser emitido.
- **Proibições sintáticas:** finalizadores (`~Class()`), `[ModuleInitializer]`, `extern`, `[DllImport]`, `__arglist`, `stackalloc` (até haver necessidade pedagógica), `dynamic`, `fixed`, ponteiros de função, `lock` sobre tipos públicos da API.
- **Proibições de namespace (exemplos):** `System.IO`, `System.Net*`, `System.Diagnostics` (inteiro), `System.Reflection*`, `System.Runtime.InteropServices`, `System.Runtime.CompilerServices` (exceto atributos gerados pelo compilador), `System.Threading` (Thread, ThreadPool, Timer; `Task`/`async` só em fase futura e com orçamento), `System.Linq.Expressions`, `System.Security*`, `Microsoft.Win32`, `System.Environment`, `System.AppDomain`, `System.GC`, `System.Activator`.
- **System.Type:** somente `Name` e `FullName` (via `typeof(T).Name` e `nameof`). `GetType()` é permitido apenas quando o resultado é usado nesses membros.

### L2 — Verificação de IL (após emissão)
- `System.Reflection.Metadata` percorre **todas** as `TypeReference`, `MemberReference`, `MethodSpecification` e atributos do PE emitido e confere com a mesma allowlist (forma canônica por assinatura).
- Pega o que o compilador gera e a análise de símbolos poderia não ver (*lowering*, *closures*, interpoladores de string).
- Rejeita qualquer `ModuleRef`/`ImplMap` (P/Invoke) ou método com corpo `extern`.
- Falha em L2 sem falha em L1 = **bug na L1**: abre incidente e acrescenta um caso ao corpus.

### L3 — Orçamentos em runtime (instrumentação)
- `__Budget.Tick` (laços e entradas de método), `__Budget.Enter/Exit` (profundidade), verificação de alocação (`GC.GetAllocatedBytesForCurrentThread`) a cada K ticks, limite de saída de console, limite de comandos.
- Exceções de orçamento **não podem ser capturadas pelo jogador**: o rewriter transforma `catch (Exception)` e `catch` sem tipo em filtros que relançam `BudgetExceededException`, e o `finally` do jogador também tem orçamento.

### L4 — Isolamento de processo
- O Worker é um processo separado. Nenhuma memória compartilhada com a Unity ou o Code Host. Nenhum handle herdado além dos pipes de comunicação (lista explícita de handles via `PROC_THREAD_ATTRIBUTE_HANDLE_LIST`).
- Watchdog de tempo real no Code Host, com *kill* imediato. O Worker é reciclado após qualquer anomalia.

### L5 — Sandbox do sistema operacional
**Windows (MVP: Job Object; Fase 9: completo):**
| Mecanismo | Configuração |
|---|---|
| **Job Object** | `KILL_ON_JOB_CLOSE`, `ACTIVE_PROCESS_LIMIT = 1` (sem filhos), `PROCESS_MEMORY` (256–512 MB), limite de CPU (*CPU rate* ≤ 1 núcleo), restrições de UI (sem clipboard, desktop, hooks globais) |
| **AppContainer** (Fase 9) | perfil próprio **sem capabilities** (sem `internetClient`, sem acesso a bibliotecas do usuário), o que nega acesso à rede externa e ao sistema de arquivos fora do perfil. Conceder ao AppContainer **somente** leitura/execução na pasta do Worker |
| Alternativa ao AppContainer | token restrito (`CreateRestrictedToken`: remover privilégios e SIDs de grupos) + **Low Integrity Level** |
| Loopback | verificar se o AppContainer bloqueia loopback no Windows-alvo (por padrão pode permitir localhost); o Worker não precisa de rede (usa pipes anônimos) |
| Criação | `CreateProcess` com `STARTUPINFOEX` + `PROC_THREAD_ATTRIBUTE_SECURITY_CAPABILITIES`, processo criado **suspenso**, atribuído ao Job e depois retomado (padrão do sandbox do Chromium) |

**Linux / macOS (Fase 21+, ADR-0018):** até lá, essas plataformas não são suportadas.
- **Linux:** `no_new_privs` + **seccomp-bpf** (nega `socket`/`connect`, `execve`, `ptrace`...) + **Landlock** (sistema de arquivos só leitura na pasta do Worker) + rlimits/cgroup quando disponível. **Não depender de user namespaces/bubblewrap**: no Ubuntu 23.10/24.04+ o AppArmor restringe namespaces não privilegiados.
- **macOS:** perfil **Seatbelt** aplicado ao Worker (o mesmo mecanismo do Chromium), hardened runtime com o entitlement de JIT exigido pelo .NET, notarização.

O código do launcher fica atrás de `ISandboxLauncher` desde a Fase 1.

### L6 — Endurecimento do host
- O Code Host trata o Worker como **hostil**: valida esquema, tamanho e tipos de cada mensagem, e nunca desserializa tipos polimórficos arbitrários (`TypeNameHandling.None` no Newtonsoft).
- A Unity trata o Code Host como **semiconfiável**: comandos passam pela Application com as **mesmas validações da UI**. O código do jogador **nunca** pode fazer algo que a UI não permite (custo, desbloqueio, limites).
- Limites de taxa: comandos por execução, consultas por execução, tamanho de resposta.

### L7 — Conteúdo de terceiros (quando houver compartilhamento)
- **Nunca** executar automaticamente código vindo de um save, desafio comunitário ou mod. Ao carregar, mostrar o código e pedir confirmação explícita.
- Pacotes oficiais de desafios **assinados**. Conteúdo comunitário roda **sempre** com L5 completo.
- Sem L5 completo (Fase 9), **não existe** recurso de compartilhamento.

## 4. Allowlist (estrutura)
- Arquivo versionado `dotnet/src/CitySky.CodeAnalysis/Allowlist/allowlist.json`, com entradas canônicas:
```json
{ "version": 3,
  "namespaces": { "System.Collections.Generic": "allow-listed-types-only" },
  "types": {
    "System.String": { "members": "*", "except": ["Intern", "IsInterned"] },
    "System.Math": { "members": "*" },
    "System.Collections.Generic.List`1": { "members": "*" },
    "System.Console": { "members": ["WriteLine", "Write"] }
  },
  "unlocks": { "System.Linq.Enumerable": "concept:collections-linq" }
}
```
- `Console.WriteLine` é redirecionado para o console do jogo (com limite).
- **Desbloqueio progressivo:** algumas APIs exigem conceito desbloqueado (ex.: LINQ depois de "laços e coleções"). Uso antes disso gera `CSK3001`, de caráter pedagógico, não de segurança.
- Toda mudança na allowlist: PR com `security-auditor`, novo caso no corpus e `CHANGELOG` da allowlist.

## 5. Corpus de testes de segurança (`dotnet/tests/CitySky.Security.Tests`)
Cada caso é um `.cs` com o resultado esperado (`RejectedAt: L1|L2`, `Contained: L3|L4|L5`, mensagem):
- leitura e escrita de arquivo (direta, via `FileInfo`, via `StreamReader`, via reflexão);
- `HttpClient`, `Socket`, `Dns.GetHostEntry`;
- `Process.Start`, `Environment.Exit`, `Environment.GetEnvironmentVariable`;
- `Type.GetType` + `Invoke`, `Activator.CreateInstance(string)`, `Delegate.CreateDelegate`;
- `Expression.Lambda(...).Compile()`, `DynamicMethod`, `AssemblyBuilder`;
- `unsafe`, `stackalloc`, `fixed`, ponteiros de função, `DllImport`, `Marshal`;
- laço infinito, recursão infinita, `while(true){}` dentro de `catch`/`finally`, alocação de 10 GB, `List` crescendo sem fim, strings gigantes;
- `Thread`, `ThreadPool.QueueUserWorkItem`, `Task.Run`, `Parallel.For`, `Timer`;
- finalizador, construtor estático pesado, `[ModuleInitializer]`;
- mensagens malformadas do Worker (fuzzing do protocolo, Fase 9);
- tentativa de abrir socket loopback (verificação de L5).

O corpus roda no CI em **todo PR** que toca `CodeAnalysis`, `CodeHost`, `Sandbox.Worker`, `PlayerApi` ou `Contracts`.

## 6. Limitações conhecidas (riscos residuais)
- Bugs em L1/L2 (allowlist incompleta ou forma canônica errada). Mitigação: L5 contém o dano; corpus e revisão.
- Ataques de canal lateral e timing não são mitigados (fora do modelo de ameaças).
- O AppContainer depende da versão do Windows. Mitigação: matriz de testes (Windows 10 22H2, Windows 11 23H2+).
- O jogador pode modificar o próprio jogo (é local). Não tentamos impedir trapaça offline.

## 7. Checklist de revisão de segurança (PR)
- [ ] Nova API exposta? → allowlist + caso no corpus + `get_public_api` revisado.
- [ ] Mudança no protocolo? → validação de esquema + golden file + fuzz.
- [ ] Mudança no Worker/Job/AppContainer? → teste de contenção (FS, rede, processo, memória).
- [ ] Algum caminho executa código do jogador fora do Worker? → **bloqueante**.
- [ ] `/security-scan` sem achados críticos.

## Referências
- Microsoft Learn: plugins e isolamento (*consider a technology provided by your OS or virtualization platform*): https://learn.microsoft.com/dotnet/core/tutorials/creating-app-with-plugin-support
- `AssemblyLoadContext`: https://learn.microsoft.com/dotnet/api/system.runtime.loader.assemblyloadcontext
- CAS não suportado como fronteira: https://msdn.microsoft.com/en-us/library/bb763046
- Implementing an AppContainer: https://learn.microsoft.com/windows/win32/secauthz/implementing-an-appcontainer
- `UpdateProcThreadAttribute`: https://learn.microsoft.com/windows/win32/api/processthreadsapi/nf-processthreadsapi-updateprocthreadattribute
- Chromium sandbox FAQ (tokens restritos, Job Objects, integridade): https://chromium.googlesource.com/chromium/src.git/+show/main/docs/design/sandbox_faq.md
- Space Engineers (allowlist + contador de instruções em scripts C# de jogadores): https://spaceengineers.wiki.gg/wiki/Scripting/The_Anatomy_of_a_Script
