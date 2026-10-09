# Game Design

> **Status:** v1.0 · **Data:** 2026-10-08
> Detalhe do momento a momento: `GAMEPLAY.md`. Pedagogia: `LEARNING_DESIGN.md`.

---

## 1. Pilares de design
| Pilar | Pergunta de verificação para qualquer funcionalidade |
|---|---|
| Código com consequência | "O código do jogador muda algo **real** na simulação?" |
| Problema primeiro | "Que dor da cidade motiva isto?" |
| Cidade explicável | "O jogador consegue descobrir **por que** isso aconteceu?" |
| Honestidade sistêmica | "Isso cria um atalho que torna o código uma trapaça?" |
| Escopo disciplinado | "Pertence ao MVP/fase atual?" (§75) |

## 2. Laços de jogo

### 2.1 Laço macro (horas)
```
Observar a cidade → Diagnosticar problemas → Decidir (construir / ajustar / programar)
      ▲                                                   │
      └──── Feedback (indicadores, cidadãos, termômetro) ◄─ Simular
```

### 2.2 Laço de engenharia (minutos)
```
Problema → Prever (PRIMM) → Escrever código → Executar/Validar → Ver efeito na cidade
   ▲                                                            │
   └──── Erro pedagógico / dica / explicação ◄──────────────────┘
```

### 2.3 Laço de crescimento (sessão)
```
Crescer (moradia, empregos) → Novas pressões (energia, orçamento, trânsito) → Novos conceitos
→ Automatizar com código → Cidade maior → ...
```

## 3. Modos
| Modo | Descrição | Fase |
|---|---|---|
| **Campanha** | cidade guiada "Nova Esperança" (nome provisório), com missões e a trilha de conceitos | MVP (7 lições) |
| **Laboratório** | desafios isolados e determinísticos | MVP |
| **Sandbox** | cidade livre, código como ferramenta opcional | Fase 10 |
| **Cenários** | situações prontas ("Cidade falida", "Crise de energia", "Código legado") | Fase 19 |

## 4. Recursos do jogador
| Recurso | Fonte | Sumidouro |
|---|---|---|
| Dinheiro (tesouro) | impostos, tarifas, aluguel social | construção, manutenção, serviços, combustível |
| Tempo de jogo | relógio | — |
| Domínio de conceitos | desafios, revisões | — (desbloqueia) |
| Atenção do jogador | — | carga cognitiva: pausa ao programar, Laboratórios |

Não há "pontos de código" nem moeda artificial de aprendizado: o progresso de engenharia é medido pelo **domínio** e recompensado com **capacidades**.

## 5. Condições de falha (suaves)
- **Falência** → intervenção com empréstimo e missão de recuperação.
- **Êxodo** → a cidade encolhe. O termômetro e o "Por quê?" orientam a recuperação.
- **Código quebrado em produção** (comportamento instalado que falha) → *fallback* para a regra padrão + missão de depuração.
Sem *game over* na Campanha. Os Cenários podem ter condição de derrota explícita.

## 6. Ritmo e tempo
- Velocidades Pausa/1x/2x/3x/5x/10x (`SIMULATION_TICK.md`). Pausa automática ao programar (configurável).
- 1 dia de jogo = 48 min reais em 1x (calibrável). A maioria jogará entre 2x e 5x.

## 7. Diferenciais frente a city builders e jogos de programação
| Característica | City builders | Jogos de programação | City: SkyCode |
|---|---|---|---|
| Simulação urbana profunda | ✅ | ❌ | ✅ (escopo progressivo) |
| Linguagem real (C#) | ❌ | às vezes | ✅ C# moderno + Roslyn |
| Engenharia de software (SOLID, testes, refatoração) | ❌ | raro | ✅ conteúdo central |
| Código com efeito sistêmico | mods | puzzles isolados | ✅ cidade viva |

## 8. Tom e narrativa
Otimista, cívico e bem-humorado. A personagem-guia "engenheira-chefe" (nome a definir) dá dicas e conduz as missões. Erros são tratados como "a usina não entendeu o pedido", nunca como "você errou". Os cidadãos têm nomes e pequenas histórias (gerados), o que reforça que o código afeta **pessoas**.

## 9. Elementos fora de escopo (por ora)
Multiplayer, mods, editor de mapas, galeria online, monetização. Ver `FUTURE_FEATURES.md`. O compartilhamento **por arquivo** (Fase 9) e o modo **sala de aula** estão no roadmap (`SHARING_SYSTEM.md`, `CLASSROOM.md`).
