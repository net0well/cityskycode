# Glossário (pt-BR ↔ código)

> **Status:** vivo · **Atualizado:** 2026-10-08 · Regra: identificadores em inglês, conteúdo em pt-BR (ADR-0014).

## Cidade
| pt-BR | Código | Nota |
|---|---|---|
| Cidade | `City` (Player API) / `CityState` (Domain) | — |
| Cidadão | `Citizen` | — |
| Domicílio | `Household` | unidade econômica |
| Lote | `Lot` | 8 × 24 m |
| Prédio | `Building` | — |
| Casa | `House` | — |
| Mercado | `Shop` | — |
| Prefeitura | `CityHall` | — |
| Usina | `PowerPlant` | — |
| Rua / segmento / nó / faixa | `Road` / `RoadSegment` / `RoadNode` / `Lane` | — |
| Zona | `Zone` (`Residential`, `Commercial`, `Industrial`, `Office`...) | Fase 10 |
| Bairro | `District` | Fase 10 |
| Tesouro municipal | `Treasury` | — |
| Orçamento | `Budget` | — |
| Livro-razão | `Ledger` | partidas dobradas |
| Transação | `Transaction` | — |
| Exterior | `External` | conta do "mundo de fora" |
| Viagem | `Trip` | — |
| Caminho | `Path` / `PathRef` | — |
| Necessidade / Satisfação | `Need` / `Satisfaction` | — |
| Felicidade | `Happiness` | — |
| Termômetro da cidade | `CityHealth` | — |
| Problema | `Problem` | — |
| Evento | `CityEvent` (jogo) / `DomainEvent` (técnico) | — |
| Isolamento (térmico) | `Insulation` | `Basic`/`Good`/`Excellent` |
| Painel solar | `SolarPanels` | — |
| Consumo de energia | `EnergyConsumption` | kW, derivado |
| Corte de carga / rodízio | `LoadShedding` | — |

## Simulação e arquitetura
| pt-BR | Código / termo | Nota |
|---|---|---|
| Tick (passo de simulação) | `Tick` | 0,25 s TM |
| Tempo de mobilidade (TM) | `MobilityTime` | — |
| Tempo de calendário (TC) | `CalendarTime` | 1 s TM = 30 s TC |
| Balde (fatia de atualização) | bucket | time-slicing |
| Comando | `ICityCommand` | toda mutação |
| Modelo de leitura | read model | o que a UI lê |
| Contribuição (causa) | `Contribution` | explicabilidade |
| Store (armazenamento orientado a dados) | `*Store` | SoA |
| LOD de simulação / apresentação | S0–S2 / R0–R3 | `SIMULATION_LOD.md` |
| Code Host | `CitySky.CodeHost` | compila e gerencia |
| Worker (sandbox) | `CitySky.Sandbox.Worker` | executa código do aluno |
| Laboratório | modo *headless* de desafio | — |
| Allowlist | lista de APIs permitidas | `CSK1xxx` |

## Aprendizagem
| pt-BR | Termo |
|---|---|
| Lição / Desafio / Missão | Lesson / Challenge / Mission |
| Dica | Hint |
| Domínio (de um conceito) | Mastery |
| Revisão espaçada | Spaced review |
| Grafo de conceitos | Concept graph |
