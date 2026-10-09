# Stack Tecnológica

> **Status:** Aceito (v1.0) · **Data:** 2026-10-08 · **Dono:** Arquitetura
> Regra de ouro (§65 do briefing): nenhuma dependência entra sem responder às **6 perguntas** (seção 6). Mudanças relevantes exigem ADR.

---

## 1. Visão geral (tabela revisada do briefing)

A tabela do briefing foi mantida, com **três correções** marcadas com ⚠️.

| Sistema | Tecnologia | Fase | Observação |
|---|---|---|---|
| Engine | **Unity 6** (6.6 nos spikes → 6.7 LTS) | 0 | ADR-0001 |
| Renderização visual | ⚠️ **URP** (não HDRP) | 0 | HDRP em manutenção. ADR-0002 |
| Prédios especiais / ferramentas / UI | GameObjects | 1 | — |
| Cidadãos (estado) | ⚠️ **Domain C#** (orientado a dados) | 5 | O ECS **apresenta** agentes, não é dono do estado. ADR-0003/0009 |
| Agentes visuais em massa (cidadãos, veículos) | ECS + Entities Graphics | 11 | MVP usa GameObjects atrás de `IAgentPresentation` |
| Tráfego | ⚠️ **Modelo meso no Domain** (autoritativo) + ECS/Jobs/Burst (visual) | 11 | ADR-0010 |
| Simulação econômica e regras | Domain C# | 6 | — |
| Estradas | Grafo próprio no Domain + Unity Splines na apresentação | 3 | ADR-0011 |
| Navegação | Grafo próprio + A*/Dijkstra. AI Navigation só em áreas livres (pós-MVP) | 5 | ADR-0010 |
| Editor C# | Monaco Editor (host gratuito: UnityWebBrowser ou janela PhotinoX sobre WebView2) | 1 | ADR-0007/0017 (spike S2) |
| Compilação / análise | Roslyn (Microsoft.CodeAnalysis 5.x) no **Code Host .NET 10** | 1 | ADR-0005 |
| Execução do código do jogador | **Sandbox Worker** (.NET 10, processo restrito) | 1 (básico) / 9 (endurecido) | ADR-0006 |
| Validação | Roslyn + testes ocultos + simulação headless | 8 | — |
| UI | UI Toolkit | 1 | — |
| Assets | Addressables + LOD (+ impostores) | 10 | MVP sem Addressables (YAGNI) |
| Performance | Burst + Jobs + Profiler + BenchmarkDotNet | contínuo | — |
| Testes | NUnit (Unity Test Framework + NUnit 4 no .NET) | 1 | ADR-0016 |
| Versionamento | Git + Git LFS | 0 | — |
| Distribuição | Microsoft Store (MSIX assinado gratuitamente) + itch.io | 20 | ADR-0017 |
| Arte e áudio | Kits CC0 (Kenney, Quaternius, Poly Haven, ambientCG) | 2 | ADR-0017 |
| Ferramental de IA (.NET) | **Claude Code + dotnet-claude-kit** | 0 | ADR-0015 |

---

## 2. Runtime e linguagens

| Contexto | Runtime | C# | Target | Observação |
|---|---|---|---|---|
| Jogo (Unity, Editor) | Mono (Unity 6.x) | **9.0** | .NET Standard 2.1 | Unity 7 / CoreCLR: reavaliar depois do MVP |
| Jogo (Player Windows) | **IL2CPP** | 9.0 | .NET Standard 2.1 | Possível porque o código do jogador roda fora |
| Domínio compartilhado (Domain/Application/Contracts/Education) | Mono **e** CoreCLR | **9.0** (fixado) | `netstandard2.1` | Mesma fonte compilada pela Unity e pelo SDK .NET |
| Code Host + Sandbox Worker | **.NET 10 LTS** (suporte até nov/2028) | 14 | `net10.0` | .NET 11 (GA nov/2026) é STS: não adotar |
| Código do jogador | .NET 10 (no Worker) | **14** (Roslyn 5.x) | — | O aluno aprende C# moderno |
| Editor Monaco | Chromium (webview) | TypeScript | — | Bundle local, offline |

---

## 3. Tecnologias Unity (avaliação individual, §64)

### Unity 6 — Engine
- **Problema que resolve:** renderização, input, áudio, editor, build multiplataforma.
- **Decisão:** ADR-0001. Versão fixada em `ProjectVersion.txt`. Upgrades só com ADR e branch dedicado.

### URP — Renderização (substitui HDRP)
- **Uso:** Forward+ (muitas luzes urbanas à noite), **GPU Resident Drawer** (instancing automático via BatchRendererGroup), **GPU Occlusion Culling**, SRP Batcher, Render Graph, **STP** (upscaling), SSAO, Decals (sinalização de rua e zoneamento), Volume framework (pós-processamento, ciclo dia/noite).
- **Perdas frente ao HDRP:** sistema de água nativo, nuvens volumétricas, fog volumétrico, céu físico (anunciado para o URP). Compensação: shaders próprios estilizados e assets de terceiros se necessário (com ficha de dependência).
- **Ver:** `GRAPHICS.md`, `LIGHTING.md`, ADR-0002.

### Entities / ECS — Simulação em escala (apresentação de agentes)
- **Uso:** representação visual de milhares de agentes (pedestres e veículos visíveis), microssimulação **visual** de tráfego.
- **Não usar para:** estado autoritativo de cidadãos, economia, regras.
- **Entrada:** Fase 11. **Ver:** ADR-0003, `SIMULATION_LOD.md`.

### Burst — Performance
- **Uso:** jobs de movimento visual, pathfinding acelerado (porta `IPathfinder` com implementação Burst), geração de malha de estradas, cálculo de overlays em grade.
- **Regra:** código Burst usa só tipos *blittable* e custos **inteiros** quando o resultado afeta o domínio (determinismo entre Mono, IL2CPP e .NET 10).

### C# Job System — Paralelismo
- **Uso:** igual ao Burst, mais leitura paralela de snapshots do domínio. O domínio **não** referencia Jobs: a aceleração entra por portas injetadas no composition root.

### Unity.Mathematics — Matemática eficiente
- **Uso:** apresentação, ECS e Burst. **Proibido no Domain** (dependência de engine). O Domain tem tipos próprios (`Fixed`, `Vec2i`, `Bezier2`), com conversores na camada de apresentação.

### Entities Graphics — Renderização em escala
- **Uso:** agentes ECS (Fase 11+). Para prédios e props (GameObjects), o **GPU Resident Drawer** do URP já entrega instancing via BRG.
- **Lição de CS2:** não reinventar o renderer. Se Entities Graphics/GRD não bastar, primeiro perfilar e só depois customizar.

### AI Navigation — Navegação
- **Uso:** apenas em áreas livres (praças, parques) e depois do MVP. **Nunca** um `NavMeshAgent` por cidadão. Ver ADR-0010.

### Splines — Estradas e curvas
- **Uso:** ferramenta de desenho, pré-visualização e extrusão de malha de estradas na apresentação. A geometria autoritativa fica no domínio (Bézier próprio). Ver ADR-0011.

### UI Toolkit — Interface
- **Uso:** HUD, painéis, inspector, gráficos (Painter2D), console, árvore de conhecimento.
- **Limite:** o editor de código é o Monaco em webview (ADR-0007), exibido num painel dedicado.

### Addressables — Gerenciamento de assets
- **Entrada:** Fase 10 (quando o catálogo de prédios crescer). No MVP: referências diretas (YAGNI).
- **Nota:** prefabs de ECS usam o *content management* do Entities (baking/subscenes), não Addressables diretamente.

### Input System
- Pacote oficial, com mapas de ação e rebinding (acessibilidade). Substitui o Input Manager legado.

### Localization (com.unity.localization)
- **Entrada:** Fase 1 (tabelas de strings). O MVP é só pt-BR, mas **nenhuma string fixa no código** desde o início.

### Unity Test Framework (NUnit) — Testes
- EditMode (adaptadores, apresentação) e PlayMode (integração). `com.unity.test-framework.performance` para testes de performance. Ver ADR-0016 e `TESTING.md`.

### Profiler (e família) — Performance
- Unity Profiler, Profile Analyzer, Memory Profiler, Frame Debugger, Burst Inspector. Externos: RenderDoc, PIX, Nsight. .NET: BenchmarkDotNet, dotnet-counters, dotnet-trace. Ver `PROFILING.md`.

### Newtonsoft.Json (com.unity.nuget.newtonsoft-json)
- **Uso:** protocolo IPC, saves no MVP, conteúdo de desafios. **Mesmo serializador nos dois lados** (Unity e Code Host), para evitar divergências.

---

## 4. Tecnologias .NET (fora da Unity)

### .NET 10 SDK / Runtime
- Code Host, Sandbox Worker, testes rápidos do domínio, ferramentas de conteúdo e CI.
- Publicação **self-contained** (win-x64) distribuída junto do jogo. O jogador não precisa instalar .NET.

### Roslyn — Compilação e análise de C#
- **Pacotes:** `Microsoft.CodeAnalysis.CSharp` 5.x (alinhado ao SDK do .NET 10), `Microsoft.CodeAnalysis.CSharp.Features` e `Microsoft.CodeAnalysis.Workspaces.Common` (completion, quick info, signature help, formatação).
- **Uso:** parsing, modelo semântico, analyzers pedagógicos e de segurança (allowlist), emissão de assembly + PDB, serviços de linguagem para o Monaco.
- **Referência de implementação:** RoslynPad (open source) para serviços de editor sobre Roslyn.
- Ver `CODE_EXECUTION.md`, `CODE_ANALYSIS.md`.

### System.Reflection.Metadata
- Verificação do IL emitido (segunda camada da allowlist). Ver `CODE_EXECUTION_SECURITY.md`.

### Microsoft.Extensions.Hosting / DependencyInjection / Logging
- Composição e ciclo de vida do Code Host (padrão .NET, coberto pelos skills do dotnet-claude-kit). **Não** usados dentro da Unity.

### NUnit 4 (.NET)
- Testes de Domain/Application/Contracts/Code Host/Conteúdo, com **modelo de constraints** (`Assert.That`), compatível com o NUnit da Unity. ADR-0016.

### BenchmarkDotNet
- Benchmarks *headless* do domínio. Lembrete: o Mono da Unity é mais lento que o .NET 10, então os números servem como **limite inferior** do custo real.

---

## 5. Editor e webview

### Monaco Editor
- Núcleo de edição do VS Code (MIT). Realce de sintaxe, multicursor, minimapa, acessibilidade.
- Hospedado num host web gratuito (painel do jogo ou janela acoplada) e alimentado por serviços Roslyn do Code Host (ADR-0007/0017).
- Bundle **local** (Vite) em `editor-web/`. Sem CDN, sem rede.

### Host do editor (decisão pendente no spike S2; somente opções gratuitas, ADR-0017)
| Candidato | Prós | Contras |
|---|---|---|
| **UnityWebBrowser** (MIT, CEF) | Gratuito, processo separado, JS↔.NET interop, integrado ao painel do jogo | CEF desatualizado (2.1.1, mar/2024, CEF 122), ~400 MB em repouso, manutenção incerta |
| **WebView2 como janela-filha no painel direito** (Code Host, via PhotinoX 5.3.5 Apache-2.0 ou SDK oficial do WebView2; ADR-0019, validado no S2b) | **WebView2 Evergreen** (segurança atualizada pelo Windows, sem custo), IME e acessibilidade nativos, Chromium fora do processo da Unity, dentro da janela do jogo; S2b: completions p95 17 ms, diagnósticos p95 272 ms, 285 MB | sem tela cheia exclusiva; foco programático exige `MoveFocus`; reposicionar ao mudar tamanho/DPI |
| ~~Janela PhotinoX separada~~ | — | **Descartada** pelo dono do produto (ADR-0019) |
| **Fallback:** editor nativo em UI Toolkit | Sem dependência | Realce e autocomplete limitados, muito trabalho |
| ~~Vuplex 3D WebView~~ | — | **Descartado:** pago (orçamento zero) |

---

## 6. Ficha de dependência (obrigatória, §65)

Toda dependência de terceiros responde a: **(1)** Qual problema resolve? **(2)** É realmente necessária? **(3)** A Unity resolve? **(4)** Impacto de manutenção? **(5)** Impacto de performance? **(6)** Risco de dependência?

| Dependência | (1) Problema | (2) Necessária? | (3) Unity resolve? | (4) Manutenção | (5) Performance | (6) Risco |
|---|---|---|---|---|---|---|
| Roslyn | Compilar/analisar C# do jogador | Sim, é o núcleo | Não (o compilador da Unity é só do editor) | Baixa (Microsoft, MIT) | Cold start 1–3 s, processo quente | Baixo |
| Monaco | Editor de código profissional | Sim, a experiência de IDE é pedagógica | Não | Média (bundle JS) | Webview ~150–400 MB RAM | Baixo (MIT, Microsoft) |
| Host do editor (UWB ou WebView2) | Hospedar o Monaco | Sim, enquanto o editor for web | Não | Média/Alta | Processo Chromium | **Médio**: UWB desatualizado; WebView2 exige janela separada. Atrás de `ICodeEditorHost` |
| Assets CC0 (Kenney, Quaternius, Poly Haven, ambientCG) | Arte e áudio sem orçamento | Sim (orçamento zero) | Não | Baixa | Depende do asset (passa pelo linter) | Baixo (CC0); atenção a marcas de terceiros nos modelos (ex.: cruz vermelha) |
| Newtonsoft.Json | Serialização IPC/saves | Sim | `JsonUtility` é limitado demais (sem dicionários/polimorfismo) | Baixa | Adequada (MessagePack se o IPC virar gargalo) | Baixo (pacote oficial Unity) |
| NUnit | Testes | Sim | Sim (UTF é NUnit) | Baixa | — | Baixo |
| BenchmarkDotNet | Benchmarks | Sim (orçamento de performance) | Não | Baixa | Só dev | Baixo |
| Microsoft.Extensions.* | DI/logging no Code Host | Sim | N/A (fora da Unity) | Baixa | Desprezível | Baixo |

### Rejeitadas ou adiadas (com motivo)
| Tecnologia | Status | Motivo |
|---|---|---|
| HDRP | Rejeitada | ADR-0002 |
| Vuplex 3D WebView e qualquer asset/ferramenta paga | Rejeitada | Orçamento zero (ADR-0017) |
| Certificado comercial de assinatura de código | Rejeitada | Pago; a Microsoft Store assina o MSIX gratuitamente |
| NavMeshAgent por cidadão | Rejeitada | Não escala (ADR-0010) |
| Zenject/Extenject, VContainer | Adiada | Pure DI (composition root manual) basta no MVP. Reavaliar com ADR se a composição ficar dolorosa |
| UniTask | Adiada | Sem necessidade comprovada de async pesado na Unity |
| MessagePack | Adiada | JSON é depurável. Trocar se o perfilamento indicar o IPC como gargalo |
| StreamJsonRpc | Rejeitada | Árvore de dependências pesada para o Mono da Unity. Protocolo próprio mínimo (ver `CODE_EXECUTION.md`) |
| Cinemachine | Adiada | A câmera de estratégia própria é simples. Avaliar para modos cinematográficos |
| FMOD/Wwise | Adiada | O áudio da Unity basta até haver necessidade de áudio adaptativo complexo |
| Netcode/Multiplayer | Fora de escopo | Single-player |
| Steamworks.NET (MIT) | Adiada (Fase 20) | Lançamento na Steam: Cloud, conquistas, Workshop; atrás de `IPlatformServices` (ADR-0018) |
| Builds Linux/macOS | Adiada (Fase 21+) | Windows primeiro (ADR-0018); arquitetura portável desde já |
| MediatR / Mediator libs | Rejeitada | Handlers simples, sem biblioteca (raw handlers) |
| ORMs / bancos | Rejeitada | Saves são arquivos versionados |

---

## 7. Ferramentas de desenvolvimento

| Ferramenta | Uso |
|---|---|
| **Git** + **Git LFS** | Versionamento. LFS para binários (`.fbx .png .psd .tga .exr .wav .ogg .mp4 .ttf .asset` grandes). Asset Serialization = *Force Text*, Visible Meta Files, **UnityYAMLMerge** como merge tool |
| **Claude Code + dotnet-claude-kit** | Assistência de IA para todo trabalho .NET/C#. Ver `AI_DEVELOPMENT_GUIDE.md` e ADR-0015 |
| **CWM.RoslynNavigator** (MCP do dotnet-claude-kit) | Navegação semântica na solução `dotnet/CitySky.slnx` |
| Rider ou Visual Studio 2026 / VS Code + C# Dev Kit | IDEs (Unity e .NET) |
| Microsoft.Unity.Analyzers | Analyzers específicos de Unity |
| GitHub Actions | CI: `dotnet test` sempre; Unity Test Runner (GameCI) nos PRs que tocam Unity |
| Node.js LTS + Vite | Build do bundle do Monaco (`editor-web/`) |

### Instalação do ferramental de IA (obrigatória para agentes)
```bash
dotnet tool install -g CWM.RoslynNavigator
# dentro do Claude Code:
/plugin marketplace add codewithmukesh/dotnet-claude-kit
/plugin install dotnet-claude-kit
```

---

## 7.1 Custo zero e distribuição (ADR-0017)
| Necessidade | Escolha gratuita | Observação |
|---|---|---|
| Engine | Unity Personal | Gratuita enquanto receita + financiamento < US$ 200 mil em 12 meses |
| IDEs | VS Code + C# Dev Kit, Visual Studio Community | Rider é gratuito só para uso não comercial; evitar, porque o produto pode ter receita |
| Modelagem 3D | Blender | — |
| Arte 3D | **Kenney** (City Kits: Roads, Commercial, Suburban, Industrial; CC0), **Quaternius** (personagens e natureza, CC0), assets gratuitos da Asset Store com licença compatível | Estilo low-poly estilizado coerente entre os kits |
| Texturas e céu | **Poly Haven**, **ambientCG** (CC0) | — |
| Áudio | Kenney Audio (CC0), Freesound filtrado por CC0 | — |
| Fontes | OFL (ex.: Inter, JetBrains Mono) | — |
| CI | GitHub Actions + GameCI (licença Unity Personal) | Minutos ilimitados em repositório público; limitados em privado |
| Assinatura e distribuição | **Microsoft Store** (cadastro individual gratuito desde set/2025; o MSIX é assinado pela Microsoft) + itch.io (secundário, sem assinatura) | Instalação por usuário, sem admin; escolas evitam bloqueio do SmartScreen/Smart App Control |

> Se o repositório for público, **não** versionar assets da Asset Store (a licença da Store não permite redistribuição). Assets CC0 podem ser versionados.

## 8. Matriz de versões (fixar na Fase 1)

| Item | Versão-alvo | Onde fixar |
|---|---|---|
| Unity | 6.7 LTS (`6000.7.x`) | `ProjectSettings/ProjectVersion.txt` |
| URP | o da versão da Unity (17.x) | `Packages/manifest.json` |
| Entities / Collections / Mathematics / Entities Graphics | *core packages* da versão Unity | embutidos no editor (6.4+) |
| .NET SDK | 10.0.x (última *patch*) | `dotnet/global.json` |
| Microsoft.CodeAnalysis.* | 5.x | `dotnet/Directory.Packages.props` (Central Package Management) |
| NUnit | 4.x (.NET) / UTF (Unity) | CPM / `manifest.json` |
| Monaco | última estável | `editor-web/package-lock.json` |

> **Regra do dotnet-claude-kit (`packages`):** sempre usar a última versão **estável** verificada no NuGet, nunca uma versão "de memória". Exceção: pacotes consumidos pela Unity seguem a compatibilidade com `netstandard2.1`.
