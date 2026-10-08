# ADR-0014: Identificadores em inglês, conteúdo em pt-BR

- **Status:** Aceito (o briefing mistura `Casa`/`Trabalhador` com `House`/`city.Build`)
- **Data:** 2026-10-08
- **Documentos afetados:** `PLAYER_API.md`, `GLOSSARY.md`, `CODING_STANDARDS.md`, `ERROR_SYSTEM.md`

## Contexto
O briefing alterna exemplos em português (`var casa = new Casa(); casa.Capacidade = 4; cidade.Construir(casa);`) e em inglês (`var house = new House(); city.Build(house);`). O C# (palavras-chave, BCL, ecossistema, mercado) é em inglês.

## Decisão
1. **Todos os identificadores** (Player API, domínio, código interno) em **inglês**.
2. **Todo conteúdo voltado ao jogador** em **pt-BR** (com localização futura): lições, dicas, mensagens de erro, XML docs da Player API (hover/autocomplete), UI, narrativa.
3. **Glossário** pt-BR ↔ código sempre acessível no jogo, e o hover mostra os dois termos.
4. Sem "aliases" em português na API (duas APIs para a mesma coisa confundem).

## Alternativas consideradas
| Alternativa | Por que não |
|---|---|
| API em português | distancia o aluno do ecossistema real; mistura inevitável com a BCL (`List`, `Math`, `string`) |
| API bilíngue (aliases) | duplicidade, ambiguidade, manutenção |

## Consequências
**Positivas:** transferência direta para o mercado e para documentação real; consistência.
**Negativas:** barreira inicial de vocabulário para iniciantes sem inglês. Mitigação: glossário, hover bilíngue, nomes simples.

## Gatilhos de revisão
Playtests mostrando o idioma como principal barreira (> 30% de dificuldade atribuída a ele).
