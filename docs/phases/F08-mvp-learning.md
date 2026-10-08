# Fase 8 — Aprendizado do MVP e 🚩 GATE DO MVP

> **Status:** Planejada · **Bloco:** B (Vertical Slice) · **Atualizado:** 2026-10-08
> **Entrada:** gate da Fase 7 · **Saída:** AC-01..AC-18 verificados + métricas do `MVP.md` §7 num **playtest em sala de aula real** · **Estimativa:** 6 semanas (+ iterações do playtest)

---

## 1. Objetivo
Transformar a cidade funcional num **produto de aprendizagem**: validação em 5 níveis, erros pedagógicos, dicas, 7 lições + desafio final, perfis de aluno, saves completos e um pacote que instala no PC da escola. Depois, provar com alunos reais que a hipótese do MVP se sustenta.

## 2. Por que agora
O MVP valida **uma** hipótese: *"um iniciante escreve poucas linhas de C#, vê a cidade mudar de verdade por causa delas e quer continuar."* Todas as fases anteriores construíram a cidade; esta constrói a experiência de aprender e mede o resultado. Sem passar no gate, o projeto **não avança** para o Bloco C: itera-se sobre o slice.

## 3. Escopo
**Inclui:** pipeline de validação (níveis 1–4 + estrutura básica), modo Laboratório (gateway *Headless*), `ERROR_SYSTEM` (~60 diagnósticos + exceções + orçamento), analyzers CSK1 (allowlist v1), CSK2 básicos, CSK3, CSK5001, dicas (6 níveis), motor de aprendizagem v0, painel de lições, conteúdo das 7 lições + ★, perfis locais, saves completos (3 slots + autosave), pacote de distribuição, áudio básico, acessibilidade básica, roteiro da primeira sessão, playtest em sala.
**Não inclui:** exportação de progresso e compartilhamento (F9), AppContainer (F9), painel do professor (F13), condicionais e laços como conteúdo (M3, F9–10), BKT (F13).

## 4. Entregáveis
- `content/challenges/` com 8 pacotes (7 lições + ★) passando no `CitySky.Content.Tests`.
- Build de playtest instalável sem admin e sem internet.
- Relatório de playtest em `docs/playtests/` (protocolo em [`docs/playtests/README.md`](../playtests/README.md)).

## 5. Backlog

### E1 — Validação e Laboratório
| ID | Tarefa | Onde | Depende de | Pronto quando |
|---|---|---|---|---|
| F8-01 | `ValidationReport` (C# 14 no Code Host) + DTO espelho C# 9 nos Contracts; níveis sequenciais | Code Host / Contracts | F1-61 | golden JSON |
| F8-02 | `HeadlessCityGateway`: Domain dentro do Worker, cenário por `seed` + `preset`, N ticks "o mais rápido possível" | PlayerApi / Worker | F1-38 | cenário de 1 dia com 6 casas ≤ 3 s (`VALIDATION_SYSTEM.md` §6) |
| F8-03 | Runner de critérios: `tests` (testes ocultos compilados junto), `cityMetric`, `structure` (Specifications básicas CSK4: `TypeExists`, `MethodReturnsValue`) | Code Host / Worker | F8-02 | os 3 tipos com testes |
| F8-04 | Snapshots de cenário: restaurar e comparar "antes × depois do seu código" | Worker | F8-02 | comparação exibida no console |
| F8-05 | Validação de **lição guiada** (modo Script): checagem do estado da cidade e dos valores tipados registrados por `city.Log(object)` no `RunResult` | Application / Code Host | F4-22 | Lições 1–5 validáveis |
| F8-06 | Botão 🧪 Validar + mensagem de aprovação com explicação e comparação com a referência | Presentation | F8-03 | — |

### E2 — Erros pedagógicos e analyzers
| ID | Tarefa | Onde | Depende de | Pronto quando |
|---|---|---|---|---|
| F8-10 | `ErrorClassifier` + `PedagogicalMessage` (4 partes) + templates na tabela `Errors.pt-BR`; "ver detalhes técnicos" | Code Host / content | F1-71 | golden files diagnóstico → mensagem |
| F8-11 | Catálogo do `ERROR_SYSTEM.md` §3 **+ `CS1955`** (membro não invocável: `city.Population()`, erro típico da Lição 5) e `CS1503` | content | F8-10 | CI falha se diagnóstico de desafio não tiver template |
| F8-12 | Exceções de runtime (NRE, índice, `ArgumentOutOfRange` da Player API, divisão por zero…) com linha original (PDB + `#line`) | Worker / Code Host | F8-10 | linha correta após o rewriter |
| F8-13 | Mensagens de orçamento: "laço possivelmente infinito na linha N" com análise simples da condição; recursão; memória | Code Host | F1-72 | **AC-08**, **AC-09** |
| F8-14 | Allowlist v1 (CSK1001–1004), CSK2001–2004, CSK3001 (desbloqueio), CSK5001 (determinismo); autocomplete filtrado pela allowlist e pelos desbloqueios | CodeAnalysis | F8-10 | **AC-10**; `System.IO` não aparece no autocomplete |
| F8-15 | Hover com XML docs pt-BR e ajuda de assinatura | Code Host / editor | F1-93 | ≤ 150 ms (quente) |

### E3 — Dicas e motor de aprendizagem
| ID | Tarefa | Onde | Depende de | Pronto quando |
|---|---|---|---|---|
| F8-20 | `HINT_SYSTEM`: 6 níveis, liberação por tentativas e tempo, `byPattern` a partir de `wrong/`, confirmação antes da solução completa | Education / Presentation | F8-03 | teste: solução completa nunca no primeiro clique |
| F8-21 | Balão da engenheira-chefe (não cobre o código), F1 contextual | Presentation | F8-20 | — |
| F8-22 | Motor v0: grafo de conceitos M0–M2 (acíclico), Mastery heurística (`LEARNING_ENGINE.md` §2), recomendação simples | Education | — | alunos sintéticos progridem como esperado |
| F8-23 | Painel de Lições (trilha linear de 7 + ★) e árvore de conhecimento com estados | Presentation | F8-22 | — |

### E4 — Conteúdo (ver §6)
| ID | Tarefa | Onde | Depende de | Pronto quando |
|---|---|---|---|---|
| F8-30 | Formato `content/challenges/<id>/` (`challenge.json`, narrativa, starter, testes, solução, `wrong/`, dicas) e `CitySky.Content.Tests` (referência passa com nota 1,0; erradas falham com o feedback esperado; dicas nos 6 níveis; grafo acíclico; cenário determinístico) | content / testes | F8-03 | CI verde para um desafio de exemplo |
| F8-31..38 | Lições 1–7 e ★, uma tarefa por lição, conforme §6 | content | F8-30 | **AC-11** para cada lição |
| F8-39 | Revisão pedagógica humana de cada lição (professor convidado, se possível) | — | F8-31..38 | checklist assinado no PR |

### E5 — Sala de aula, saves e distribuição
| ID | Tarefa | Onde | Depende de | Pronto quando |
|---|---|---|---|---|
| F8-40 | Seleção de perfil local (apelido + avatar, PIN opcional de 4 dígitos), `profile.json` por perfil, isolamento de saves e progresso | Application / Presentation | F1-53 | **AC-16** |
| F8-41 | Saves completos: 3 slots + autosave a cada 5 min reais (rotação de 3, I/O fora do main thread), código `.cs` legível, validação de invariantes ao carregar | Application / Infrastructure | F1-101 | **AC-12** (hash igual após round-trip) |
| F8-42 | Retomar em < 30 s a partir da tela inicial | Presentation | F8-41 | medido no hardware mínimo |
| F8-43 | Pacote de distribuição conforme o S7 (MSIX via Store em audiência privada **ou** portátil), offline, sem admin | `tools/` | S7 | **AC-17** num PC de escola |
| F8-44 | Telemetria **local** de playtest (tempo por lição, tentativas, dicas, erros, FPS), exportável manualmente, sem rede | Application | — | arquivo gerado; nenhuma chamada de rede (teste) |

### E6 — Polimento para o playtest
| ID | Tarefa | Onde | Depende de | Pronto quando |
|---|---|---|---|---|
| F8-50 | Roteiro da primeira sessão (`GAMEPLAY.md` §2): sobrevoo, tutorial de rua e casas, chegada dos moradores, introdução da engenheira-chefe | Presentation / content | E4 | sessão de 90 min jogável do início ao fim |
| F8-51 | Áudio básico: UI, construção, "build succeeded" discreto, erro não punitivo, aprovação, apagão; volumes separados | Presentation | — | conforme `AUDIO.md` §2 (MVP) |
| F8-52 | Acessibilidade básica: escala de UI, fonte do editor, paletas para daltonismo, alto contraste no editor, "sem sons de erro" | Presentation | — | checklist do `UI_UX.md` §5 (itens do MVP) |
| F8-53 | Varredura AC-15: nenhuma string visível fixa no código | testes | F1-100 | **AC-15** |
| F8-54 | Rodada final do AC-14 (10x por 30 min no hardware mínimo) e do AC-13 (hash em Editor, Player IL2CPP e Code Host) | testes | — | **AC-13**, **AC-14** |

### E7 — Playtest e gate
| ID | Tarefa | Onde | Depende de | Pronto quando |
|---|---|---|---|---|
| F8-60 | Preparar o playtest: escola, termo de consentimento do responsável e assentimento do aluno, levantamento das máquinas, instalação prévia | `docs/playtests/` | F8-43 | checklist do protocolo completo |
| F8-61 | Playtest em sala com 5–10 iniciantes, em PCs da escola (duas aulas de 50 min) | — | F8-60 | dados coletados |
| F8-62 | Relatório (`docs/playtests/TEMPLATE.md`) + lista de mudanças priorizada | `docs/playtests/` | F8-61 | relatório publicado |
| F8-63 | Iteração: corrigir os bloqueios e repetir o playtest se alguma métrica falhar | — | F8-62 | métricas do §7 atingidas |

## 6. Conteúdo das lições
### 6.1 Restrição de design que vale para todas
No MVP o aluno **ainda não viu condicionais nem laços** (Módulo M3 só chega na Fase 9–10, `LEARNING_PATH.md`). Por isso nenhuma lição exige `if`, `switch`, `for` ou `foreach`, e coleções aparecem só de forma pontual e explicada (`[0]` na Lição 5, `.Count` na Lição 7). O isolamento é passado como **fator numérico** na Lição 6, em vez de exigir um `switch` sobre `Insulation`.

### 6.2 Lições
Cada lição tem os blocos do `LEARNING_DESIGN.md` §5 e o pacote do `CHALLENGE_SYSTEM.md` §2. Abaixo, o essencial para escrever o pacote.

| # | Modo | Problema da cidade | Conceito | Solução de referência (resumo) | Erro provocado de propósito | Critério de aprovação |
|---|---|---|---|---|---|---|
| 1 | Script | "A prefeitura precisa se comunicar" | instrução, chamada de método, string | `city.Log("Bom dia, cidade!");` | apagar o `;` → `CS1002`; esquecer as aspas → `CS0103` | ao menos uma mensagem não vazia no console |
| 2 | Script | "Quanto sobra se construirmos 3 casas?" | variável, `decimal`, operadores | `decimal treasury = city.Budget.Treasury; decimal houseCost = 12400.50m; decimal remaining = treasury - 3 * houseCost; city.Log(remaining);` | `decimal houseCost = 12400.50;` → `CS0664` ("por que dinheiro não usa `double`?") | último valor registrado é `decimal` e igual ao esperado no snapshot |
| 3 | Script | "Sua cidade precisa crescer" | objeto, `new` | `var house = new House(); city.Build(house);` | `House house; city.Build(house);` → `CS0165` | uma casa a mais; `BuildResult.Success` |
| 4 | Script | "A conta de luz está alta" | propriedade (leitura/escrita, somente leitura) | `var house = new House(); house.Residents = 4; house.Insulation = Insulation.Good; city.Build(house);` e depois uma reforma com `Excellent` | `house.EnergyConsumption = 0;` → `CS0200` (encapsulamento); `house.Residents = 10;` → `ArgumentOutOfRangeException` pt-BR | casa nova com 4 moradores e consumo ≤ 2,0 kW; uma reforma paga registrada |
| 5 | Script | "Onde construir?" | método com parâmetros, sobrecarga, propriedade × método | `var lot = city.FindFreeLots()[0];` (dado no starter) `city.Build(house, lot); city.Log(city.Population);` | `city.Build(lot, house);` → `CS1503`; `city.Population()` → `CS1955` | casa construída exatamente no lote indicado |
| 6 | **Laboratório** | "Precisamos prever o consumo antes de construir" | método com retorno | `return (BaseKwHouse + BaseKwPerResident * residents) * insulationFactor;` em `EnergyCalculator.CalculateEnergyConsumption(int residents, double insulationFactor)` | o starter não compila (`CS0161`, lacuna intencional); `wrong/returns-zero.cs`, `wrong/no-factor.cs`, `wrong/adds-residents.cs` | testes ocultos (ex.: 4 moradores × fator 0,8 = 1,92 kW, tolerância 0,001) + estrutura (retorna valor) + saldo de energia ≥ 0 no cenário; aprovado → **instalar na cidade** (`applyToCity: formula`) |
| 7 | **Laboratório** | "Quais casas estão sem energia e quanto falta?" | classe própria, métodos de instância | `class EnergyAuditor` com `int CountUnpowered(City city)` (`city.Energy.Unpowered.Count`), `double MissingKw(City city)` (`Math.Max(0, -city.Energy.BalanceKw)`) e `string Summary(City city)`; `Program.cs` cria o auditor e registra o resumo | `EnergyAuditor.MissingKw(city)` → `CS0120` (precisa de instância) | testes ocultos em 2 cenários (déficit conhecido; superávit → 0) + estrutura (classe com métodos de instância) |
| ★ | Script + cidade | "Crise de energia": a cidade entra em déficit à noite (exige o balanceamento F7-07) | integração | livre: reformas (L4), construção no lugar certo (L5), auditor (L7), orçamento e UI | — | saldo de energia ≥ 0 em todas as horas de 1 dia TC, felicidade média ≥ 60% e tesouro ≥ 0 |

### 6.3 Estrutura de cada pacote
`narrative.pt-BR.md` (problema, previsão PRIMM, teoria ≤ 150 palavras, explicação final) · `starter/` · `tests/HiddenTests.cs` (Laboratório) · `solution/` · `wrong/` com `expected.json` · `hints.pt-BR.json` (3 dicas, solução parcial, referência à completa, explicação) · exercício de variação · revisão espaçada agendada como "manutenção da cidade".

### 6.4 Tamanho
Cada lição ≤ 20 min (mediana no playtest, **AC-18**); a trilha inteira em duas aulas de 50 min. Sessão 1: Lições 1–4. Sessão 2: Lições 5–7 e ★.

## 7. Testes e qualidade
Conteúdo (todas as referências e erradas no CI), golden reports de validação, golden files de mensagens, vazamento de testes ocultos (nenhuma informação antes do nível de dica correspondente), determinismo do Laboratório (2 execuções → mesmo relatório), segurança (corpus completo do MVP), perfis, saves, desempenho.

## 8. Gate de saída (🚩 GATE DO MVP)
- [ ] **AC-01 a AC-18** verificados (automatizados onde possível) e registrados.
- [ ] Métricas do `MVP.md` §7 no playtest: ≥ 70% concluem as Lições 1–5 sem ajuda externa em até 60 min; ≥ 90% explicam que "o código mudou a cidade"; ≥ 60% querem continuar; nenhuma lição com abandono > 30%.
- [ ] 0 crashes nas sessões de playtest.
- [ ] Relatório de playtest publicado; documentos dos sistemas do MVP com status "Implementado".
- [ ] `RISKS.md` revisado; ADRs com gatilhos disparados reavaliados (ADR-0008 escala temporal, ADR-0014 idioma, ADR-0017 hardware).
- [ ] **Decisão explícita do dono do produto:** avançar para o Bloco C **ou** iterar sobre o slice.

## 9. Riscos
| Risco | Ref. | Mitigação nesta fase |
|---|---|---|
| Sobrecarga cognitiva | R09 | pausa ao programar, Laboratórios, *fading*, ≤ 20 min por lição |
| Idioma da API como barreira | ADR-0014 | hover bilíngue, glossário; medir no playtest (> 30% → revisar ADR) |
| Escola sem Windows ou com PCs abaixo do mínimo | R21, R29 | levantar o parque antes (F8-60); escolher escola com Windows 10 22H2/11 |
| Bloqueio de instalação | R23 | pacote do S7; instalar antes da aula |
| LGPD com menores | R18 | protocolo com consentimento; telemetria só local; dados anonimizados |

## 10. Documentos e ADRs a atualizar
`VALIDATION_SYSTEM.md`, `ERROR_SYSTEM.md`, `HINT_SYSTEM.md`, `LEARNING_ENGINE.md`, `CHALLENGE_SYSTEM.md`, `LEARNING_PATH.md`, `CLASSROOM.md`, `SAVE_SYSTEM.md`, `CODE_ANALYSIS.md`, `MVP.md` (status de cada AC), todos os documentos de sistema do MVP → "Implementado".

## 11. Ferramental
`/spec` por lição (critérios viram testes de conteúdo) · `/tdd` (validação, dicas, motor) · `security-auditor` (allowlist v1, Laboratório) · `/verify` + `/health-check` no gate.

## 12. Encerramento
- [ ] Gate verificado e registrado
- [ ] `/health-check`
- [ ] `RISKS.md` revisado
- [ ] Velocidade real do MVP registrada; **estimativas das Fases 9–20 feitas** com base nela
- [ ] Backlog detalhado da Fase 9 (e da 10, se for em paralelo)
