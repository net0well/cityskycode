# Sistema de Domicílios

> **Status:** v1.0 · **Data:** 2026-10-08 · **MVP:** sim (1–4 adultos, uma conta por domicílio)

---

## 1. Intenção
O **domicílio** é a unidade econômica e residencial: ocupa uma moradia, tem uma conta bancária, recebe salários e gasta em consumo. Usar domicílios (e não indivíduos) para o dinheiro reduz o custo e reflete melhor a realidade.

## 2. Dados (`HouseholdStore`)
| Campo | Descrição |
|---|---|
| `Id` | `HouseholdId` |
| `Home` | `BuildingId` (casa) e unidade (Fase 10: prédios com várias unidades) |
| `Members` | até 6 `CitizenId` (MVP: 1–4 adultos) |
| `Account` | `AccountId` no ledger |
| `IncomeLevel` | Low / Mid / High (derivado da renda), para consumo e impostos |
| `MoveInTick` | data de chegada |
| `UnhappyDays` | contador para emigração |
| `Wealth` | saldo (lido do ledger) |

## 3. Regras
- **Formação (chegada):** tamanho sorteado (RNG determinístico) entre 1 e min(4, capacidade livre). Saldo inicial igual a 30 dias de despesas típicas (vem do `External`: dinheiro novo entrando na cidade, registrado no ledger).
- **Consumo:** necessidade de bens ~ f(membros, renda). Compras no mercado (`CommerceSystem`).
- **Contas:** energia (diária), impostos (diário, Fase 6), aluguel/IPTU (Fase 10).
- **Dificuldade financeira:** saldo < 0 → necessidade *Consumo* deprimida e penalidade de felicidade "Dívidas".
- **Emigração:** felicidade média < 30% por 3 dias → sai. O dinheiro restante vai para o `External`. A moradia é liberada.
- **Fase 13+:** nascimentos, envelhecimento, formação de novos domicílios (jovens adultos saem de casa), óbitos.

## 4. Read models
`HouseholdDetails` (Inspector da casa: "Moradores: 4", renda, gastos, felicidade média, problemas).

## 5. Testes
- Conservação: chegada e saída mexem só em `External` ↔ conta do domicílio.
- Ninguém fica sem moradia: se a casa é demolida, o domicílio é realocado (se houver vaga) ou emigra.
- Emigração após exatamente 3 dias abaixo do limiar.
