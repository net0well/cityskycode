# ADR-0013: Calcular overlays em grade e escolher a cor na apresentação

- **Status:** Aceito (diverge da interface `GetColor(Vector3)` do briefing)
- **Data:** 2026-10-08
- **Documentos afetados:** `MAP_OVERLAYS.md`, `MAP_SYSTEM.md`

## Contexto
A interface proposta `Color GetColor(Vector3 position)` usa tipos da Unity (não roda no domínio nem no Code Host), é chamada por posição (custo e IPC proibitivos para overlays do aluno) e mistura cálculo e cor (SRP).

## Decisão
1. `IOverlaySource.Compute(CityReadModel, OverlayField)` no domínio/aplicação: valores por **grade de 32 m**, por prédio ou por segmento, com legenda.
2. **Cor na apresentação** (LUT/gradiente em shader; cor por instância), com paletas para daltonismo.
3. Versão pedagógica `IMapOverlay.Evaluate(CellInfo)` avaliada **em lote** no Worker sobre a grade, em baixa frequência. Continua ensinando interface, polimorfismo e Strategy.

## Alternativas consideradas
| Alternativa | Por que não |
|---|---|
| `GetColor(Vector3)` por posição | engine no domínio; custo; SRP |
| Overlays calculados em shader | regras de jogo em HLSL: não testáveis, não usáveis pelo aluno |

## Consequências
**Positivas:** rápido, testável, usável pelo aluno e coerente com o que o jogo ensina (SRP).
**Negativas:** resolução limitada à grade (32 m), aceitável para overlays.

## Gatilhos de revisão
Necessidade de overlays de alta resolução (ex.: ruído rua a rua): avaliar grade de 16 m por região.
