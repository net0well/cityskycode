# Termômetro da Cidade (City Health Indicator)

> **Status:** v1.0 · **Data:** 2026-10-08 · **MVP:** v0 (4 indicadores) · Completo: Fase 13

---

## 1. Intenção (§15)
Resumir o estado geral da cidade num indicador **legível**, **explicável** e com **consequências**, não decorativo.

## 2. Níveis
| Nível | Índice | Significado |
|---|---|---|
| 🔥 **CRÍTICO** | < 20 **ou** qualquer indicador em estado crítico | ação imediata necessária |
| 🔴 Ruim | 20–39 | vários sistemas falhando |
| 🟠 Instável | 40–54 | problemas relevantes |
| 🟡 Atenção | 55–69 | algo precisa de cuidado |
| 🟢 Saudável | 70–84 | funcionando bem |
| 🔵 Excelente | ≥ 85 | cidade exemplar |

**Histerese:** para mudar de nível, o índice precisa cruzar o limiar com ±3 pontos de margem, o que evita oscilação. A **tendência** (↑ ↓ →) é calculada nas últimas 24 h TC.

## 3. Indicadores (§15)
Cada indicador é normalizado para `[0, 1]` por uma curva definida no catálogo (pontos "ruim" e "bom"), com um limiar **crítico** opcional.

| Indicador | Medida | Ruim → Bom | Crítico | MVP | Peso |
|---|---|---|---|---|---|
| Felicidade | média da cidade | 40% → 75% | < 25% | ✅ | 3 |
| Desemprego | % da força de trabalho | 15% → 4% | > 30% | ✅ | 2 |
| Energia | (geração − consumo)/consumo | −5% → +15% | déficit > 20% | ✅ | 2 |
| Economia | saldo diário e dias de reserva | déficit → superávit + 30 dias | tesouro < 0 | ✅ | 2 |
| Água | abastecimento | — | déficit > 20% | 12 | 2 |
| Criminalidade | taxa por 1.000 hab. | — | — | 13 | 1 |
| Poluição | média ponderada pela população | — | — | 12 | 1 |
| Trânsito | atraso médio vs. fluxo livre | — | — | 11 | 1 |
| Saúde | cobertura × saúde média | — | — | 13 | 1 |
| Educação | cobertura escolar | — | — | 13 | 1 |
| Serviços | satisfação média de serviços (lixo etc.) | — | — | 12–13 | 1 |

```
Índice = 100 × Σ pesoᵢ × normᵢ / Σ pesoᵢ
Se algum indicador ∈ crítico → nível = CRÍTICO (independentemente do índice)
```

## 4. Consequências (não decorativo)
| Nível | Efeitos de jogo |
|---|---|
| Excelente / Saudável | atratividade para migração +; desbloqueio de marcos; eventos positivos mais prováveis (Fase 19) |
| Atenção / Instável | missões de diagnóstico sugeridas; atratividade neutra |
| Ruim | atratividade −; empresas adiam expansão |
| CRÍTICO | alerta persistente; "Modo investigação" sugerido; missão de emergência; congelamento de crescimento |

## 5. Explicação (drill-down)
Clicar no termômetro abre:
```
Termômetro: 🟠 Instável (48, ↓)
  Felicidade   0,40 ███░░  (−) causas → [abrir "Por quê?"]
  Desemprego   0,85 ████░
  Energia      0,10 █░░░░  CRÍTICO? não (déficit 12%) → [overlay Energia]
  Economia     0,55 ███░░
```
Cada linha leva ao overlay, ao relatório ou à lista de problemas relacionados.

## 6. Frequência
1x por hora TC, com suavização (EMA) dos indicadores para estabilidade.

## 7. Testes
Normalização (curvas), override crítico, histerese (sequência que oscila em volta do limiar não muda de nível), cálculo do índice com pesos.
