# Spike S7: Pacote MSIX, instalação sem admin e Microsoft Store

- **Status:** ⚠️ Parcial (2026-10-10) — política 10.2.2 (R30) analisada e sob controle; empacotamento MSIX + Store pendentes da **conta Partner Center** (ação do dono)
- **Prazo:** 2 dias · **Ordem:** 10º
- **Depende de:** S1b (Host iniciado pelo player), S6 (sandbox), conta Partner Center (F0-07) · **Bloqueia:** Fase 1 (`IFileLocations`, layout do pacote)
- **Código:** `spikes/S7-msix-store/` (descartável)
- **ADRs relacionados:** [ADR-0017](../ADR/0017-product-scope-classroom.md), [ADR-0006](../ADR/0006-code-security.md), [ADR-0018](../ADR/0018-multiplatform-and-steam.md) · Documentos: [`CLASSROOM.md`](../CLASSROOM.md) §1, [`SECURITY.md`](../SECURITY.md) §1

## Pergunta
O player IL2CPP de teste, o Code Host e o Worker, empacotados num **MSIX**, instalam **por usuário, sem admin**, iniciam os processos auxiliares, e o Worker continua preso no **Job Object + AppContainer** a partir do app empacotado? A publicação de teste na **Microsoft Store** (audiência privada) funciona com conta individual gratuita, e a política da Store aceita um app que compila e executa código escrito pelo usuário?

## Hipótese
Sim para o empacotamento: um app *full trust* (`runFullTrust`) pode iniciar executáveis do próprio pacote, e a pasta de instalação já concede leitura a *ALL APPLICATION PACKAGES*, o que deve dispensar a ACL manual do S6. A incerteza está na **política da Store**: a cláusula 10.2.2 proíbe mudar ou estender a funcionalidade descrita por inclusão dinâmica de código. Código **escrito pelo próprio usuário**, num app educacional que declara isso, provavelmente é aceito (há compiladores e IDEs na Store), mas precisa ser confirmado.

## Por que importa
A Store é o canal escolhido para escolas: instala sem admin e evita o bloqueio de SmartScreen/Smart App Control (R23). Se o pacote quebrar o sandbox, ou se a Store recusar o app (R30), a distribuição em escolas precisa de outro caminho (itch.io + instruções para a TI, sem assinatura) e o ADR-0017 é revisado.

## Critério de sucesso (definido antes de começar)
> **Parte feita sem conta (2026-10-10): análise da política 10.2.2 (R30).** O empacotamento MSIX, o sideload, o envio à Store e a VM Windows 10 dependem de **conta Partner Center + Windows SDK/certificado + VM = ação do dono** e ficam pendentes.

| Métrica | Alvo | Resultado |
|---|---|---|
| MSIX gerado (`makeappx`) com player + Host + Worker | sim | ⏳ pendente (receita documentada abaixo; precisa do Windows SDK) |
| Instalação local (sideload com certificado de teste) na máquina de desenvolvimento | sim | ⏳ pendente (sideload de cert de teste pede admin para confiar no cert) |
| Reserva do nome e envio do pacote ao Partner Center | aceito | ⏳ **pendente — precisa de conta Partner Center** |
| Instalação **pela Store** numa conta Windows **padrão (sem admin)** | sim | ⏳ pendente (depende do envio) |
| A partir do app instalado: S1b (Host + IPC) passa | sim | ⏳ pendente — mas o lançamento por `CreateProcess` nativo (S1b) é permitido sob `runFullTrust` (ver abaixo) |
| A partir do app instalado: corpus do S6 passa (Worker contido) | sim | ⏳ pendente |
| Caminho real onde o app grava saves e perfis (virtualização MSIX) | — | ⏳ pendente (será `%LOCALAPPDATA%\Packages\<id>\LocalCache\...`; `IFileLocations` deve usar `ApplicationData`) |
| Desinstalação remove tudo | sim | ⏳ pendente |
| Tamanho do pacote | registrado | ⏳ pendente (estimativa: player IL2CPP + 1 runtime .NET self-contained compartilhado por Host e Worker ≈ 150–200 MB) |
| **Política 10.2.2 vigente lida e analisada (R30)** | sim | ✅ **feito** — texto 7.20 (14/09/2026) lido; design **alinhado** (ver Resultado); dúvida ao suporte fica para quando houver conta |
| Windows 10 22H2 (VM) instala e roda o pacote | sim | ⏳ pendente (VM) |

## Escopo
**Inclui:** manifesto do pacote (identidade, `runFullTrust`, ícones mínimos), Host e Worker publicados na **mesma pasta** para compartilhar o runtime *self-contained* (~70 MB uma vez só), `makeappx` + `signtool` com certificado de teste para sideload, envio à Store em audiência privada, instalação em conta sem admin, repetição de S1b e S6 a partir do pacote.
**Não inclui:** página pública da Store, certificação final de lançamento (Fase 20), itch.io (secundário, sem pacote).

## Ambiente
Windows 11 (dev) com Windows SDK (`makeappx`, `signtool`); VM Windows 10 22H2; conta de usuário padrão de teste; conta Partner Center individual.

## Plano de execução
**Dia 1:** montar o layout (player IL2CPP do S1b + Host + Worker na mesma pasta de runtime); manifesto; `makeappx pack`; certificado de teste; instalar localmente; verificar caminhos de escrita (saves, logs, perfis do AppContainer) e rodar S1b e S6 a partir do app instalado.
**Dia 2:** reservar o nome e enviar o pacote para audiência privada; enquanto a certificação corre, ler a política 10.2.2 vigente e abrir chamado no suporte do Partner Center descrevendo o caso ("app educacional que compila e executa, num sandbox, código C# escrito pelo próprio usuário; nenhum código é baixado"); instalar pela Store na conta sem admin quando aprovado; repetir na VM Windows 10; relatório.

## Método de medição
Checklist da tabela; tamanho do `.msix`; tempo de instalação; caminhos registrados com Process Monitor.

## Armadilhas conhecidas
- A certificação da Store pode levar dias: enviar no início do Dia 2 e registrar o tempo real; o spike pode fechar com a instalação por sideload e a conclusão da Store chegar depois (registrar como pendência, sem bloquear a Fase 1).
- Instalar o certificado de teste no repositório *Trusted People* da máquina pede admin: é só para o teste local; o critério "sem admin" vale para a instalação **pela Store**.
- Apps empacotados têm escrita em `AppData` virtualizada por pacote: `IFileLocations` precisa usar o caminho correto, ou os saves "somem" para o usuário.
- **Windows 10:** o suporte terminou em 14/out/2025; o ESU para consumidores foi estendido até **12/out/2027** (Microsoft, jun/2026); escolas dependem de ESU comercial ou educacional. Registrar se o pacote funciona e se a Store ainda o distribui para Windows 10.

## Saídas esperadas
- Layout do pacote e caminhos de escrita → `IFileLocations` (F1-53) e `SAVE_SYSTEM.md`.
- Confirmação (ou não) do sandbox dentro do pacote → ADR-0006.
- Posição sobre a política 10.2.2 → `RISKS.md` R30 e ADR-0017.
- Decisão sobre suporte a Windows 10 → `MVP.md` §2 e `RISKS.md` R29.

---
*Parcial em 2026-10-10 — a análise da política (R30) foi feita sem conta; o empacotamento e a Store dependem do dono.*

## O que foi feito
- **Análise da política 10.2.2** (R30) a partir do texto oficial vigente (Microsoft Store Policies **7.20**, `ms.date: 2026-09-14`).
- Receita de empacotamento MSIX e checklist de ação para quando a conta Partner Center existir (abaixo).
- **Não feito** (bloqueado por conta/SDK/VM): gerar e assinar o `.msix`, sideload, envio à Store, instalação sem admin pela Store, repetir S1b/S6 a partir do pacote, VM Windows 10.

## Resultado — política 10.2.2 (texto e análise)
Texto vigente (7.20):
> **10.2.2** — Your product must not attempt to fundamentally change or extend its described functionality or introduce features or functionality that are in violation of Store Policies through any form of dynamic inclusion of code. Your product should not, for example, download a remote script and subsequently execute that script in a manner that is not consistent with the described functionality.

A regra proíbe três coisas: (a) **mudar/estender a funcionalidade *descrita*** por código dinâmico; (b) **introduzir recursos que violem as políticas** da Store por código dinâmico; (c) o exemplo nomeado: **baixar um script remoto e executá-lo de forma inconsistente** com o que foi descrito.

**Por que o City: SkyCode se encaixa:**
1. **Nenhum código é baixado nem remoto.** O jogador escreve C# **localmente**, compilado por Roslyn **localmente**, offline (ADR-0017/LGPD). O exemplo (c) — o único caso concreto da política — **não se aplica**.
2. **Executar o código do jogador É a funcionalidade descrita.** O produto se declara, nos metadados da Store, como um *sandbox educacional de programação* onde o aluno escreve e roda C# para aprender. Rodar esse código é **consistente** com a descrição, não uma mudança dela — igual aos muitos IDEs/interpretadores/apps de ensino de código já presentes na Store.
3. **O sandbox impede violar outras políticas** (clausula b): allowlist L1 + verificação de IL L2 + orçamentos L3 + Job Object/AppContainer L5 cortam rede, disco e APIs arbitrárias. O código do jogador **não tem poder que a UI não tenha** (regra inviolável do projeto).
4. **Compartilhamento por `.citysky` é só código-fonte, nunca executa sozinho**, e só depois do sandbox L5 (Fase 9): o arquivo é aberto no mesmo editor/sandbox descrito, não é "código dinâmico de origem remota".

**Conclusão (R30):** o desenho está **alinhado** com a 10.2.2; o risco cai de "pode invalidar a arquitetura" para "cuidado na redação da ficha da Store + revisão humana da certificação pode ser conservadora". **Mitigações:** descrever o app com precisão (sandbox de programação local, offline, sem código remoto, isolado); se a certificação recusar mesmo assim, a própria política oferece a **10.2.9** (distribuir por *download URL* HTTPS versionada, direto) e o ADR-0017 já prevê **itch.io e download direto** — ou seja, a recusa da Store **não bloqueia** a distribuição em escolas, só muda o canal (R23: perde-se a isenção de SmartScreen/instalação sem admin da Store).

**Compatibilidade do lançamento sob MSIX:** um app desktop empacotado declara a *restricted capability* **`runFullTrust`** (comum e aceita para apps desktop na Store), que concede direitos Win32 completos — inclusive **`CreateProcess`** para iniciar o Code Host e o Worker (o caminho do S1b/S6). Logo, o empacotamento MSIX não conflita com a arquitetura de processos; resta confirmar na prática (pendente).

## Receita de empacotamento (para executar com a conta/SDK)
1. Layout: player IL2CPP + Code Host + Worker na **mesma pasta**, com **um** runtime .NET self-contained compartilhado (publicar Host e Worker com `--no-self-contained` apontando para um runtime comum, ou um único framework-dependent + runtime embarcado uma vez).
2. `AppxManifest.xml`: `Identity`, `<rescap:Capability Name="runFullTrust"/>`, `Application` apontando para o `.exe` do player, ícones mínimos.
3. `makeappx pack /d <pasta> /p CitySkyCode.msix`.
4. `signtool` com certificado de teste → sideload local (confiar no cert pede admin, só para teste).
5. Partner Center: reservar o nome, enviar em **audiência privada**; abrir chamado descrevendo o caso (app educacional que compila/executa C# do usuário num sandbox, nada baixado).
6. Medir: caminho de escrita (Process Monitor → `%LOCALAPPDATA%\Packages\<id>\...`), tamanho do `.msix`, repetir S1b e S6 a partir do app instalado, instalar pela Store numa conta **sem admin**, repetir na VM Windows 10 22H2.

## Decisão
- [~] **Parcial / segue com a arquitetura.** O risco que podia derrubar o projeto (R30, política 10.2.2) foi **analisado e está sob controle**: o design é compatível e, no pior caso, há canais alternativos (10.2.9 / itch.io / direto). O empacotamento MSIX e a publicação na Store são **mecânicos** e ficam pendentes da **conta Partner Center** (ação do dono), sem bloquear a Fase 1 além do que já se sabe.

**ADR criado ou atualizado:** nenhum alterado ainda; quando o empacotamento rodar: `RISKS.md` R30 (rebaixar), `SECURITY.md` §1, `IFileLocations` (F1-53, usar `ApplicationData`/`LocalCache`). Posição de Windows 10 (R29) permanece como no plano (ESU até 12/out/2027).

## Checklist / ação do dono
- [ ] Criar conta **Partner Center** (individual; taxa única). Então rodar a receita acima.
- [ ] Instalar o **Windows SDK** (`makeappx`, `signtool`) na máquina de dev.
- [ ] (Opcional) VM **Windows 10 22H2** no Hyper-V para o teste de compatibilidade.
- [ ] Abrir chamado no suporte do Partner Center com o texto do caso (10.2.2) e **registrar a resposta** aqui.

## Riscos que sobraram
- **Certificação humana da Store** pode ser conservadora apesar da análise — mitigado por redação precisa + fallback 10.2.9/itch.io/direto.
- **Empacotamento não exercitado** (sem SDK/conta): a compatibilidade `runFullTrust` + `CreateProcess` + caminhos virtualizados é teoria sólida, mas não medida.
- **Windows 10** (R29): confirmar se a Store ainda distribui para Win10 e se o pacote roda lá (VM pendente).
