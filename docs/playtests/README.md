# Protocolo de Playtest

> **Status:** v1.0 · **Data:** 2026-10-08 · **ADR:** [0017](../ADR/0017-product-scope-classroom.md) · Usado no **gate do MVP** (Fase 8) e nos playtests de cada fase
> Citado por `MVP.md` §8, `LEARNING_DESIGN.md` §9 e `CLASSROOM.md` §7. Relatórios seguem o [`TEMPLATE.md`](TEMPLATE.md) e ficam nesta pasta como `AAAA-MM-DD-<fase>-<local>.md`.

---

## 1. Objetivos por tipo de playtest
| Tipo | Quando | Pergunta principal |
|---|---|---|
| **Interno** (equipe, amigos) | fim de cada fase | o fluxo funciona sem travar? algo óbvio confunde? |
| **Em sala de aula** (gate do MVP) | Fase 8 | a hipótese do MVP se sustenta com iniciantes reais, em PCs da escola? |
| **Piloto escolar** (várias semanas) | Fase 20 | o jogo funciona como recurso didático ao longo de um módulo? |

## 2. Ética e privacidade (obrigatório)
O público inclui **menores de idade**. O playtest segue a LGPD (Lei 13.709/2018, art. 14: melhor interesse e consentimento específico de um responsável) e o princípio de "nada sai do PC" do ADR-0017.

- **Autorização da escola** (direção/coordenação) por escrito.
- **Termo de consentimento do responsável legal** e **termo de assentimento do aluno**, em linguagem simples: o que será feito, quanto tempo, que dados, como são guardados, quando são apagados, que participar é voluntário e que é possível parar a qualquer momento sem prejuízo.
- **Sem gravação de rosto ou voz.** Gravação de **tela** só com consentimento explícito no termo, e apenas se necessária.
- Participantes identificados por código (**P01, P02…**). Nenhum nome, e-mail ou foto no relatório. Apelidos dos perfis do jogo também são códigos.
- Telemetria **só local** (arquivo exportado manualmente pela equipe ao fim da sessão); o jogo nunca envia nada pela rede.
- Dados brutos guardados localmente e **apagados** até 90 dias após o relatório; o relatório só tem dados agregados ou anonimizados.
- **Sem nota e sem comparação entre alunos.** O jogo é que está sendo avaliado, não o aluno. O professor da turma é informado disso.

## 3. Preparação (checklist)
- [ ] Escola com **Windows 10 22H2 ou 11** confirmada (R21, R29); levantar modelo, CPU, GPU, RAM e versão do Windows de cada PC.
- [ ] Jogo instalado **antes** da aula, no perfil de usuário que os alunos usam, **sem admin** (AC-17); testar 1 PC completo na véspera.
- [ ] Sem internet durante a sessão (testar com a rede desligada).
- [ ] Um perfil novo por aluno; telemetria local ligada.
- [ ] Termos assinados recolhidos.
- [ ] Observadores (1 para cada 3–4 alunos) com a folha de observação (§6).
- [ ] Pré-teste impresso ou no próprio jogo (§5).
- [ ] Plano B: PCs reserva, build portátil em pendrive.

## 4. Roteiro (MVP: duas aulas de 50 min)
| Min | Sessão 1 | Sessão 2 |
|---|---|---|
| 0–5 | apresentação (sem ensinar C#), pré-teste (5 min) | retomar o save (meta: < 30 s) |
| 5–42 | jogo a partir do início: tutorial, Lições 1–4 | Lições 5–7 e desafio ★ |
| 42–47 | — | pós-teste (5 min) |
| 47–50 | questionário curto (§5) | questionário + conversa em grupo (3 perguntas) |

**Regras para os observadores:** não ajudar a menos que o aluno peça; ao ajudar, registrar o momento e o motivo; não corrigir o código do aluno, apontar a dica do jogo; anotar frases espontâneas ("ah, então é isso!", "não entendi"). *Think-aloud* individual é difícil em sala; usar em 1–2 alunos voluntários, se possível, numa mesa separada.

## 5. Instrumentos
### 5.1 Telemetria local (automática)
Tempo por lição, tentativas de execução e de validação, dicas usadas por nível, códigos de erro mais frequentes, abandono (lição aberta e não concluída), crashes, FPS médio e mínimo, tempo para retomar.

### 5.2 Pré-teste e pós-teste conceitual (MVP, 6 itens, ~5 min)
1. O que acontece quando o computador executa `int total = 3 + 4;`?
2. Qual é a diferença entre `House` e `new House()`?
3. Por que `house.EnergyConsumption = 0;` pode dar erro, e por que isso é bom?
4. Em `city.Build(house, lot)`, o que são `house` e `lot`?
5. O que a palavra `return` faz num método?
6. Por que usar `decimal`, e não `double`, para dinheiro?

Respostas abertas, corrigidas por rubrica de 0–2 pontos (0 = errado ou em branco; 1 = parcialmente certo; 2 = certo). O ganho é comparado por item.

### 5.3 Questionário (escala 1–5, mais abertas)
- Foi divertido. · Eu entendi o que o código fazia com a cidade. · As dicas me ajudaram. · Eu quero continuar jogando. · Os textos eram fáceis de ler.
- Abertas: "O que o seu código mudou na cidade?" · "O que foi mais difícil?" · "O que você mudaria?"

### 5.4 Conversa em grupo (3 perguntas)
O que vocês aprenderam? · Em que momento ficaram travados? · Indicariam para um colega? Por quê?

## 6. Folha de observação (por aluno)
| Campo | Exemplo |
|---|---|
| Código do participante | P03 |
| Lição / minuto | L4 / 18 min |
| Evento | pediu ajuda · travou > 2 min · erro repetido · comentário espontâneo · desistiu |
| Descrição curta | "tentou `EnergyConsumption = 0` três vezes; leu a dica 2 e entendeu" |
| Ajuda dada (se houver) | "apontei o botão de dica" |

## 7. Métricas e critérios do gate do MVP (`MVP.md` §7 e AC-18)
| Métrica | Fonte | Critério |
|---|---|---|
| Concluem as Lições 1–5 sem ajuda externa em até 60 min | telemetria + folha | ≥ 70% |
| Explicam com as próprias palavras que "o código mudou a cidade" | questionário aberto + conversa | ≥ 90% |
| Querem continuar | questionário (≥ 4) + observação | ≥ 60% |
| Abandono por lição | telemetria | nenhuma lição > 30% |
| Mediana de tempo por lição | telemetria | ≤ 20 min (AC-18) |
| Crashes | telemetria + observação | 0 |
| Retomar o save | telemetria | < 30 s |

## 8. Análise e decisão
1. Consolidar telemetria e folhas por lição.
2. Listar problemas com **severidade** (bloqueia · atrapalha · cosmético) e **frequência** (quantos alunos).
3. Ligar cada problema a uma causa provável (texto, dica, UI, conceito, desempenho) e a uma mudança proposta.
4. Preencher o relatório e levar ao dono do produto a decisão do gate: **avançar** ou **iterar** (e o que muda na próxima rodada).
