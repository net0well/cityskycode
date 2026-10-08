# Sistema de Esgoto

> **Status:** v1.0 (intenção de design) · **Data:** 2026-10-08 · **Fase:** 12 · **MVP:** não

---

## 1. Componentes
Coleta (tubulação sob as ruas, compartilhando o grafo da água com uma camada própria) · `SewageOutlet` (despejo em corpo d'água) · `SewageTreatmentPlant` (capacidade m³/dia, eficiência de tratamento).

## 2. Regras
- Volume de esgoto ≈ 80–90% do consumo de água do prédio.
- Por componente conexo: volume gerado vs. capacidade de tratamento. O excedente vai **sem tratamento** para o despejo.
- Despejo sem tratamento → **poluição da água** a jusante (campo no grid de água do terreno) → contaminação de captações (`WATER_SYSTEM.md`), felicidade *Environment* ↓, valor do terreno ↓ perto do despejo.
- Prédio sem coleta (fora da rede) → necessidade sanitária ↓ (parte de *Health*) e mau cheiro (ruído/ambiente).

## 3. Problemas
"Esgoto a céu aberto no bairro X", "Estação de tratamento saturada", "Rio contaminado". Todos com causas rastreáveis.

## 4. Pedagogia
Sistemas acoplados (água ↔ esgoto ↔ saúde): ótimo cenário para **Observer** (eventos de contaminação) e para depuração de causa e efeito em cadeia.
