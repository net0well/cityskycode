# Visualização de Debug e Modo de Investigação

> **Status:** v1.0 · **Data:** 2026-10-08 · **MVP:** Inspector + "Show Citizen Paths" · Inclui §37 (Modo de investigação) e §38 (Debug visual)

---

## 1. Intenção
Tornar a cidade **inspecionável como um programa em execução**. Isso serve ao jogador (investigar problemas) e ao aluno (aprender **depuração**: observar estado, relações, histórico e causas).

## 2. Modo de investigação (Inspector, §37)
Clicar em cidadão, prédio, rua, veículo, empresa, estação, rede elétrica ou tubulação abre um painel com abas:

| Aba | Conteúdo | Exemplo |
|---|---|---|
| **Estado** | propriedades atuais | Casa #1029: moradores 4, consumo 1,9 kW, energia ✔ |
| **Propriedades** | parâmetros de projeto vs. derivados | Isolamento: Bom (parâmetro) → Consumo: 1,9 kW (derivado) |
| **Relações** | links clicáveis | Moradores → João, Ana…; Trabalho de João → Mercado Central |
| **Dependências** | do que depende | Energia ← Rede #2 ← Usina Norte |
| **Histórico** | série das últimas 24–72 h TC | felicidade, consumo, extrato |
| **Problemas** | problemas e causas | "Sem energia por 9 h (déficit da rede)" |
| **Código** (Fase 9+) | qual código do aluno afeta este objeto | "Consumo calculado por `EnergyCalculator.CalculateEnergyConsumption` (v3)" |

> A aba **Propriedades** com "parâmetro × derivado" é a ponte direta para a lição de **encapsulamento**. A aba **Código** liga o efeito à causa no código: depuração de verdade.

O Inspector lê **read models** (`BuildingDetails`, `CitizenDetails`...), nunca o domínio diretamente.

## 3. Modos de debug visual (§38)
| Modo | Visual | Fase |
|---|---|---|
| **Show Citizen Paths** | rota atual e plano do dia do cidadão selecionado: Casa → Trabalho → Mercado → Casa (§38), com horários | **MVP** |
| Show Traffic Routes | rotas que passam pelo segmento selecionado (origem/destino agregados) | 11 |
| Show Electricity Network | componentes conexos coloridos; usinas; prédios cortados | 9 (pós-MVP imediato; o `MVP.md` §3.6 prevê um único debug visual) |
| Show Water Network | rede, bombas, pressão | 12 |
| Show Garbage Routes | rotas dos caminhões, paradas, carga | 12 |
| Show Job Connections | linhas casa ↔ trabalho (amostradas) | 10 |
| Show Household Connections | membros, casa, trabalhos, escola | 13 |
| Show Supply Chain | fornecedor → empresa → consumidor | 10+ |
| Show Money Flow | ver `MONEY_FLOW.md` | 10 |

**Implementação:** registro de `IDebugLayer` (Strategy) na apresentação. Linhas e setas desenhadas com malha dinâmica instanciada (*line batches*), sem `LineRenderer` por linha e sem GameObject por elemento. Limite de elementos (top N) com indicação "mostrando 200 de 3.412".

## 4. Ferramentas internas (desenvolvimento)
- Painel de métricas da simulação: ms por sistema, contadores, hash de estado, velocidade efetiva.
- "Passo a passo": avançar 1 tick ou 1 hora TC com a simulação pausada.
- Console de desenvolvedor (build de desenvolvimento): executar comandos da Application diretamente.
- Overlay "dirty": células e objetos atualizados no último tick.

## 5. Testes
Read models do Inspector (dados corretos), relações navegáveis (links válidos), limites de elementos respeitados.
