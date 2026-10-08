# Fase 13 — Serviços públicos, problemas urbanos e painel do professor

> **Status:** Intenção de design · **Bloco:** C · **Atualizado:** 2026-10-08
> **Entrada:** gates das Fases 9 e 12 · **Saída:** problemas da cidade viram missões compatíveis com o nível do aluno; painel do professor importa uma turma de 40 alunos em < 2 s

---

## 1. Objetivo
Completar o city builder com serviços (saúde, educação, polícia, bombeiros), fechar o sistema de problemas (§26–27) e entregar as ferramentas de sala de aula que dependem de arquivos: painel do professor, pacote de turma e pacotes de desafio do professor.

## 2. Por que nesta posição
Serviços exigem faixas etárias, veículos de serviço e cobertura por rede (F11–F12). O painel do professor depende da exportação de progresso (F9), e os pacotes de desafio do professor são código de terceiros, portanto exigem o L5 completo (F9).

## 3. Escopo
**Inclui:**
- Faixas etárias, nascimentos, envelhecimento, novos domicílios, óbitos com discrição (`HOUSEHOLD_SYSTEM.md` §3).
- Saúde (`HEALTHCARE_SYSTEM.md`): clínica, hospital com composição de departamentos, ambulâncias, saúde individual.
- Educação da cidade (`EDUCATION_SYSTEM.md`): escolas, matrícula com capacidade, nível de educação, empregos qualificados (escritórios).
- Polícia e bombeiros (`CRIME_SYSTEM.md`): risco por célula **sem atributo demográfico** (escolha ética explícita), incêndios com propagação, despacho.
- Modelo genérico de serviço com cobertura por **Dijkstra multi-fonte**; verba por serviço (50–150%).
- Necessidades *Health*, *Education*, *Safety*; termômetro completo (11 indicadores).
- **Sistema de problemas completo** (catálogo do §26) e **problema → missão** (`LEARNING_ENGINE.md` §5).
- Motor de aprendizagem v1: *Bayesian Knowledge Tracing* calibrado com os dados dos playtests.
- Sugestões do orçamento com impacto estimado (simulação headless de 7 dias em segundo plano).
- **Painel do professor local**, **pacote de turma** (ritmo e liberações), **pacotes de desafio do professor** (editor simples; conteúdo não confiável no sandbox) (`CLASSROOM.md` §2).
- Overlays Saúde, Educação, Crime, coberturas; debug "Show Household Connections".
- Conteúdo: **Módulo M13** (grafos: BFS/Dijkstra, cobertura), etapa 2 avançada da missão do lixo.

**Não inclui:** universidade e biblioteca (F19), epidemias e greves (F19), integração com plataformas escolares online (fora do roadmap).

## 4. Entregáveis (épicos)
E1 demografia · E2 saúde · E3 educação · E4 segurança e incêndio · E5 cobertura e verbas · E6 problemas → missões · E7 motor v1 (BKT) · E8 painel do professor, pacote de turma, pacotes de desafio · E9 visualização · E10 conteúdo M13.

## 5. Gate de saída
- [ ] Cada problema do catálogo do §26 com detector, evidências, ações via UI e pelo menos uma missão por nível compatível (ou teaser "você aprenderá a automatizar isso em…").
- [ ] Painel do professor importa 40 arquivos de progresso em < 2 s e exporta CSV.
- [ ] Pacote de desafio de professor malicioso (corpus) é contido pelo sandbox e nunca executa sem ação explícita.
- [ ] Alunos sintéticos: recomendações coerentes com o BKT.
- [ ] Teste estatístico: nenhum atributo demográfico de cidadão aumenta o risco individual de crime.

## 6. Riscos
R12 (balanceamento com muitos sistemas), R25 (pacotes de desafio do professor), R18 (LGPD no painel), sensibilidade do conteúdo (óbitos, crime) para público jovem.

## 7. Documentos e ADRs
`HEALTHCARE_SYSTEM.md`, `EDUCATION_SYSTEM.md`, `CRIME_SYSTEM.md`, `CITY_HEALTH_SYSTEM.md`, `EVENT_SYSTEM.md` (B2), `LEARNING_ENGINE.md`, `CLASSROOM.md`, `BUDGET_SYSTEM.md` §4, `LEARNING_PATH.md` (M13).
