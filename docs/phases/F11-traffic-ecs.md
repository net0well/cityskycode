# Fase 11 — Trânsito v1 e introdução do ECS

> **Status:** Intenção de design · **Bloco:** C · **Atualizado:** 2026-10-08
> **Entrada:** gate da Fase 10 · **Saída:** cidade de **10.000 cidadãos** com trânsito causal e visível; ADR-0010 revisado com base no spike S8

---

## 1. Objetivo
Veículos com origem, destino e rota, congestionamento que afeta a cidade (§20) e milhares de agentes visíveis com ECS, sem perder determinismo nem a validação headless.

## 2. Por que nesta posição
O trânsito é o sistema mais caro e arriscado de um city builder (`CRITICAL_ANALYSIS.md` D11). Ele só entra quando a cidade é grande o bastante para ter trânsito, e entra junto com o ECS porque é a primeira vez que a escala visual exige (ADR-0003).

## 3. Escopo
**Inclui:**
- **Spike S8** (início da fase): meso autoritativo × micro autoritativo numa cidade de 10k; comparar custo, determinismo entre runtimes e "sensação". Revisar ADR-0010.
- **Spike S9**: custo da ponte domínio → ECS (snapshots por frame) com 2.000 agentes visíveis.
- Modelo **mesoscópico** autoritativo (`TRAFFIC_SYSTEM.md` §3): BPR, capacidade de interseção, *spillback*, custos de rota dinâmicos.
- Veículos (`VEHICLE_SYSTEM.md`): carro como atributo do domicílio, carga, escolha de modo (a pé × carro).
- Estradas avançadas: tipos de rua, faixas, mão única, `UpgradeSegment`, **curvas Bézier** (3b), rotatórias, semáforos com plano fixo (State), faixas de pedestre.
- `CitySky.Agents.Ecs` + Entities Graphics: pedestres e veículos visuais; microssimulação **visual** (IDM simplificado, parada no vermelho do domínio).
- `IPathfinder` acelerado (Burst) com resultado idêntico ao gerenciado; grafo viário por faixa.
- Necessidade *Transport*; eventos "Congestionamento severo" e "Acidente".
- Overlay de Tráfego, debug "Show Traffic Routes", Inspector da rua com contagem.
- Player API: `Vehicle` (abstrata), `IRouteStrategy`, `city.Traffic` (relatórios).

**Não inclui:** transporte público (F19), veículos de serviço (F12–F13), semáforos atuados (F19).

## 4. Entregáveis (épicos)
E0 spikes S8 e S9 · E1 meso · E2 veículos e escolha de modo · E3 estradas avançadas · E4 ECS e Entities Graphics · E5 pathfinding Burst · E6 efeitos (felicidade, poluição futura, produtividade) · E7 visualização · E8 Player API.

## 5. Gate de saída
- [ ] ADR-0010 revisado com os números do S8.
- [ ] 10.000 cidadãos, ~2.000 agentes visíveis, ≥ 30 FPS no hardware mínimo (`PERFORMANCE.md` §2).
- [ ] Meso ≤ 0,5 ms por passo de 1 s TM com 10k cidadãos.
- [ ] Pathfinding gerenciado × Burst: resultados idênticos em todos os grafos de teste.
- [ ] Teste de independência da câmera continua verde com ECS.
- [ ] Conservação de veículos (entram = saem + presentes) e determinismo verdes.

## 6. Riscos
R20 (meso não parecer vivo), R06 (custo no Mono/IL2CPP), R07 (determinismo com Burst), R08 (malha de interseções com curvas e rotatórias), APIs do Entities ainda mudando (R11).

## 7. Documentos e ADRs
`TRAFFIC_SYSTEM.md`, `VEHICLE_SYSTEM.md`, `ROAD_SYSTEM.md`, `PATHFINDING.md`, `SIMULATION_LOD.md`, `PLAYER_API.md`, ADR-0003, ADR-0010, ADR-0011.
