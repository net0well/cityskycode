# ADR-0003: Arquitetura híbrida, com ECS só para a apresentação de agentes em massa

- **Status:** Aceito
- **Data:** 2026-10-08
- **Documentos afetados:** `ARCHITECTURE.md`, `SIMULATION_ARCHITECTURE.md`, `SIMULATION_LOD.md`, `TRAFFIC_SYSTEM.md`

## Contexto
O briefing pede ECS/DOTS para cidadãos, veículos, tráfego, necessidades, consumo e produção, e também regras de necessidades e felicidade no "Domain C#" independente da Unity. As duas exigências se contradizem. O ECS da Unity é poderoso, mas complexo (baking, subscenes, APIs ainda em mudança: core package na 6.4, EntityId de 8 bytes na 6.5, remoção de APIs na linha 6.x). O MVP tem 50 cidadãos.

## Decisão
1. O **Domain** (C# puro, orientado a dados: stores SoA + IDs) é a **autoridade** de todo o estado e de todas as regras: cidadãos, necessidades, economia, tráfego meso.
2. O **ECS** é usado **na apresentação** para agentes em massa: pedestres e veículos visíveis, microssimulação visual de tráfego e animação. Entra na **Fase 11**.
3. Aceleração de laços quentes do domínio (pathfinding, campos) com **Jobs/Burst** via **portas** injetadas, com resultados idênticos aos da implementação gerenciada (custos inteiros).
4. **GameObjects** para ferramentas, prédios especiais, UI, câmera e objetos únicos.
5. Desde o MVP, os agentes ficam atrás de `IAgentPresentation` (implementação GameObject no MVP, ECS depois).

## Alternativas consideradas
| Alternativa | Por que não |
|---|---|
| ECS como dono do estado de cidadãos | domínio passa a depender da Unity; Laboratórios *headless* no Code Host impossíveis; determinismo entre runtimes difícil |
| Tudo em GameObjects | não escala para dezenas de milhares de agentes visíveis |
| ECS desde o MVP | complexidade sem benefício para 50 cidadãos; atrasa a validação do laço central |

## Consequências
**Positivas:** domínio testável e portátil; ECS onde ele brilha; MVP simples.
**Negativas:** ponte domínio → ECS (snapshots por frame); dois "mundos" a sincronizar.
**Riscos:** custo da ponte em escala (mitigação: só viagens ativas visíveis, buffers nativos, Fase 18).

## Gatilhos de revisão
"ECS for All" permitindo componentes em GameObjects de forma estável · perfilamento mostrando a ponte como gargalo.
