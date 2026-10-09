# Visão do Produto

> **Status:** v1.0 · **Data:** 2026-10-08

---

## 1. Em uma frase
> **City: SkyCode é um simulador de cidade 3D em que o jogador não apenas constrói a cidade: ele a programa em C#, e o código tem consequências reais no mundo.**

## 2. Princípio central (§80)
> *"O jogador deve sentir que está aprendendo engenharia de software porque está tentando fazer uma cidade funcionar."*

Não é um tutorial de C# com uma cidade decorativa. É uma **cidade realmente simulada**, onde:
```
Código → Objetos → Sistemas → Simulação → Cidade → Cidadãos → Problemas → Feedback → Aprendizado
```
E o jogador pode olhar para a cidade e pensar: *"Essa cidade funciona porque eu programei os sistemas que fazem ela funcionar."*

## 3. Para quem
| Persona | Motivação | O que o jogo oferece |
|---|---|---|
| **Iniciante curioso** (14–25 anos) | quer aprender a programar sem "aula chata" | problemas concretos, feedback visual, progressão clara |
| **Estudante de computação** | quer entender POO, SOLID e padrões além da teoria | dores reais de manutenção e refatoração com resultado visível |
| **Fã de city builder** | quer uma cidade bonita e desafiadora | simulação honesta, código como superpoder de gestão |
| **Professor(a)** (confirmado, ADR-0017) | quer uma ferramenta engajante para a turma, que funcione no laboratório da escola | trilha estruturada, lições de até 20 min, progresso exportável, painel do professor local, material alinhado à norma de Computação da BNCC (`CLASSROOM.md`) |

## 4. Pilares
1. **Código com consequência:** todo código executado altera a simulação de verdade, e nunca é "teatro" (§1).
2. **Problema primeiro:** conceitos chegam como solução para dores sentidas (§71).
3. **Cidade viva e explicável:** a cidade se mexe, e tudo tem causa investigável (§14, §37).
4. **Engenharia real:** C# moderno, compilador real, testes reais, refatoração real e ferramentas de IDE.
5. **Honestidade sistêmica:** sem atalhos mágicos. O código é engenharia, não trapaça.

## 5. O que o jogador leva ao final (§2)
Fundamentos de C# → POO → Modelagem de domínio → Composição → Interfaces → Herança → Polimorfismo → SOLID → Design Patterns → Arquitetura → Testes → Refatoração → Sistemas complexos, mais a intuição de **escala** (por que nem tudo deve ser "um objeto por coisa").

## 6. O que NÃO é
- Não é um jogo de "exercícios desconectados".
- Não é uma IDE com um cenário enfeitado.
- Não é um clone de *Cities: Skylines* (inspiração de experiência, não de assets nem de identidade).
- Não é multiplayer, nem mobile, nem web (no horizonte atual). O compartilhamento entre jogadores é **por arquivo**, sem servidor.
- Não depende de orçamento: ferramentas, bibliotecas e assets são gratuitos (ADR-0017).

## 6.1 Natureza do produto
**Produto educacional** (decisão do dono do produto, 2026-10-08), usado em sala de aula e em casa. Isso torna privacidade (LGPD), acessibilidade, funcionamento offline e instalação sem administrador **requisitos**, não extras.

## 7. Experiência de abertura (§1)
O jogador começa com um terreno, uma rua, a prefeitura, pouco dinheiro e a mensagem *"Sua cidade precisa crescer"*. Constrói a primeira casa pela interface e logo descobre que pode fazer o mesmo, e mais, escrevendo:
```csharp
var house = new House();
city.Build(house);
```
A casa aparece. Pouco depois, ao projetar casas mais eficientes, ele percebe que **não pode** simplesmente dizer que a casa consome zero energia (`CS0200`), e aprende por quê. É aqui que a engenharia começa.

## 8. Sucesso
- **MVP:** iniciantes concluem as primeiras lições sem ajuda e querem continuar (`MVP.md` §7).
- **Produto:** jogadores concluem o arco POO → SOLID e relatam transferência para projetos reais. A cidade de cada jogador é diferente porque o código dele é diferente.
