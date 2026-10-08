# ADR-0009: Separar cidadão (dados) de agente (apresentação), com LOD de simulação independente da câmera

- **Status:** Aceito (diverge do §54–55 do briefing)
- **Data:** 2026-10-08
- **Documentos afetados:** `SIMULATION_LOD.md`, `CITIZEN_SYSTEM.md`, `TRAFFIC_SYSTEM.md`

## Contexto
O briefing propõe fidelidade de simulação pela **distância da câmera**. Isso cria efeito observador: os resultados mudam conforme onde o jogador olha, o que quebra determinismo, replay, Laboratórios *headless* e justiça na avaliação de código. *Cities: Skylines* separa cidadãos (até ~1M de registros) de instâncias ativas (~65k), separação que inspira esta decisão.

## Decisão
1. **Cidadão** = registro no domínio (SoA). **Agente** = representação visual efêmera.
2. **LOD de simulação por estado** (S0 em viagem, S1 em atividade, S2 coortes se necessário), **independente da câmera**.
3. **LOD de apresentação por câmera** (R0–R3), com orçamento de agentes visíveis. Agentes cosméticos marcados como tal.
4. "Seguir cidadão" usa um agente dedicado preso ao progresso autoritativo.
5. Atualizações por cidadão em **baldes** (time-slicing).

## Alternativas consideradas
| Alternativa | Por que não |
|---|---|
| LOD de simulação por câmera (briefing) | efeito observador; não determinístico |
| Simular todos os agentes microscopicamente | custo proibitivo em escala |

## Consequências
**Positivas:** escala com custo previsível; resultados iguais com ou sem câmera.
**Negativas:** veículos e pedestres visíveis são em parte "representativos" (não são cidadãos específicos).
**Riscos:** percepção de "falsidade" (mitigação: seguir/Inspector sempre reais; poucos cosméticos).

## Gatilhos de revisão
Playtest indicando quebra de imersão · perfilamento indicando necessidade de S2.
