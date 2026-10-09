# City: SkyCode

> Nome definido pelo dono do produto em 2026-10-08. Antes do lançamento público é obrigatória uma busca de marca (risco R17). No código, o namespace continua `CitySky`.

**Um simulador de cidade 3D em que você não apenas constrói a cidade: você a programa em C#.**

```csharp
var house = new House { Residents = 4, Insulation = Insulation.Good };
city.Build(house);   // a casa realmente aparece, e a rede de energia realmente muda
```

O jogador aprende **C#, POO, SOLID, padrões de projeto, arquitetura e testes** porque precisa fazer a cidade funcionar: energia, economia, felicidade, trânsito. O código tem consequências reais na simulação.

## Status
**Fase 0: Pesquisa e Arquitetura.** Este repositório contém, por enquanto, a documentação completa de produto, arquitetura e roadmap. O código começa na Fase 1.

## Stack (resumo)
| Camada | Tecnologia |
|---|---|
| Engine | Unity 6 (6.7 LTS), **URP**, IL2CPP, Windows primeiro |
| Simulação | Domain C# puro, determinístico, orientado a dados; ECS/Burst para agentes em massa (Fase 11+) |
| Código do jogador | Roslyn 5 em processo **.NET 10** separado (Code Host) + **Sandbox Worker** restrito |
| Editor | Monaco em webview + serviços de linguagem Roslyn |
| UI | UI Toolkit |
| Testes | NUnit (Unity Test Framework + NUnit 4) |
| IA para .NET | **Claude Code + [dotnet-claude-kit](https://github.com/codewithmukesh/dotnet-claude-kit)** (obrigatório) |

## Onde começar
- 📚 [docs/README.md](docs/README.md): índice da documentação
- ⚠️ [docs/CRITICAL_ANALYSIS.md](docs/CRITICAL_ANALYSIS.md): riscos e onde a arquitetura diverge do briefing
- 🗺️ [docs/PROJECT_ARCHITECTURE.md](docs/PROJECT_ARCHITECTURE.md): diagrama geral
- 🎯 [docs/MVP.md](docs/MVP.md): escopo da primeira versão
- 🛣️ [docs/DEVELOPMENT_ROADMAP.md](docs/DEVELOPMENT_ROADMAP.md): fases
- 📋 [docs/phases/](docs/phases/README.md): plano detalhado de cada fase (backlog, gates, estimativas)
- 🧪 [docs/spikes/](docs/spikes/README.md): planos dos spikes da Fase 0, o próximo passo prático
- 🤖 [CLAUDE.md](CLAUDE.md) e [docs/AI_DEVELOPMENT_GUIDE.md](docs/AI_DEVELOPMENT_GUIDE.md): regras para agentes de IA

## Briefing original
[main.md](main.md)
