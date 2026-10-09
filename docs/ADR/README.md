# Architecture Decision Records (ADRs)

> Decisões arquiteturalmente relevantes (§65, §67). Formato: [`TEMPLATE.md`](TEMPLATE.md). Um ADR **aceito** só muda por outro ADR que o **substitua**.

| # | Título | Status | Diverge do briefing? |
|---|---|---|---|
| [0001](0001-engine-selection.md) | Seleção de engine: Unity 6 | Aceito (plataformas revisadas pelo 0018) | não |
| [0002](0002-render-pipeline.md) | Render pipeline: **URP** | Aceito | **sim** (HDRP → URP) |
| [0003](0003-ecs-strategy.md) | Estratégia de ECS: híbrida, ECS só para apresentação de agentes em massa | Aceito | parcialmente |
| [0004](0004-domain-architecture.md) | Arquitetura de domínio: camadas, shared source, orientado a dados | Aceito | adaptação |
| [0005](0005-code-execution.md) | Execução de código: Code Host + Worker fora do processo | Aceito | **sim** (detalhamento) |
| [0006](0006-code-security.md) | Segurança de código: defesa em camadas + sandbox do SO | Aceito | não (aprofunda) |
| [0007](0007-code-editor.md) | Editor: Monaco + serviços Roslyn (host gratuito a definir no S2) | Aceito (revisado pelo 0017 e pelo 0019) | não |
| [0008](0008-simulation-tick.md) | Tick fixo determinístico, dois relógios | Aceito | **sim** (Hz reais → ticks) |
| [0009](0009-citizen-simulation.md) | Cidadão (dados) ≠ agente (apresentação); LOD sem câmera | Aceito | **sim** (LOD por câmera) |
| [0010](0010-pathfinding.md) | Pathfinding em grafo próprio; tráfego meso autoritativo | Aceito (revisão na Fase 11) | parcialmente |
| [0011](0011-road-system.md) | Estradas: grafo no domínio, Splines só na apresentação | Aceito | adaptação |
| [0012](0012-persistence.md) | Persistência: container versionado, só domínio | Aceito | não |
| [0013](0013-map-overlay-system.md) | Overlays em grade, cor na apresentação | Aceito | **sim** (`GetColor(Vector3)`) |
| [0014](0014-player-api-language.md) | API em inglês, conteúdo em pt-BR | Aceito | **sim** (idioma misto) |
| [0015](0015-ai-tooling-dotnet-claude-kit.md) | Ferramental de IA: dotnet-claude-kit | Aceito | adição |
| [0016](0016-test-framework.md) | Framework de testes: NUnit em todo lugar | Aceito | adição |
| [0017](0017-product-scope-classroom.md) | Produto educacional: sala de aula, compartilhamento por arquivo, custo zero | Aceito | adição (decisões do dono do produto) |
| [0018](0018-multiplatform-and-steam.md) | Windows primeiro, arquitetura portável; Linux/Steam Deck e macOS depois; Steam no futuro | Aceito (complementa 0001 e 0007) | adição (decisões do dono do produto) |
| [0019](0019-embedded-editor-panel.md) | Editor embutido no painel lateral direito (minimizar/normal/expandir); janela e tela cheia sem bordas | Aceito (revisa 0007) | adição (decisões do dono do produto + S2b) |
