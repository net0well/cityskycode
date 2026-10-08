# Orçamento da Cidade

> **Status:** v1.0 · **Data:** 2026-10-08 · **MVP:** sim (subconjunto)

---

## 1. Estrutura (§17)
```
Receitas                                   Despesas
├── Imposto residencial  (MVP)             ├── Educação           (Fase 13)
├── Imposto comercial    (MVP)             ├── Saúde              (Fase 13)
├── Imposto industrial   (Fase 10)         ├── Polícia            (Fase 13)
├── Taxas de serviço     (MVP: energia,    ├── Bombeiros          (Fase 13)
│                          aluguel social) ├── Lixo               (Fase 12)
└── Outras receitas      (Fase 10)         ├── Ruas: manutenção   (MVP)
                                           ├── Transporte público (Fase 19)
                                           ├── Energia: combustível e operação (MVP)
                                           ├── Água               (Fase 12)
                                           └── Infraestrutura: construção (MVP)
```
Cada linha é uma **categoria do ledger**. O orçamento é um **relatório** sobre o ledger, não um segundo sistema de dinheiro.

## 2. Controles do jogador
| Controle | MVP | Efeito |
|---|---|---|
| Alíquota residencial (0–20%) | ✅ | receita × felicidade (penalidade por imposto) |
| Alíquota comercial (0–20%) | ✅ | receita × lucro das empresas (falências) |
| Alíquota industrial | Fase 10 | — |
| Verba por serviço (50–150%) | Fase 13 | eficiência do serviço × custo |
| Tarifas (energia, água, transporte) | Fase 12 | receita × felicidade/consumo |
| Empréstimos | Fase 10 | caixa imediato × juros |

## 3. Relatórios
- **Diário** (fechamento às 00:00): receitas e despesas por categoria, saldo, variação.
- **Projeção:** "receita/despesa por dia" com base nas últimas 24 h TC (atualizada a cada hora), o que dá feedback rápido ao jogador.
- **Alertas:** déficit por 3 dias seguidos, tesouro < 7 dias de despesas, tesouro < 0 (dispara o problema "Déficit financeiro" e o indicador crítico do termômetro).

## 4. Decisões (§17)
Quando "a cidade gasta mais do que arrecada", o painel sugere ações com **impacto estimado** (simulação rápida *headless* de 7 dias em segundo plano, Fase 13): aumentar impostos (−X felicidade), reduzir serviços, construir infraestrutura mais eficiente, estimular a economia, reduzir desperdícios.

## 5. Falência (falha suave)
Tesouro < 0 por 7 dias → "intervenção": empréstimo emergencial com juros e missão de recuperação. Não há *game over* no modo campanha.

## 6. Player API
`city.Budget.Treasury`, `IncomePerDay`, `ExpensesPerDay`, `ResidentialTaxRate`, `SetResidentialTaxRate(rate)` (MVP). Relatórios por categoria a partir da Fase 10 (coleções e LINQ).

## 7. Testes
Relatório = soma do ledger por categoria (propriedade); projeção estável; alertas nos limiares corretos.
