# ADR-0001: Usar Unity 6 como engine, Windows primeiro

- **Status:** Aceito
- **Data:** 2026-10-08
- **Documentos afetados:** `ENGINE_DECISION.md`, `TECH_STACK.md`

## Contexto
O produto ensina C# por meio de uma simulação urbana 3D em escala. Requisitos: C# como linguagem do projeto e do aluno, integração com Roslyn, simulação de milhares de agentes, renderização de cidades, UI de aplicação e equipe pequena. Comparação completa em `ENGINE_DECISION.md`.

## Decisão
1. **Unity 6**: spikes em **6.6**, desenvolvimento em **6.7 LTS** assim que lançada. Versão exata fixada no repositório.
2. Plataforma inicial **Windows 10/11** (x64, e ARM64 se o custo de teste for baixo). Linux/Steam Deck e macOS **depois do lançamento no Windows**, com a arquitetura portável desde já ([ADR-0018](0018-multiplatform-and-steam.md)). **Sem** consoles, mobile, WebGL ou Chromebook.
3. Player com **IL2CPP**, viável porque o código do aluno roda fora do processo (ADR-0005).
4. Não depender da Unity 7 (CoreCLR) até avaliação pós-MVP.

## Alternativas consideradas
| Alternativa | Por que não |
|---|---|
| Unreal Engine 5 | C++/Blueprints; executar C# exigiria hospedar o runtime .NET ou duplicar o domínio; custo arquitetural alto |
| Godot 4 (.NET) | ótima integração com .NET moderno, mas sem equivalente maduro a DOTS/Burst/Entities Graphics para escala; ecossistema 3D menor. **Contingência** |
| Stride / engine própria | ecossistema e risco de continuidade |

## Consequências
**Positivas:** uma linguagem em todo o projeto; Burst/Jobs/Entities disponíveis; ecossistema e ferramentas.
**Negativas:** C# 9 no código Unity (dois perfis de padrão); dependência da trajetória comercial e técnica da Unity.
**Riscos:** IPC/Process no IL2CPP (spike S1); mudanças de licenciamento.

## Gatilhos de revisão
Unity 7 estável com C# moderno no player · mudança relevante de licença · falha do spike S1 sem alternativa.

## Referências
Ver `ENGINE_DECISION.md` §Referências.
