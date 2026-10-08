# Fase 9 — Sandbox endurecido, replay e compartilhamento

> **Status:** Intenção de design (backlog detalhado no encerramento da Fase 8) · **Bloco:** C · **Atualizado:** 2026-10-08
> **Entrada:** 🚩 gate do MVP aprovado · **Saída:** L5 completo + corpus de código e de pacotes maliciosos verde em Windows 10 22H2 e 11 + fuzzing do protocolo sem falhas

---

## 1. Objetivo
Tornar seguro executar código **de outras pessoas** e liberar o que depende disso: compartilhamento por arquivo, comportamentos do jogador chamados pela simulação, replay na cidade e exportação de progresso para o professor.

## 2. Por que nesta posição
O compartilhamento foi confirmado pelo dono do produto (ADR-0017) e, numa sala de aula, um pacote malicioso circula por uma turma inteira. O `CODE_EXECUTION_SECURITY.md` é explícito: **sem L5 completo, não existe compartilhamento**. É também a primeira fase em que o código do aluno passa a ser chamado **pela simulação** (IPC em lote), o que muda o perfil de risco.

## 3. Escopo
**Inclui:**
- **L5 completo:** AppContainer sem capabilities (ou LPAC, ou token restrito + Low IL, conforme o S6), lista explícita de handles, matriz Windows 10 22H2 / 11; ficha de dependência para o P/Invoke (ex.: CsWin32).
- **Corpus ampliado** (`CODE_EXECUTION_SECURITY.md` §5) e **fuzzing** do protocolo Code Host ↔ Worker e Unity ↔ Code Host.
- **Comportamentos** (`CODE_EXECUTION.md` §5.3): `IBehaviorEvaluator` com IPC em lote, orçamento de 20 ms (soft) / 200 ms (hard), *fallback* para a regra padrão com aviso pedagógico; `city.Policies`; primeira política: `IEnergyPolicy` (ordem de corte, com regras mínimas).
- **Replay** (`SAVE_SYSTEM.md` §6): log de comandos + keyframes diários; voltar a T = keyframe ≤ T + re-simulação; comparar execuções em Laboratórios.
- **Compartilhamento por arquivo** `.citysky` (`SHARING_SYSTEM.md`): validação de zip (limites, zip slip, extensões, hashes), pré-visualização somente leitura, ativação explícita do código importado, corpus de pacotes maliciosos.
- **Exportação de progresso** `.cityskyprogress` (`CLASSROOM.md` §2).
- **Editor:** realce semântico e formatação (Roslyn); **traço de execução** em Laboratórios (*time-travel* simples).
- **Pós-MVP imediato:** debug "Show Electricity Network".
- **Conteúdo:** início do Módulo M3 (condicionais) com a `IEnergyPolicy` como motivação.

**Não inclui:** galeria online (fora do roadmap), Steam Workshop (F20), painel do professor (F13).

## 4. Entregáveis (épicos)
E1 L5 completo · E2 corpus + fuzzing · E3 comportamentos em lote · E4 replay · E5 `.citysky` · E6 progresso exportável · E7 editor (realce, formatação, traço) · E8 conteúdo M3 inicial.

## 5. Gate de saída
- [ ] Revisão completa do `security-auditor` (checklist do `CODE_EXECUTION_SECURITY.md` §7) sem achados críticos.
- [ ] Corpus de código **e** de pacotes verde em Windows 10 22H2 e Windows 11.
- [ ] Fuzzing do protocolo por N horas (definir no backlog) sem crash do Host nem do jogo.
- [ ] Replay: voltar e re-simular produz o mesmo hash.
- [ ] Comportamento lento cai no *fallback* com aviso, sem travar a simulação.
- [ ] Importar um `.citysky` nunca executa código sem ação explícita (teste automatizado).

## 6. Riscos
R04 (sandbox burlado), R25 (código malicioso entre alunos), R29 (Windows 10), latência de IPC por lote (medir), matriz de versões do Windows.

## 7. Documentos e ADRs
`CODE_EXECUTION_SECURITY.md`, `SHARING_SYSTEM.md`, `CLASSROOM.md`, `SAVE_SYSTEM.md`, `CODE_EDITOR.md`, `ENERGY_SYSTEM.md` §7, ADR-0006 (L5 final), ADR-0012 (replay).

## 8. Ferramental
`security-auditor` (obrigatório em todo PR), `/security-scan`, `test-engineer` (fuzzing e golden saves de replay).
