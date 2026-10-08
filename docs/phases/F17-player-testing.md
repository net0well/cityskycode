# Fase 17 — Testes escritos pelo jogador

> **Status:** Intenção de design · **Bloco:** D · **Atualizado:** 2026-10-08
> **Entrada:** gate da Fase 16 · **Saída:** o aluno escreve `[Test]` que rodam no Worker; desafios "pegue o bug" e "mate os mutantes" validados

---

## 1. Objetivo
Ensinar testes (§44): AAA, `Assert.That`, casos de borda, fakes com DIP, "teste que pega bug" e TDD leve, com bugs que **acontecem na cidade** (`TESTING.md` Parte B).

## 2. Por que nesta posição
Fakes e testes isolados dependem de DIP (F15). Testes de regressão fecham a missão do lixo (etapa 6, §74).

## 3. Escopo
**Inclui:**
- `nunit.framework` na compilação e na allowlist, só para atributos e `Assert`/`Is`/`Has`.
- Descoberta e execução de `[Test]` **dentro do Worker** por reflexão própria (o runner do NUnit não é usado), cada teste com orçamento.
- Console de testes estilo IDE (✔/✘, mensagem do `Assert`, linha); rodar teste individual.
- Tipos de desafio do `TESTING.md` §B3: escreva o teste; pegue o bug; **mate os mutantes** (≥ 80%, com gerador de mutantes: `<`↔`<=`, `+`↔`-`, constante alterada); corrija sem quebrar; TDD (nível 5 detecta lógica "especial para o teste").
- `CitySky.Testing` (fakes e builders para o aluno).
- Bugs encenados na cidade (ex.: tarifa cobrando em dobro de quem tem painel solar) e missão do lixo etapa 6.
- Pré/pós-teste conceitual (`LEARNING_DESIGN.md` §9).

## 4. Entregáveis
Runner de testes no Worker · gerador de mutantes · 5 tipos de desafio · `CitySky.Testing` · conteúdo M10.

## 5. Gate de saída
- [ ] Corpus de segurança ampliado com abusos via testes (reflexão, atributos, laços em `[SetUp]`) e verde.
- [ ] Gerador de mutantes determinístico (mesmo código → mesmos mutantes).
- [ ] Desafios M10 aprovados no CI de conteúdo e em playtest.

## 6. Riscos
Superfície de reflexão para descobrir testes (a reflexão roda no nosso código do Worker, nunca exposta ao aluno; revisão do `security-auditor`), custo de executar N mutantes × M testes (orçamento por desafio).

## 7. Documentos e ADRs
`TESTING.md` Parte B, `CODE_EXECUTION.md` §5.4, `CODE_EXECUTION_SECURITY.md` (allowlist), `PLAYER_API.md` §3, ADR-0016.
