# Funcionalidades Futuras (fora do roadmap atual)

> **Status:** vivo · **Atualizado:** 2026-10-08
> Destino das ideias que **não pertencem ao MVP/fase atual** (§75): documentar e **não implementar**. Itens só saem daqui por decisão de roadmap.

| Ideia | Valor | Pré-requisitos / riscos | Observação |
|---|---|---|---|
| **Tutor com IA** (dicas abertas por LLM) | alto (personalização) | privacidade (menores, LGPD), custo, respostas ancoradas no desafio, sem entregar a solução | protótipo depois da Fase 17, só com opt-in |
| **Turmas na nuvem / contas / integração com Google Classroom ou Moodle** | alto (escolas) | servidor, custos recorrentes, LGPD | a versão **local** (exportação de progresso e painel do professor por arquivos) entrou no roadmap: Fases 9 e 13 (ADR-0017) |
| **Galeria online de cidades e soluções** | alto (comunidade) | servidor, moderação, LGPD, custo | o compartilhamento **por arquivo** entrou no roadmap (Fase 9, `SHARING_SYSTEM.md`) |
| **Mods / workshop** | médio | segurança de conteúdo, versionamento da API | — |
| **Depurador com breakpoints** no código do aluno | alto | depurador .NET no Worker sandboxed (complexo) | o traço de execução (Fase 9) cobre parte |
| **Async/await e concorrência** como conteúdo | médio | orçamento de tasks no sandbox, determinismo | Módulo avançado |
| **Unity 7 / CoreCLR**: C# moderno no domínio | médio | estabilidade da Unity 7; migração da shared source | reavaliar após o MVP (ADR-0001) |
| **Linux / Steam Deck** | alto (escolas, Deck) | sandbox seccomp + Landlock, build nativo | Fase 21+ (ADR-0018) |
| **macOS** | médio | Seatbelt, notarização (~US$ 99/ano) | Fase 21+, após o Linux (ADR-0018) |
| **Versão web (para Chromebooks)** | alto (escolas com Chromebook) | Unity Web + Roslyn em WebAssembly + execução em Web Worker; limites de memória e threads, sem Burst | pesquisa; seria praticamente outro produto |
| **Metrô, trem, aeroporto, porto** | médio | transporte público v1 | após a Fase 19 |
| **Edição de terreno** | médio | malha de estradas com relevo, saves diferenciais | — |
| **Clima extremo e desastres naturais** | médio | eventos v1 | — |
| **Políticas por bairro** | médio | distritos | — |
| **Ranking/competição de soluções** | baixo-médio | validação em servidor (anti-trapaça) | — |
| **Modo "engenharia reversa"** (ler código da cidade e achar bugs) | alto (pedagógico) | conteúdo | candidato para a Fase 17+ |
| **Exportar projeto do aluno como solução .NET real** (abrir no VS Code) | alto (transferência) | empacotar Player API como NuGet de referência + simulador *headless* CLI | ponte com o mundo real |
| **Localização (en, es)** | médio | tabelas prontas desde o MVP | Fase 20 (en) |
| **VR / mobile / web** | baixo | execução de código inviável nessas plataformas | descartado no horizonte atual |
