# Documentação do City: SkyCode

> **Fase atual:** 0, Pesquisa e Arquitetura · **Atualizado:** 2026-10-08
> Briefing original: [`/main.md`](../main.md). As decisões que **divergem** do briefing estão justificadas em [`CRITICAL_ANALYSIS.md`](CRITICAL_ANALYSIS.md).

## Comece por aqui
| Ordem | Documento | Para quê |
|---|---|---|
| 1 | [CRITICAL_ANALYSIS.md](CRITICAL_ANALYSIS.md) | riscos, discordâncias com o briefing e recomendações |
| 2 | [VISION.md](VISION.md) | o que é o produto e por quê |
| 3 | [PROJECT_ARCHITECTURE.md](PROJECT_ARCHITECTURE.md) | **diagrama geral** em uma página |
| 4 | [ARCHITECTURE.md](ARCHITECTURE.md) | camadas, processos, assemblies, regras |
| 5 | [TECH_STACK.md](TECH_STACK.md) | **stack completa** e fichas de dependência |
| 6 | [MVP.md](MVP.md) | **escopo exato** da primeira versão |
| 7 | [DEVELOPMENT_ROADMAP.md](DEVELOPMENT_ROADMAP.md) | **todas as fases** (reordenadas por risco) |
| 8 | [phases/README.md](phases/README.md) | **plano de cada fase**: backlog com IDs, gates, riscos, estimativas |
| 9 | [spikes/README.md](spikes/README.md) | **planos dos 10 spikes da Fase 0** (o próximo passo prático) |
| 10 | [AI_DEVELOPMENT_GUIDE.md](AI_DEVELOPMENT_GUIDE.md) | regras para IAs + **dotnet-claude-kit** |
| 11 | [ADR-0017](ADR/0017-product-scope-classroom.md) | decisões de produto: sala de aula, compartilhamento, custo zero |
| 12 | [ADR-0018](ADR/0018-multiplatform-and-steam.md) | Windows primeiro; outras plataformas e Steam depois |

## Índice completo

### Produto e design
[VISION](VISION.md) · [GAME_DESIGN](GAME_DESIGN.md) · [GAMEPLAY](GAMEPLAY.md) · [CLASSROOM](CLASSROOM.md) · [SHARING_SYSTEM](SHARING_SYSTEM.md) · [UI_UX](UI_UX.md) · [AUDIO](AUDIO.md) · [FUTURE_FEATURES](FUTURE_FEATURES.md) · [GLOSSARY](GLOSSARY.md)

### Aprendizagem
[LEARNING_DESIGN](LEARNING_DESIGN.md) · [LEARNING_PATH](LEARNING_PATH.md) · [LEARNING_ENGINE](LEARNING_ENGINE.md) · [PROGRESSION](PROGRESSION.md) · [CHALLENGE_SYSTEM](CHALLENGE_SYSTEM.md) · [VALIDATION_SYSTEM](VALIDATION_SYSTEM.md) · [HINT_SYSTEM](HINT_SYSTEM.md) · [ERROR_SYSTEM](ERROR_SYSTEM.md) · [SOLID_LEARNING](SOLID_LEARNING.md) · [DESIGN_PATTERNS](DESIGN_PATTERNS.md) · [TESTING](TESTING.md)

### Arquitetura e tecnologia
[ENGINE_DECISION](ENGINE_DECISION.md) · [TECH_STACK](TECH_STACK.md) · [ARCHITECTURE](ARCHITECTURE.md) · [PROJECT_ARCHITECTURE](PROJECT_ARCHITECTURE.md) · [DOMAIN_MODEL](DOMAIN_MODEL.md) · [CODING_STANDARDS](CODING_STANDARDS.md) · [AI_DEVELOPMENT_GUIDE](AI_DEVELOPMENT_GUIDE.md) · [ADR/](ADR/README.md)

### Simulação
[SIMULATION_ARCHITECTURE](SIMULATION_ARCHITECTURE.md) · [SIMULATION_TICK](SIMULATION_TICK.md) · [SIMULATION_LOD](SIMULATION_LOD.md) · [CITY_SIMULATION](CITY_SIMULATION.md)

### Sistemas da cidade
| Grupo | Documentos |
|---|---|
| População | [CITIZEN_SYSTEM](CITIZEN_SYSTEM.md) · [HOUSEHOLD_SYSTEM](HOUSEHOLD_SYSTEM.md) |
| Construção e território | [BUILDING_SYSTEM](BUILDING_SYSTEM.md) · [ZONE_SYSTEM](ZONE_SYSTEM.md) · [MAP_SYSTEM](MAP_SYSTEM.md) |
| Mobilidade | [ROAD_SYSTEM](ROAD_SYSTEM.md) · [TRAFFIC_SYSTEM](TRAFFIC_SYSTEM.md) · [VEHICLE_SYSTEM](VEHICLE_SYSTEM.md) · [PATHFINDING](PATHFINDING.md) · [PUBLIC_TRANSPORT](PUBLIC_TRANSPORT.md) |
| Economia | [ECONOMY_SYSTEM](ECONOMY_SYSTEM.md) · [MONEY_FLOW](MONEY_FLOW.md) · [BUDGET_SYSTEM](BUDGET_SYSTEM.md) |
| Bem-estar | [HAPPINESS_SYSTEM](HAPPINESS_SYSTEM.md) · [CITY_HEALTH_SYSTEM](CITY_HEALTH_SYSTEM.md) |
| Utilidades | [ENERGY_SYSTEM](ENERGY_SYSTEM.md) · [WATER_SYSTEM](WATER_SYSTEM.md) · [SEWAGE_SYSTEM](SEWAGE_SYSTEM.md) · [GARBAGE_SYSTEM](GARBAGE_SYSTEM.md) · [POLLUTION_SYSTEM](POLLUTION_SYSTEM.md) |
| Serviços | [EDUCATION_SYSTEM](EDUCATION_SYSTEM.md) · [HEALTHCARE_SYSTEM](HEALTHCARE_SYSTEM.md) · [CRIME_SYSTEM](CRIME_SYSTEM.md) |
| Ocorrências e ambiente | [EVENT_SYSTEM](EVENT_SYSTEM.md) · [WEATHER_SYSTEM](WEATHER_SYSTEM.md) · [DAY_NIGHT_SYSTEM](DAY_NIGHT_SYSTEM.md) |
| Visualização | [MAP_OVERLAYS](MAP_OVERLAYS.md) · [DEBUG_VISUALIZATION](DEBUG_VISUALIZATION.md) · [CAMERA_SYSTEM](CAMERA_SYSTEM.md) |

### Código do jogador
[PLAYER_API](PLAYER_API.md) · [CODE_EDITOR](CODE_EDITOR.md) · [CODE_EXECUTION](CODE_EXECUTION.md) · [CODE_EXECUTION_SECURITY](CODE_EXECUTION_SECURITY.md) · [CODE_ANALYSIS](CODE_ANALYSIS.md)

### Qualidade, operação e apresentação
[SAVE_SYSTEM](SAVE_SYSTEM.md) · [PERFORMANCE](PERFORMANCE.md) · [PROFILING](PROFILING.md) · [SECURITY](SECURITY.md) · [ASSET_PIPELINE](ASSET_PIPELINE.md) · [GRAPHICS](GRAPHICS.md) · [LIGHTING](LIGHTING.md) · [Fontes de arte e ferramentas gratuitas](art/ART_SOURCES.md) · [Registro de licenças](art/ASSET_LICENSES.md)

### Gestão
[MVP](MVP.md) · [DEVELOPMENT_ROADMAP](DEVELOPMENT_ROADMAP.md) · [RISKS](RISKS.md) · [REFERENCES](REFERENCES.md)

### Planos de execução
| Grupo | Documentos |
|---|---|
| Fases do MVP (backlog detalhado) | [F00 Spikes](phases/F00-research-and-spikes.md) · [F01 Fundação](phases/F01-foundation.md) · [F02 Mundo](phases/F02-world.md) · [F03 Estradas](phases/F03-roads.md) · [F04 Construções](phases/F04-buildings.md) · [F05 Cidadãos](phases/F05-citizens.md) · [F06 Economia](phases/F06-economy.md) · [F07 Energia e felicidade](phases/F07-energy-happiness.md) · [F08 Aprendizado + gate do MVP](phases/F08-mvp-learning.md) |
| Fases pós-MVP (intenções) | [F09](phases/F09-hardened-sandbox-replay-sharing.md) · [F10](phases/F10-zoning-growth.md) · [F11](phases/F11-traffic-ecs.md) · [F12](phases/F12-utilities.md) · [F13](phases/F13-services-problems-teacher.md) · [F14](phases/F14-oop-curriculum.md) · [F15](phases/F15-solid.md) · [F16](phases/F16-design-patterns.md) · [F17](phases/F17-player-testing.md) · [F18](phases/F18-scale.md) · [F19](phases/F19-content.md) · [F20](phases/F20-polish-launch.md) · [F21+](phases/F21-other-platforms.md) |
| Spikes da Fase 0 | [S6](spikes/S6-sandbox-windows.md) · [S1a](spikes/S1a-codehost-pipeline.md) · [S4a](spikes/S4a-headless-domain.md) · [S2b](spikes/S2b-photinox-monaco.md) · [S1b](spikes/S1b-il2cpp-ipc.md) · [S2a](spikes/S2a-unitywebbrowser.md) · [S3](spikes/S3-urp-render-budget.md) · [S4b](spikes/S4b-mono-il2cpp-factor.md) · [S5](spikes/S5-road-mesh.md) · [S7](spikes/S7-msix-store.md) |
| Playtests | [Protocolo](playtests/README.md) · [Modelo de relatório](playtests/TEMPLATE.md) |

## Convenções desta documentação
- Cada documento declara **Status**, **Data**, **ADRs** relacionados e se pertence ao **MVP**.
- Documentos de sistemas pós-MVP são **intenções de design**: curtos e revisados quando a fase chegar.
- Referências ao briefing usam a seção original (ex.: "§47").
- Diagramas em **Mermaid** (renderizam no GitHub e na maioria das IDEs).
- Alterou o comportamento? Atualize o documento no mesmo PR (Definition of Done).
