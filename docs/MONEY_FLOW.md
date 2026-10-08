# Fluxo de Dinheiro (Money Flow)

> **Status:** v1.0 · **Data:** 2026-10-08 · **MVP:** painel de categorias (sim) · visualização 3D (Fase 10)

---

## 1. Intenção (§39)
"Transformar economia em algo visual." O jogador vê o dinheiro **circulando**: quem paga quem, quanto e por quê.

## 2. Fonte de dados
O **ledger** (`ECONOMY_SYSTEM.md`) é a única fonte. A visualização nunca inventa números: ela **agrega** transações.

`MoneyFlowAggregator` (Application): a cada hora TC, consolida transações em arestas `(origem, destino, categoria) → soma`, nos níveis:
- **Cidade** (por tipo de agente: Domicílios, Empresas, Prefeitura, Exterior);
- **Bairro** (Fase 10);
- **Entidade** (prédio/domicílio selecionado).

## 3. Visualizações
| Modo | Descrição | Fase |
|---|---|---|
| **Diagrama de fluxo (2D, painel)** | Sankey simplificado: Exterior → Empresas → Domicílios → Prefeitura → Serviços, com espessura proporcional | MVP |
| **Arcos no mapa (3D)** | arcos animados entre prédios/bairros, com "moedas" (partículas) indo da origem ao destino e cor por categoria | 10 |
| **Seguir o dinheiro** | selecionar um prédio mostra entradas (verde) e saídas (vermelho) das últimas 24 h TC | 10 |
| **Extrato** | lista de transações de um domicílio ou empresa | MVP (Inspector) |

Exemplo (§39), modo cidade:
```
Empresa ──salário──► Cidadão ──consumo──► Empresa ──imposto──► Prefeitura ──serviço──► Cidade
```

## 4. Desempenho
- Arcos limitados aos **top N** fluxos (ex.: 200) por magnitude. O resto aparece agregado como "outros".
- Partículas instanciadas (GPU), sem GameObjects por moeda.

## 5. Pedagogia
- Rastrear um bug econômico ("por que o tesouro caiu ontem?") com dados reais é uma aula de **depuração**.
- Mais tarde, o aluno escreve consultas sobre o ledger (LINQ) para gerar os próprios relatórios.
