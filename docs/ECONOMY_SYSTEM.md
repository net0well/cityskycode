# Sistema Econômico

> **Status:** v1.0 · **Data:** 2026-10-08 · **MVP:** sim (preços fixos, 1 empresa, ledger completo)
> Relacionados: `MONEY_FLOW.md` (fluxo e visualização), `BUDGET_SYSTEM.md` (prefeitura).

---

## 1. Intenção
"O dinheiro deve possuir origem e destino" (§16). Nada de `PlayerMoney += 100`. A economia é uma rede de **contas** e **transações**, e todo valor tem um motivo rastreável.

## 2. Ledger de partidas dobradas
- Toda transação **debita** uma conta e **credita** outra, com o mesmo valor. O dinheiro não aparece nem some.
- Contas: `Household`, `Company`, `Treasury` (prefeitura) e **`External`** (o mundo fora da cidade: fornecedores, compradores, migrantes).
- **Invariante:** `Σ saldos internos + saldo(External) = constante`. É verificado em teste após cada tick e em modo debug na execução.
- Dinheiro só "entra" na cidade via `External` (exportações, migrantes com poupança, transferências) e só "sai" via `External` (importações, emigrantes, combustível).

```csharp
public readonly struct Transaction
{
    public readonly long Tick;
    public readonly AccountId From, To;
    public readonly Money Amount;           // > 0
    public readonly TxCategory Category;    // Wage, Purchase, IncomeTax, BusinessTax, EnergyBill, Rent,
                                            // Construction, Maintenance, FuelImport, GoodsImport, Export,
                                            // MigrationIn, MigrationOut, Refund, Subsidy...
    public readonly EntityRef Ref;          // quem/onde (prédio, domicílio, empresa)
}
```

## 3. Fluxo (§16)
```
Empresa → Produção → Emprego → Salário → Cidadão → Consumo → Empresa → Receita
        → Impostos → Governo → Serviços públicos → População
```
### MVP (concreto)
| Fluxo | De → Para | Quando |
|---|---|---|
| Salários | Mercado/Tesouro → Domicílio | diário (18:00), proporcional aos dias trabalhados |
| Imposto de renda (retido) | Domicílio → Tesouro | junto com o salário (alíquota residencial) |
| Compras | Domicílio → Mercado | ao concluir a atividade "Compras" |
| Reposição de estoque | Mercado → External | quando o estoque < 30% (preço de atacado) |
| Imposto comercial | Mercado → Tesouro | diário, sobre o lucro positivo |
| Conta de energia | Domicílio/Mercado → Tesouro | diário (kWh × tarifa) |
| Aluguel social | Domicílio → Tesouro | diário (casas construídas pela prefeitura no MVP) |
| Combustível da usina | Tesouro → External | diário (kWh gerados × custo) |
| Manutenção | Tesouro → External | diário |
| Construção / reforma | Tesouro → External | no comando |
| Migração | External ↔ Domicílio | chegada/saída |

Valores (salários, preços, tarifas, alíquotas) ficam no catálogo `content/catalogs/economy.json` e são balanceados por testes *headless*.

## 4. Empresas (§19)
| Campo | Descrição |
|---|---|
| `Kind` | Shop (MVP), Restaurant, Factory, Farm, Office, LogisticsCompany, ConstructionCompany (Fase 10+) |
| Funcionários | vagas e empregados |
| Despesas | salários, estoque, energia, impostos, aluguel |
| Receitas | vendas |
| Produção/venda | bens por categoria |
| Crescimento | lucro sustentado → expande (Fase 10: densifica o prédio) |
| Falência | saldo < 0 por N dias → fecha, demite e libera o lote |

MVP: **um** tipo (Shop) com preço fixo. Falência possível (ensina que o comércio precisa de clientes).

## 5. Mercado (§18, Fase 10+)
- Categorias de bens: `Food`, `Retail`, `IndustrialGoods`, `Services`.
- Oferta (capacidade de produção/venda) × demanda (consumo dos domicílios + intermediário).
- **Preço** ajusta devagar em direção ao equilíbrio (*tâtonnement* com amortecimento e limites de ±2%/dia), para evitar oscilação.
- Muitas lojas e poucos consumidores → oferta ↑, demanda ↓, lucro ↓ → algumas fecham (§18). É um **laço de equilíbrio** documentado em `CITY_SIMULATION.md`.

## 6. Explicabilidade
Painéis "De onde vem / para onde vai o dinheiro" com agregação por categoria, prédio e bairro (`MONEY_FLOW.md`). O Inspector de um domicílio mostra o extrato dos últimos 7 dias.

## 7. Escala
- Transações individuais por cidadão a 100k seriam caras. Por isso, salários, impostos e contas são liquidados **por domicílio, em lote diário**, e compras são agregadas por visita.
- O ledger guarda **transações detalhadas por 7 dias de jogo** (anel circular) e **agregados** por categoria/dia por 1 ano.

## 8. Testes
- Conservação após cada tick (propriedade).
- Cenários: salário pago só a quem trabalhou; imposto retido corretamente; mercado sem clientes vai à falência em N dias; preços nunca negativos.
- Balanceamento: vila do MVP é sustentável com alíquota padrão, e insustentável com alíquota 0% e 10 casas sem empregos.

## 9. Pedagogia
`decimal` para dinheiro (Lição 2; `CS0664` com o sufixo `m`), extratos (coleções), relatórios (LINQ), regras de imposto (condicionais), `ITaxPolicy` (Strategy/Specification).
