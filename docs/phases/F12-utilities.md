# Fase 12 — Água, esgoto, lixo e poluição

> **Status:** Intenção de design · **Bloco:** C · **Atualizado:** 2026-10-08
> **Entrada:** gate da Fase 11 · **Saída:** redes de água e esgoto, coleta de lixo e campos de poluição funcionando com causas rastreáveis; missão do lixo (etapas 1–2) aprovada em playtest

---

## 1. Objetivo
Completar a infraestrutura urbana com sistemas acoplados (água ↔ esgoto ↔ saúde; trânsito ↔ poluição) e entregar a primeira metade da **missão completa do briefing** (§74, coleta de lixo).

## 2. Por que nesta posição
Depende de veículos (caminhões de lixo usam o trânsito da F11) e prepara os serviços da F13 (saúde depende de água contaminada e poluição). É também onde entram coleções e algoritmos (Módulo M4).

## 3. Escopo
**Inclui:**
- Água (`WATER_SYSTEM.md`): fontes, bombas, tubulação sob as ruas, pressão simplificada por Dijkstra multi-fonte, contaminação a jusante, vazamentos.
- Esgoto (`SEWAGE_SYSTEM.md`): coleta, tratamento, despejo sem tratamento → poluição da água.
- Lixo (`GARBAGE_SYSTEM.md`): produção, acúmulo, aterro, reciclagem, frota de caminhões, rota padrão gulosa; `IGarbageRoutingStrategy` do aluno avaliada em lote no Worker.
- Poluição (`POLLUTION_SYSTEM.md`): ar, solo, ruído, água na grade de 32 m, porta `IFieldSolver`, fontes principais por célula.
- Energia v2: linhas de transmissão, renováveis (solar, eólica, hidro), baterias, injeção na rede.
- Tarifas (água, energia) no orçamento; deterioração de prédios sem manutenção.
- Necessidades *Water* e *Environment*.
- Overlays Água, Esgoto, Poluição (ar/solo/ruído), Lixo; debug "Show Water Network" e "Show Garbage Routes".
- Conteúdo: **Módulo M4** (coleções, busca linear, mínimo/máximo, ordenação, LINQ básico) com a missão do lixo, etapas 1 (`GarbageCollector.Collect(areas)`) e 2 (depósito mais próximo).

**Não inclui:** saúde e epidemias (F13/F19), incinerador (F19).

## 4. Entregáveis (épicos)
E1 água · E2 esgoto · E3 lixo + veículos de serviço · E4 poluição · E5 energia v2 · E6 tarifas e deterioração · E7 visualização · E8 conteúdo M4 + missão do lixo 1–2.

## 5. Gate de saída
- [ ] Cada problema (`WATER_SYSTEM.md` §3, "bairro norte acumulando lixo") detectado com evidências e investigável por overlay + Inspector.
- [ ] Estratégia de rota do aluno comparada com a padrão em métricas (kg/dia, km, acúmulo máximo).
- [ ] Campos de poluição: implementação gerenciada × acelerada idênticas (inteiros).
- [ ] Orçamento de desempenho de 10k mantido.

## 6. Riscos
R12 (sistemas acoplados instáveis), explosão de overlays e carga cognitiva (R09), hidráulica "real demais" (manter o modelo simplificado, `CRITICAL_ANALYSIS.md` §3).

## 7. Documentos e ADRs
`WATER_SYSTEM.md`, `SEWAGE_SYSTEM.md`, `GARBAGE_SYSTEM.md`, `POLLUTION_SYSTEM.md`, `ENERGY_SYSTEM.md`, `BUDGET_SYSTEM.md`, `MAP_OVERLAYS.md`, `LEARNING_PATH.md` (M4), `PLAYER_API.md`.
