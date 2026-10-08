# Motor de Aprendizagem

> **Status:** v1.0 · **Data:** 2026-10-08 · **MVP:** v0 (progresso por lição + domínio simples) · Completo: Fase 13+
> Projeto: `CitySky.Education` (shared, C# 9) + conteúdo em `content/`.

---

## 1. Responsabilidades
1. Manter o **grafo de conceitos** e o estado de desbloqueio.
2. Manter o **modelo do aluno** (domínio estimado por conceito).
3. **Recomendar** a próxima atividade: lição, Laboratório, revisão ou missão de problema.
4. **Mapear problemas da cidade → missões** compatíveis com o nível do aluno.
5. Agendar **revisões espaçadas**.
6. Registrar eventos de aprendizagem (local; telemetria remota só com opt-in).

## 2. Modelo do aluno
Por conceito `c`:
| Campo | Descrição |
|---|---|
| `Mastery` (0–1) | probabilidade estimada de domínio |
| `Attempts`, `Successes` | contagens |
| `HintsUsed` | por nível |
| `LastPracticed` | data de jogo e data real |
| `NextReview` | agendamento espaçado |
| `CommonErrors` | contagem por código de erro (ex.: `CS0165` frequente → reforço) |

**Atualização (v0, MVP):** heurística transparente:
```
sucesso sem dica:          Mastery += 0,25 × (1 − Mastery)
sucesso com dicas 1–2:     Mastery += 0,15 × (1 − Mastery)
sucesso com dica 3/parcial: Mastery += 0,05 × (1 − Mastery)
solução completa revelada: Mastery += 0
falha:                     Mastery −= 0,05 × Mastery
```
**v1 (Fase 13+):** *Bayesian Knowledge Tracing* simplificado (parâmetros por conceito: P(L0), P(T), P(G), P(S)) calibrado com dados de playtest.

## 3. Revisão espaçada
- Após o domínio (≥ 0,85), as revisões ficam em intervalos crescentes (1, 3, 7, 14 dias **reais** de jogo do aluno), apresentadas como **manutenção da cidade** ("os projetos antigos de casas precisam de revisão"), nunca como "prova".
- Revisão errada → Mastery cai e o intervalo é reiniciado.

## 4. Recomendação
```
candidatos = atividades cujos pré-requisitos têm Mastery ≥ 0,6
prioridade = revisões vencidas
           > missão ligada a problema ativo da cidade (se compatível)
           > próximo conceito do grafo (menor profundidade)
           > exercício de variação do conceito com menor Mastery
```
O aluno pode **sempre** escolher outra atividade (autonomia). A recomendação é sugestão.

## 5. Problema → missão (§27)
| Problema da cidade | Missões por nível |
|---|---|
| Déficit de energia | UI: construir usina · M1: calcular déficit · M2: método de consumo · M3: construir em série com laço · M7: `IEnergyPolicy` |
| Lixo acumulado | UI: caminhão extra · M4: depósito mais próximo · M13: Dijkstra · M7/M9: Strategy de rota |
| Déficit financeiro | UI: impostos · M1: projeção de saldo · M3: regras condicionais de alíquota · M9: Specification de políticas |

Se nenhum nível compatível existir, o problema mostra **apenas** a solução via UI e um teaser: "você aprenderá a automatizar isso em...".

## 6. Dados e privacidade
- Perfil salvo localmente (`profile.json`, separado das cidades).
- Exportação manual do progresso (`.cityskyprogress`) para o professor (Fase 9), importada no **painel do professor local** (Fase 13). Ver `CLASSROOM.md`.
- Perfis locais por aluno no mesmo PC (MVP): cada perfil tem o seu modelo do aluno.
- Nenhuma telemetria remota no MVP. Opt-in explícito depois, com dados anonimizados (LGPD, possivelmente menores: ver `SECURITY.md`).

## 7. Testes
- Grafo de conceitos acíclico; todo desafio aponta para conceitos existentes.
- Simulação de "alunos sintéticos" (perfis que sempre acertam, sempre usam dicas, alternam) para verificar a progressão e as recomendações.
