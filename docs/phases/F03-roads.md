# Fase 3 — Estradas

> **Status:** Planejada · **Bloco:** B (Vertical Slice) · **Atualizado:** 2026-10-08
> **Entrada:** gate da Fase 2; relatório do S5 · **Saída:** AC-02 automatizado; 100 interseções regeneradas em ≤ 50 ms · **Estimativa:** 4 semanas

---

## 1. Objetivo
O jogador desenha ruas retas que se conectam em interseções automáticas, gera lotes nos dois lados e paga por metro. A rede existe no domínio como grafo, com calçadas para os pedestres da Fase 5.

## 2. Por que agora
A rede viária é o esqueleto: define lotes (Fase 4), caminhos a pé (Fase 5) e condução de energia (Fase 7). É também o ponto técnico de maior risco gráfico (interseções), que o S5 já atacou.

## 3. Escopo
**Inclui:** um tipo de rua (mão dupla, 2 faixas, 50 km/h, calçadas), segmentos retos de qualquer ângulo, interseções de até 4 ramos (ângulo mínimo de 30°), divisão automática ao cruzar, demolição, custo por metro com acréscimo por inclinação, lotes 8 × 24 m, grafo de calçadas (CSR), nomes de rua gerados, suavização do terreno sob a rua, Inspector de rua, leitura pela Player API.
**Não inclui:** curvas, rotatórias, faixas configuráveis, mão única, semáforos, faixa de pedestre (F11); pontes e túneis (F19); construir ruas por código (F10).

### Decisão de dependência: o ledger nasce aqui
O AC-02 exige que a rua debite o custo. Para não criar um "dinheiro provisório" que depois teria de ser removido (arquitetura paralela, proibida pelo §76), esta fase cria o **ledger mínimo**: contas `Treasury` e `External`, `Transaction` e a categoria `Construction`. A Fase 6 completa o ledger com as demais contas e categorias.

## 4. Entregáveis
- Ferramenta de rua com fantasma, custo e validade (verde/vermelho).
- Grafo viário + grafo de calçadas no domínio, com testes.
- Malha de pista, meio-fio, calçada e interseções, combinada por *chunk* de 256 m.

## 5. Backlog

### E1 — Domínio: grafo viário
| ID | Tarefa | Onde | Depende de | Pronto quando |
|---|---|---|---|---|
| F3-01 | `Position` (mm), `Bezier2` (reta = Bézier degenerada), utilitários de geometria inteira (interseção de segmentos, ângulo, distância) | Domain | F1 | testes com casos degenerados (paralelos, colineares) |
| F3-02 | `RoadNode`, `RoadSegment`, `RoadNetwork` (store), catálogo `RoadType` (1 tipo) em `content/catalogs/roads.json` | Domain / content | F3-01 | carregamento validado por `CitySky.Content.Tests` |
| F3-03 | Regras: comprimento ≥ 16 m, sem cruzar prédio, inclinação ≤ 12%, no máximo 4 ramos, ângulo ≥ 30° | Domain | F3-02, F2-02 | teste por regra (aceita e recusa) |
| F3-04 | Divisão de segmento ao cruzar ou ao terminar sobre outro (cria nó); *snap* em nó existente | Domain | F3-03 | testes de T, X, prolongamento |
| F3-05 | Faixas derivadas do `RoadType` e conectividade padrão nos nós (base para F11) | Domain | F3-04 | testes de conectividade |
| F3-06 | Grafo de calçadas (CSR): esquinas, entradas de lote, travessias nas interseções; custo em ms a pé | Domain | F3-04 | grafo de referência com caminho conhecido |
| F3-07 | Nomes de rua determinísticos (tabelas pt-BR + seed) | Domain / content | F3-02 | mesmo seed → mesmos nomes |

### E2 — Domínio: lotes e ledger mínimo
| ID | Tarefa | Onde | Depende de | Pronto quando |
|---|---|---|---|---|
| F3-10 | `LotStore`: geração 8 × 24 m nos dois lados, descarte por colisão (lotes, ruas, máscara, inclinação > 25%) | Domain | F3-04 | testes de geração e descarte |
| F3-11 | Ledger mínimo: `Account` (`Treasury`, `External`), `Transaction`, categoria `Construction`, invariante de conservação | Domain | F1 | teste de conservação após cada tick |
| F3-12 | Comandos `BuildRoadSegment` e `DemolishRoadSegment` com validação, custo (por metro + inclinação) e reembolso; gancho "lote perde acesso" (prédios chegam na F4) | Application | F3-03, F3-11 | **AC-02** automatizado em teste de domínio |
| F3-13 | Eventos `RoadSegmentBuilt`, `RoadSegmentDemolished`, `LotsCreated` e versão de região do grafo (para o cache de caminhos da F5) | Domain | F3-12 | assinantes recebem em ordem |

### E3 — Apresentação
| ID | Tarefa | Onde | Depende de | Pronto quando |
|---|---|---|---|---|
| F3-20 | Ferramenta de rua (tecla R): *snap* em nó e segmento, fantasma com custo e validade, confirmação por clique, Ctrl+Z em até 5 s | Presentation | F3-12 | fluxo conforme `GAMEPLAY.md` §3 |
| F3-21 | Gerador de malha de segmento (extrusão de perfil pista + meio-fio + calçada, UV contínua), reescrito a partir do S5 | Presentation | S5 | snapshots de referência iguais |
| F3-22 | Gerador de malha de interseção (bordas recuadas, cantos arredondados, triangulação) | Presentation | F3-21 | 12 configurações de referência sem artefatos |
| F3-23 | Combinação por *chunk* de 256 m, regeneração incremental só dos chunks afetados | Presentation | F3-22 | 100 interseções ≤ 50 ms (player IL2CPP) |
| F3-24 | Decals de sinalização (faixas centrais) | Presentation | F3-21 | dentro do orçamento de draw calls |
| F3-25 | Suavização do terreno sob a rua (`SetHeights` regional com orçamento) e coerência com o domínio | Presentation / Domain | F2-03 | sem rua flutuando ou enterrada > 5 cm |
| F3-26 | Ferramenta de demolição (X) para ruas, com confirmação quando lotes perdem acesso | Presentation | F3-12 | — |
| F3-27 | Visualização de lotes livres durante a ferramenta de construção | Presentation | F3-10 | lotes aparecem ao desenhar |

### E4 — Leitura, Inspector e Player API
| ID | Tarefa | Onde | Depende de | Pronto quando |
|---|---|---|---|---|
| F3-30 | Read model `RoadDetails` + Inspector da rua (nome, comprimento, tipo, custo de manutenção) | Application / Presentation | F3-12 | — |
| F3-31 | Player API: `city.GetRoads()`, `Road.Name`, `Road.LengthMeters` (consulta via `LiveCityGateway`) + allowlist + caso no corpus | PlayerApi | F1-80 | `get_public_api` revisado |
| F3-32 | Rua inicial e tesouro de $100.000 no "jogo novo" (parte do AC-01) | Application | F3-12 | jogo novo abre com rua |

## 6. Conteúdo pedagógico
Nenhuma lição nova. `city.GetRoads()` só será usada em lições a partir da Fase 10 (laços: "construa uma grade de quarteirões"). O custo por metro já aparece no console e no orçamento, preparando a Lição 2 ("quanto temos no cofre?").

## 7. Testes e qualidade
Domínio: divisão, ângulo, conectividade, lotes, custo, conservação do ledger. Apresentação: snapshots de malha das 12 configurações, desempenho de regeneração, GC por regeneração limitado (buffers reaproveitados). Determinismo: script de comandos com ruas → hash dourado.

## 8. Gate de saída
- [ ] **AC-02**: rua de 200 m debita o custo correto, cria lotes dos dois lados e permite construir neles (construção real fica para a F4; aqui, lotes válidos e livres).
- [ ] 100 interseções regeneradas em ≤ 50 ms no player IL2CPP.
- [ ] Snapshots das 12 configurações de referência aprovados.
- [ ] Invariante de conservação do ledger verde.
- [ ] Orçamento de FPS da F2 mantido com uma malha de 5 km de ruas.

## 9. Riscos
| Risco | Ref. | Mitigação nesta fase |
|---|---|---|
| Artefatos de interseção | R08 | MVP só com retas; algoritmo do S5; snapshots |
| Ledger provisório virar dívida | — | ledger mínimo real (F3-11), sem "dinheiro temporário" |

## 10. Documentos e ADRs a atualizar
`ROAD_SYSTEM.md`, `PATHFINDING.md` (grafo de calçadas real), `ECONOMY_SYSTEM.md` (ledger mínimo), `PLAYER_API.md`, ADR-0011 (notas do S5).

## 11. Ferramental
`/tdd` em todo o E1 e E2 · `/spec` para o algoritmo de interseção · `security-auditor` em F3-31.

## 12. Encerramento
- [ ] Gate verificado e registrado
- [ ] `/health-check`
- [ ] `RISKS.md` revisado (R08)
- [ ] Velocidade real registrada
