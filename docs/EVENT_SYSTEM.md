# Sistema de Eventos

> **Status:** v1.0 · **Data:** 2026-10-08 · **MVP:** eventos de domínio (infra) + "Apagão" · eventos dinâmicos: Fase 19
> Este documento cobre duas coisas: **(A)** a infraestrutura de **eventos de domínio** e **(B)** os **eventos dinâmicos** de jogo (§40) e o **sistema de problemas** (§26).

---

## A. Eventos de domínio (infraestrutura)
- *Structs* acumulados no `DomainEventBuffer` durante o tick e despachados **após** o tick, em ordem de emissão (determinística).
- Assinantes: read models, `ProblemDetectionSystem`, áudio, UI, `LearningEngine` (gatilhos de missão), conquistas.
- **Regras:** assinantes não mutam o estado (emitem comandos). Sem `event` C# em entidades. Sem *callbacks* do domínio para a Unity.
- Player API (Fase 15/16): `city.Events.PowerShortage += handler` (Observer), em que o handler roda no Worker como comportamento em lote.

## B1. Eventos dinâmicos (§40)

### Modelo (dados)
```json
{ "id": "drought", "trigger": { "season": "summer", "probabilityPerDay": 0.02, "requires": "weather.dryDays>=10" },
  "duration": { "minDays": 5, "maxDays": 12 },
  "area": "city",
  "modifiers": [
    { "target": "water.sourceCapacity", "op": "multiply", "value": 0.6 },
    { "target": "fire.risk", "op": "multiply", "value": 1.5 } ],
  "resolution": "duration",
  "notify": "events.drought.start" }
```
- **Modificadores** se aplicam numa `ModifierStack` (Decorator) consultada pelos sistemas. O efeito é limitado e documentado.
- RNG determinístico (fluxo próprio). O mesmo save + os mesmos comandos produzem os mesmos eventos.

### Catálogo (fases)
| Evento | Efeitos principais | Fase |
|---|---|---|
| **Apagão** (déficit de energia) | cortes, comércio fechado | **MVP** |
| Congestionamento severo | tempo de viagem ↑ | 11 |
| Incêndio | danos, emergência | 13 |
| Enchente | água em células baixas perto de rios, ruas bloqueadas | 19 |
| Seca | capacidade de água ↓, risco de incêndio ↑ | 19 |
| Epidemia | saúde ↓, demanda hospitalar ↑ (ligada à água contaminada) | 19 |
| Greve | um serviço a 0–30% por N dias (ligada a salários e verbas) | 19 |
| Acidente | via bloqueada temporariamente | 11 |
| Crise econômica | demanda externa ↓, exportações ↓ | 19 |
| Crescimento populacional | onda de migração (oportunidade e pressão) | 19 |

**Design:** eventos são **reações plausíveis** ao estado da cidade (seca exige dias secos; greve exige salários baixos), não punições aleatórias. Cada evento tem causa explicável.

## B2. Sistema de problemas (§26)
- `ProblemDetectionSystem` (1 h TC) avalia **detectores**, que são **Specifications** sobre o estado (ex.: `DistrictGarbageAbove(70%).For(2.Days)`).
- Um `Problem` tem: tipo, severidade, local, evidências (causas com valores), ações sugeridas (UI), **conceitos relacionados** e missões disponíveis (`LEARNING_ENGINE.md` §5).
- Ciclo: Detectado → Em investigação (o jogador abriu) → Em tratamento (ação tomada) → Resolvido (condição falsa por X h) | Agravado.
- Catálogo (§26): trânsito intenso, falta de energia, falta de água, lixo acumulado, desemprego, criminalidade, poluição, falta de moradia, baixa educação, baixa saúde, déficit financeiro, falta de transporte.

## Testes
Determinismo dos eventos (seed), modificadores aplicados e removidos corretamente, detectores (tabelas de cenários), ciclo de vida dos problemas.
