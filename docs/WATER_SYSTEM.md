# Sistema de Água

> **Status:** v1.0 (intenção de design) · **Data:** 2026-10-08 · **Fase:** 12 · **MVP:** não

---

## 1. Componentes (§23)
`WaterSource` (rio, lago, poço) · `WaterPump` (capacidade m³/dia) · `WaterPipe` (ao longo das ruas, por padrão) · `WaterTreatment` (qualidade) · `WaterConsumer` (prédios) · `Sewage` (ver `SEWAGE_SYSTEM.md`).

## 2. Modelo simplificado (por design: hidráulica real é complexa demais, ver CRITICAL_ANALYSIS §3)
- **Rede** = componentes conexos do grafo de tubulações (por padrão sob as ruas, como na energia).
- **Capacidade:** Σ bombas do componente × eficiência (verba, condição) vs. Σ consumo.
- **Pressão (simplificada):** pontuação por nó = f(distância em rede até a bomba mais próxima, diferença de elevação). Calculada por **Dijkstra multi-fonte** a partir das bombas. Baixa pressão → abastecimento parcial nos pontos altos e distantes.
- **Contaminação:** bomba captando a jusante de um despejo de esgoto não tratado (direção do rio definida no terreno) → qualidade baixa → saúde ↓ (Fase 13).
- **Vazamentos:** canos com condição baixa (idade, sem manutenção) perdem X% (consumo "fantasma"), detectável no Inspector, o que vira tema de **depuração** ("o consumo não fecha").

## 3. Problemas reais (§23)
| Problema | Detecção | Efeito |
|---|---|---|
| Falta de água | consumo > capacidade no componente | necessidade *Water* ↓, comércio afetado |
| Baixa pressão | pontuação < limiar no nó | abastecimento parcial |
| Contaminação | captação a jusante de esgoto | saúde ↓, evento de epidemia (Fase 19) |
| Capacidade insuficiente | saturação > 90% por 3 dias | alerta antecipado |
| Vazamentos | perda > 10% | custo ↑, mistério para investigar |

## 4. Overlays e debug
Overlay **Água** (abastecimento e pressão), "Show Water Network" (componentes, fluxo estimado da bomba ao consumidor).

## 5. Pedagogia
Dijkstra multi-fonte (Módulo 13), depuração de vazamento (Inspector + consultas), `IWaterAllocationPolicy` em escassez (Strategy).
