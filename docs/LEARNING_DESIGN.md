# Design de Aprendizagem

> **Status:** v1.0 · **Data:** 2026-10-08 · **MVP:** sim
> Princípio central (§80): *"O jogador deve sentir que está aprendendo engenharia de software porque está tentando fazer uma cidade funcionar."*

---

## 1. Fundamentos pedagógicos adotados
| Fundamento | Como aparece no jogo |
|---|---|
| **Problema primeiro, teoria depois** (§71); *Productive Failure* (Kapur) | O jogador enfrenta o problema da cidade, tenta, esbarra no limite, e só então o conceito é apresentado como "a ferramenta que faltava" |
| **Teoria da Carga Cognitiva** (Sweller) | Pausa ao programar, Laboratórios isolados, exemplos resolvidos antes de problemas abertos, uma ideia nova por lição |
| **Exemplos resolvidos → *fading*** | Sequência por conceito: ler exemplo → completar lacunas → modificar → escrever do zero |
| **PRIMM** (Predict-Run-Investigate-Modify-Make) | "O que você acha que este código fará com a cidade?" → executa → Inspector → altera → cria |
| **Feedback imediato e específico** | Erros traduzidos em problemas da cidade (§51), com conceito e dica |
| **Prática de recuperação e revisão espaçada** | Mini-desafios de revisão que reaparecem como "manutenção" da cidade |
| **Mastery learning** | Avança quando demonstra domínio (não por tempo); o modelo do aluno estima o domínio por conceito |
| **Motivação intrínseca** | Autonomia (sandbox), competência (feedback claro), propósito (a cidade e os cidadãos dependem do código) |

## 2. Duas camadas conectadas (§73)
```
Camada 1 — City Builder                 Camada 2 — Engenharia de Software
constrói, administra, expande  ◄──────►  escreve, modela, refatora, testa, depura
          │                                        ▲
          └── problemas reais da cidade ───────────┘
              (energia, lixo, trânsito, orçamento)
```
**Regra de conexão:** todo conceito ensinado tem **(a)** um problema de cidade que o motiva, **(b)** um efeito visível na cidade quando aplicado e **(c)** uma forma de falhar que a cidade evidencia.

## 3. Modos de jogo e o papel do código
| Modo | Código | Propósito |
|---|---|---|
| **Campanha** (cidade guiada) | missões introduzem conceitos na ordem do grafo | aprendizagem estruturada |
| **Laboratório** (cenário isolado, determinístico) | desafio validado | prática focada, sem o caos da cidade |
| **Sandbox** (cidade livre) | ferramenta opcional de automação e análise | autonomia, aplicação |

**Regra de ouro:** o código é **opcional para jogar** e **necessário para dominar**. Sem código, a cidade funciona, mas fica cara, lenta e manual. Com código, fica eficiente e inteligente. Isso evita frustrar quem ainda não sabe programar e recompensa quem aprende.

## 4. Tensão sandbox × currículo (e a solução)
Problemas emergentes podem exigir conceitos que o aluno ainda não aprendeu (ex.: trânsito exigindo Strategy no início do jogo). Solução:
1. **Grafo de conceitos** (DAG, ver `LEARNING_PATH.md`), não uma árvore linear.
2. Cada tipo de problema tem **soluções em níveis**: UI (sempre disponível) → script simples → solução OO → solução com padrão.
3. O sistema de problemas oferece **missões compatíveis** com o domínio atual do aluno (`LEARNING_ENGINE.md`).
4. Sistemas complexos (trânsito, serviços) só são **desbloqueados na cidade** quando os pré-requisitos conceituais básicos foram vistos.

## 5. Anatomia de uma lição (§50)
| Bloco | Conteúdo | Exemplo (Lição 4: Propriedades) |
|---|---|---|
| **Problema** | situação da cidade | "A conta de luz das casas está alta. Dá para projetar casas mais eficientes?" |
| **Previsão** | pergunta PRIMM | "Se mudarmos `Insulation` para `Excellent`, o que acontece com o consumo?" |
| **Exemplo** | código resolvido e comentado | `house.Insulation = Insulation.Good;` |
| **Teoria (curta)** | ≤ 150 palavras + diagrama | propriedade = acesso controlado a um dado do objeto |
| **Desafio** | tarefa com critérios | reduzir o consumo médio para ≤ 2,5 kW sem estourar o orçamento |
| **Aplicação na cidade** | efeito visível | overlay de energia, conta de luz cai no orçamento |
| **Erro provocado** | armadilha didática | tentar `house.EnergyConsumption = 0` → `CS0200` → "por que não posso?" → encapsulamento |
| **Feedback** | mensagens pt-BR | ver `ERROR_SYSTEM.md` |
| **Exercício** | variação curta | "projete a casa mais barata com consumo ≤ 3 kW" |
| **Revisão** | recuperação espaçada | dias depois: "manutenção" pede para reconfigurar casas antigas |

## 6. Erros como conteúdo
Os erros mais comuns de iniciantes (`ERROR_SYSTEM.md`) são **provocados de propósito** em momentos seguros, para que o primeiro contato com eles seja guiado, e não frustrante.

## 7. O arco "da POO ao orientado a dados"
1. **POO** (classes, objetos, encapsulamento, composição, herança, polimorfismo, interfaces) com os prédios e veículos da cidade.
2. **Limites da herança:** "ônibus elétrico herda de ônibus, mas não abastece" (LSP) → composição.
3. **Arquitetura:** a `CityManager` legada → serviços → camadas.
4. **Escala:** "sua cidade tem 50 mil cidadãos e seu código ficou lento" → processamento em lote, coleções, complexidade → noção de *data-oriented design*. O jogo revela que a engine **também** funciona assim (bastidores).

## 8. Acessibilidade pedagógica
- Linguagem simples, glossário sempre acessível (pt-BR ↔ código).
- Leitura: textos curtos, áudio opcional (TTS do SO), contraste alto.
- Ritmo: pausa ao programar, sem cronômetros punitivos.
- Erros nunca culpam: "a usina não entendeu o pedido" em vez de "você errou".

## 9. Avaliação da eficácia
- Playtests a cada fase (protocolo em `docs/playtests/`): tarefas, *think-aloud*, questionário curto.
- Métricas locais (opt-in): tentativas por desafio, dicas usadas, tempo, abandono, erros mais frequentes.
- Pré/pós-teste conceitual curto nas Fases 8, 14 e 17.
