# Fase 4 — Construções

> **Status:** Planejada · **Bloco:** B (Vertical Slice) · **Atualizado:** 2026-10-08
> **Entrada:** gate da Fase 3 · **Saída:** AC-01, AC-03 e AC-05: "a casa aparece" a partir de código · **Estimativa:** 3 semanas

---

## 1. Objetivo
Construir, reformar e demolir `House`, `Shop` e `PowerPlant` pela UI **e** pelo código (`city.Build`), com as mesmas regras e custos. A `CityHall` existe desde o início.

## 2. Por que agora
É o **marco central do vertical slice** (§68): `var house = new House(); city.Build(house);` e a casa aparece. A partir daqui o laço "código → cidade" existe de verdade, ainda que a simulação seja rasa.

## 3. Escopo
**Inclui:** catálogo de arquétipos, `BuildingStore`, regras derivadas da casa (`BUILDING_SYSTEM.md` §3), comandos `BuildBuilding`, `ModifyBuilding`, `DemolishBuilding` (reembolso de 25%), um prédio por lote, prefeitura no jogo novo, ferramentas de UI com fantasma, Ctrl+Z, Inspector de prédio (parâmetro × derivado), Player API v0.1 de construção, gateway *Live* com consultas ao snapshot e pausa durante o script.
**Não inclui:** moradores e empregos funcionando (F5), dinheiro além de `Construction`/`Refund` (F6), consumo afetando a rede (F7), obras com duração (F10), zoneamento (F10).

## 4. Entregáveis
- `content/catalogs/buildings.json` (valores ilustrativos, balanceados na F6–F7).
- Ferramentas casa (H), mercado (M), usina (P), demolir (X).
- Player API v0.1: `Building`, `House`, `Shop`, `PowerPlant`, `CityHall`, `Lot`, `Insulation`, `BuildResult`, `City.Build` (2 sobrecargas), `FindFreeLots`, `GetBuildings`.

## 5. Backlog

### E1 — Catálogo e domínio
| ID | Tarefa | Onde | Depende de | Pronto quando |
|---|---|---|---|---|
| F4-01 | Catálogo de arquétipos (`house.small`, `shop.small`, `cityhall`, `powerplant.gas`) + loader no Application + validação no `CitySky.Content.Tests` | content / Application | F1 | catálogo inválido falha no CI |
| F4-02 | `BuildingStore` (SoA): `Id`, `ArchetypeId`, `LotId`, `Owner`, `Params`, `Derived`, `Condition`, `Powered`, `ConstructedAt` | Domain | F3-10 | testes de adição e remoção |
| F4-03 | `HouseRules`: `EnergyConsumption` (W, inteiro), `Cost`, `MaintenancePerDay`; solar por hora em tabela inteira | Domain | F4-01 | tabela de casos (ex.: 4 moradores, Good, sem solar = 1.920 W) |
| F4-04 | Comandos `BuildBuilding` (desbloqueio, lote livre e compatível, saldo, faixa de parâmetros; `Lot` nulo = próximo lote livre por ordem de ID), `ModifyBuilding` (reforma = diferença de custo), `DemolishBuilding` (25%, prefeitura proibida) | Application | F4-02, F3-11 | um teste por validação; categorias `Construction`/`Refund` no ledger |
| F4-05 | Jogo novo: terreno, prefeitura, rua inicial, $100.000 | Application | F4-04, F3-32 | **AC-01** (≤ 20 s no hardware recomendado) |
| F4-06 | Eventos `BuildingConstructed`, `BuildingModified`, `BuildingDemolished` | Domain | F4-04 | — |

### E2 — Apresentação
| ID | Tarefa | Onde | Depende de | Pronto quando |
|---|---|---|---|---|
| F4-10 | `BuildingVisualCatalog` (ScriptableObject): arquétipo + variação por seed do lote → prefab com LOD | Presentation | F2-31 | duas casas vizinhas têm variações diferentes |
| F4-11 | Ferramentas de construção com fantasma (custo, validade), animação de "pop", som sutil | Presentation | F4-04 | conforme `GAMEPLAY.md` §3–4 |
| F4-12 | Ctrl+Z com reembolso integral em até 5 s (Command pattern na Application) | Application / Presentation | F4-04 | desfazer devolve o valor exato |
| F4-13 | Read model `BuildingDetails` + Inspector (exemplo do §36 em `BUILDING_SYSTEM.md` §6), aba Propriedades separando **parâmetros** e **derivados** | Application / Presentation | F2-41 | — |
| F4-14 | *Highlight* dos objetos alterados por código (feedback do `GAMEPLAY.md` §4) | Presentation | F4-20 | casa construída por script pisca ao aparecer |

### E3 — Código do jogador
| ID | Tarefa | Onde | Depende de | Pronto quando |
|---|---|---|---|---|
| F4-20 | Player API v0.1 (`PLAYER_API.md` §2, sem `Citizen` nem `Budget`), com semântica de projeto × prédio real (§2.1) e XML docs pt-BR | PlayerApi | F1-80 | hover mostra docs; `get_public_api` revisado |
| F4-21 | `LiveCityGateway`: consultas por IPC ao snapshot do tick T; mutações num `CommandBuffer`; limites de comandos e consultas por execução | PlayerApi / Code Host / Infrastructure | F1-50 | 500 comandos por execução no máximo; excedente com mensagem |
| F4-22 | Pausa automática durante o script; comandos aplicados em T+1 na ordem de emissão; resultado por comando no console (falha parcial didática) | Application | F4-21 | console lista sucesso e falha por comando |
| F4-23 | Allowlist ampliada para os tipos da Player API; casos novos no corpus; revisão do `security-auditor` | CodeAnalysis / testes | F4-20 | corpus verde |
| F4-24 | Template pt-BR do `CS0200` (propriedade somente leitura) com o texto de encapsulamento | Code Host / content | F4-20 | **AC-05** |
| F4-25 | Medir ida e volta: `var house = new House(); city.Build(house);` → casa visível | — | F4-22 | **AC-03**: ≤ 1,5 s com Code Host quente |

## 6. Conteúdo pedagógico
A Lição 3 (objeto, `new`) e a Lição 4 (propriedade, `CS0200`) passam a ser **possíveis**; o conteúdo delas é escrito na Fase 8. Esta fase garante as mensagens mínimas de `CS0200` e `CS0165` (variável não atribuída: `House house; city.Build(house);`).

## 7. Testes e qualidade
Tabela de cálculos derivados; validações de comando; reembolsos e Ctrl+Z; um prédio por lote; determinismo com comandos de origem `PlayerCode`; corpus de segurança; PlayMode ponta a ponta "script → casa aparece".

## 8. Gate de saída
- [ ] **AC-01**, **AC-03**, **AC-05** verificados (AC-03 medido no player IL2CPP).
- [ ] Construir pela UI e pelo código produz o **mesmo** comando e o mesmo resultado (teste).
- [ ] Comando inválido não desfaz os anteriores (teste).
- [ ] `/security-scan` sem achados críticos no gateway e na allowlist.

## 9. Riscos
| Risco | Ref. | Mitigação nesta fase |
|---|---|---|
| Código virar trapaça | R10 | grandezas derivadas, mesmas validações da UI, revisão de cada membro da API |
| Latência do gateway (muitas consultas por IPC) | — | snapshot congelado + limites; medir no AC-03 |

## 10. Documentos e ADRs a atualizar
`BUILDING_SYSTEM.md`, `PLAYER_API.md` (v0.1 real), `CODE_EXECUTION.md` §5.1, `ERROR_SYSTEM.md` (templates iniciais), `CODE_EXECUTION_SECURITY.md` §4 (allowlist).

## 11. Ferramental
`/spec` + `/tdd` (regras e comandos) · `security-auditor` + `/security-scan` (gateway, allowlist) · `get_public_api` em todo PR da Player API.

## 12. Encerramento
- [ ] Gate verificado e registrado
- [ ] `/health-check`
- [ ] `RISKS.md` revisado (R10)
- [ ] Velocidade real registrada
