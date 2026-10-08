# Sistema de Energia

> **Status:** v1.0 · **Data:** 2026-10-08 · **MVP:** sim (rede única, usina a gás, painéis solares sem injeção)

---

## 1. Componentes (§22)
| Componente | MVP | Descrição |
|---|---|---|
| `PowerPlant` | ✅ gás (capacidade no balanceamento, F7-07; ~60–80 kW para a vila do MVP) | geração constante; custo de combustível por kWh |
| Solar residencial | ✅ | parâmetro da casa; curva diária; sem injeção na rede no MVP |
| `PowerGrid` | ✅ | componentes conexos da rede |
| `PowerLine` | Fase 12 | conecta áreas sem rua; capacidade de transmissão |
| `Consumer` | ✅ | todo prédio com consumo |
| Usinas solar/eólica/hidro, baterias | Fase 12 | variabilidade e armazenamento |

## 2. Conectividade
- **MVP:** as **ruas conduzem energia**. Um prédio está na rede se o seu lote tem acesso a um segmento conectado (pelo grafo viário) a um segmento adjacente a uma usina.
- Componentes conexos são calculados por *union-find* no grafo viário. Só são recalculados quando a rede muda (*dirty*).
- Fase 12: linhas de transmissão explícitas, capacidade por linha e perdas.

## 3. Balanço (por componente, 1x por hora TC ou quando *dirty*)
```
Geração  = Σ capacidade das usinas do componente (× disponibilidade)
Consumo  = Σ consumo dos prédios (com solar descontado, mínimo 0)
Saldo    = Geração − Consumo
```
Exemplo do §22: Geração 100 MW, Consumo 120 MW → **Déficit de 20 MW** → consequências reais.

## 4. Déficit: corte por prioridade (*load shedding*)
1. Ordenar consumidores por prioridade: **CityHall** (1) > **Shop** (2) > **House** (3). Fase 13: hospitais (0) e serviços de emergência (0).
2. Atender em ordem até esgotar a geração.
3. Na prioridade em que a energia acaba, aplicar **rodízio determinístico**: a cada hora TC, a janela de casas cortadas avança na ordem por ID, distribuindo o corte, como num racionamento.
4. Prédios cortados: `Powered = false`.

| Efeito do corte | Sistema |
|---|---|
| Necessidade *Energy* cai (felicidade) | Happiness |
| Mercado sem energia não vende (fecha) | Commerce |
| Prefeitura sem energia → serviços a 50% (Fase 13) | Services |
| Problema "Falta de energia" + evento "Apagão" | Problems/Events |
| Overlay de Energia: vermelho | Presentation |

## 5. Custos
- Combustível: kWh **gerados** × custo (o tesouro paga ao `External`).
- Tarifa: kWh **consumidos** × tarifa (os consumidores pagam ao tesouro).
- Isso cria a lição econômica "usina ociosa custa pouco, usina no limite custa muito; eficiência energética economiza para todos".

## 6. Solar residencial
```
solarW(h) = peakW × max(0, sin(π × (h − 6) / 12))   para 6 ≤ h ≤ 18, senão 0
```
Implementado com tabela inteira por hora (determinística). Sem injeção na rede no MVP: o excedente é perdido. A injeção, mais baterias, entram na Fase 12 e viram tema de desafio.

## 7. Player API e pedagogia (MVP)
- `city.Energy.GenerationKw`, `ConsumptionKw`, `BalanceKw`, `Unpowered`.
- Lições 4, 6 e 7 e o desafio final "Crise de energia" giram em torno deste sistema.
- Fase 9: `IEnergyPolicy` (o aluno define a ordem de corte, por exemplo para proteger casas com idosos), validada contra regras mínimas (prefeitura nunca é cortada antes das casas, se houver).

## 8. Read models e overlays
`EnergyReport` (HUD/painel), overlay **Energia** (abastecido/cortado; Fase 12: carga por linha), debug "Show Electricity Network" (componentes conexos coloridos).

## 9. Testes
Componentes conexos (ilha sem usina → sem energia), balanço, ordem de corte, rodízio (cada casa cortada no máximo X h por dia em déficit de Y%), solar por hora, custos (ledger).
