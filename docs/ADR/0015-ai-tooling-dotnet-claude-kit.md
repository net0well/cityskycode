# ADR-0015: Adotar dotnet-claude-kit como ferramental de IA para todo trabalho .NET

- **Status:** Aceito
- **Data:** 2026-10-08
- **Documentos afetados:** `AI_DEVELOPMENT_GUIDE.md`, `/CLAUDE.md`, `CODING_STANDARDS.md`, `TESTING.md`, `TECH_STACK.md`

## Contexto
O projeto será desenvolvido com forte apoio de agentes de IA (§76). Para que diferentes agentes e sessões produzam código consistente, é preciso um conjunto comum de conhecimento, fluxos de trabalho e ferramentas. O **dotnet-claude-kit** (codewithmukesh, MIT, v0.11) oferece, para Claude Code: 47 skills, 10 agentes especialistas, 16 comandos (`/spec`, `/plan`, `/tdd`, `/verify`, `/code-review`, `/arch-check`, `/security-scan`, `/health-check`, `/build-fix`, `/de-sloppify`, `/outdated`, `/migrate`, `/checkpoint`, `/wrap-up`, `/dotnet-init`, `/scaffold`), 10 regras, *hooks*, e o MCP **CWM.RoslynNavigator** (consultas semânticas via Roslyn: `find_symbol`, `find_references`, `get_public_api`, `detect_circular_dependencies`, `detect_antipatterns`...). O kit mira .NET 10 / C# 14 e aplicações ASP.NET Core.

## Decisão
1. **Uso obrigatório** do dotnet-claude-kit em todo trabalho C#/.NET: domínio compartilhado, Code Host, Worker, Player API, analyzers, testes e CI.
2. **MCP-first:** agentes consultam o Roslyn MCP antes de ler arquivos inteiros. Solução de referência: `dotnet/CitySky.slnx` (que inclui o domínio via shared source).
3. **Fluxo padrão:** `/spec` → `/plan` → `/tdd` → `/verify` → `/code-review`; `/arch-check` sempre; `/security-scan` no escopo de execução de código; `/health-check` por fase.
4. **Overrides documentados** (`CLAUDE.md`, `AI_DEVELOPMENT_GUIDE.md` §3), que prevalecem sobre o kit: C# 9 em Unity/compartilhado; NUnit em vez de xUnit; `SimulationClock` no domínio; sem `CancellationToken` no domínio; arquitetura já decidida; zero dependências no compartilhado; skills web (minimal-api, EF Core, Aspire etc.) não aplicáveis sem ADR.
5. `/dotnet-init`, se executado, deve usar **merge** com o `CLAUDE.md` existente.

## Alternativas consideradas
| Alternativa | Por que não |
|---|---|
| Sem kit (só CLAUDE.md próprio) | reinventar skills, comandos e o MCP Roslyn; menos consistência |
| managedcode/dotnet-skills ou dotnet/skills | catálogos de skills sem o conjunto integrado de comandos + MCP Roslyn + agentes; podem complementar no futuro |

## Consequências
**Positivas:** consistência entre agentes; economia de tokens (MCP); qualidade (gates `/verify`, `/arch-check`, `/security-scan`).
**Negativas:** o kit é opinativo para web/ASP.NET, o que exige overrides; skills consomem contexto.
**Riscos:** agentes aplicarem convenções C# 14 no código Unity. Mitigação: `LangVersion 9` no csproj de shared source (o compilador barra) + overrides explícitos.

## Gatilhos de revisão
Mudança grande de versão do kit · Unity com C# moderno (overrides O1 deixam de ser necessários) · kit descontinuado.

## Referências
https://github.com/codewithmukesh/dotnet-claude-kit · https://codewithmukesh.com/blog/dotnet-claude-kit-guide/
