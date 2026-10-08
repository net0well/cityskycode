# Sistema de Poluição

> **Status:** v1.0 (intenção de design) · **Data:** 2026-10-08 · **Fase:** 12 · **MVP:** não

---

## 1. Tipos
| Tipo | Fontes | Propagação | Efeitos |
|---|---|---|---|
| **Ar** | indústria, usinas térmicas, trânsito (emissões por link) | difusão + decaimento + vento (clima) | saúde, *Environment*, valor do terreno |
| **Solo** | indústria, aterros | quase estática (decai devagar) | bloqueia certos usos (residencial, agricultura) |
| **Ruído** | vias movimentadas, comércio noturno, indústria | atenuação com a distância, sem acúmulo | *Environment* à noite, sono |
| **Água** | esgoto sem tratamento, indústria | a jusante nos corpos d'água | contaminação da captação (`WATER_SYSTEM.md`) |

## 2. Modelo de campo
- **Grade de análise** de 32 m (`MAP_SYSTEM.md`). Valor inteiro por célula e por tipo.
- Atualização a cada hora TC:
```
P'(c) = P(c)·(1 − decaimento) + emissão(c) + k·Σ_vizinhos (P(v) − P(c)) + advecção(vento)
```
- Implementação gerenciada no domínio (porta `IFieldSolver`). Versão Burst na Fase 18, com resultado idêntico (inteiros).

## 3. Explicabilidade
O Inspector de uma célula ou casa mostra as **fontes principais** (top 3 emissores que contribuem para a célula, por rastreamento aproximado de contribuição na difusão: registrar emissões por fonte num raio). "Por que essa casa respira mal? → Fábrica X (60%), Avenida Y (25%)".

## 4. Overlays
Ar, Solo, Ruído (gradientes, legíveis para daltonismo).

## 5. Pedagogia
Simulação em grade (arrays 2D, laços aninhados), `IMapOverlay` do aluno (Fase 15/16) e *trade-offs* de política ambiental.
