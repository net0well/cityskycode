# Fase 6 — Economia mínima

> **Status:** Planejada · **Bloco:** B (Vertical Slice) · **Atualizado:** 2026-10-08
> **Entrada:** gate da Fase 5 · **Saída:** invariante de conservação verde em todos os cenários + testes de balanceamento dentro das faixas · **Estimativa:** 3 semanas

---

## 1. Objetivo
O dinheiro circula com origem e destino (§16): salários, compras, impostos, estoque, manutenção e migração passam pelo ledger de partidas dobradas, e o jogador vê o orçamento da prefeitura e ajusta as alíquotas.

## 2. Por que agora
Com cidadãos trabalhando e comprando, a economia ganha atores. Ela é pré-requisito da energia (conta de luz, combustível) e da felicidade (consumo, dívidas, imposto), e alimenta a Lição 2 ("quanto temos no cofre?").

## 3. Escopo
**Inclui:** ledger completo do MVP (contas `Household`, `Company`, `Treasury`, `External`; categorias do `ECONOMY_SYSTEM.md` §3 exceto energia), anel de transações de 7 dias + agregados diários, `CompanyStore` (Shop), estoque e reposição, falência, `CityBudget` como relatório do ledger, fechamento diário, projeção, alertas, alíquotas residencial e comercial (0–20%), painel financeiro com gráfico de 7 dias, Sankey 2D simplificado, extrato no Inspector, Player API `Budget`, testes de balanceamento headless.
**Não inclui:** conta de energia e combustível (F7, depende do consumo), mercado com preço dinâmico (F10), empréstimos (F10), aluguel/IPTU (F10), arcos 3D de dinheiro (F10).

## 4. Entregáveis
- Ledger completo do MVP com invariante testado a cada tick.
- Painel de orçamento e alíquotas.
- CLI headless que simula N dias e exporta CSV por indicador (ferramenta de balanceamento, `CITY_SIMULATION.md` §4).

## 5. Backlog

### E1 — Ledger e empresas
| ID | Tarefa | Onde | Depende de | Pronto quando |
|---|---|---|---|---|
| F6-01 | Contas `Household` e `Company`; categorias `Wage`, `IncomeTax`, `Purchase`, `GoodsImport`, `BusinessTax`, `Maintenance`, `Rent` (social), `MigrationIn/Out`, `Refund` | Domain | F3-11 | conservação após cada tick em todos os cenários |
| F6-02 | Anel de transações detalhadas (7 dias TC) + agregados por categoria/dia (1 ano) | Domain | F6-01 | memória limitada em teste de 1 ano |
| F6-03 | `CompanyStore` (Shop): funcionários, estoque, receitas e despesas do dia | Domain | F4-02, F5-07 | — |
| F6-04 | `CommerceSystem`: compra ao concluir "Compras" (preço fixo do catálogo), reposição de `External` quando o estoque < 30% | Domain | F6-03, F5-05 | mercado sem estoque não vende |
| F6-05 | `LedgerSettlementSystem`: salários às 18:00 proporcionais aos dias trabalhados, imposto de renda retido, imposto comercial sobre lucro positivo, manutenção, aluguel social | Domain | F6-01 | salário só para quem trabalhou (teste) |
| F6-06 | Falência: saldo < 0 por N dias → fecha, demite, libera o lote | Domain | F6-03 | mercado sem clientes fecha em N dias (teste) |
| F6-07 | Catálogo `content/catalogs/economy.json` (salários, preços, alíquotas padrão, manutenção) | content | F6-01 | validado no CI |

### E2 — Orçamento
| ID | Tarefa | Onde | Depende de | Pronto quando |
|---|---|---|---|---|
| F6-10 | `CityBudget` como **relatório** sobre o ledger (nunca um segundo sistema de dinheiro) | Domain / Application | F6-01 | relatório = soma do ledger por categoria (propriedade) |
| F6-11 | `BudgetCloseSystem` (00:00), projeção por dia (últimas 24 h TC, atualizada por hora), evento `BudgetDayClosed` | Domain | F6-10 | projeção estável em cenário constante |
| F6-12 | Alertas: déficit 3 dias seguidos, tesouro < 7 dias de despesas, tesouro < 0 | Domain | F6-11 | alertas nos limiares exatos |
| F6-13 | Comando `SetTaxRate` (residencial e comercial, 0–20%) | Application | F6-05 | fora da faixa → `CommandResult` de erro |

### E3 — Interface e Player API
| ID | Tarefa | Onde | Depende de | Pronto quando |
|---|---|---|---|---|
| F6-20 | HUD: tesouro com variação por dia; painel Orçamento (receitas e despesas por categoria, gráfico de 7 dias com `Painter2D`) | Presentation | F6-10 | — |
| F6-21 | Diagrama de fluxo 2D (Sankey simplificado: Exterior → Empresas → Domicílios → Prefeitura) a partir do `MoneyFlowAggregator` | Application / Presentation | F6-02 | números batem com o ledger |
| F6-22 | Extrato no Inspector de domicílio e de empresa (últimos 7 dias) | Presentation | F6-02 | — |
| F6-23 | Player API `Budget`: `Treasury`, `IncomePerDay`, `ExpensesPerDay`, `ResidentialTaxRate`, `SetResidentialTaxRate`; `decimal` na API, centavos no domínio | PlayerApi | F6-10, F6-13 | conversão sem perda (teste); corpus atualizado |
| F6-24 | Template pt-BR do `CS0664` ("literal `decimal` precisa do sufixo `m`": a aula de dinheiro) | Code Host / content | F6-23 | mensagem aparece para `decimal x = 12.5;` |

### E4 — Balanceamento
| ID | Tarefa | Onde | Depende de | Pronto quando |
|---|---|---|---|---|
| F6-30 | CLI headless `citysky-sim` (N dias → CSV) | `dotnet/tools/` | F1-38 | gera CSV de 30 dias |
| F6-31 | Testes de balanceamento: vila do MVP **sustentável** com alíquota padrão; **insustentável** com alíquota 0% e 10 casas sem empregos | `dotnet/tests` (noturno) | F6-05 | os dois cenários dentro do esperado |

## 6. Conteúdo pedagógico
Habilita a **Lição 2** (variáveis, `decimal`, `CS0664`). O fluxo de dinheiro com origem e destino vira material de depuração futura ("por que o tesouro caiu ontem?").

## 7. Testes e qualidade
Propriedade de conservação a cada tick; relatório = ledger; salários, impostos e falência por cenário; balanceamento noturno; determinismo com dinheiro; nenhum `float`/`double` em valores monetários do domínio (teste de arquitetura).

## 8. Gate de saída
- [ ] Conservação verde em todos os cenários de teste, por 30 dias TC.
- [ ] F6-31 verde.
- [ ] Orçamento exibido bate centavo a centavo com o ledger.
- [ ] Hash determinístico intacto.

## 9. Riscos
| Risco | Ref. | Mitigação nesta fase |
|---|---|---|
| Balanceamento instável | R12 | preços fixos; CLI de exploração; testes noturnos |
| Orçamento virar "segundo dinheiro" | — | F6-10 como relatório puro, com teste de propriedade |

## 10. Documentos e ADRs a atualizar
`ECONOMY_SYSTEM.md`, `MONEY_FLOW.md`, `BUDGET_SYSTEM.md`, `HOUSEHOLD_SYSTEM.md`, `PLAYER_API.md`, `CITY_SIMULATION.md` §4 (CLI real).

## 11. Ferramental
`/tdd` (ledger, liquidação, falência) · `test-engineer` (testes de propriedade e balanceamento) · `get_public_api` (Budget).

## 12. Encerramento
- [ ] Gate verificado e registrado
- [ ] `/health-check`
- [ ] `RISKS.md` revisado (R12)
- [ ] Velocidade real registrada
