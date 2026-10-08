# Fase 16 — Padrões de projeto

> **Status:** Intenção de design · **Bloco:** D · **Atualizado:** 2026-10-08
> **Entrada:** gate da Fase 15 · **Saída:** os 9 padrões do §43 ensinados, cada um nascido de uma dor real da cidade e validado por estrutura + comportamento

---

## 1. Objetivo
Introduzir Strategy, Observer, Command, State, Factory, Builder, Specification, Repository e Mediator **somente quando resolvem um problema real** (§43, §71), e mostrar nos "bastidores" que a própria engine usa os mesmos padrões (`DESIGN_PATTERNS.md` §B).

## 2. Por que nesta posição
Padrões pressupõem interfaces (F14) e a experiência de refatorar com SOLID (F15). O exemplo-âncora do briefing (§71: "12 tipos de transporte e o código cheio de `if`") só existe depois do trânsito e dos veículos de serviço.

## 3. Escopo
**Inclui:**
- Uma missão por padrão no formato do `DESIGN_PATTERNS.md` §A (ingênuo → mudança que multiplica a dor → revelação → refatoração validada → bastidores), mais Decorator e Template Method como extras.
- Missão do lixo etapa 5 (Strategy + OCP/SRP, §74).
- APIs que os padrões exigem: `city.Events` completo (Observer), ações desfazíveis do aluno (Command), estados de semáforo (State), `BuildingFactory` do aluno para crescimento, `HospitalBuilder`, Specifications de lote e de problema, `IBlueprintRepository` (biblioteca de projetos do aluno), `EmergencyDispatcher` (Mediator).
- Políticas de zoneamento escritas pelo aluno (Specification).
- Regras CSK4 por padrão (ex.: "existe implementação de `IRouteStrategy` e não há *type switch* sobre `Vehicle`").

## 4. Entregáveis
11 missões · extensões da Player API · regras CSK4 · lições de bastidores (código real simplificado da engine).

## 5. Gate de saída
- [ ] Cada missão passa no CI de conteúdo; regras CSK4 sem falso positivo.
- [ ] Playtest: o aluno consegue dizer qual dor cada padrão resolveu (pergunta aberta no questionário).

## 6. Riscos
Padrões virarem "decoreba" se a dor não for sentida (medir no playtest), API crescendo demais (cada membro novo precisa de motivo pedagógico, `PLAYER_API.md` §1).

## 7. Documentos e ADRs
`DESIGN_PATTERNS.md`, `PLAYER_API.md`, `CODE_ANALYSIS.md`, `GARBAGE_SYSTEM.md` §5, `ZONE_SYSTEM.md` §6.
