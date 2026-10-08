# Sistema de Clima

> **Status:** v1.0 (intenção de design) · **Data:** 2026-10-08 · **Fase:** 19 (visual de chuva simples pode entrar antes) · **MVP:** não

---

## 1. Estados (§59)
`Clear` (sol), `Cloudy` (nuvens), `Rain` (chuva), `Storm` (tempestade), `Fog` (neblina). Estações (Fase 19) alteram as probabilidades.

## 2. Modelo
- **Cadeia de Markov** por hora TC com matriz de transição por estação (dados em `content/catalogs/weather.json`), com RNG determinístico.
- Variáveis contínuas derivadas: intensidade de chuva, cobertura de nuvens, visibilidade, vento (direção/velocidade, usado pela poluição), temperatura (Fase 19: consumo de energia por climatização).

## 3. Efeitos (§59), sempre via **modificadores** limitados
| Clima | Trânsito | Felicidade | Consumo | Energia | Acidentes | Produtividade |
|---|---|---|---|---|---|---|
| Chuva | velocidade −15% | −2 | +5% (iluminação) | solar −60% | +30% | −3% |
| Tempestade | −30%, vias alagáveis | −5 | +8% | solar −90%, risco de falha de linha | +80% | −8% |
| Neblina | −20% | −1 | +3% | solar −40% | +40% | — |
| Sol forte (verão) | — | +2 | +10% (ar-condicionado) | solar +10% | — | — |

## 4. Apresentação (URP)
Chuva: partículas (VFX Graph) apenas perto da câmera, superfícies molhadas por parâmetro global de "umidade" nos shaders (brilho + escurecimento), poças por máscara. Neblina: fog de altura + exponencial do URP (fog volumétrico via shader próprio ou asset, se justificado). Nuvens: sombra de nuvens projetada (*cookie* animado na luz direcional) e céu. Ver `GRAPHICS.md`.

## 5. Pedagogia
State (máquina de clima), Observer (o aluno reage à chuva com políticas de trânsito), probabilidade e determinismo (seed).
