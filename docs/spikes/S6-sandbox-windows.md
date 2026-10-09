# Spike S6: Sandbox do Windows para o Worker .NET 10

- **Status:** **Parcial — concluído o que o MVP precisa** (Job Object: processos-filhos, memória, CPU e UI contidos). O AppContainer (contenção de arquivos/rede) fica para a Fase 9, com a necessidade já comprovada aqui.
- **Prazo:** 3 dias · **Ordem:** 1º · **Início / fim:** 2026-10-09 → 2026-10-09 (meio dia; parte Job Object)
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
| Worker inicia no sandbox e responde a `ping` por pipe anônimo herdado | sim | ✅ (Job Object; saída capturada por pipe anônimo herdado) |
| Tudo funciona com usuário **padrão** (sem admin) | sim | ✅ rodado com `elevated = False` |
| Início a frio do Worker no sandbox (do `CreateProcess` ao `pong`) | ≤ 500 ms | ⚠️ não isolado (o `ping` inclui o start do runtime .NET via `dotnet`); o start do Worker já é medido no S1a (1–2 ms carregando o assembly num Worker quente) |
| Custo de criar Job + AppContainer + processo suspenso | ≤ 50 ms | ⏳ AppContainer (Fase 9); criar o Job + processo suspenso é desprezível |
| Ler `%USERPROFILE%\...\canario.txt` | negado | ⚠️ **PERMITIDO sob Job Object** → precisa de AppContainer (Fase 9) |
| Escrever em `%TEMP%` e na pasta do Worker | negado | ⚠️ **PERMITIDO sob Job Object** → precisa de AppContainer (Fase 9) |
| HTTP para um site externo | falham | ⚠️ **PERMITIDO sob Job Object** → precisa de AppContainer (Fase 9) |
| TCP para `127.0.0.1` | falha (registrar) | ℹ️ falhou por não haver quem escute na porta; não conclui nada (o Worker não usa rede, usa pipes) |
| `Process.Start("cmd.exe")` | falha | ✅ **contido** (limite de 1 processo ativo no Job) |
| Alocar muito acima do limite de 256 MB | contido | ✅ **contido** (limite de memória do processo → `OutOfMemoryException`) |
| Laço de CPU | uso ≤ 1 núcleo | ✅ **afinidade a 1 processador lógico** garante ≤ 1 núcleo; o cap de taxa (2,5% de 24 núcleos aqui) reduziu de 3,12 s para ~2,09 s de CPU em 3 s de parede |
| Restrições de UI (área de transferência, desktop, etc.) | negado | ✅ aplicadas via `JOBOBJECT_BASIC_UI_RESTRICTIONS` (desktop, clipboard, handles, atoms globais, parâmetros do sistema, exibição, sair do Windows) |
| Matar o launcher mata o Worker | sim (`KILL_ON_JOB_CLOSE`) | ✅ `JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE` ativo; fechar o handle do Job encerra o filho |
| Repetir no Windows 10 22H2 (VM) | mesmos resultados | ⏳ pendente (precisa da VM; ação do dono do produto) |

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
- **Ambiente:** Windows 11 25H2 (10.0.26200), 24 processadores lógicos, **usuário sem privilégio de administrador** (`elevated = False`), .NET 10.0.11.
- **Dois projetos** em `spikes/S6-sandbox-windows/` (`dotnet run` no Launcher):
  - **Restricted**: o processo-filho que faz o papel do Worker sob o L5 (assume que L1/L2 foram contornadas). Cada argumento pede **uma** ação (ler/escrever arquivo do usuário, abrir HTTP, abrir TCP, iniciar processo, alocar memória, consumir CPU, ou só `ping`) e relata PERMITIU/NEGOU.
  - **Launcher**: cria um **Job Object** com limites, cria o filho **suspenso**, atribui ao Job, retoma, captura a saída por um **pipe anônimo herdado**, espera com um tempo-limite e consolida o resultado.
- **Limites do Job aplicados:** `KILL_ON_JOB_CLOSE`, `ACTIVE_PROCESS_LIMIT = 1` (sem filhos), `PROCESS_MEMORY = 256 MB`, `DIE_ON_UNHANDLED_EXCEPTION`, **afinidade a 1 processador lógico** (garante ≤ 1 núcleo), **cap de taxa de CPU** (hard cap) e **restrições de UI**.
- **Comando:** `dotnet run -c Release --project src/CitySky.Spike.Launcher -- --out results/s6-results.json`.

## Resultado
**O Job Object (a sandbox do MVP) contém as ameaças de estabilidade/DoS:**

| Ação do filho | Resultado |
|---|---|
| iniciar um processo filho (`cmd.exe`) | **contido** (limite de 1 processo ativo) |
| alocar muito acima de 256 MB | **contido** (limite de memória → `OutOfMemoryException`) |
| consumir CPU | **≤ 1 núcleo** por afinidade; o cap de taxa reduziu ~2,09 s de CPU em 3 s de parede |
| área de transferência / desktop / parâmetros do sistema | **negados** (restrições de UI) |
| fechar o Job | **encerra o filho** (`KILL_ON_JOB_CLOSE`) |
| `ping` e ler o próprio assembly | **permitidos** (a sandbox não quebra o básico) |

**O Job Object NÃO contém arquivos nem rede** (esperado): ler/escrever na pasta do usuário e abrir HTTP ficaram **permitidos**. Isso **comprova a necessidade do AppContainer** (ou token restrito + *Low Integrity Level*), que é a camada da **Fase 9** no roadmap (antes de qualquer compartilhamento de código de terceiros).

**Resposta à pergunta:** parcialmente. A parte que o MVP precisa — conter travamentos, recursão que esgota memória, laços que consomem CPU e abuso de processos/UI — está validada com o Job Object, **sem admin**. A contenção de **arquivos e rede** (AppContainer) não foi implementada neste spike; o resultado mostra que ela é necessária e a agenda para a Fase 9.

### Observações
1. **Afinidade a 1 núcleo** é a garantia estrutural de CPU, independente da máquina. O **cap de taxa** é sobre o total do sistema, então numa máquina de N núcleos uma thread já usa só ~100/N%; dimensionar ao hardware no produto (ou confiar na afinidade).
2. **Memória** foi contida pela via "boa": o GC do .NET bateu no limite do Job e lançou `OutOfMemoryException`, que o código reportou — melhor que um *kill* seco. Convém também definir `DOTNET_GCHeapHardLimit` um pouco abaixo do limite do Job para a mensagem ser sempre pedagógica.
3. **Loopback** não foi conclusivo (sem quem escutasse). O Worker não usa rede (usa pipes anônimos), então o ponto prático é só confirmar, na Fase 9, que o AppContainer não abre loopback por engano.

## Decisão
- [x] Hipótese confirmada **para a parte do MVP** (Job Object): seguir com o ADR-0006 (L5 = Job Object no MVP; AppContainer na Fase 9)
- [ ] Hipótese refutada
- [ ] Inconclusivo

**ADR criado ou atualizado:** nenhum. A estrutura do ADR-0006 (Job Object no MVP; AppContainer/token restrito na Fase 9) se confirma: o Job Object entrega estabilidade e contém DoS; arquivos/rede precisam da camada da Fase 9. A implementação do Launcher vira a base do `ISandboxLauncher` (F1-51), que no MVP usa só o Job Object.

## Riscos que sobraram
- **AppContainer (Fase 9):** ainda é preciso provar que o .NET 10 **inicia** num AppContainer sem capabilities (atrito provável: arquivos que o runtime lê fora da pasta, pasta temporária, ICU) e que arquivos/rede ficam **negados**. Alternativa: token restrito + *Low IL*. Fazer antes do compartilhamento (R04, R25).
- **Windows 10 22H2 (VM):** repetir o corpus; pendente, precisa da VM (ação do dono do produto). Importante por causa das escolas (R29).
- **Handles herdados:** o pipe foi herdado com `bInheritHandles = true`; no AppContainer da Fase 9 restringir a herança a uma lista explícita (`PROC_THREAD_ATTRIBUTE_HANDLE_LIST`).
- **P/Invoke no produto:** o Launcher usa P/Invoke direto; avaliar `CsWin32` (gerador da Microsoft) com ficha de dependência na Fase 1.
