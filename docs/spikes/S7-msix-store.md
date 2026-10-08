# Spike S7: Pacote MSIX, instalação sem admin e Microsoft Store

- **Status:** Planejado
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
| Métrica | Alvo | Resultado |
|---|---|---|
| MSIX gerado (`makeappx`) com player + Host + Worker | sim | |
| Instalação local (sideload com certificado de teste) na máquina de desenvolvimento | sim | |
| Reserva do nome e envio do pacote ao Partner Center (audiência privada ou *package flight*) | aceito | |
| Instalação **pela Store** numa conta Windows **padrão (sem admin)** | sim | |
| A partir do app instalado: S1b (Host + IPC) passa | sim | |
| A partir do app instalado: corpus do S6 passa (Worker contido) | sim | |
| Caminho real onde o app grava saves e perfis (virtualização do MSIX) registrado | — | |
| Desinstalação remove tudo (exceto o que o usuário escolher manter) | sim | |
| Tamanho do pacote (player + runtime .NET compartilhado por Host e Worker) | registrado | |
| Política 10.2.2 vigente lida; dúvida enviada ao suporte do Partner Center; resposta registrada | sim | |
| Windows 10 22H2 (VM) instala e roda o pacote | sim | |

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
*As seções abaixo são preenchidas ao terminar.*

## O que foi feito

## Resultado

## Decisão
- [ ] Hipótese confirmada: seguir com a arquitetura documentada
- [ ] Hipótese refutada: alternativa adotada = …
- [ ] Inconclusivo: próximo passo = …

**ADR criado ou atualizado:**

## Riscos que sobraram
