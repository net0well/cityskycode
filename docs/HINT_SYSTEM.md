# Sistema de Dicas

> **Status:** v1.0 · **Data:** 2026-10-08 · **MVP:** sim

---

## 1. Estrutura (§49)
| Nível | Conteúdo | Disponível quando |
|---|---|---|
| **Dica 1: direção** | lembra o objetivo e o conceito ("o método precisa *devolver* um valor") | após 1 tentativa falha **ou** 2 min sem progresso |
| **Dica 2: estratégia** | aponta o caminho ("multiplique o consumo por pessoa pelo número de moradores") | após a Dica 1 + 1 tentativa |
| **Dica 3: concreta** | mostra a forma da solução, sem o código final ("use `return residents * ...;`") | após a Dica 2 + 1 tentativa |
| **Solução parcial** | código com lacunas (`return ___ * ___;`) | após a Dica 3 |
| **Solução completa** | código de referência | após a solução parcial + confirmação ("tem certeza? tentar mais uma vez costuma ensinar mais") |
| **Explicação** | por que funciona, alternativas, erros comuns | **sempre** após passar (com ou sem dicas) |

## 2. Princípios
- **Nunca entregar a solução imediatamente.** Os níveis são liberados por tentativas e tempo, não por um clique só.
- **Sem punição visível.** Não há perda de pontos nem vergonha. O uso de dicas alimenta o **modelo do aluno** (domínio estimado menor → mais revisão espaçada daquele conceito).
- **Dicas contextuais:** quando o erro é conhecido (ex.: retornou 0), a Dica 1 é escolhida a partir do **padrão de erro** (`wrong/` cadastrado no desafio), e não só a genérica.
- **Explicação compara** a solução do aluno com a de referência e celebra soluções diferentes que também passaram.

## 3. Formato (`hints.pt-BR.json`)
```json
{
  "generic": [
    { "level": 1, "text": "O método precisa devolver quanto de energia a casa consome. O que a palavra `return` faz?" },
    { "level": 2, "text": "Cada morador consome uma quantidade base. E o isolamento reduz o total..." },
    { "level": 3, "text": "Calcule `base * residents` e depois aplique o fator de isolamento." }
  ],
  "byPattern": {
    "returns-zero": { "level": 1, "text": "Seu método está devolvendo 0. A cidade acha que as casas não gastam energia!" }
  },
  "partialSolution": "return BaseKwPerResident * residents * ___;",
  "fullSolutionRef": "solution/EnergyCalculator.cs",
  "explanation": "narrative.pt-BR.md#explicacao"
}
```

## 4. UI
Botão 💡 com indicador de dicas disponíveis. A dica aparece como "conselho da engenheira-chefe" (personagem-guia), num balão que não cobre o código. Atalho: F1.

## 5. Futuro (FUTURE_FEATURES)
Tutor com IA (LLM) para dicas abertas: só com privacidade adequada (menores, LGPD), custos controlados e respostas ancoradas no conteúdo do desafio. **Fora do MVP.**
