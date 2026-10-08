# ADR-0016: Usar NUnit em todo lugar (Unity, .NET e código do jogador)

- **Status:** Aceito
- **Data:** 2026-10-08
- **Documentos afetados:** `TESTING.md`, `CODING_STANDARDS.md`, `AI_DEVELOPMENT_GUIDE.md`

## Contexto
- O **Unity Test Framework** é baseado em NUnit (obrigatório para testes na Unity).
- O briefing (§44) mostra testes do jogador com `[Test]` (NUnit).
- O dotnet-claude-kit recomenda **xUnit v3** (com WebApplicationFactory/Testcontainers) para projetos .NET.

## Decisão
1. **NUnit** em todo lugar: UTF na Unity, **NUnit 4** nos projetos .NET, e API NUnit (atributos + `Assert.That`) no código do jogador (Fase 17).
2. **Modelo de constraints** (`Assert.That(x, Is.EqualTo(y))`), compatível entre a versão da Unity e o NUnit 4, sem `ClassicAssert`.
3. Do skill `testing` do dotnet-claude-kit, mantemos: AAA, testar comportamento, nomes descritivos, testes de integração valiosos (no nosso caso *headless* e PlayMode). Não se aplicam: WebApplicationFactory, Testcontainers, xUnit.

## Alternativas consideradas
| Alternativa | Por que não |
|---|---|
| xUnit v3 no .NET + NUnit na Unity | dois vocabulários de teste; código de teste não reaproveitável; confunde agentes e pessoas |
| MSTest | nenhuma vantagem aqui |

## Consequências
**Positivas:** um vocabulário só, para equipe, agentes e alunos; helpers de teste compartilháveis.
**Negativas:** diverge do padrão do kit (override O2 documentado).

## Gatilhos de revisão
Unity mudar de framework de testes (improvável) · necessidade de recurso exclusivo de outro framework.
