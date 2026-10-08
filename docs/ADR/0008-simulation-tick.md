# ADR-0008: Tick fixo determinístico com dois relógios derivados

- **Status:** Aceito (valores iniciais calibráveis)
- **Data:** 2026-10-08
- **Documentos afetados:** `SIMULATION_TICK.md`, `SIMULATION_ARCHITECTURE.md`

## Contexto
O briefing define frequências em Hz reais (Economy 1 Hz, Traffic 10–20 Hz). Isso faz o resultado depender da velocidade de jogo e do frame rate, o que inviabiliza determinismo, replay e validação *headless*.

## Decisão
1. **Tick fixo e numerado** = 0,25 s de **Tempo de Mobilidade (TM)**. **Tempo de Calendário (TC)** = TM × R, com R = 30. 1 h TC = 480 ticks; 1 dia TC = 11.520 ticks.
2. Sistemas agendados por **período em ticks + fase**; sistemas por cidadão em **baldes**.
3. Velocidade = ticks por segundo real (1x = 4; 10x = 40). Limites de ticks por frame e de ms: se faltar CPU, a **velocidade efetiva cai**, mas nenhum tick é pulado.
4. Apresentação **interpola** por frame com `alpha`. Nada de tempo real no domínio.

## Alternativas consideradas
| Alternativa | Por que não |
|---|---|
| Hz reais por sistema (briefing) | resultado depende da velocidade e do frame rate |
| Tick variável (delta time) | não determinístico |
| Um relógio só (sem TM/TC) | ou veículos absurdamente rápidos, ou dias de 24 h reais |

## Consequências
**Positivas:** determinismo, replay, Laboratórios e testes por hash.
**Negativas:** escala temporal (viagens "longas" no calendário) precisa de calibração em playtest.

## Gatilhos de revisão
Playtests das Fases 5/11 (valor de R, escala de mobilidade) · perfilamento indicando tick curto/longo demais.
