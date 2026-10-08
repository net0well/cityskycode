# Fase 10 — Zoneamento e crescimento

> **Status:** Intenção de design · **Bloco:** C · **Atualizado:** 2026-10-08
> **Entrada:** 🚩 gate do MVP (pode correr em paralelo à Fase 9, se houver mais de uma pessoa) · **Saída:** cidade de **1.000 cidadãos** crescendo por demanda RCI explicável, a ≥ 30 FPS no hardware mínimo

---

## 1. Objetivo
Passar de "o jogador constrói cada prédio" para um **city builder de verdade**: zonas, demanda, setor privado construindo, valor do terreno, mercado simples e o modo Sandbox.

## 2. Por que nesta posição
É o primeiro passo de escala (50 → 1.000) e o que transforma a vila do MVP numa cidade que cresce sozinha. Também abre espaço para o Módulo M3 (condicionais e laços) com problemas reais: construir em série, reagir a déficit.

## 3. Escopo
**Inclui:**
- Zonas (`ZONE_SYSTEM.md`): Residencial (densidades), Comercial, Industrial, Escritórios; pintura de zonas em lotes.
- Demanda RCI + O com contribuições; `ZoneGrowthSystem` (Specification + Factory); abandono.
- Valor do terreno (campo na grade de 32 m).
- Obras com duração e construtoras; empresas além do Shop (`ECONOMY_SYSTEM.md` §4); **mercado** com preço amortecido (±2%/dia); imposto industrial; empréstimos; aluguel/IPTU.
- Distritos (polígonos), visão de bairro, marcadores de câmera, seleção de área com estatísticas.
- Overlays: População/Densidade, Renda, Valor do terreno, Empregos, Zonas. Debug: Job Connections, Supply Chain, Money Flow 3D (arcos + partículas).
- Necessidade *Entertainment* (parques, lazer); produtividade pela felicidade.
- Marcos de cidade (`PROGRESSION.md` §2) e modo **Sandbox**.
- **Addressables** para o catálogo de prédios (com ficha de dependência).
- Vegetação urbana via GRD com LOD/impostores; remoção de árvores ao construir.
- Player API: `Zone`, `District`, `city.Zone(area, ZoneType)`, `ResidentialBuilding` como tipo-base, construir ruas por código (com custo normal), relatórios do orçamento por categoria.
- Conteúdo: **Módulo M3** (condicionais e laços: "construa uma grade de 4×4 quarteirões", regras de alíquota).

**Não inclui:** trânsito (F11), serviços públicos (F13), escritórios dependentes de educação (F13).

## 4. Entregáveis (épicos)
E1 zonas e demanda · E2 crescimento privado e abandono · E3 valor do terreno · E4 economia v2 (empresas, mercado, empréstimos) · E5 distritos, câmera e seleção · E6 overlays e debug · E7 progressão (marcos, Sandbox) · E8 Addressables e vegetação · E9 Player API + M3.

## 5. Gate de saída
- [ ] Cidade roteirizada cresce até 1.000 cidadãos por demanda, com cada termo da demanda explicado no painel.
- [ ] Testes de balanceamento: desemprego 3–12%, felicidade média 55–80, mercado sem oscilação divergente em 1 ano de jogo.
- [ ] ≥ 30 FPS no hardware mínimo com 1.000 cidadãos (~300 agentes visíveis).
- [ ] Lições do M3 aprovadas em playtest curto.

## 6. Riscos
R12 (laços de realimentação do mercado e do crescimento), R05 (GPU com mais prédios), R01 (escopo: esta fase é grande; dividir em 10a/10b se o backlog passar de 8 semanas).

## 7. Documentos e ADRs
`ZONE_SYSTEM.md`, `ECONOMY_SYSTEM.md` §5, `BUDGET_SYSTEM.md`, `MAP_OVERLAYS.md`, `MONEY_FLOW.md`, `PROGRESSION.md`, `ASSET_PIPELINE.md` (Addressables), `PLAYER_API.md`, `LEARNING_PATH.md` (M3).
