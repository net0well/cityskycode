# Fase 7 — Energia, felicidade e termômetro da cidade

> **Status:** Planejada · **Bloco:** B (Vertical Slice) · **Atualizado:** 2026-10-08
> **Entrada:** gate da Fase 6 · **Saída:** AC-04, AC-06 e AC-07 verificados · **Estimativa:** 4 semanas

---

## 1. Objetivo
A cidade tem geração e consumo de energia com consequências reais: déficit corta casas por prioridade, moradores ficam infelizes e o jogo **explica por quê**. O termômetro resume o estado da cidade.

## 2. Por que agora
Energia é o sistema em torno do qual giram as Lições 4, 6, 7 e o desafio final. Felicidade explicável responde à pergunta central do §14 ("por que meus cidadãos estão infelizes?"). Sem esta fase, o código do aluno não tem consequência sistêmica para mostrar.

## 3. Escopo
**Inclui:** rede elétrica pelas ruas (componentes conexos por *union-find*, recálculo *dirty*), balanço por componente, corte por prioridade com rodízio determinístico, solar residencial (sem injeção), combustível e conta de luz no ledger, necessidades do MVP (Moradia, Emprego, Energia, Consumo), felicidade normalizada com pior fator, penalidade de imposto e suavização, contribuições (explicabilidade), emigração, atratividade completa, termômetro v0 (4 indicadores), problema "Falta de energia" e evento "Apagão", overlays Energia e Felicidade, painel "Por quê?", Player API de energia e felicidade.
**Não inclui:** linhas de transmissão, renováveis e baterias (F12); necessidades de saúde, educação, segurança etc. (F10–F13); "Show Electricity Network" (pós-MVP imediato, F9: o `MVP.md` §3.6 prevê um único debug visual).

## 4. Entregáveis
- Simulação de energia e felicidade completa para o MVP.
- Painel "Por quê?" e drill-down do termômetro.
- Dois overlays com paletas para daltonismo.

## 5. Backlog

### E1 — Energia
| ID | Tarefa | Onde | Depende de | Pronto quando |
|---|---|---|---|---|
| F7-01 | `PowerGrid`: componentes conexos no grafo viário (*union-find*), recalculados só quando a rede muda | Domain | F3-04 | ilha sem usina fica sem energia (teste) |
| F7-02 | `PowerGridSystem` (1 h TC ou *dirty*): geração × consumo por componente, com solar descontado (mínimo 0) | Domain | F7-01, F4-03 | balanço por tabela de casos |
| F7-03 | Corte por prioridade (CityHall > Shop > House) com **rodízio determinístico** por ID na faixa em que a energia acaba | Domain | F7-02 | cada casa cortada no máximo X h/dia em déficit de Y% (teste de justiça) |
| F7-04 | Efeitos do corte: mercado sem energia não vende; `Powered = false` | Domain | F7-03, F6-04 | — |
| F7-05 | Ledger: combustível (kWh **gerados** × custo, Tesouro → External) e conta de luz (kWh **consumidos** × tarifa, consumidores → Tesouro) | Domain | F7-02, F6-01 | conservação intacta |
| F7-06 | Eventos `PowerShortageStarted/Ended` | Domain | F7-03 | — |
| F7-07 | **Balancear a capacidade da usina** para que a crise exista. Com os valores ilustrativos atuais (usina de 400 kW; casas de 1–4 kW; ≤ 50 cidadãos ≈ 70 kW de pico), **nunca há déficit**, e as Lições 4, 7 e o desafio ★ perdem o sentido. Proposta: usina pequena de ~60–80 kW e pico noturno de consumo residencial; ajustar no catálogo com a CLI `citysky-sim` | content | F7-02, F6-30 | cenário da campanha entra em déficit noturno por volta de 35–40 moradores; teste de balanceamento registra a faixa |

### E2 — Necessidades, felicidade e migração
| ID | Tarefa | Onde | Depende de | Pronto quando |
|---|---|---|---|---|
| F7-10 | `NeedsSystem` (baldes de 480): Moradia, Emprego (decaimento em 5 dias), Energia (fração das últimas 24 h abastecida), Consumo (estoque do domicílio) | Domain | F5, F6, F7-03 | tabela por necessidade |
| F7-11 | `HappinessSystem` (baldes de 480, fase deslocada): média ponderada (pesos 3/3/2/2) + 30% do pior fator − penalidades com teto, suavização 0,25/h, tudo em ponto fixo; **contribuições** registradas | Domain | F7-10 | monotonicidade; pior fator domina; soma das contribuições = 100 − alvo (± arredondamento) |
| F7-12 | Reescrever `Attractiveness.Compute` (F5-04) com felicidade média, empregos e termômetro; emigração após 3 dias < 30% | Domain | F7-11 | emigração exatamente no 3º dia (teste) |
| F7-13 | Penalidade "Dívidas" (saldo do domicílio < 0) | Domain | F7-11 | — |

### E3 — Termômetro e problemas
| ID | Tarefa | Onde | Depende de | Pronto quando |
|---|---|---|---|---|
| F7-20 | `CityHealthSystem` v0: Felicidade, Desemprego, Energia, Economia; curvas no catálogo; *override* crítico; histerese ±3; tendência 24 h | Domain | F7-11, F6-11 | sequência oscilando no limiar não muda de nível |
| F7-21 | `ProblemDetectionSystem` com o detector "Falta de energia" (Specification) e o evento de jogo "Apagão" | Domain | F7-06 | ciclo Detectado → Resolvido |
| F7-22 | Notificações (toast com ícone + texto + link para a causa) | Presentation | F7-21 | — |

### E4 — Visualização e explicação
| ID | Tarefa | Onde | Depende de | Pronto quando |
|---|---|---|---|---|
| F7-30 | `IOverlaySource`, `OverlayField`, legenda; `EnergyOverlay` (categórico por prédio) e `HappinessOverlay` (por prédio) | Application | F7-03, F7-11 | testes de valores esperados |
| F7-31 | `OverlayRenderer`: cena dessaturada + cor por instância compatível com o GPU Resident Drawer; paletas para daltonismo; teclas 1–2 | Presentation | F7-30 | dentro do orçamento de FPS |
| F7-32 | Inspector: aba Problemas e causas ("Sem energia por 9 h ← déficit de 40 kW ← usina sobrecarregada") | Presentation | F7-11 | **AC-06** |
| F7-33 | Painel "Por quê?": 3 maiores causas negativas por número de pessoas afetadas, com link para overlay/objeto | Application / Presentation | F7-11 | **AC-07** |
| F7-34 | Termômetro no HUD com drill-down (`CITY_HEALTH_SYSTEM.md` §5) | Presentation | F7-20 | — |

### E5 — Player API
| ID | Tarefa | Onde | Depende de | Pronto quando |
|---|---|---|---|---|
| F7-40 | `EnergyReport` (`GenerationKw`, `ConsumptionKw`, `BalanceKw`, `Unpowered`), `Building.IsPowered`, `House.Happiness`, `House.Occupants`, `City.Happiness`, `Citizen` (somente leitura) e `city.GetCitizens()` | PlayerApi | E1–E3 | corpus e `get_public_api` revisados |
| F7-41 | Reforma (`house.Insulation = …` após construir) reduz o consumo no Inspector **e** na rede | PlayerApi / Domain | F7-02, F4-04 | **AC-04** completo |

## 6. Conteúdo pedagógico
Habilita as **Lições 4, 6 e 7** e o desafio **★ Crise de energia**. O painel "Por quê?" é a primeira aula informal de depuração: seguir a cadeia de causas até a origem.

## 7. Testes e qualidade
Componentes conexos, balanço, ordem de corte e justiça do rodízio, solar por hora, custos no ledger, fórmula da felicidade (tabelas, monotonicidade, pior fator, soma das contribuições, convergência em ~10 h), histerese do termômetro, detector e ciclo do problema, overlays. **Rodada preliminar do AC-14** (10x por 30 min no hardware mínimo).

## 8. Gate de saída
- [ ] **AC-04**, **AC-06**, **AC-07** verificados.
- [ ] Testes de energia e felicidade verdes; conservação intacta.
- [ ] Rodada preliminar do AC-14 sem queda abaixo de 30 FPS e sem crescimento de memória > 5%.
- [ ] Hash determinístico intacto.

## 9. Riscos
| Risco | Ref. | Mitigação nesta fase |
|---|---|---|
| Laço de realimentação energia ↔ comércio instável | R12 | corte por prioridade, não aleatório; testes de balanceamento |
| Usina superdimensionada: nunca há crise | — | F7-07 |
| Explicação inconsistente com o valor | — | teste "soma das contribuições = 100 − alvo" |
| Sobrecarga visual com overlays | R09 | só 2 overlays; dessaturação; legenda |

## 10. Documentos e ADRs a atualizar
`ENERGY_SYSTEM.md`, `HAPPINESS_SYSTEM.md`, `CITY_HEALTH_SYSTEM.md`, `EVENT_SYSTEM.md` (B2 v0), `MAP_OVERLAYS.md`, `PLAYER_API.md`, `DEBUG_VISUALIZATION.md`.

## 11. Ferramental
`/spec` (felicidade, termômetro) · `/tdd` em todo o domínio · `test-engineer` (propriedades) · `performance-analyst` (baldes e overlays).

## 12. Encerramento
- [ ] Gate verificado e registrado
- [ ] `/health-check`
- [ ] `RISKS.md` revisado (R09, R12)
- [ ] Velocidade real registrada
