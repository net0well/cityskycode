# Sistema de Educação (serviço da cidade)

> **Status:** v1.0 (intenção de design) · **Data:** 2026-10-08 · **Fase:** 13 · **MVP:** não
> **Não confundir** com o sistema de aprendizagem do **jogador** (`LEARNING_ENGINE.md`). Este documento trata das escolas **da cidade simulada**.

---

## 1. Componentes
`ElementarySchool`, `HighSchool`, `University` (Fase 19), `Library` (Fase 19). Seguem o **modelo genérico de serviço** (`CITY_SIMULATION.md` §3): custo, capacidade (vagas), cobertura, eficiência, funcionários e demanda.

## 2. Regras
- Requer **faixas etárias** (crianças e jovens), introduzidas na Fase 13 junto com nascimentos e envelhecimento.
- Matrícula: estudante → escola com vaga mais próxima (Dijkstra multi-fonte + capacidade). Viagens escolares diárias.
- Progressão: anos letivos concluídos elevam o **nível de educação** (0–4) do cidadão.
- Efeitos: empregos qualificados (escritórios e indústria avançada exigem nível), renda, felicidade (*Education*), menor criminalidade.
- Falta de vagas → necessidade *Education* ↓ nos domicílios com crianças → problema "Baixa educação".

## 3. Métricas
Cobertura (% de crianças matriculadas), lotação, nível médio da população, *mismatch* (vagas qualificadas sem candidatos).

## 4. Pedagogia
Algoritmo de alocação com capacidade (filas e ordenação), relatórios (LINQ), Specification (elegibilidade).
