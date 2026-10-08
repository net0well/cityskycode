# Fase 5 — Primeiros cidadãos

> **Status:** Planejada · **Bloco:** B (Vertical Slice) · **Atualizado:** 2026-10-08
> **Entrada:** gate da Fase 4; relatórios do S4a e S4b · **Saída:** 50 cidadãos vivendo 3 dias de jogo de forma determinística, com 0 alocação por frame · **Estimativa:** 4 semanas

---

## 1. Objetivo
Domicílios chegam às casas livres, os adultos procuram emprego, trabalham no mercado, na usina e na prefeitura, vão às compras e voltam a pé pelas calçadas. O jogador pode seguir qualquer cidadão.

## 2. Por que agora
Cidadãos são o que torna a cidade "viva" (§12) e são a ponte para a economia (salários, compras) e para a felicidade. É também onde o modelo orientado a dados (stores SoA, baldes) encontra pela primeira vez uma carga real.

## 3. Escopo
**Inclui:** `HouseholdStore`, `CitizenStore`, nomes determinísticos pt-BR, máquina de estados da atividade (`CITIZEN_SYSTEM.md` §3), template "trabalhador diurno" com variação de ±1 h, viagens a pé, A* no grafo de calçadas com cache versionado, planejamento com orçamento (32 buscas/tick), emprego por distância, chegada de domicílios (atratividade provisória), agentes GameObject atrás de `IAgentPresentation`, seguir cidadão, "Show Citizen Paths", Inspector do cidadão.
**Não inclui:** felicidade e emigração por infelicidade (F7), salários e compras com dinheiro (F6), crianças e idosos (F13), veículos (F11), ECS (F11).

### Regra provisória de atratividade
Sem felicidade (F7), a chegada usa só **moradia livre** e **vagas abertas**. A regra fica num único método do domínio (`Attractiveness.Compute`) e é **reescrita** na F7 com a regra completa. Não se cria interface para isso: há uma regra só, que muda de conteúdo, e uma abstração "para o futuro" violaria o §76.

## 4. Entregáveis
- População de até 50 adultos com rotina diária visível.
- Calibração registrada de `R` e `MobilitySpeedScale` (questão aberta do `SIMULATION_TICK.md` §9).

## 5. Backlog

### E1 — Domínio: população
| ID | Tarefa | Onde | Depende de | Pronto quando |
|---|---|---|---|---|
| F5-01 | `HouseholdStore` e `CitizenStore` (SoA, iteração sem LINQ, `swap-remove`) | Domain | F1 | testes de adição/remoção e invariantes |
| F5-02 | Gerador de nomes determinístico (tabelas pt-BR em `content/`) | Domain / content | F5-01 | mesmo seed → mesmo nome; nenhum nome armazenado |
| F5-03 | `MigrationSystem` (diário, 06:00): cria domicílios de 1–4 adultos em casas livres, com saldo inicial vindo de `External` (categoria `MigrationIn`) | Domain | F5-01, F3-11 | conservação do ledger intacta |
| F5-04 | `Attractiveness.Compute` provisória (moradia + vagas) | Domain | F5-03 | testes de tabela; reescrita na F7-12 |
| F5-05 | `ScheduleSystem` (baldes de 120): máquina de estados `AtHome → TravelingToWork → Working → TravelingToShop → Shopping → TravelingHome → Sleeping` | Domain | F5-01 | teste de todas as transições e horários de borda |
| F5-06 | Horário de saída = início do turno − tempo estimado (cache) − folga | Domain | F5-05, F5-12 | cidadão chega no horário em cenário de referência |
| F5-07 | `EmploymentSystem` (1 h TC): vagas × desempregados, prioridade por distância; eventos `CitizenHired`/`CitizenFired` | Domain | F5-01, F4-02 | testes de *matching* |

### E2 — Domínio: mobilidade
| ID | Tarefa | Onde | Depende de | Pronto quando |
|---|---|---|---|---|
| F5-10 | `Trip`/`TripStore` (modo `Walk`), progresso em `Fixed` | Domain | F1 | — |
| F5-11 | `IPathfinder` com A* gerenciado: custos inteiros, heurística admissível, desempate por ID do nó | Domain | F3-06 | caminho ótimo em grafos de referência; desempate determinístico |
| F5-12 | `PathCache` por `(origem, destino, modo, versão)` com invalidação preguiçosa por região | Domain | F5-11, F3-13 | editar rua invalida só caminhos da região |
| F5-13 | `TripPlanningSystem` (até 32 buscas/tick) e `TripProgressSystem` (chegadas → `TripCompleted`) | Domain | F5-10..12 | excedente espera o próximo tick |
| F5-14 | Ilha desconectada → sem caminho → evento para o futuro problema "bairro isolado" (F13) | Domain | F5-11 | cidadão fica em casa; evento emitido |

### E3 — Apresentação
| ID | Tarefa | Onde | Depende de | Pronto quando |
|---|---|---|---|---|
| F5-20 | Read model `AgentSnapshot` (só viagens ativas) e `CitizenDetails` (exemplo do §36) | Application | F5-13 | — |
| F5-21 | `IAgentPresentation` + `GameObjectAgentPresenter`: pool, interpolação `progress(t + α)`, `MaxVisibleAgents = 50`, modelo Quaternius animado | Agents.GameObjects | F5-20 | **0 alocação por frame** em regime |
| F5-22 | Seguir cidadão: agente dedicado preso ao progresso autoritativo; em atividade, foca o prédio (`SIMULATION_LOD.md` §4) | Presentation | F5-21, F2-11 | chega exatamente quando o domínio diz |
| F5-23 | Debug "Show Citizen Paths": rota atual e plano do dia com horários (*line batches*, sem `LineRenderer` por linha) | Presentation | F5-20 | — |
| F5-24 | Inspector do cidadão (nome, idade, profissão, casa, trabalho, próxima atividade, destino, rota) | Presentation | F5-20 | — |

### E4 — Calibração e desempenho
| ID | Tarefa | Onde | Depende de | Pronto quando |
|---|---|---|---|---|
| F5-30 | Mini-playtest interno da escala temporal: `R = 30`, `MobilitySpeedScale = 3`, cidade ≤ 1 km; ajustar e registrar. Incluir a **distribuição de horários**: o S4a mediu pico de 72% da população viajando ao mesmo tempo (média de 11%), em parte por horários sincronizados (todos acordam às 06:00). Variar turnos, acordar conforme o tempo de viagem, e medir a fração de viagens simultâneas média e de pico | — | E1–E3 | decisão em `SIMULATION_TICK.md` §9; pico de viagens simultâneas registrado e premissa do `PERFORMANCE.md` §2 atualizada |
| F5-31 | Benchmark de domínio (BenchmarkDotNet) com 50, 1k e 10k cidadãos sintéticos; comparar com S4a/S4b | `dotnet/benchmarks` | E1, E2 | números registrados em `docs/perf/` |
| F5-32 | Teste de independência da câmera: mesma cidade headless e em PlayMode com 3 trajetórias de câmera → hashes iguais | testes | F5-21 | verde |

## 6. Conteúdo pedagógico
Nenhuma lição nova. A Player API ganha só leitura de cidadão na F7/F8 (`Citizen` é somente leitura: o aluno não cria pessoas, `CRITICAL_ANALYSIS.md` D6).

## 7. Testes e qualidade
Máquina de estados; agenda; A* e cache; invariantes a cada tick (todo cidadão tem domicílio ativo; ninguém "viajando" sem `Trip`); determinismo de 3 dias; independência da câmera; GC zero por frame no presenter.

## 8. Gate de saída
- [ ] 50 cidadãos por 3 dias TC: hash idêntico em 2 execuções e em 1x × 10x.
- [ ] Invariantes de população verdes a cada tick.
- [ ] F5-32 verde.
- [ ] 0 alocação por frame com 50 agentes visíveis; simulação ≤ 2 ms/frame (média, 10x) no hardware mínimo.
- [ ] Decisão de escala temporal registrada.

## 9. Riscos
| Risco | Ref. | Mitigação nesta fase |
|---|---|---|
| Escala temporal incomodar | R13 | F5-30 |
| Mono/IL2CPP mais lento que o benchmark | R06 | S4b + F5-31 |
| Atratividade provisória virar permanente | — | método único + reescrita obrigatória na F7 (F7-12) |

## 10. Documentos e ADRs a atualizar
`CITIZEN_SYSTEM.md`, `HOUSEHOLD_SYSTEM.md`, `PATHFINDING.md`, `SIMULATION_LOD.md`, `SIMULATION_TICK.md` (calibração), `PERFORMANCE.md`.

## 11. Ferramental
`/tdd` (máquina de estados, A*, cache) · `performance-analyst` (benchmarks, alocações) · `test-engineer` (invariantes, determinismo).

## 12. Encerramento
- [ ] Gate verificado e registrado
- [ ] `/health-check`
- [ ] `RISKS.md` revisado (R06, R13)
- [ ] Velocidade real registrada
