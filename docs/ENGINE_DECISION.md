# Decisão de Engine

> **Status:** Aceito · **Data:** 2026-10-08 · **ADR:** [ADR-0001](ADR/0001-engine-selection.md) · **MVP:** sim

## 1. Contexto

O CitySkyCode não é "um city builder que também tem código". É um **simulador urbano programável em C#**. A engine precisa, em ordem de importância:

1. conviver bem com **C#**: o domínio, a API do jogador e as ferramentas são todos C#;
2. permitir **integrar compilação e análise de C#** (Roslyn) e execução segura do código do jogador;
3. suportar **simulação em larga escala** (dezenas de milhares de agentes);
4. renderizar **cidades 3D grandes** com boa performance em hardware modesto;
5. oferecer **UI de software profissional** (painéis, inspector, gráficos, editor de código);
6. ter **ferramentas de editor** e um ecossistema que reduzam o custo de uma equipe pequena.

## 2. Critérios e pesos

| Critério | Peso | Justificativa |
|---|---:|---|
| Integração com C#/.NET e Roslyn | 25 | Núcleo do produto |
| Simulação em escala (ECS/Jobs/paralelismo) | 15 | Cidades grandes |
| Renderização de cidades (instancing, LOD, culling) | 15 | Visual de city builder |
| Produtividade da equipe pequena / ferramentas | 15 | Equipe enxuta + IA |
| UI de aplicação (painéis, gráficos, editor) | 10 | Camada de engenharia de software |
| Ecossistema (assets, documentação, comunidade) | 10 | Custo de arte e de aprendizado |
| Riscos de plataforma / negócio / licença | 10 | Sustentabilidade |

## 3. Unity 6

| Tecnologia | Avaliação para este projeto |
|---|---|
| **C# nativo** | Toda a lógica de jogo é C#. O domínio pode ser compartilhado como código-fonte com um processo .NET 10 (Code Host). **Limitação:** a Unity 6 usa **C# 9 / .NET Standard 2.1** (Mono; IL2CPP no player). A migração para CoreCLR está em curso (players experimentais na 6.7 LTS; editor CoreCLR na Unity 7 alpha, ainda em C# 9). C# 14/.NET 10 ainda sem data. |
| **ECS (Entities)** | Maduro o bastante para agentes em massa (passou a *core package* na 6.4). As APIs ainda mudam (iniciativa "ECS for All", EntityId de 8 bytes na 6.5). Usar com moderação e atrás de abstrações. |
| **DOTS / Burst / Jobs** | Burst + Jobs dão paralelismo e SIMD com segurança de memória. É o diferencial frente às alternativas para o processamento de agentes e de grafos. |
| **Entities Graphics** | Instancing e LOD em escala via BatchRendererGroup. Lição de *Cities: Skylines II*: não usar Entities Graphics e escrever um renderer próprio (com culling e LOD fracos) custou caro. |
| **HDRP** | Alta fidelidade, mas **em modo manutenção** desde a estratégia de 2026 da Unity. **Não escolhido**, ver ADR-0002 (URP). |
| **URP** | Pipeline com investimento ativo: Forward+, GPU Resident Drawer, GPU Occlusion Culling, Render Graph, APV, STP. Céu físico, GI em tempo real e SSR estão anunciados. Roda de GPU integrada a GPU dedicada. |
| **AI Navigation** | Útil apenas para áreas livres. A navegação urbana usa um grafo próprio (ADR-0010). |
| **Splines** | Bom para as **ferramentas de edição** e a geração de malha de estradas na camada de apresentação. O domínio guarda a geometria em tipos próprios (ADR-0011). |
| **UI Toolkit** | UI retida e estilizável (USS), boa para painéis, inspector e gráficos. Hospeda o editor Monaco via webview (ADR-0007). |
| **Terrain** | Suficiente para o MVP (heightmap, pintura, árvores instanciadas). Adaptar as estradas ao relevo exige trabalho próprio. |
| **Ferramentas de editor** | Inspector customizável, scriptable tools, Profiler, Frame Debugger, Memory Profiler, Test Framework. |
| **Ecossistema** | Asset Store enorme, muita documentação, comunidade de city builders. |
| **Licença/negócio** | Personal gratuito até US$ 200 mil de receita+financiamento em 12 meses. Pro ≈ US$ 2.310/assento/ano (2026). A taxa de runtime foi cancelada em 2024. Histórico de mudanças de preço é um risco a monitorar. |

## 4. Unreal Engine 5

| Aspecto | Avaliação para este projeto |
|---|---|
| **Qualidade gráfica** | Superior "de fábrica" (Lumen, Nanite, Virtual Shadow Maps). Nanite combina muito bem com geometria densa de cidades. |
| **C++** | Linguagem principal. Toda a camada de jogo ficaria em C++, enquanto o produto ensina C#: dissonância total entre o que o time escreve e o que o jogador aprende. |
| **Blueprints** | Ótimo para prototipar, mas é programação visual. Não ajuda a ensinar C# e cria uma terceira "linguagem" no projeto. |
| **Execução de C#** | Não há suporte oficial. Exigiria embutir o runtime .NET (hosting de CoreCLR via `hostfxr`) ou um plugin de terceiros (ex.: UnrealSharp, comunitário), com interop C++↔C# para todo o domínio. Alternativa: Code Host externo, mas então **todo o domínio** precisaria existir também em C++ ou ser replicado via IPC. |
| **Complexidade** | Curva de aprendizado e tempo de build maiores, além de projetos pesados. Inadequado para uma equipe pequena focada em ferramentas educacionais. |
| **Integração externa** | Possível (processos, sockets), mas sem o benefício de compartilhar código-fonte do domínio com o Code Host. |
| **Mass Entity** | O framework ECS da Unreal (Mass) atende multidões, mas em C++. |
| **Custo arquitetural** | Muito alto: duas linguagens, duas implementações de regras de negócio, ou uma camada de interop complexa. |
| **Licença** | Royalty de 5% acima de US$ 1 milhão de receita bruta por produto (modelo vigente). |

## 5. Alternativas consideradas brevemente

| Engine | Ponto forte | Por que não |
|---|---|---|
| **Godot 4 (.NET)** | Roda sobre .NET moderno (CoreCLR): Roslyn e `AssemblyLoadContext` funcionam no processo. Open source (MIT). | Sem equivalente maduro a DOTS/Burst/Entities Graphics para dezenas de milhares de agentes. Ecossistema de assets 3D e ferramentas de renderização em escala menor. Exportação C# para web/mobile limitada. Continua sendo a **alternativa de contingência**, e o domínio independente torna a portabilidade viável. |
| **Stride (C#)** | C# moderno ponta a ponta | Comunidade e manutenção pequenas, ecossistema limitado, risco alto de continuidade. |
| **Engine própria (Silk.NET/Veldrid)** | Controle total | Custo proibitivo para a equipe. |

## 6. Matriz de pontuação (0–5, ponderada)

| Critério (peso) | Unity 6 | Unreal 5 | Godot 4 .NET |
|---|---:|---:|---:|
| C#/.NET + Roslyn (25) | 4 | 1 | 5 |
| Simulação em escala (15) | 5 | 4 | 2 |
| Renderização de cidades (15) | 4 | 5 | 3 |
| Produtividade da equipe (15) | 4 | 2 | 4 |
| UI de aplicação (10) | 4 | 3 | 4 |
| Ecossistema (10) | 5 | 4 | 3 |
| Riscos (10) | 3 | 4 | 4 |
| **Total ponderado (máx. 500)** | **415** | **315** | **365** |

> Godot pontua alto em C#, mas perde exatamente onde a escala da cidade exige. O placar de Unity em "C#/.NET" é 4 e não 5 por causa do C# 9 e da impossibilidade de executar código dinâmico no IL2CPP. **Ambos os problemas são resolvidos pelo Code Host fora do processo (ADR-0005)**, o que torna a Unity a melhor combinação.

## 7. Decisão

**Unity 6 com URP**, arquitetura híbrida (Domain C# independente + GameObjects + ECS onde a escala exigir) e um **Code Host .NET 10** separado para compilar e executar o código do jogador.

### Versão
- **Fase 0 (spikes):** Unity **6.6** (`6000.6.x`, *Update release* atual).
- **A partir da Fase 1:** migrar para **Unity 6.7 LTS** assim que for lançada (está em beta em out/2026). Fixar a versão exata em `ProjectSettings/ProjectVersion.txt`.
- **Por que não 6.3 LTS:** Entities virou *core package* na 6.4 e `InstanceID` deu lugar a `EntityId` na 6.5. Começar na 6.3 significaria fazer essas migrações depois.
- **Unity 7** (editor CoreCLR, beta prevista para dez/2026 e lançamento no 1º tri/2027): **não depender**. Reavaliar após o MVP (revisão do ADR-0001), sobretudo pela chance de C# moderno no domínio.

### Plataforma (revisada pelo [ADR-0018](ADR/0018-multiplatform-and-steam.md))
- **Windows primeiro** (x64; ARM64 se barato): MVP, piloto escolar e primeiro lançamento (Microsoft Store, itch.io, Steam).
- **Depois do lançamento no Windows (Fase 21+):** Linux x64 (inclui Steam Deck, com build nativo) e macOS (Apple Silicon e Intel). A Unity e o .NET 10 suportam os três. Cada sistema terá o seu sandbox, e o macOS exige notarização (~US$ 99/ano).
- A **arquitetura é portável desde já**: o que é específico de sistema fica atrás de interfaces (ADR-0018).
- **Fora de escopo:** consoles (execução de código gerado em runtime não é permitida), WebGL e mobile (AOT obrigatório, sem processo auxiliar), e Chromebook (sem apps nativos; Steam para Chromebook encerrada em jan/2026). Uma versão web é pesquisa futura.

### Backend de scripting do player
- **IL2CPP** (desempenho), possível porque o código do jogador **não** roda no processo da Unity.
- Requer validar no spike S1 que `System.Diagnostics.Process` e o transporte de IPC funcionam no IL2CPP Windows. Se o IPC falhar, cair para TCP em loopback.

## 8. Consequências

**Positivas**
- O time escreve C# (Unity) e C# (.NET 10), e o jogador aprende C# moderno: uma linguagem só.
- O domínio compartilhado roda no jogo e no Code Host (validação *headless* de desafios).
- Burst/Jobs/Entities ficam disponíveis quando a escala exigir.

**Negativas**
- C# 9 no código Unity, que exige dois perfis de padrão de código (`CODING_STANDARDS.md`).
- Dependência da trajetória da Unity (preços, CoreCLR, ECS). Mitigação: domínio sem dependência de engine.
- O visual fica abaixo do que Unreal entrega "de fábrica". Mitigação: direção de arte estilizada.

## 9. Gatilhos de revisão
- Lançamento da Unity 7 estável com C# moderno no player.
- Mudança relevante de licenciamento da Unity.
- Spike S1 (IPC/Process em IL2CPP) falhar sem alternativa.

## Referências
- Unity — *Render Pipelines strategy for 2026*: https://unity.com/topics/render-pipelines-strategy-for-2026
- Unity — *CoreCLR, Scripting, and .NET Update, October 2026*: https://discussions.unity.com/t/coreclr-scripting-and-net-update-october-2026/1738338
- Unity Manual — C# compiler (C# 9): https://docs.unity3d.com/6000.0/Documentation/Manual/csharp-compiler.html
- Unity — ECS Development Status (dez/2025; 6.6): https://discussions.unity.com/t/ecs-development-status-december-2025/1699284
- Análise técnica de *Cities: Skylines II* (renderer próprio, sem Entities Graphics): https://blog.paavo.me/cities-skylines-2-performance/
- Unity — preços 2026: https://unity.com/pricing
