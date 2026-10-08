# Fase 18 — Escala

> **Status:** Intenção de design · **Bloco:** E · **Atualizado:** 2026-10-08
> **Entrada:** gate da Fase 13 · **Saída:** 50.000 cidadãos a ≥ 25 FPS no mínimo; 100.000+ a ≥ 40 FPS no recomendado (`PERFORMANCE.md` §2)

---

## 1. Objetivo
Levar a cidade de 10 mil para 100 mil+ cidadãos sem reescrever a arquitetura, e transformar a própria escala em conteúdo (Módulo M12: "50 mil cidadãos e seu código ficou lento").

## 2. Por que nesta posição
Otimizar antes de ter todos os sistemas seria otimizar no escuro. A arquitetura já foi desenhada para isso (stores SoA, baldes, portas de aceleração, LOD independente da câmera); esta fase executa o plano com medições.

## 3. Escopo
**Inclui:**
- **Spike S10:** domínio numa thread de simulação dedicada com *double buffer* de read models (custo de cópia, latência, determinismo).
- Thread de simulação dedicada (`ARCHITECTURE.md` §6).
- LOD de apresentação completo (R1–R3): VAT, impostores, pontos de luz noturnos (`SIMULATION_LOD.md` §3).
- Pathfinding hierárquico por bairro (HPA*-like); limite de viagens concorrentes com viagens "virtuais" no meso.
- IDs com geração (reuso de índices).
- Campos (`IFieldSolver`) e meso em Burst via portas, com resultados idênticos.
- Save binário por seção (ADR-0012: saves > 10 MB ou > 500 ms).
- *Picking* por ID em buffer de GPU.
- Coortes S2, **somente se** o perfilamento exigir, com teste estatístico (±3% nos agregados).
- Conteúdo: **Módulo M12** (complexidade, lote × um a um, noção de orientação a dados) e arco "da POO ao orientado a dados", parte final.

## 4. Entregáveis
Relatórios de perfilamento antes/depois em `docs/perf/` · thread dedicada · LOD completo · pathfinding hierárquico · save binário · conteúdo M12.

## 5. Gate de saída
- [ ] Metas de FPS e de ms/tick do `PERFORMANCE.md` para 50k e 100k, medidas em build IL2CPP.
- [ ] Determinismo e independência da câmera verdes com a thread dedicada.
- [ ] Save de 100k em < 1 s, fora do main thread.
- [ ] Se S2 for ativado: agregados dentro de ±3% de S0/S1.

## 6. Riscos
R06 (Mono/IL2CPP), R07 (determinismo com threads e Burst), R05 (GPU em cidades grandes), complexidade de depuração com duas threads.

## 7. Documentos e ADRs
`PERFORMANCE.md`, `SIMULATION_LOD.md`, `SIMULATION_ARCHITECTURE.md` §10, `PATHFINDING.md`, `SAVE_SYSTEM.md`, ADR-0003, ADR-0012 (formato binário).
