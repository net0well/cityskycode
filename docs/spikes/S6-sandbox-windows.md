# Spike S6: Sandbox do Windows para o Worker .NET 10

- **Status:** Planejado
- **Prazo:** 3 dias · **Ordem:** 1º
- **Depende de:** .NET SDK 10 (já instalado) · **Bloqueia:** S1a, S7, Fase 1 (`ISandboxLauncher`)
- **Código:** `spikes/S6-sandbox-windows/` (descartável)
- **ADRs relacionados:** [ADR-0006](../ADR/0006-code-security.md), [ADR-0005](../ADR/0005-code-execution.md) · Documento: [`CODE_EXECUTION_SECURITY.md`](../CODE_EXECUTION_SECURITY.md) §3 L4–L5

## Pergunta
Um Worker .NET 10 *self-contained* inicia e responde dentro de um **Job Object** combinado com **AppContainer sem capabilities** (ou, como alternativa, token restrito + Low Integrity Level), **sem privilégio de administrador**, e o sistema operacional bloqueia acesso a arquivos fora da pasta do Worker, rede (externa e loopback), criação de processos, e limita memória e CPU?

## Hipótese
Sim. O AppContainer funciona para processos Win32 comuns e não exige admin para criar o perfil. O .NET deve iniciar desde que a pasta do Worker tenha uma ACE de leitura e execução para o SID do AppContainer (ou para *ALL APPLICATION PACKAGES*). Os pontos de atrito prováveis são: arquivos que o runtime tenta ler fora da pasta (ICU, configuração), a pasta temporária e o tempo de início.

## Por que importa
O L5 é a **única camada que contém um atacante** (`CODE_EXECUTION_SECURITY.md` §1). Se o .NET 10 não rodar em nenhuma forma de sandbox do SO, o modelo de execução precisa mudar (ex.: interpretar em vez de executar, ou abandonar o compartilhamento de código), o que afeta ADR-0005, ADR-0006 e ADR-0017.

## Critério de sucesso (definido antes de começar)
| Métrica | Alvo | Resultado |
|---|---|---|
| Worker inicia no sandbox e responde a `ping` por pipe anônimo herdado | sim | |
| Tudo funciona com usuário **padrão** (sem admin) | sim | |
| Início a frio do Worker no sandbox (do `CreateProcess` ao `pong`) | ≤ 500 ms | |
| Custo de criar Job + AppContainer + processo suspenso | ≤ 50 ms | |
| Ler `%USERPROFILE%\Documents\canario.txt` | negado | |
| Escrever em `%TEMP%` do usuário e na pasta do Worker | negado | |
| HTTP para um site externo e resolução DNS | falham | |
| TCP para `127.0.0.1` (listener aberto pelo launcher) | falha (registrar o comportamento real) | |
| `Process.Start("cmd.exe")` | falha | |
| Alocar 1 GB com limite de 256 MB | contido; launcher detecta e reporta | |
| 8 threads em laço quente | uso ≤ 1 núcleo (limite de CPU) | |
| Ler/escrever a área de transferência | negado | |
| Escrever em `HKCU\Software\CitySkySpike` | negado | |
| Matar o launcher mata o Worker | sim (`KILL_ON_JOB_CLOSE`) | |
| Repetir tudo no Windows 10 22H2 (VM) | mesmos resultados | |

## Escopo
**Inclui:** launcher .NET 10 (console) com P/Invoke; Worker .NET 10 com "modos de ataque" escolhidos por argumento (código C# compilado normalmente, **sem** Roslyn: este spike testa o L5 **isolado**, assumindo que L1 e L2 foram burlados); três variantes de sandbox: (A) AppContainer, (B) **LPAC** (*Less Privileged AppContainer*), (C) token restrito + Low IL; todas dentro de um Job Object.
**Não inclui:** Roslyn e orçamentos (S1a), MSIX (S7), Linux/macOS (Fase 21+), fuzzing (Fase 9).

## Ambiente
Windows 11 Pro (máquina de desenvolvimento) e VM Windows 10 22H2 (Hyper-V). .NET SDK 10.0.400. Usuário de teste **sem admin** criado na máquina. P/Invoke manual ou `Microsoft.Windows.CsWin32` (gerador de código da Microsoft, MIT): pode ser usado no spike; para o produto, decidir na Fase 9 com ficha de dependência.

## Plano de execução
**Dia 1 — Job Object e processo suspenso**
1. Launcher: `CreateJobObject` + `SetInformationJobObject` com `JobObjectExtendedLimitInformation` (`JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE`, `JOB_OBJECT_LIMIT_ACTIVE_PROCESS = 1`, `JOB_OBJECT_LIMIT_PROCESS_MEMORY = 256 MB`), `JobObjectCpuRateControlInformation` (*hard cap* ≈ 1 núcleo) e `JobObjectBasicUIRestrictions` (clipboard, desktop, hooks globais, configurações do sistema).
2. Pipes anônimos para stdin/stdout; `STARTUPINFOEX` com `PROC_THREAD_ATTRIBUTE_HANDLE_LIST` contendo **só** os pipes.
3. `CreateProcess` com `CREATE_SUSPENDED | EXTENDED_STARTUPINFO_PRESENT` → `AssignProcessToJobObject` → `ResumeThread` (padrão do sandbox do Chromium).
4. Worker publicado *self-contained* (`win-x64`, **não** *single-file*) responde `pong`. Medir tempos.

**Dia 2 — AppContainer e LPAC**
1. `CreateAppContainerProfile("CitySky.Worker.Spike", …)` (ou `DeriveAppContainerSidFromAppContainerName` se já existir); `SECURITY_CAPABILITIES` com **zero** capabilities; `PROC_THREAD_ATTRIBUTE_SECURITY_CAPABILITIES`.
2. Conceder RX na pasta do Worker ao SID do AppContainer (`SetNamedSecurityInfo` ou `icacls … /grant *S-1-15-2-…:(OI)(CI)RX`). Registrar quais outros caminhos o runtime precisou (Process Monitor com filtro `ACCESS DENIED`).
3. Variante LPAC: `PROC_THREAD_ATTRIBUTE_ALL_APPLICATION_PACKAGES_POLICY` com *opt-out*. Verificar se o .NET 10 inicia (o LPAC nega até leituras que *ALL APPLICATION PACKAGES* teria).
4. Se o runtime tropeçar em globalização, testar `InvariantGlobalization=true` no Worker (o Worker não precisa de cultura do usuário; mensagens pt-BR são montadas no Code Host).

**Dia 3 — Corpus, alternativa e Windows 10**
1. Rodar o corpus da tabela nas variantes A, B e C.
2. Variante C: `CreateRestrictedToken` (remover privilégios e desabilitar SIDs de grupo) + rótulo Low IL + `CreateProcessAsUser`.
3. Repetir na VM Windows 10 22H2.
4. Rodar `/security-scan` no código do launcher; relatório; revisão do `security-auditor`.

## Método de medição
Tempos com `Stopwatch` no launcher (média e p95 de 50 execuções, frio e quente). Bloqueios: cada modo de ataque devolve `ALLOWED`, `DENIED(<exceção/código Win32>)` ou é morto (código de saída do Job). Process Monitor para listar acessos negados durante o início do runtime.

## Armadilhas conhecidas
- **Loopback:** o comportamento do AppContainer com `127.0.0.1` depende da configuração de isolamento de rede; registrar o que acontece de fato. O Worker não precisa de rede (usa pipes anônimos), então qualquer conexão bem-sucedida é um achado.
- **Limite de memória:** o Job mata o processo sem mensagem útil. Testar também `DOTNET_GCHeapHardLimit` ligeiramente abaixo do limite do Job: o .NET lança `OutOfMemoryException` antes, e o Worker consegue reportar "memória esgotada" com contexto (mensagem pedagógica melhor).
- *Single-file* extrai bibliotecas nativas para uma pasta temporária, que o sandbox nega. Usar publicação em pasta.
- Perfis de AppContainer ficam registrados no usuário: limpar com `DeleteAppContainerProfile` no fim dos testes.
- Antivírus pode atrasar o primeiro início; medir com e sem exclusão, e registrar.

## Saídas esperadas
- Escolha do mecanismo do L5 (A, B ou C) e dos parâmetros do Job → **ADR-0006** atualizado.
- Lista de ACLs e variáveis de ambiente que o Worker precisa → entrada para o S7 (pacote MSIX) e para `ISandboxLauncher` (F1-51).
- Casos do corpus que viram testes em `CitySky.Security.Tests` (reescritos na Fase 1).

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
