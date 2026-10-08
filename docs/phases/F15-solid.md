# Fase 15 — SOLID

> **Status:** Intenção de design · **Bloco:** D · **Atualizado:** 2026-10-08
> **Entrada:** gates das Fases 13 e 14 · **Saída:** "código legado da prefeitura" (`CityManager`) refatorável, com cada princípio validado por testes de comportamento + regras estruturais CSK4

---

## 1. Objetivo
Ensinar SRP, OCP, LSP, ISP e DIP **pela dor** (§42): o aluno herda uma `CityManager` que faz tudo, recebe pedidos de mudança que doem e sente o alívio depois de refatorar (`SOLID_LEARNING.md`).

## 2. Por que nesta posição
A `CityManager` mexe em impostos, energia, lixo, frota e relatórios. Todos esses sistemas precisam existir (F6–F13) e o aluno precisa saber interfaces (F14).

## 3. Escopo
**Inclui:**
- `content/legacy/CityManager.cs` (~400 linhas) com testes ocultos de comportamento. **A arquitetura real do jogo nunca tem uma `CityManager`** (`CRITICAL_ANALYSIS.md` D15).
- Uma missão por princípio (`SOLID_LEARNING.md` §3), na sequência pedagógica do §4 (usar → mudança que dói → tentativa → princípio → refatoração guiada → mudança fácil → revisão).
- Regras estruturais CSK4: coesão (LCOM simplificado), dependência de tipo concreto (DIP), `NotSupportedException` em sobrescritas (LSP), interfaces com ≤ K membros (ISP), "diff das classes antigas vazio" (OCP).
- Code actions pedagógicas (extrair interface, extrair classe) e renomear (Roslyn `Renamer`).
- Métricas antes/depois ("relatório do engenheiro da prefeitura").
- `IMapOverlay` do aluno (avaliado em lote na grade) e eventos `city.Events` (Observer), iniciados aqui e completados na F16.
- Módulo M11 (refatoração e arquitetura), parte 1.

## 4. Entregáveis
Pacote da `CityManager` + 5 missões · regras CSK4 · code actions · `IMapOverlay` do aluno.

## 5. Gate de saída
- [ ] Cada missão tem solução de referência aprovada e soluções erradas típicas reprovadas com o feedback certo.
- [ ] Regras CSK4 sem falso positivo nas soluções de referência (teste).
- [ ] Overlay do aluno lento cai para o valor anterior com aviso, sem afetar o FPS.

## 6. Riscos
Heurísticas de coesão com falsos positivos (validar com várias soluções corretas), frustração com código legado longo (dividir em partes de ≤ 20 min, regra da sala de aula).

## 7. Documentos e ADRs
`SOLID_LEARNING.md`, `CODE_ANALYSIS.md` (CSK4), `CODE_EDITOR.md` §4, `MAP_OVERLAYS.md` §2, `EVENT_SYSTEM.md` §A.
