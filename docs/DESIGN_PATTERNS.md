# Padrões de Projeto

> **Status:** v1.0 · **Data:** 2026-10-08 · **Fase (conteúdo):** 16 · **MVP:** não (como conteúdo)
> Duas listas: **(A)** padrões ensinados ao jogador; **(B)** padrões usados na **nossa** arquitetura (para a equipe e as IAs).

---

## A. Padrões como conteúdo: "cada padrão nasce de uma necessidade da cidade" (§43)

| Padrão | Dor da cidade | Antes (código do aluno) | Depois | Onde a cidade usa |
|---|---|---|---|---|
| **Strategy** | 12 tipos de transporte com rotas diferentes e um `if` gigante (§71) | `if (v is Bus) … else if (v is Truck) …` | `IRouteStrategy` por tipo | rotas, overlays (`IMapOverlay`), políticas de imposto |
| **Observer** | "Quero ser avisado quando faltar energia" (polling a cada hora) | laço consultando `city.Energy` | `city.Events.PowerShortage += OnShortage` | alertas, conquistas, painéis |
| **Command** | "Quero desfazer a última construção" e registrar ações | chamadas diretas | `ICityAction { Execute(); Undo(); }` + histórico | ferramenta de construção do aluno, macros |
| **State** | semáforo e cidadão com `if (estado == …)` espalhados | flags e booleanos | `TrafficLightState` (Verde/Amarelo/Vermelho) | semáforos, agenda do cidadão, eventos |
| **Factory** | criar o prédio certo para cada zona e densidade | `switch` em vários lugares | `BuildingFactory.Create(zone, density)` | crescimento automático |
| **Builder** | hospital com muitos parâmetros opcionais (departamentos, leitos, equipe) | construtor com 9 parâmetros | `new HospitalBuilder().WithEmergency().WithBeds(80).Build()` | prédios complexos (composição) |
| **Specification** | regras de zoneamento e de problemas combináveis ("lote residencial, plano, perto de escola, longe de indústria") | `if`s aninhados duplicados | `new IsFlat().And(new NearSchool(500)).And(new Not(new NearIndustry(300)))` | busca de lotes, detecção de problemas |
| **Repository** | "salvar e carregar meus projetos de casas"; consultas espalhadas | listas globais | `IBlueprintRepository` | biblioteca de projetos do aluno |
| **Mediator** | serviços conversando entre si diretamente (polícia ↔ bombeiros ↔ hospital em incêndio) | referências cruzadas | `EmergencyDispatcher` coordena | despacho de emergências |
| *(extras)* **Decorator** | modificadores que se acumulam (política "tarifa social" + evento "seca") sobre a tarifa base | condicionais acumulados | decoradores de `ITariff` | tarifas, modificadores |
| *(extras)* **Template Method** | todos os serviços seguem "receber demanda → atender → relatar", com variações | código duplicado | classe-base com passos abstratos | serviços públicos |

### Formato de cada missão de padrão
1. Missão funcional resolvida "do jeito ingênuo" (o aluno escreve o `if`).
2. Mudança que multiplica a dor (novo tipo, nova regra).
3. Revelação do padrão (nome, diagrama, exemplo da cidade).
4. Refatoração validada (CSK4: estrutura + testes de comportamento).
5. "Bastidores": onde a **própria engine** do CitySky usa o mesmo padrão.

## B. Padrões na nossa arquitetura (referência para desenvolvimento)

| Padrão | Onde | Motivo |
|---|---|---|
| **Command** | `ICityCommand` + `CommandQueue` | toda mutação é um comando validado; replay; mesma via para UI e código |
| **Observer** (eventos de domínio com buffer) | `DomainEventBuffer` | desacoplar read models, problemas e áudio, de forma determinística |
| **Strategy** | `IPathfinder`, `IAgentPresentation`, `ICodeEditorHost`, `ICityGateway`, overlays | trocar implementação sem mudar quem usa |
| **Adapter / Ports & Adapters** | Infrastructure (Save, IPC, Log) | domínio independente da engine |
| **Specification** | regras estruturais CSK4, detecção de problemas, validação de lotes | regras combináveis e testáveis |
| **State** | agenda do cidadão, semáforos, ciclo de vida do Worker | máquinas de estado explícitas |
| **Factory** | criação de prédios a partir do catálogo, criação de Workers | centralizar a montagem |
| **Builder** (de teste) | `CityBuilder` nos testes | cenários legíveis (Test Data Builder) |
| **Object Pool** | GameObjects de agentes, buffers | evitar GC |
| **Double Buffer** | read models (Fase 18) | thread de simulação × apresentação |
| **Composition Root / Pure DI** | `CitySky.Bootstrap` | sem singletons nem service locator |
| ~~Singleton~~ | **proibido sem ADR** | estado global atrapalha testes e determinismo |
| ~~Service Locator~~ | proibido | dependências ocultas |

> Coerência pedagógica: o jogo **pratica o que ensina**. Os "bastidores" mostrados ao aluno são código real (simplificado) da engine.
