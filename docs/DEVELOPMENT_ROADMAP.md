# Roadmap de Desenvolvimento

> **Status:** v1.0 · **Data:** 2026-10-08 · **Fase atual:** **0, Pesquisa e Arquitetura**
> Princípios: **orientado a risco** (o mais arriscado e diferenciador primeiro), **walking skeleton** (código → cidade funcionando desde a Fase 1), **gates** de saída mensuráveis e **conteúdo pedagógico junto com cada sistema**.
> **Plano detalhado de cada fase** (entrada, escopo, backlog com IDs `Fn-xx`, gate, riscos): [`docs/phases/`](phases/README.md). Este documento é o resumo; em caso de divergência, vale o plano da fase.

---

## 1. Por que o roadmap original foi reordenado

O roadmap do briefing (§70) colocava Editor C# na Fase 11, Execução segura na 12 e Desafios na 13. Isso **contradiz o MVP** (§68), que exige código funcionando, e adia o maior risco técnico e de produto. Ver `CRITICAL_ANALYSIS.md` (D3).

### Mapeamento fase original → nova fase
| Original | Nova | Comentário |
|---|---|---|
| 0 Pesquisa e arquitetura | **0** | + spikes técnicos obrigatórios |
| 1 Protótipo gráfico | **2** | depois da fundação |
| 2 Mapa e câmera | **2** | unida ao protótipo gráfico |
| 3 Estradas | **3** | MVP: segmentos retos |
| 4 Construções | **4** | via UI **e** via código |
| 5 Primeiros cidadãos | **5** | — |
| 6 Economia | **6** | — |
| 7 Energia e água | **7** (energia) / **12** (água) | água fora do MVP |
| 8 Trânsito | **11** | entra junto com o ECS |
| 9 Felicidade e necessidades | **7** | junto com energia (MVP) |
| 10 Problemas urbanos | **13** | v0 no MVP (apagão) |
| 11 Editor C# | **1** (skeleton) → **8** (completo) | antecipado |
| 12 Execução segura | **1** (básico) → **9** (endurecido) | antecipado |
| 13 Desafios | **8** | antecipado |
| 14 POO | **14** | — |
| 15 SOLID | **15** | — |
| 16 Design Patterns | **16** | — |
| 17 Testes | **17** | — |
| 18 Otimização ECS/DOTS | **11** (introdução) + **18** (escala) | — |
| 19 Conteúdo | **19** | + transporte público, clima, eventos |
| 20 Polimento | **20** | — |

---

## 2. Visão em blocos

```
BLOCO A — Fundação e risco        Fases 0–1
BLOCO B — Vertical Slice (MVP)    Fases 2–8   ──► GATE: Playtest do MVP
BLOCO C — City builder real       Fases 9–13
BLOCO D — Engenharia de software  Fases 14–17 (em paralelo ao conteúdo de C)
BLOCO E — Escala e lançamento     Fases 18–20
```

---

## 3. Fases

### Fase 0 — Pesquisa, arquitetura e spikes ⟵ **atual** · [plano](phases/F00-research-and-spikes.md)
**Objetivo:** reduzir os riscos que podem invalidar a arquitetura.
**Entregáveis:** esta documentação; ADRs 0001–0018; planos das fases (`docs/phases/`); spikes S1–S7 executados e com o resultado preenchido no próprio plano (índice, ordem e pré-requisitos em [`docs/spikes/README.md`](spikes/README.md)).

| Spike | Pergunta a responder | Critério de sucesso | Planos |
|---|---|---|---|
| **S1** Code Host + IPC + IL2CPP | Um player IL2CPP Windows inicia um processo .NET 10, troca mensagens (named pipe ou TCP loopback) e recebe comandos compilados via Roslyn? | Ida e volta ≤ 50 ms para mensagem pequena; compilar e executar "hello" ≤ 1,5 s quente | [S1a](spikes/S1a-codehost-pipeline.md) · [S1b](spikes/S1b-il2cpp-ipc.md) |
| **S2** Host do Monaco (gratuito) | UnityWebBrowser × janela PhotinoX acoplada (WebView2 no Windows) × editor nativo: Monaco carrega offline, recebe teclado e IME (acentos!), mostra diagnósticos vindos da Unity? | Digitação sem perda de teclas, acentos ok, ≤ 400 MB RAM, foco estável, versão do Chromium aceitável | [S2a](spikes/S2a-unitywebbrowser.md) · [S2b](spikes/S2b-photinox-monaco.md) |
| **S3** Orçamento de renderização URP | 5.000 prédios low-poly + 20.000 árvores + 1.000 agentes com GRD + occlusion culling em GPU integrada? | ≥ 30 FPS em Iris Xe a 1080p preset Baixo | [S3](spikes/S3-urp-render-budget.md) |
| **S4** Domínio headless | 100k cidadãos em SoA com agenda e necessidades em baldes: custo por tick no .NET 10 **e** no Mono da Unity? | ≤ 1 ms/tick (.NET); medir fator Mono; hash igual entre runtimes | [S4a](spikes/S4a-headless-domain.md) · [S4b](spikes/S4b-mono-il2cpp-factor.md) |
| **S5** Malha de estradas | Gerar malha de segmentos retos + interseção de 4 ramos + calçadas, em runtime, sem artefatos? | 100 interseções geradas em ≤ 50 ms | [S5](spikes/S5-road-mesh.md) |
| **S6** Sandbox Windows | Worker num Job Object (limites de memória/CPU/processos) + token restrito ou AppContainer: o .NET 10 roda? Bloqueia rede e escrita em disco? | Corpus de ataque básico bloqueado | [S6](spikes/S6-sandbox-windows.md) |
| **S7** Distribuição gratuita (ADR-0017) | O jogo + Code Host + Worker empacotados como **MSIX** instalam por usuário (sem admin), iniciam os processos auxiliares, e o Worker ainda roda em AppContainer/Job Object a partir de um app empacotado? Publicação de teste na Microsoft Store e leitura da política de código dinâmico (10.2.2) | Instalação sem admin em Windows 10/11; S1 e S6 continuam passando dentro do pacote | [S7](spikes/S7-msix-store.md) |

**Gate de saída:** S1, S2, S6 e S7 resolvidos (ou com alternativa aceita em ADR). S3, S4 e S5 não bloqueiam a Fase 1, mas precisam terminar antes das fases que dependem deles (S5 → F3, S3 → gate da F2, S4 → F5).
**Decisões pendentes do dono do produto** (afetam a Fase 1): repositório público ou privado, licença do código, conta dona do repositório. Ver o plano da Fase 0, §5.3.

### Fase 1 — Fundação (walking skeleton) · [plano](phases/F01-foundation.md)
- Repositório, `.gitattributes` (LFS), `.gitignore`, CI (dotnet test + Unity tests no Windows; dotnet test também num runner Linux como canário de portabilidade, ADR-0018), `CitySky.slnx`, asmdefs, pacote `com.citysky.core`.
- Interfaces de plataforma desde o início: `IIpcTransport`, `ISandboxLauncher`, `ICodeEditorHost`, `IPlatformServices`, `IFileLocations` (só implementações Windows por enquanto).
- Composition root, `SimulationClock` + `Scheduler` com 1 sistema fictício, `CommandQueue`, eventos e read models mínimos.
- Code Host + Worker básico (Job Object), protocolo `Contracts` v1.
- Monaco mínimo + console. **Marco:** `city.Log("Olá")` digitado no jogo aparece no console.
- Save/Load do esqueleto com `schemaVersion`.
- **Gate:** teste de determinismo (hash) passando em Mono e .NET 10.

### Fase 2 — Mundo: terreno, câmera, iluminação, seleção · [plano](phases/F02-world.md)
- Terreno 2×2 km, câmera de estratégia, ciclo dia/noite, direção de arte inicial (paleta, materiais).
- Seleção e Inspector genérico. Grade de análise (32 m).
- **Gate:** orçamento de FPS do S3 mantido na cena real.

### Fase 3 — Estradas (MVP) · [plano](phases/F03-roads.md)
- Grafo viário no domínio, ferramenta de desenho, malha (segmentos retos, interseções), lotes, demolição, custos.
- Grafo de calçadas para pedestres.

### Fase 4 — Construções · [plano](phases/F04-buildings.md)
- Catálogo (House, Shop, PowerPlant, CityHall), construção via UI **e** via `city.Build` (Player API v0.1), reforma, demolição.
- **Marco do slice:** "a casa aparece" a partir de código.

### Fase 5 — Primeiros cidadãos · [plano](phases/F05-citizens.md)
- Domicílios, chegada e saída, agenda diária, viagens a pé (A*), agentes GameObject atrás de `IAgentPresentation`, seguir cidadão.

### Fase 6 — Economia mínima · [plano](phases/F06-economy.md)
- Ledger de partidas dobradas, salários, compras, impostos, orçamento, painel financeiro. Invariante de conservação testada.

### Fase 7 — Energia, felicidade e saúde da cidade · [plano](phases/F07-energy-happiness.md)
- Rede elétrica (componente conexo, corte por prioridade), necessidades, felicidade explicável, termômetro v0, overlays Energia/Felicidade, apagão como problema v0.
- Balancear a capacidade da usina para que a crise de energia das lições aconteça (tarefa F7-07: o valor ilustrativo de 400 kW é grande demais para 50 cidadãos).

### Fase 8 — Aprendizado do MVP · [plano](phases/F08-mvp-learning.md)
- Pipeline completo de validação (5 níveis), `ERROR_SYSTEM` (~60 diagnósticos), dicas, 7 lições + desafio final, painel de lições, progresso.
- **Sala de aula (mínimo):** perfis locais por aluno no mesmo PC, lições de até 20 min (`CLASSROOM.md`).
- **🚩 GATE DO MVP:** critérios de aceite do `MVP.md` + **playtest em sala de aula real com 5–10 alunos iniciantes**, em PCs da escola. Sem passar no gate, não se avança para o Bloco C: itera-se sobre o slice.

### Fase 9 — Execução segura endurecida + replay + compartilhamento · [plano](phases/F09-hardened-sandbox-replay-sharing.md)
- AppContainer/token restrito, corpus de segurança ampliado, *fuzzing* do protocolo.
- Log de comandos + snapshots, comparar execuções em Laboratórios.
- Comportamentos do jogador chamados pela simulação (IPC em lote).
- **Compartilhamento por arquivo** `.citysky` (código, cidade, solução), sem execução automática, **só depois** do L5 completo e do corpus de pacotes maliciosos (`SHARING_SYSTEM.md`).
- **Exportação de progresso** do aluno para o professor (`CLASSROOM.md`).
- Pós-MVP imediato: debug "Show Electricity Network"; realce semântico, formatação e traço de execução no editor; início do Módulo M3 (condicionais) com a `IEnergyPolicy`.

### Fase 10 — Zoneamento e crescimento · [plano](phases/F10-zoning-growth.md)
- Zonas (R/C/I/Office), demanda RCI, construção privada automática, valor do terreno, Addressables para o catálogo.

### Fase 11 — Trânsito v1 + introdução de ECS · [plano](phases/F11-traffic-ecs.md)
- Veículos, modelo mesoscópico autoritativo, semáforos, `CitySky.Agents.Ecs` (pedestres e veículos visuais), Entities Graphics, pathfinding acelerado (Burst) atrás de `IPathfinder`.
- **Spike S8:** meso autoritativo × micro autoritativo (ADR-0010 revisado). **Spike S9:** custo da ponte domínio → ECS.
- Escala alvo: **1.000 → 10.000 cidadãos**.

### Fase 12 — Água, esgoto, lixo e poluição · [plano](phases/F12-utilities.md)
- Redes de água e esgoto, coleta de lixo (veículos de serviço), campos de poluição (ar, solo, ruído).

### Fase 13 — Serviços públicos, problemas urbanos e painel do professor · [plano](phases/F13-services-problems-teacher.md)
- Saúde, educação (com faixas etárias), polícia, bombeiros, cobertura por Dijkstra multi-fonte, sistema de problemas completo, problemas → missões.
- **Painel do professor local** (importa os arquivos de progresso da turma), **pacote de turma** (ritmo e liberações) e **pacotes de desafio do professor** (`CLASSROOM.md`).

### Fase 14 — Currículo POO · [plano](phases/F14-oop-curriculum.md)
- Encapsulamento, composição, herança, polimorfismo e interfaces, com missões nascidas dos sistemas das Fases 10–13 (ex.: tipos de veículos de serviço).

### Fase 15 — SOLID · [plano](phases/F15-solid.md)
- "Código legado da prefeitura" (`CityManager`) para refatorar; validações de arquitetura via Roslyn.

### Fase 16 — Design Patterns · [plano](phases/F16-design-patterns.md)
- Strategy, Observer, Command, State, Factory, Builder, Specification, Repository e Mediator, cada um nascido de uma dor real (`DESIGN_PATTERNS.md`).

### Fase 17 — Testes (do jogador) · [plano](phases/F17-player-testing.md)
- `[Test]` no código do jogador, desafios "escreva o teste que pega o bug", validação por mutantes.

### Fase 18 — Escala · [plano](phases/F18-scale.md)
- Simulação em thread dedicada (spike S10), LOD de apresentação completo, impostores, hierarquia de pathfinding.
- Metas: **50.000 → 100.000+ cidadãos**.

### Fase 19 — Conteúdo · [plano](phases/F19-content.md)
- Transporte público, clima, eventos dinâmicos, mais lições, cenários, conquistas.

### Fase 20 — Polimento, piloto escolar e lançamento · [plano](phases/F20-polish-launch.md)
- Acessibilidade, localização (en), áudio adaptativo, onboarding, desempenho final, revisão de marca/nome.
- **Piloto escolar** com turma real; **material do professor**; mapeamento à norma de Computação complementar à BNCC (Resolução CNE/CEB nº 1/2022).
- Publicação **Windows**: **Microsoft Store** (MSIX assinado gratuitamente) e itch.io para escolas; **Steam** (Steamworks.NET: Cloud, conquistas, Workshop) para o público em casa. Steam Direct: US$ 100, devolvidos após US$ 1.000 em vendas (ADR-0018).

### Fase 21+ — Outras plataformas (ADR-0018; reavaliar após o lançamento no Windows) · [plano](phases/F21-other-platforms.md)
- **Linux / Steam Deck** (build nativo, sem Proton): sandbox L5 com `no_new_privs` + seccomp-bpf + Landlock (spike S12), IPC por Unix domain socket, host do editor dentro do jogo (janelas separadas são ruins no Gaming Mode), texto ≥ 12 px a 1280×800, Steam Input. Prioridade por causa de laboratórios escolares e do Steam Deck.
- **macOS:** Seatbelt + hardened runtime + notarização (Apple Developer ~US$ 99/ano, decisão do dono do produto).
- **Chromebook:** só com uma versão web (pesquisa: Roslyn em WebAssembly + sandbox do navegador).

---

## 4. Escala por fase
| Marco | Cidadãos | Agentes visíveis | Tecnologia de agentes |
|---|---:|---:|---|
| MVP (Fase 8) | 50 | 50 | GameObjects |
| Fase 11 | 10.000 | ~2.000 | ECS |
| Fase 18 | 100.000+ | ~10.000 (limite) | ECS + LOD de apresentação |

## 5. Trilhas contínuas (todas as fases)
- **Documentação:** cada PR atualiza o documento do sistema afetado.
- **Performance:** benchmarks no CI com limiares (`PERFORMANCE.md`).
- **Segurança:** corpus de ataque cresce a cada novo recurso da Player API.
- **Pedagogia:** cada sistema novo traz pelo menos 1 problema → conceito mapeado.
- **Sala de aula** (ADR-0017): cada fase verifica se as lições cabem em 20 min, se o jogo roda no preset Baixo em PC de escola e se nada exige internet, admin ou conta.
- **Custo zero:** nenhuma dependência paga entra; assets passam por registro de licença (CC0 ou compatível).
- **Portabilidade** (ADR-0018): nenhuma API exclusiva do Windows fora das implementações de plataforma; o analisador CA1416 do .NET aponta violações no código .NET.
- **IA:** fluxo dotnet-claude-kit `/spec → /plan → /tdd → /verify → /code-review` para trabalho .NET (ver `AI_DEVELOPMENT_GUIDE.md`).
