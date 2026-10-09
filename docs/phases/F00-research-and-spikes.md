# Fase 0 — Pesquisa, arquitetura e spikes

> **Status:** **Em andamento** (documentação ✅; spikes ⏳) · **Bloco:** A · **Atualizado:** 2026-10-08
> **Entrada:** briefing (`main.md`) · **Saída:** S1, S2, S6 e S7 resolvidos ou com alternativa aceita em ADR · **Estimativa:** 5 semanas (23 dias de spikes + preparação do ambiente)
> Plano dos spikes: [`docs/spikes/README.md`](../spikes/README.md)

---

## 1. Objetivo
Reduzir os riscos que podem **invalidar a arquitetura** antes de escrever código de produto.

## 2. Por que agora
A arquitetura inteira se apoia em quatro apostas não verificadas: (1) um player IL2CPP consegue falar com um processo .NET 10; (2) um Worker .NET 10 roda preso num sandbox do Windows; (3) existe um host gratuito decente para o Monaco; (4) o pacote instala sem admin em PC de escola. Se qualquer uma falhar, o desenho do Code Host, do editor ou da distribuição muda. Descobrir isso na Fase 1 custa dias; descobrir na Fase 8 custa meses.

## 3. Escopo
**Inclui:** documentação de produto e arquitetura (feita), ADRs 0001–0018 (feitos), preparação do ambiente, 10 spikes com relatório, atualização dos ADRs afetados pelos resultados.
**Não inclui:** código de produto. Código de spike é **descartável** (`spikes/<id>/`) e nunca é copiado para `unity/` ou `dotnet/src/` sem ser reescrito.

## 4. Entregáveis
- [x] `docs/` completo (≈ 75 documentos do §66 + extras) e ADRs 0001–0018.
- [x] Planos das fases (`docs/phases/`) e planos dos spikes (`docs/spikes/S*.md`).
- [ ] Ambiente pronto (tarefas F0-01 a F0-07).
- [ ] 10 relatórios de spike preenchidos.
- [ ] ADRs 0005 (transporte IPC), 0006 (mecanismo do L5) e 0007 (host do editor) atualizados com a decisão.
- [ ] `RISKS.md` revisado com os resultados.

## 5. Backlog

### 5.1 Preparação do ambiente
| ID | Tarefa | Depende de | Pronto quando |
|---|---|---|---|
| F0-01 | ✅ **Mover o projeto para fora do OneDrive** (ex.: `C:\dev\cityskycode`). Unity `Library/` e `.git` dentro de pasta sincronizada causam lentidão, arquivos bloqueados e risco de corromper o repositório | — | pasta nova aberta no Claude Code; OneDrive sem cópia ativa. *Feito em 2026-10-08: cópia verificada em `C:\dev\cityskycode`; falta apagar a cópia antiga do OneDrive* |
| F0-02 | ✅ `git init`, `.gitattributes` (LFS para binários, `*.cs text eol=lf`), `.gitignore` mínimo, commit `docs: initial phase 0 documentation` | F0-01 | `git log` mostra o commit; `git lfs env` ok. *Feito: https://github.com/net0well/cityskycode* |
| F0-03 | **Decisões pendentes do dono do produto** (ver §5.3) registradas | — | respostas anotadas neste documento e, se for o caso, em ADR |
| F0-04 | Unity Hub + **Unity 6.6** + *Windows Build Support (IL2CPP)*; conta Unity Personal ativada; Visual Studio 2026 com as cargas "Desenvolvimento de jogos com Unity" e "Desenvolvimento para desktop com C++" | — | projeto vazio gera player IL2CPP que abre. *2026-10-09: Unity 6000.6.5f1 + IL2CPP instalada e VS com C++ confirmado; **falta ativar a licença Personal** (ação do dono do produto)* |
| F0-05 | `dotnet tool update -g CWM.RoslynNavigator`; dotnet-claude-kit instalado e atualizado | — | `/plugin` lista o kit; MCP responde |
| F0-06 | Conseguir um **PC com GPU integrada** (Intel Iris Xe ou similar, 8 GB): notebook emprestado ou PC de escola parceira | — | máquina disponível por 3 dias para o S3 |
| F0-07 | Conta **Microsoft Partner Center** individual (gratuita, com verificação de identidade) | — | nome do app reservado (provisório) |

### 5.2 Spikes (ordem recomendada; detalhes em cada plano)
| ID | Spike | Plano | Prazo | Depende de | Bloqueia |
|---|---|---|---|---|---|
| F0-10 | ⚠️ **S6** Sandbox Windows (parcial: Job Object contém processos/memória/CPU/UI, sem admin; AppContainer p/ arquivos+rede fica na Fase 9) | [S6](../spikes/S6-sandbox-windows.md) | 3 d | F0-01 | F1 |
| F0-11 | ✅ **S1a** Pipeline do Code Host (concluído: 10/10 do corpus; achados sobre `goto`, corpo-expressão, Debug e atributos de infra) | [S1a](../spikes/S1a-codehost-pipeline.md) | 3 d | — (L5 fica no S6) | F1 |
| F0-12 | ✅ **S4a** Domínio headless 100k (concluído: 0,055 ms/tick médio, 0,52 ms pior) | [S4a](../spikes/S4a-headless-domain.md) | 2 d | — | F5 |
| F0-13 | ✅ **S2b** PhotinoX + Monaco (concluído: editor embutido no painel direito, ADR-0019) | [S2b](../spikes/S2b-photinox-monaco.md) | 2 d | — | F1 |
| F0-14 | **S1b** IL2CPP ↔ Code Host | [S1b](../spikes/S1b-il2cpp-ipc.md) | 2 d | F0-04, S1a | F1 |
| F0-15 | **S2a** UnityWebBrowser no painel | [S2a](../spikes/S2a-unitywebbrowser.md) | 2 d | F0-04, S2b | F1 |
| F0-16 | **S3** Orçamento de renderização URP | [S3](../spikes/S3-urp-render-budget.md) | 3 d | F0-04, F0-06 | gate F2 |
| F0-17 | **S4b** Fator Mono/IL2CPP | [S4b](../spikes/S4b-mono-il2cpp-factor.md) | 1 d | S4a, F0-04 | F5 |
| F0-18 | **S5** Malha de ruas | [S5](../spikes/S5-road-mesh.md) | 3 d | F0-04 | F3 |
| F0-19 | **S7** MSIX + Microsoft Store | [S7](../spikes/S7-msix-store.md) | 2 d | S1b, S6, F0-07 | F1 |

S6, S1a, S4a e S2b só precisam do .NET 10 e rodam enquanto a Unity é instalada.

### 5.3 Decisões pendentes do dono do produto
A documentação não define estes pontos, e eles afetam a Fase 1:

| Pergunta | Por que importa | Opções |
|---|---|---|
| ✅ Repositório **público ou privado**? **Decidido: privado** (github.com/net0well/cityskycode, 2026-10-08); assets gratuitos da Asset Store podem ser versionados enquanto for privado | GitHub Actions tem minutos ilimitados só em repositório público; assets da Asset Store **não podem** ser versionados em repositório público (`ASSET_PIPELINE.md` §5); repositório público expõe os testes ocultos dos desafios (aceitável: não há ranking) | público · privado · privado agora e público no lançamento |
| **Licença do código** do projeto | define se terceiros podem contribuir, reutilizar ou revender; uma licença OSI torna o projeto elegível à assinatura gratuita da SignPath Foundation (hoje descartada, ADR-0017) | MIT/Apache-2.0 · GPL · proprietária (código fechado) |
| Conta GitHub/organização | dona do repositório, dos *secrets* (licença Unity no CI) e do Partner Center | pessoal · organização |

### 5.4 Fechamento
| ID | Tarefa | Depende de | Pronto quando |
|---|---|---|---|
| F0-30 | Atualizar ADR-0005 com o transporte escolhido (named pipe ou TCP loopback) | S1b | ADR com seção "Atualização (S1b)" |
| F0-31 | Atualizar ADR-0006 com o mecanismo do L5 (AppContainer, LPAC ou token restrito + Low IL) | S6, S7 | idem |
| F0-32 | Atualizar ADR-0007 com o host do editor (UWB, PhotinoX ou nativo) | S2a, S2b | ADR com status "Aceito (host: …)" |
| F0-33 | Ajustar orçamentos em `PERFORMANCE.md` com S3, S4a e S4b | S3, S4b | tabelas com números medidos |
| F0-34 | Revisar `RISKS.md` (R02, R03, R05, R06, R08, R15, R22, R23, R29, R30) | todos | probabilidades atualizadas |
| F0-35 | Detalhar riscos remanescentes no plano da Fase 1 | F0-34 | seção de riscos do F01 atualizada |

## 6. Conteúdo pedagógico
Nenhum: a fase não produz conteúdo para o aluno. Mas os spikes geram material de "bastidores" para lições futuras (por exemplo, por que o código do aluno roda num processo separado: lição de segurança e arquitetura no Módulo 11).

## 7. Testes e qualidade
- Cada spike define o critério de sucesso **antes** de começar (tabela no plano) e registra os números.
- Spikes .NET usam `/plan` antes e `/security-scan` no S6 e no S1a, mesmo sendo descartáveis: o objetivo é descobrir cedo o que o scanner vai apontar no código de produto.

## 8. Gate de saída
- [ ] S1 (S1a + S1b), S2 (S2a + S2b), S6 e S7 com hipótese confirmada **ou** alternativa aceita em ADR.
- [ ] S3, S4 e S5 com relatório; se falharem, a mudança de abordagem fica registrada antes da fase que eles bloqueiam.
- [ ] Decisões do §5.3 respondidas.
- [ ] F0-30 a F0-35 concluídas.

## 9. Riscos
| Risco | Ref. | Mitigação nesta fase |
|---|---|---|
| IPC não funciona no IL2CPP | R02 | S1b com fallback TCP |
| Host do editor inadequado | R03, R15, R22 | S2a × S2b × fallback nativo |
| Sandbox não roda o .NET 10 | R04 | S6 testa AppContainer, LPAC e token restrito |
| Bloqueio em PC de escola | R23 | S7 com instalação sem admin a partir da Store |
| Store recusar app que compila e executa código | **R30** | S7 lê a política 10.2.2 vigente e consulta o suporte do Partner Center |
| Windows 10 sem suporte nas escolas | **R29** | S6/S7 testam Win10 22H2; decisão de suporte registrada |
| Ambiente atrasar (Unity, PC fraco, Partner Center) | — | spikes .NET primeiro; pedir o PC com Iris Xe cedo |

## 10. Documentos e ADRs a atualizar
ADR-0005, 0006, 0007 · `PERFORMANCE.md` · `RISKS.md` · `TECH_STACK.md` (versões medidas, host do editor) · `CODE_EDITOR.md` · `CODE_EXECUTION.md` §6 (transporte) · `CODE_EXECUTION_SECURITY.md` §3 L5 · este plano e o `docs/spikes/README.md` (status).

## 11. Ferramental
`/plan` antes de cada spike .NET · `/security-scan` (S6, S1a) · `security-auditor` revisa o relatório do S6 · `performance-analyst` revisa a metodologia do S4a.

## 12. Encerramento
- [ ] Gate verificado e registrado
- [ ] `RISKS.md` revisado
- [ ] Velocidade real dos spikes registrada (dias planejados × gastos) e estimativa da Fase 1 recalibrada
- [ ] Plano da Fase 1 revisado com o que os spikes ensinaram
