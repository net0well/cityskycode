# MVP — Escopo Exato da Primeira Versão

> **Status:** Aceito (v1.0) · **Data:** 2026-10-08 · **Fases:** 0–8 do [`DEVELOPMENT_ROADMAP.md`](DEVELOPMENT_ROADMAP.md)
> **Regra:** o que não está na seção 3 **não está no MVP**. Ideias novas vão para `FUTURE_FEATURES.md` ou para o roadmap (§75 do briefing).

---

## 1. Hipótese que o MVP precisa validar

> **"Um iniciante escreve poucas linhas de C#, vê a cidade mudar de verdade por causa delas e quer continuar."**

O MVP **não** valida escala, gráficos de ponta nem profundidade de city builder. Ele valida o **laço central**:

```
Terreno → Estrada → Casa → Cidadão → Dinheiro → Energia → Felicidade → Código C# → Validação → Feedback
```

## 2. Público e plataforma do MVP
- Iniciantes em programação, a partir de ~14 anos, em pt-BR, **em sala de aula** e em casa (ADR-0017).
- Windows 10/11 x64. Pacote único (jogo + Code Host self-contained + host do editor). **Instala e roda sem admin e sem internet.** Distribuição para o playtest: MSIX pela Microsoft Store (assinatura gratuita) ou build portátil, conforme o spike S7.
- **Hardware mínimo (confirmado pelo dono do produto):** CPU 4 núcleos, 8 GB RAM, GPU integrada moderna (Intel Iris Xe / Radeon 680M) → 30 FPS em 1080p no preset Baixo.
- **Recomendado:** 6 núcleos, 16 GB, GTX 1660 / RX 6600 ou superior → 60 FPS em 1080p no preset Alto.
- **Custo zero:** só ferramentas, bibliotecas e assets gratuitos (arte e áudio CC0).

---

## 3. Escopo incluído

### 3.1 Mundo
| Item | Escopo exato |
|---|---|
| Terreno | 2 × 2 km com área construível central de 1 × 1 km. Relevo suave (heightmap fixo). 1 lago **decorativo** (sem simulação de água). Árvores instanciadas do Terrain. |
| Câmera | Pan (WASD/arrastar), zoom ao cursor (15 m–1.500 m), rotação, inclinação (20°–85°), foco por duplo clique, seguir cidadão. |
| Iluminação | URP, sol direcional com ciclo dia/noite (manhã, dia, tarde, noite), céu procedural, SSAO, emissivos noturnos (janelas e postes). **Sem** clima. |
| Seleção | Clique em prédio, cidadão ou estrada → Inspector. |

### 3.2 Construção
| Item | Escopo exato |
|---|---|
| Estrada | **Um** tipo: rua de mão dupla, 2 faixas, 50 km/h, calçadas. Segmentos **retos** de qualquer ângulo, interseções automáticas de até 4 ramos, demolição. Custo por metro. |
| Lotes | Gerados automaticamente ao longo das ruas (frente de 8 m × fundo de 24 m). |
| Casa (`House`) | Parâmetros: `Residents` (1–6), `Insulation` (`Basic`/`Good`/`Excellent`), `HasSolarPanels`. Derivados (somente leitura): `EnergyConsumption` (kW), `Cost`, `MaintenancePerDay`. |
| Comércio (`Shop`) | Pequeno mercado: 4 empregos, vende "bens de consumo" a preço fixo, compra estoque de fora da cidade. |
| Prefeitura (`CityHall`) | Existe desde o início (não construível no MVP), 3 empregos, sede do orçamento. |
| Usina (`PowerPlant`) | Gerador a gás, capacidade constante, 3 empregos, custo operacional diário. Necessária para o sistema de energia (não estava listada no §68, mas sem geração não há "energia" a simular). **Capacidade a definir no balanceamento (tarefa F7-07):** o valor ilustrativo de 400 kW é grande demais para ≤ 50 cidadãos (~70 kW de pico) e impediria a crise de energia das lições. |
| Demolição | Qualquer prédio construído pelo jogador (com reembolso parcial). |

### 3.3 Simulação
| Sistema | Escopo exato |
|---|---|
| Tempo | Tick fixo, velocidades **Pausa/1x/2x/3x/5x/10x**, ciclo de calendário (ver `SIMULATION_TICK.md`). |
| População | Até **50 cidadãos** (somente adultos), em domicílios de 1–4 pessoas. Chegam se houver moradia livre e a cidade for atrativa. Saem se ficarem infelizes por 3 dias de jogo. |
| Rotina | Casa → trabalho (dia) → mercado (a cada ~2 dias, à tarde) → casa. Agentes pedestres **GameObject** andando nas calçadas. **Sem veículos.** |
| Pathfinding | A* no grafo de calçadas (custos inteiros). |
| Dinheiro | Ledger de partidas dobradas: tesouro municipal, domicílios, empresas (mercado), "exterior". Salários, compras, impostos (renda e comércio), tarifa de energia, construção e manutenção. Preços **fixos**. |
| Orçamento | Receitas e despesas por categoria, saldo diário, alíquota residencial ajustável (0–20%). |
| Energia | Geração × consumo numa única rede (as ruas conduzem energia). Déficit → corte por prioridade (casas por último a serem atendidas) → efeitos reais. |
| Felicidade | Necessidades: Moradia, Emprego, Energia, Consumo, mais penalidade por impostos. Modelo normalizado e explicável (`HAPPINESS_SYSTEM.md`). |
| Saúde da cidade | Termômetro v0 com 4 indicadores (felicidade média, emprego, saldo de energia, saldo orçamentário) e níveis 🔥🔴🟠🟡🟢🔵. |

### 3.4 Código
| Item | Escopo exato |
|---|---|
| Editor | Monaco em webview (ou fallback definido no spike S2): realce, autocomplete, hover com documentação pt-BR, sublinhado de erros. |
| Code Host | Processo .NET 10 com Roslyn: compilação, análise (allowlist + analyzers pedagógicos), mapeamento de erros pt-BR. |
| Execução | Sandbox Worker (processo separado) com orçamentos de tempo (2 s), instruções, recursão e memória (256 MB). **Job Object** ativo; AppContainer/token restrito fica para a Fase 9. |
| Modos | **Script** (comandos na cidade viva) e **Laboratório** (desafio validado em cenário *headless*). |
| Player API | `City`, `House`, `Shop`, `PowerPlant`, `Lot`, `Road` (leitura), `Citizen` (leitura), `Budget`, `EnergyReport`, `city.Log`. Ver `PLAYER_API.md`. |
| Console | Estilo IDE: "Build succeeded", avisos, saída do programa, resumo da cidade. Erros clicáveis levam à linha. |

### 3.5 Aprendizado (7 lições)
| # | Lição | Conceito | Resultado na cidade |
|---|---|---|---|
| 1 | Bom dia, prefeito(a) | instrução, chamada de método, string | `city.Log(...)` aparece no console |
| 2 | Quanto temos no cofre? | **variável**, tipos (`int`, `decimal`) | lê o orçamento e calcula a sobra |
| 3 | Sua primeira casa | **objeto** (`new`) | `var house = new House(); city.Build(house);` → **a casa aparece** |
| 4 | Projetando a casa | **propriedade** (leitura/escrita, somente leitura, `CS0200`) | isolamento ↓ consumo, **a simulação de energia muda** |
| 5 | Onde construir? | **método** com parâmetros, sobrecarga | `city.Build(house, lot)` |
| 6 | Calculando o consumo | **método** com retorno (criar) | Laboratório: `CalculateEnergyConsumption()` validado por testes ocultos |
| 7 | Seu primeiro projeto | **classe** (criar a sua) | `class EnergyAuditor` gera um relatório das casas sem energia |
| ★ | Crise de energia | integração | desafio final: equilibrar a rede com código e orçamento |

Cada lição tem: teoria curta, exemplo, desafio, aplicação na cidade, erro comum provocado, feedback, exercício e revisão (§50). Também tem **3 dicas + solução parcial + solução completa + explicação** (§49).

**Tamanho para sala de aula:** cada lição cabe em **até 20 min** (mediana no playtest), e a trilha inteira cabe em **duas aulas de 50 min**. Dá para salvar a qualquer momento e retomar em menos de 30 s.

### 3.6 Interface
- **Seleção de perfil local** na abertura (apelido + avatar, PIN opcional): vários alunos no mesmo PC, sem conta (`CLASSROOM.md`).
- HUD: população, tesouro (e variação/dia), felicidade, termômetro, data/hora, velocidade.
- Barra de ferramentas: estrada, casa, mercado, usina, demolir.
- Inspector de casa, cidadão, prédio e rua (exemplos do §36).
- 2 overlays: **Energia** (abastecido/cortado) e **Felicidade**. 1 debug visual: **caminho do cidadão selecionado**.
- Painel de Código (editor + console) acoplável, com **pausa automática** ao executar.
- Painel de Lições (trilha das 7 lições).

### 3.7 Persistência
- 3 slots + autosave (a cada 5 min reais). Save = estado do domínio + código do jogador (arquivos `.cs` legíveis) + progresso (arquivo de perfil separado).
- Formato JSON compactado com `schemaVersion` (ver `SAVE_SYSTEM.md`).

---

## 4. Fora do escopo do MVP (explicitamente)

Veículos e tráfego · zoneamento e crescimento automático · água, esgoto, lixo e poluição · serviços públicos (saúde, educação, polícia, bombeiros) · transporte público · clima · eventos (exceto apagão por déficit) · curvas, rotatórias, pontes e túneis · edição de terreno · crianças, idosos, escola · mercado com preço dinâmico · ECS/DOTS · Addressables · NavMesh · comportamentos do jogador chamados por tick (IPC por tick) · herança, interfaces, SOLID, padrões · testes escritos pelo jogador · replay/rebobinar · AppContainer (Fase 9) · macOS/Linux · localização além de pt-BR · conquistas · compartilhamento de código ou cidades (Fase 9, exige o sandbox completo) · exportação de progresso (Fase 9) · painel do professor (Fase 13) · telemetria remota (nunca sem consentimento).

---

## 5. Critérios de aceite (testáveis)

| ID | Critério |
|---|---|
| AC-01 | Iniciar jogo novo leva a uma cidade com terreno, prefeitura, uma rua inicial e o tesouro de $100.000 em ≤ 20 s (hardware recomendado). |
| AC-02 | Desenhar uma rua de 200 m debita o custo correto, cria lotes nos dois lados e permite construir neles. |
| AC-03 | `var house = new House(); city.Build(house);` executado no editor faz uma casa aparecer no próximo lote livre em ≤ 1,5 s (Code Host aquecido) e debita o custo; o ledger registra "Construção". |
| AC-04 | Alterar `house.Insulation = Insulation.Excellent;` numa casa construída gera uma reforma paga, e o consumo exibido no Inspector e na rede de energia diminui. |
| AC-05 | `house.EnergyConsumption = 0;` gera o erro `CS0200` com explicação pt-BR sobre propriedades somente leitura e encapsulamento. |
| AC-06 | Um déficit de energia corta as casas por prioridade. As casas cortadas aparecem no overlay de Energia, e a felicidade dos moradores cai, com a causa "Sem energia" visível no Inspector. |
| AC-07 | O painel "Por que estão infelizes?" lista as 3 maiores causas negativas com valores. |
| AC-08 | Um laço infinito no código do jogador é interrompido em ≤ 2 s com a mensagem "Laço possivelmente infinito na linha N". O jogo continua a ≥ 30 FPS e o Code Host continua disponível. |
| AC-09 | Recursão infinita **não** derruba o jogo nem o Code Host. O Worker é reciclado e a mensagem pedagógica aponta a recursão. |
| AC-10 | `System.IO.File.ReadAllText(...)` é rejeitado na análise com a mensagem "Este recurso não está disponível na cidade" (sem executar nada). |
| AC-11 | As 7 lições podem ser concluídas. As soluções de referência passam nos testes no CI, e as soluções erradas de exemplo falham com o feedback esperado. |
| AC-12 | Salvar e carregar restaura cidade, código e progresso de forma idêntica (hash de estado igual). |
| AC-13 | Mesma seed + mesmos comandos → mesmo hash de estado após 3 dias de jogo, no Editor (Mono), no Player (IL2CPP) e no Code Host (.NET 10). |
| AC-14 | 10x por 30 min reais sem queda abaixo de 30 FPS no hardware mínimo e sem crescimento de memória > 5%. |
| AC-15 | Nenhum texto exibido ao jogador está fixo no código (todos vêm de tabelas de localização). |
| AC-16 | Dois perfis no mesmo PC mantêm cidades, código e progresso separados; trocar de perfil não perde estado. |
| AC-17 | O jogo instala e roda com usuário **sem privilégio de administrador** e **sem internet**, num PC com o hardware mínimo. |
| AC-18 | No playtest em sala, a mediana de tempo de cada lição fica em até 20 min. |

## 6. Orçamentos do MVP
| Métrica | Alvo |
|---|---|
| FPS (recomendado/mínimo) | 60 / 30 |
| Simulação por frame (média, 10x) | ≤ 2 ms |
| Memória total (jogo + Code Host + Worker + webview) | ≤ 3 GB |
| Ida e volta de script simples (quente / frio) | ≤ 1,5 s / ≤ 4 s |
| Feedback de diagnóstico ao digitar | ≤ 400 ms após pausa de digitação |
| Crash em sessão de playtest | 0 |

## 7. Métricas de sucesso (playtest em sala de aula com 5–10 iniciantes, em PCs da escola)
- ≥ 70% concluem as lições 1–5 sem ajuda externa em até 60 min.
- ≥ 90% conseguem explicar com suas palavras que "o código mudou a cidade".
- ≥ 60% querem continuar jogando (pergunta direta + observação).
- Nenhuma lição com taxa de abandono > 30%.

## 8. Definition of Done do MVP
- Todos os AC-01..AC-18 verificados (automatizados onde possível).
- Documentos dos sistemas do MVP atualizados para "Implementado".
- `RISKS.md` revisado; ADRs com gatilhos de revisão disparados foram reavaliados.
- Playtest realizado e relatório em `docs/playtests/`.
