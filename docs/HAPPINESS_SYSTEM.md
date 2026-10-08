# Sistema de Felicidade

> **Status:** v1.0 · **Data:** 2026-10-08 · **MVP:** sim (4 necessidades + penalidade de impostos)
> Relacionado: `CITY_HEALTH_SYSTEM.md`. Necessidades (§13) são definidas aqui.

---

## 1. Por que não uma soma simples
A fórmula do briefing (§14) soma termos de escalas diferentes sem limite e permite **compensação total**: parques infinitos "pagariam" a falta de moradia. Adotamos um modelo **normalizado, ponderado, com o pior fator pesando, suavizado e explicável**.

## 2. Necessidades (§13)
Cada necessidade tem **satisfação** `s ∈ [0, 1]` (no domínio: 0–10000).

| Necessidade | Cálculo da satisfação (resumo) | MVP |
|---|---|---|
| **Housing** (moradia) | 1 se tem casa; × fator de lotação (moradores/capacidade) × condição | ✅ |
| **Employment** (emprego) | 1 se empregado; desempregado: decai de 0,6 a 0,2 em 5 dias | ✅ |
| **Energy** (energia) | fração das últimas 24 h TC com a casa abastecida | ✅ |
| **Consumption** (consumo/"fome") | estoque de bens do domicílio / necessidade (cai diariamente, sobe com compras) | ✅ |
| Health (saúde) | acesso a clínica/hospital (cobertura) × saúde individual | 13 |
| Education (educação) | vaga escolar para crianças/jovens; nível × emprego compatível | 13 |
| Safety (segurança) | cobertura policial × criminalidade local | 13 |
| Entertainment (lazer) | parques e comércio de lazer acessíveis | 10 |
| Transport (transporte) | tempo de deslocamento × referência | 11 |
| Social | domicílio > 1 pessoa, vizinhança, eventos | 19 |
| Environment (ambiente) | 1 − poluição/ruído local normalizados | 12 |
| Water (água) | abastecimento nas últimas 24 h TC | 12 |

## 3. Fórmula
```
base   = Σ wᵢ·sᵢ / Σ wᵢ                     (média ponderada; pesos por perfil)
pior   = min(sᵢ)                             (necessidade mais crítica)
bruto  = 100 × (0,7 × base + 0,3 × pior)
pen    = penalidades (impostos, poluição, trânsito, dívidas), cada uma com teto
alvo   = clamp(bruto − Σ pen, 0, 100)
H(t)   = H(t−1) + 0,25 × (alvo − H(t−1))     (suavização exponencial por hora TC)
```
- **Pesos MVP:** Housing 3, Employment 3, Energy 2, Consumption 2.
- **Penalidade de imposto residencial:** 0 até 8%; linear até 10 pontos em 12%; até 30 pontos em 20%.
- **Pior fator (30%):** uma privação severa (sem energia há dias) derruba a felicidade mesmo com o resto bom, o que é realista e didático.
- Tudo em aritmética inteira (ponto fixo) no domínio.

## 4. Explicabilidade: "Por que meus cidadãos estão infelizes?"
Para cada cidadão, as **contribuições** são armazenadas de forma agregada por prédio/bairro e recalculadas sob demanda para o cidadão inspecionado:
```
Felicidade 46% (↓ 18 nas últimas 24 h)
  −12  Energia: casa sem energia por 9 h  ← déficit da rede (−40 kW) ← usina sobrecarregada
  −6   Imposto residencial 13%
  −4   Consumo: estoque baixo (mercado sem estoque)
  +    Moradia: ok · Emprego: ok
Sugestões: construir usina/energia solar · reduzir imposto · ver mercado
```
- Painel "Por quê?" no nível **cidade/bairro**: as 3 maiores causas negativas por número de pessoas afetadas (AC-07).
- Cada causa tem link para o overlay, o objeto ou o problema correspondente.

## 5. Efeitos da felicidade
| Efeito | Regra |
|---|---|
| Migração | média do domicílio < 30% por 3 dias → emigra; média da cidade alta → atrai |
| Produtividade (Fase 10) | trabalhadores felizes produzem até +10% |
| Termômetro | indicador principal (`CITY_HEALTH_SYSTEM.md`) |
| Crime (Fase 13) | infelicidade + desemprego → risco |
| Valor do terreno (Fase 10) | felicidade do bairro |

## 6. Frequência e custo
`NeedsSystem` e `HappinessSystem`: cada cidadão 1x por hora TC (baldes de 480), com fases defasadas. Agregados por prédio/bairro/cidade atualizados de forma incremental.

## 7. Testes
- Tabelas de casos da fórmula (inclusive "pior fator domina").
- Monotonicidade: melhorar uma necessidade nunca reduz a felicidade-alvo.
- Explicação: a soma das contribuições = 100 − alvo (com tolerância de arredondamento).
- Suavização: convergência para o alvo em ~10 h TC.
