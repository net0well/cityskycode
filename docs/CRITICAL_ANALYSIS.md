# Análise Crítica do Projeto City: SkyCode

> **Status:** v1.0 · **Data:** 2026-10-08 · **Fase:** 0 (Pesquisa e Arquitetura)
> **Leitura obrigatória** antes de qualquer outro documento técnico. Este documento existe porque o briefing (`main.md`, §78) pediu explicitamente: *"Não concorde automaticamente comigo. Se uma ideia for ruim, explique."*

---

## 1. Resumo executivo

A visão central é forte e diferenciada: **um city builder onde o código do jogador é a ferramenta de gestão**. Ela tem um "porquê" pedagógico raro: o jogador aprende engenharia de software porque precisa que a cidade funcione.

Porém, o briefing, lido literalmente, descreve **três produtos de grande porte ao mesmo tempo**:

1. um city builder com escopo próximo de *Cities: Skylines* (feito por estúdios de dezenas de pessoas ao longo de anos);
2. uma IDE embarcada (editor, compilador, análise, depuração, testes);
3. uma plataforma de aprendizagem adaptativa (currículo, desafios, validação, dicas, modelo do aluno).

Os cinco maiores riscos, em ordem:

| # | Risco | Por que é grave | Resposta desta documentação |
|---|-------|-----------------|-----------------------------|
| R1 | **Escopo** | Qualquer um dos três produtos acima já basta para consumir uma equipe pequena. | MVP mínimo, roadmap orientado a risco, regra "pertence ao MVP?" (§75). |
| R2 | **Execução segura de C#** | Código do jogador rodando dentro do processo da Unity pode derrubar o jogo (StackOverflow não é capturável) e não tem fronteira de segurança. | Execução **fora do processo** (Code Host .NET 10 + Worker isolado). Ver ADR-0005/0006. |
| R3 | **Performance de renderização em escala** | *Cities: Skylines II* (Unity + HDRP + DOTS) foi lançado com problemas graves de GPU: faltavam LODs, o culling era fraco e as sombras custavam caro. A simulação DOTS escalava bem. | URP, orçamento de geometria/sombras, LOD obrigatório desde o 1º asset. Ver ADR-0002 e `PERFORMANCE.md`. |
| R4 | **Tensão pedagógica sandbox × currículo** | Problemas emergentes da cidade (ex.: trânsito) podem exigir conceitos que o jogador ainda não aprendeu. | Desafios em **cenários controlados (Laboratórios)** e mapeamento problema→conceito com pré-requisitos. Ver `LEARNING_DESIGN.md`. |
| R5 | **Transição tecnológica da Unity** | HDRP em modo manutenção, migração Mono→CoreCLR em curso (Unity 6.8/7.0), "ECS for All" sendo entregue aos poucos. | Domínio independente de engine, travas de versão, revisões de ADR planejadas. |

---

## 2. Onde discordo do briefing (e o que proponho)

Cada item traz **o que foi proposto**, **o problema** e **a recomendação adotada**. As decisões arquiteturalmente relevantes viraram ADRs.

### D1. HDRP como pipeline de renderização → **URP**

- **Proposto:** HDRP para iluminação, volumetria, água e atmosfera.
- **Problema:**
  - A estratégia oficial da Unity para 2026 coloca o **HDRP em manutenção**: sem novas funcionalidades, só estabilidade e correções. A própria Unity *"strongly encourage[s] using URP when starting any new project"*. Toda a evolução visual (céu físico, GI em tempo real, SSR, exposição automática) vai para o URP.
  - Um city builder costuma ter o gargalo na GPU, por causa de geometria, draw calls e sombras. O caso *Cities: Skylines II* (HDRP) é o alerta mais próximo possível.
  - O público é de **estudantes**, muitas vezes com notebooks e GPU integrada. O HDRP exige hardware de alto desempenho.
- **Recomendação:** **URP** (Forward+, GPU Resident Drawer, GPU Occlusion Culling, STP). O visual "estilizado-realista, limpo e legível" do briefing combina melhor com o URP do que com o fotorrealismo do HDRP. Perde-se a água e a volumetria nativas do HDRP, compensadas com shaders próprios ou assets. → **ADR-0002**.

### D2. ECS/DOTS para cidadãos desde o início → **ECS só onde a escala exigir, e não no MVP**

- **Proposto:** ECS para cidadãos, veículos, tráfego, necessidades, consumo e produção.
- **Problema:** o ECS custa bastante em complexidade (baking, subscenes, authoring, depuração mais difícil, API ainda em mudança: Entities migrou para *core package* na 6.4 e removeu APIs como `Entities.ForEach`/Aspects na linha 6.x). Um MVP com 10 a 50 cidadãos não precisa de ECS, e a regra da própria equipe (§31) já diz para "evitar transformar todo o projeto em ECS simplesmente porque ECS existe".
- **Contradição interna do briefing:** "regras de necessidades e felicidade no *Domain C#*" e "necessidades em ECS" não podem valer ao mesmo tempo.
- **Recomendação:**
  - O **Domínio** (C# puro, orientado a dados, com IDs e arrays) é a **autoridade** sobre o estado do cidadão: agenda, necessidades, dinheiro.
  - O **ECS** cuida apenas da **apresentação de agentes em massa** (posição visual, animação, veículos visíveis), a partir da fase de Trânsito.
  - Desde o MVP, os agentes ficam atrás de uma interface (`IAgentPresentation`): primeiro com uma implementação em GameObjects, depois em ECS.
  → **ADR-0003, ADR-0009**.

### D3. Roadmap com Editor C# na Fase 11 → **código desde a Fase 1 ("walking skeleton")**

- **Proposto:** Fases 1–10 são city builder; editor C# (11), execução segura (12) e desafios (13) vêm depois.
- **Problema:** isso contradiz o próprio MVP (§68) e o vertical slice (§69), que exigem código C# funcionando. Pior: deixa para o fim **o componente mais arriscado e mais diferenciador** (código → cidade). Se ele falhar na Fase 11, dez fases de trabalho terão sido construídas sobre uma premissa não validada.
- **Recomendação:** roadmap **orientado a risco**. Na Fase 0 entram spikes técnicos (Code Host, Monaco em webview, orçamento de renderização URP). Na Fase 1 entra um *walking skeleton*: `city.Log("olá")` digitado no editor aparece no console do jogo. O MVP termina com o vertical slice completo. → `DEVELOPMENT_ROADMAP.md` (inclui tabela de mapeamento fase original → nova fase).

### D4. Execução de código no processo da Unity → **processo separado (Code Host)**

- **Problemas técnicos verificados:**
  - **IL2CPP** (AOT) não executa código carregado em runtime. Compilar e carregar assemblies dentro do player obrigaria a usar o backend Mono em produção.
  - `AssemblyLoadContext` (e AppDomain, no Mono) **não é fronteira de segurança**. A orientação da Microsoft é usar isolamento de SO/processo para código não confiável. O CAS (Code Access Security) não existe no .NET moderno.
  - `StackOverflowException` **encerra o processo** no .NET e não pode ser capturada. Uma recursão infinita do aluno derrubaria o jogo inteiro e talvez corrompesse o save.
  - A Unity 6 trava o código em **C# 9 / .NET Standard 2.1**. A própria Unity 7 alpha (out/2026, editor CoreCLR) ainda mira C# 9.
- **Recomendação:**
  - Um **Code Host** em .NET 10 (Roslyn 5.x, C# 14 para o aluno) e um **Sandbox Worker** descartável em processo restrito (Job Object + token restrito/AppContainer no Windows).
  - O jogador escreve contra uma **Player API** que gera **comandos**. Os comandos são validados pela camada Application, com as mesmas regras e custos da UI.
  - Benefícios extras: a Unity pode usar IL2CPP, e o aluno aprende **C# moderno**, não C# 9.
  → **ADR-0005, ADR-0006**.

### D5. `IMapOverlay.GetColor(Vector3 position)` → **overlays calculados em grade**

- **Problemas:**
  1. `Color` e `Vector3` são tipos da Unity, o que viola a regra de domínio independente (§30).
  2. Uma chamada por posição (por pixel, vértice ou prédio) é uma armadilha de performance: com 100 mil prédios, seriam 100 mil chamadas virtuais por atualização, possivelmente via IPC se o overlay for do jogador.
  3. Misturar "calcular o valor" com "escolher a cor" quebra o SRP, justamente o princípio que o jogo quer ensinar.
- **Recomendação:** o overlay produz um **campo escalar em grade** (`OverlayField`, por exemplo células de 32 m), com baixa frequência. A apresentação converte valores em cores (LUT/gradiente no shader). A versão pedagógica continua simples (`double Evaluate(CellInfo cell)`) e ensina interface, polimorfismo e Strategy. → **ADR-0013**, `MAP_OVERLAYS.md`.

### D6. "Código como poder divino" (`house.EnergyConsumption = 5`, `trabalhador.Salario = 2500`) → **código como engenharia**

- **Problema:** se o jogador pode **atribuir** o consumo de energia, basta escrever `= 0` e a crise de energia está "resolvida". Se pode **criar** trabalhadores e fixar salários, a economia vira cheat. O código deixa de ter "consequências reais" (§1) e passa a ser um console de trapaça.
- **Recomendação:**
  - **Grandezas físicas são derivadas**: consumo, custo de manutenção, capacidade real. O jogador altera **parâmetros de projeto** com trade-offs (isolamento, painel solar, número de moradores) e o sistema calcula o resto. Tentar atribuir `EnergyConsumption` gera o erro `CS0200` (propriedade somente leitura), que vira **aula de encapsulamento**.
  - Cidadãos **emergem** da demanda e da migração. O jogador cria **políticas, algoritmos e componentes**, não pessoas nem dinheiro do nada.
  - Toda ação via código **custa o mesmo** que via UI e passa pelas mesmas validações.
  - Em **Laboratórios** (desafios), o jogador implementa o cálculo (`CalculateEnergyConsumption()`), que é validado por testes ocultos.

### D7. "O jogador cria `public class Hospital {}` e um hospital aparece" → **instâncias, tipos-base e registro explícito**

- **Problema pedagógico:** declarar uma classe **não cria nada**. Quem cria é a instância (`new`), que precisa ser colocada no mundo (`city.Build(...)`). Ensinar que "declarar classe = objeto no mundo" constrói um modelo mental errado, justamente no conceito mais fundamental de POO.
- **Problema técnico:** a engine não sabe renderizar nem simular uma classe arbitrária.
- **Recomendação:**
  - Classes do jogador **herdam de tipos-base** da Player API (`ServiceBuilding`, `ResidentialBuilding`) ou **implementam interfaces** de extensão (`IRouteStrategy`, `IMapOverlay`).
  - O visual e a simulação vêm do **arquétipo** do tipo-base. O comportamento sobrescrito roda no Code Host.
  - Instanciar e registrar são passos explícitos e didáticos.

### D8. Felicidade como soma linear (`Housing + Employment + … − Taxes`) → **modelo normalizado, ponderado e explicável**

- **Problema:** somas não normalizadas misturam escalas (impostos em %, trânsito em minutos) e não têm limite. Uma soma também permite que o excesso de uma necessidade compense a falta total de outra (muitos parques compensando falta de moradia).
- **Recomendação:**
  - Cada necessidade tem uma **satisfação de 0 a 1**, com peso por perfil.
  - A agregação combina média ponderada e o **pior fator**, de modo que uma privação severa domine o resultado. Penalidades ambientais têm limite, e há suavização temporal.
  - Toda contribuição fica registrada para responder **"por que meus cidadãos estão infelizes?"** com causas rastreáveis.
  → `HAPPINESS_SYSTEM.md`.

### D9. Frequências em Hz reais (Economy 1 Hz, Traffic 10–20 Hz) → **frequências em tempo de simulação**

- **Problema:** em 10x, a "Economia a 1 Hz" processa a mesma quantidade de dinheiro com 10x menos passos por dia de jogo, ou seja, **o resultado muda com a velocidade**. A simulação precisa ser **determinística e independente da velocidade** para permitir replay, validação e snapshots.
- **Recomendação:** um **tick fixo numerado**, com sistemas agendados em *ticks de simulação* e dois "relógios" derivados: tempo de mobilidade e tempo de calendário. A velocidade só muda quantos ticks rodam por segundo real, nunca o resultado. → **ADR-0008**, `SIMULATION_TICK.md`.

### D10. LOD de simulação baseado na distância da câmera → **LOD de simulação independente da câmera**

- **Proposto (§54–55):** cidadão próximo = simulação detalhada; distante = simplificada; muito distante = estatística.
- **Problema grave:** se a fidelidade da simulação depende da câmera, **a cidade se comporta diferente conforme onde o jogador olha** (efeito observador). O engarrafamento some quando você afasta a câmera. Isso quebra:
  - o determinismo;
  - o replay;
  - a validação de desafios (que roda *headless*, sem câmera);
  - a justiça das comparações entre execuções de código.
- **Recomendação:**
  - **LOD de simulação por estado e atividade**, não por câmera: em viagem / em atividade / agregado.
  - **LOD de apresentação por câmera**: agente animado / simplificado / impostor / não renderizado.
  - O que muda com a câmera é **só o que se vê**, nunca o que acontece.
  → **ADR-0009**, `SIMULATION_LOD.md`.

### D11. Trânsito agente-a-agente como verdade da simulação → **modelo mesoscópico autoritativo + microssimulação visual**

- **Problema:** um tráfego microscópico autoritativo para 100 mil cidadãos é o sistema mais caro e arriscado de um city builder. *Cities: Skylines II* chegou a cerca de 1.200 sistemas DOTS. Ele também amarra o resultado da simulação a detalhes de física e float, o que dificulta o determinismo entre Mono, IL2CPP e .NET 10 (onde rodam os desafios).
- **Recomendação (provisória, a validar na Fase 11):**
  - **Autoritativo:** modelo **mesoscópico** por trecho e faixa (densidade → velocidade → tempo de viagem, com capacidade de interseções e semáforos), em aritmética inteira.
  - **Visual:** veículos em ECS que **representam** os fluxos (param no vermelho, mudam de faixa, formam filas visíveis), sem influenciar o resultado.
  - Os engarrafamentos continuam **visíveis e causais**, porque a fila do modelo meso vira fila visual.
  → **ADR-0010**, `TRAFFIC_SYSTEM.md`.

### D12. NavMesh para pedestres → **grafo próprio; NavMesh opcional para áreas livres**

Concordo com a ressalva do briefing e vou além: em city builders, a navegação acontece **sobre a rede viária e de calçadas** (um grafo), não sobre uma malha livre. O NavMesh só agrega valor em praças e parques abertos, e mesmo assim depois do MVP. → **ADR-0010**.

### D13. Idioma misto na API (`Casa`, `Trabalhador`, `empresa.Contratar` × `House`, `city.Build`) → **API em inglês, conteúdo em pt-BR**

- **Problema:** dois idiomas na mesma API confundem iniciantes e criam inconsistência, além de afastar o aluno do ecossistema real (.NET BCL, NuGet, documentação, mercado).
- **Recomendação:** identificadores em **inglês**. Lições, dicas, mensagens de erro, XML docs, UI e narrativa em **pt-BR** (com localização futura). O glossário pt-BR ↔ código fica em `GLOSSARY.md`. → **ADR-0014**.

### D14. Unidades e valores irreais (casa com "3.2 MW", dinheiro em float)

- Uma casa consome **quilowatts**, não megawatts (3,2 MW abastece centenas de casas). Num jogo **educacional**, ordens de grandeza erradas ensinam errado. → kW para consumidores, MW para usinas.
- Dinheiro nunca em `float`/`double`. Internamente fica em **centavos (`long`)**, e na Player API como `decimal`, o que vira lição: "por que não usar float para dinheiro?".

### D15. `GameObject.Find` proibido (concordo) e Singletons

Concordo integralmente. Acrescento que a `CityManager` "que faz tudo" (§42) é ótima **como vilã pedagógica** (o "código legado da prefeitura" que o aluno refatora), mas **nunca** como arquitetura real do jogo. → Composition root explícito, sem singletons globais sem ADR (§76).

### D16. Plataformas

O briefing não define plataformas, e a decisão é crítica: **compilação de código em runtime não é permitida em consoles** e não funciona em WebGL nem em mobile (AOT/IL2CPP obrigatório, sem processos auxiliares).

**Atualização (ADR-0018):** a meta de longo prazo é rodar "em todos os computadores" e publicar na Steam, mas **primeiro no Windows**. Linux/Steam Deck e macOS vêm depois do lançamento no Windows, e a arquitetura fica portável desde já. "Todos" não inclui **Chromebook**, que não roda apps nativos e perdeu a Steam em jan/2026. Para esse público, só uma futura versão web.

### D17. Nome do produto

"CitySkyCode" lembra **"Cities: Skylines"** (marca da Paradox Interactive). Para um projeto de estudos isso é irrelevante, mas **antes de qualquer lançamento público** é preciso fazer uma busca de marca e possivelmente renomear. Neste repositório, o namespace de código usa `CitySky` como nome provisório.

**Atualização (2026-10-08):** o dono do produto definiu o nome **City: SkyCode**. Com os dois-pontos, o formato ficou **ainda mais próximo** de "Cities: Skylines" ("palavra: Sky…"). Isso aumenta o risco de confusão com a marca da Paradox, sobretudo em lojas (Microsoft Store, Steam), onde os nomes aparecem lado a lado. Recomendação: manter o nome durante o desenvolvimento, fazer a busca de marca (INPI no Brasil; EUIPO/USPTO para lojas internacionais) **antes da Fase 20**, e ter um nome alternativo pronto. Risco R17 atualizado.

### D18. Quantidade de documentos (≈75 arquivos)

- **Risco:** "doc rot", ou seja, documentos de sistemas que só existirão daqui a anos ficam desatualizados e contradizem o código.
- **Recomendação:** cada documento declara **Status, Fase e se pertence ao MVP**. Documentos de sistemas pós-MVP são **intenções de design** curtas e marcadas como tal. O `AI_DEVELOPMENT_GUIDE.md` obriga a atualizar a documentação a cada mudança, e o *Definition of Done* inclui documentação.

---

## 3. Funcionalidades excessivamente complexas (cortar, adiar ou simplificar)

| Funcionalidade | Complexidade | Decisão |
|---|---|---|
| Rotatórias, pontes, túneis, faixas editáveis | Geração de malha de interseções e regras de conexão de faixas são problemas difíceis | **Pós-MVP**. MVP: segmentos retos com interseções simples. |
| Estradas curvas (Splines) | Malha de interseção com curvas, terreno, colisões | Fase 3 (segmentos retos) → Fase 3b (curvas Bézier) |
| 100.000+ cidadãos individuais | Memória, pathfinding, renderização | Escalonado: 50 (MVP) → 1k → 10k → 50k → 100k, cada degrau com perfilamento |
| Mercado com oferta, demanda e preço dinâmico | Instabilidade numérica, balanceamento | MVP com preços fixos. Mercado na Fase de Economia v2, com amortecimento. |
| Água com pressão, contaminação e vazamentos | Hidráulica real é complexa demais | Modelo simplificado de rede (capacidade por componente conexo + "pressão" por distância/elevação) |
| Clima afetando tudo | Explosão combinatória de modificadores | Clima produz **modificadores** com efeitos limitados e documentados |
| Transporte público completo | Linhas, transbordos, escolha modal | Pós-MVP (Fase 19) |
| IntelliSense completo + depurador com breakpoints | Um depurador real para código em sandbox é um projeto à parte | MVP: diagnósticos + autocomplete + hover. Depuração por **traço de execução** e Inspector depois. |
| Replay completo com rebobinar | Snapshots grandes e determinismo estrito | Log de comandos + snapshots periódicos. Rebobinar só em Laboratórios (cenários pequenos). |
| Enchentes, epidemias, greves | Cada uma é um sistema | Eventos como dados (gatilho + modificadores + duração), introduzidos aos poucos |
| Visual "nível Cities: Skylines" | Custo de arte enorme | Direção de arte low-poly estilizada com kits CC0 gratuitos (orçamento zero), variação procedural |

---

## 4. Gargalos e riscos por categoria

> Lista consolidada e mantida em `RISKS.md` (registro vivo). Aqui fica a análise.

### 4.1 Performance
- **Renderização (GPU) é o gargalo mais provável**, não a simulação: geometria sub-pixel, sombras de milhares de objetos pequenos e falta de LOD. *Mitigação:* orçamento por classe de asset, LOD e impostores obrigatórios, filtro de *shadow casters*, GPU Resident Drawer, occlusion culling e linter de assets no CI.
- **Mono no player é mais lento que o .NET 10.** Benchmarks do domínio feitos em .NET (BenchmarkDotNet) são **otimistas**. *Mitigação:* medir também no build IL2CPP; reservar Burst/Jobs para os laços quentes via portas (`IPathfinder`).
- **IPC por tick** (comportamentos do jogador chamados pela simulação). *Mitigação:* chamadas em lote, baixa frequência, orçamento de tempo, latência de 1 tick e *fallback* para a regra padrão.

### 4.2 Segurança
- Código de terceiros: soluções compartilhadas, saves baixados (que **contêm código**) e mods futuros. *Mitigação:* sandbox em camadas (`CODE_EXECUTION_SECURITY.md`) e **nunca executar automaticamente** código vindo de save ou download.
- Webview (Chromium embarcado) amplia a superfície de ataque. *Mitigação:* conteúdo 100% local, navegação externa bloqueada, CSP restritiva.
- LGPD: o público pode incluir **menores**. Telemetria só local por padrão e opt-in explícito (`SECURITY.md`).

### 4.3 Execução de C#
- Inicialização a frio do Roslyn (1–3 s). *Mitigação:* processo quente, pré-aquecimento e compilação incremental.
- Mensagens do compilador são hostis para iniciantes. *Mitigação:* `ERROR_SYSTEM.md` traduz os cerca de 60 diagnósticos mais comuns.
- Determinismo entre runtimes: Mono/IL2CPP (jogo) e CoreCLR (Code Host) podem divergir em ponto flutuante. *Mitigação:* estado autoritativo do domínio em **aritmética inteira / ponto fixo**.

### 4.4 Simulação
- Laços de realimentação instáveis (impostos ↑ → empresas fecham → desemprego → receita ↓ → impostos ↑). *Mitigação:* amortecimento, limites, testes de balanceamento *headless* (cidades roteirizadas por 10 anos de jogo, com faixas de valores esperados).
- "Caixa-preta": o jogador não entende causas. *Mitigação:* explicabilidade como requisito de **todo** sistema (cada métrica registra contribuições).

### 4.5 Pathfinding
- Milhares de buscas por hora de jogo. *Mitigação:* custos inteiros, A* em grafo compacto (CSR), cache por (origem, destino, versão de custo), **Dijkstra multi-fonte** para "serviço mais próximo" (uma busca por tipo de serviço em vez de uma por cidadão), hierarquia por bairro na escala 50k+.
- Invalidação de cache quando o jogador edita ruas. *Mitigação:* versionamento por região do grafo.

### 4.6 Memória
- 100k cidadãos × estado completo cabe facilmente em SoA (~20–40 MB). O risco real está em **objetos gerenciados por entidade** (pressão no GC do Mono/Boehm, que não é geracional). *Mitigação:* stores em arrays de structs, sem `class` por cidadão no domínio, pooling, zero alocação por frame nos caminhos quentes.

### 4.7 Renderização
- Ver 4.1. Acrescento: cidades construídas pelo jogador são **dinâmicas**, então **GI baked não serve**. *Mitigação:* iluminação de céu procedural + SSAO + emissivos noturnos + Forward+ para muitas luzes pequenas, com LOD de luzes.

### 4.8 Arquitetura
- Duplicação entre Domain (Unity) e Domain (Code Host). *Mitigação:* **fonte única** em pacote UPM local, compilado também por um `.csproj` SDK (ver `ARCHITECTURE.md` §5).
- Vazamento de tipos da Unity para o domínio. *Mitigação:* `noEngineReferences: true` nos asmdefs, testes de arquitetura e `/arch-check` do dotnet-claude-kit.
- Convenções de C# moderno (dotnet-claude-kit mira C# 14) aplicadas por engano a código Unity (C# 9). *Mitigação:* dois perfis de padrão de código (`CODING_STANDARDS.md`) e overrides explícitos no `CLAUDE.md`.

### 4.9 Pedagógicos
- **Sobrecarga cognitiva:** gerir a cidade e aprender a programar ao mesmo tempo, com atenção dividida. *Mitigação:* pausa automática ao abrir o editor em missões, Laboratórios isolados e *fading* de andaimes (exemplos resolvidos → completar lacunas → escrever do zero).
- **Dois públicos:** fãs de city builder × iniciantes em programação. *Mitigação:* o modo Campanha guia, o modo Sandbox libera, e o código é sempre **opcional para jogar e necessário para dominar**.
- **"Trapaça didática":** o jogo entregar a solução cedo demais. *Mitigação:* dicas progressivas com custo zero de punição, mas com efeito no modelo de domínio do aluno (`HINT_SYSTEM.md`).
- **Ordem linear do §50 × problemas emergentes.** *Mitigação:* grafo de conceitos (DAG) com pré-requisitos, não uma árvore linear.
- **Herança ensinada × engine que prefere composição:** em vez de esconder a contradição, ela vira **arco narrativo** (POO → limites da herança → composição → orientação a dados), como o próprio briefing intuiu.

### 4.10 Produção
- Equipe presumivelmente pequena (1–3 pessoas + agentes de IA). *Mitigação:* MVP minúsculo e *gates* de playtest com alunos reais antes de expandir.
- Custo de arte com orçamento zero. *Mitigação:* kits CC0 (Kenney, Quaternius) e variação procedural (`ASSET_PIPELINE.md`).
- Opções gratuitas menos maduras (ex.: host do editor). *Mitigação:* sempre atrás de uma interface, com alternativa documentada.

---

## 5. O que mantenho da proposta (concordâncias fundamentadas)

- **Unity sobre Unreal**: C# nativo, ecossistema, ferramentas de editor e o fato de o produto *ser sobre C#*. → ADR-0001.
- **Domínio independente da Unity**: ganhou ainda mais importância, porque o mesmo domínio roda *headless* no Code Host para validar desafios.
- **Arquitetura híbrida** (Domain + GameObjects + ECS onde necessário).
- **Simulação em ticks** desacoplada do frame rate.
- **API educacional de domínio** (nada de `GameObject.Find`).
- **Roslyn** para análise e compilação.
- **LOD de simulação** como conceito, corrigido para ser independente da câmera (D10).
- **Vertical slice primeiro** e "problema primeiro, teoria depois".
- **ADRs** para decisões relevantes e a pergunta "isso pertence ao MVP?".

---

## 6. Premissas assumidas

1. Equipe pequena (1–3 desenvolvedores) assistida por agentes de IA (Claude Code + dotnet-claude-kit).
2. Plataforma: **Windows primeiro**. Linux/Steam Deck e macOS depois do lançamento no Windows (ADR-0018).
3. Público: a partir de ~14 anos, iniciantes em programação; idioma inicial **pt-BR**.
4. Jogo **single-player e offline**. O compartilhamento é **por arquivo**, sem servidor (ver §7).

## 7. Decisões do dono do produto (2026-10-08)

As cinco perguntas em aberto foram respondidas e registradas no [ADR-0017](ADR/0017-product-scope-classroom.md):

| Pergunta | Decisão | Consequência principal |
|---|---|---|
| Hardware mínimo | seguir a recomendação: Iris Xe, 8 GB, 30 FPS no preset Baixo | medir em PCs reais de escola no piloto; Chromebook/Linux não atendidos por enquanto (R21) |
| Uso em sala de aula | **sim** | perfis locais no MVP, lições ≤ 20 min, exportação de progresso (Fase 9), painel do professor local (Fase 13), alinhamento com a norma de Computação da BNCC (`CLASSROOM.md`) |
| Compartilhamento entre jogadores | **sim** | por arquivo `.citysky`, só código-fonte, nunca executa sozinho; exige L5 completo (Fase 9) (`SHARING_SYSTEM.md`) |
| Orçamento | **zero** | sem Vuplex; arte e áudio CC0; ferramentas gratuitas; distribuição pela Microsoft Store (MSIX assinado de graça) |
| Meta | **produto educacional** | LGPD sem coleta de dados, acessibilidade, material do professor, revisão do nome antes do lançamento |
| Plataformas (ADR-0018) | **Windows primeiro**; "todos os computadores" e Steam no futuro | arquitetura portável (interfaces para IPC, sandbox, editor e serviços de plataforma); Linux/Steam Deck e macOS na Fase 21+; Steam Workshop como compartilhamento online; custos futuros: Steam US$ 100 por jogo, Apple ~US$ 99/ano; Chromebook fora |

---

## Referências
Ver `REFERENCES.md` (fontes da pesquisa: estratégia de render pipelines 2026 da Unity, status CoreCLR out/2026, análise técnica de *Cities: Skylines II*, orientação da Microsoft sobre isolamento de código, documentação de AppContainer etc.).
