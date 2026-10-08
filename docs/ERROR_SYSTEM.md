# Sistema de Erros Pedagógicos

> **Status:** v1.0 · **Data:** 2026-10-08 · **MVP:** sim (catálogo inicial de ~60 diagnósticos)
> "Não dizer apenas *Compilation Error*" (§51).

---

## 1. Formato da mensagem
Toda mensagem ao aluno tem até 4 partes, e o tom nunca culpa:
```
🏙️ Problema na cidade:    <consequência no mundo, se houver>
🔎 Problema detectado:     <o que aconteceu no código, com linha>
📘 Conceito relacionado:   <link para a lição/glossário>
💡 Dica:                   <próximo passo concreto>
```
O texto técnico original (ex.: `CS0103: The name 'hous' does not exist in the current context`) fica disponível em "ver detalhes técnicos", porque aprender a ler mensagens reais também faz parte do currículo (Módulo 11).

## 2. Fontes de erro e mapeamento
| Fonte | Exemplo | Tradução |
|---|---|---|
| Sintaxe (Roslyn) | `CS1002` | "Faltou o `;` no fim da instrução da linha 3. Em C#, cada instrução termina com ponto e vírgula." |
| Compilação (Roslyn) | `CS0103` | "O nome `hous` não existe. Será que você quis dizer `house`? (declarado na linha 2)". Usa distância de edição para sugerir |
| Allowlist (CSK1) | `CSK1001` | "`System.IO.File` não está disponível na cidade. Seu código só pode usar recursos da cidade, por segurança." |
| Progressão (CSK3) | `CSK3001` | "LINQ ainda não foi desbloqueado. Você vai aprender na lição Coleções. Por enquanto, use um `foreach`." |
| Runtime (exceção) | `NullReferenceException` | "Na linha 8, `house.Lot` está vazio (`null`), porque a casa ainda não foi construída. Construa antes de perguntar o lote." |
| Orçamento | `BudgetExceeded` | "Seu laço na linha 12 parece não terminar: `i` diminui enquanto a condição espera que cresça." (análise simples da condição do laço) |
| Comportamento | falha de teste | "Esperávamos 3,1 kW para 4 moradores com isolamento bom; o método devolveu 0." |
| Arquitetura (CSK4) | `CSK4001` | "A classe `Router` decide a rota com `if (vehicle is Bus)`. Cada novo tipo de veículo vai exigir mudar esta classe (OCP)." |
| Comando recusado | `InsufficientFunds` | "A cidade não tem dinheiro para essa casa: faltam $3.200. Que tal revisar o projeto ou o orçamento?" |

## 3. Catálogo inicial (MVP): diagnósticos C# mais comuns de iniciantes
`CS1002` ; esperado · `CS1513`/`CS1514` chaves · `CS1026` ) esperado · `CS1010` quebra de linha em string · `CS0103` nome inexistente · `CS0246` tipo não encontrado · `CS1061` membro inexistente · `CS0117` membro estático inexistente · `CS0029`/`CS0266` conversão · `CS0019` operador entre tipos · `CS0165` variável não atribuída · `CS0128` variável duplicada · `CS0136` conflito de escopo · `CS0161` nem todo caminho retorna · `CS0127`/`CS0126` retorno em `void` / sem valor · `CS0201` instrução inválida · `CS0200` propriedade somente leitura · `CS0272` setter inacessível · `CS0122` membro inacessível · `CS0120` precisa de instância · `CS0176` membro estático via instância · `CS1501`/`CS7036` argumentos · `CS1503` tipo de argumento · `CS0023` operador em tipo inválido · `CS0664` literal `decimal` sem `m` (**dinheiro!**) · `CS0234` namespace inexistente · `CS0101` tipo duplicado · `CS0111` método duplicado · `CS0050`/`CS0051` acessibilidade inconsistente · `CS0115` override sem virtual · `CS0534` abstrato não implementado · `CS0535` interface não implementada · `CS0144` instanciar abstrata/interface · `CS0236` inicializador de campo · `CS0818` `var` sem inicializador · `CS0815` `var` com null · `CS8600`–`CS8625` nulabilidade (avisos explicados) · `CS0162` código inalcançável · `CS0168`/`CS0219` variável não usada · `CS0642` `;` vazio após `if` · `CS0665` `=` em condição.

Runtime: `NullReferenceException`, `IndexOutOfRangeException`, `ArgumentOutOfRangeException` (inclusive as da Player API), `DivideByZeroException`, `InvalidCastException`, `KeyNotFoundException`, `InvalidOperationException` (coleção modificada no `foreach`), `FormatException`, `OverflowException`, estouro de orçamento (laço/recursão/memória).

## 4. Arquitetura
```
Diagnostic/Exception/Failure ──► ErrorClassifier ──► ErrorTemplate (pt-BR, parâmetros)
                                     │                    │
                                     └─ contexto: símbolo, linha, valores, conceito
                                                          ▼
                                                 PedagogicalMessage {city, detected, concept, hint, technical}
```
- Templates em tabelas de localização (`Errors.pt-BR`), com parâmetros nomeados.
- `ErrorClassifier` usa o modelo semântico (ex.: para `CS0103`, busca nomes parecidos no escopo).
- **Linha e coluna** sempre vindas do PDB e de `#line`, refletindo o código **original** do aluno (não o instrumentado).

## 5. Mensagens da cidade (camada narrativa)
Quando o erro tem efeito no mundo (ex.: casa não construída por falta de dinheiro, usina parada porque o método retornou 0), a parte "Problema na cidade" descreve a consequência, e o Inspector destaca o objeto afetado. Isso amarra o erro à camada 1 (§73).

## 6. Testes
- Golden files: diagnóstico → mensagem pt-BR (detecta regressões de texto).
- Cobertura: todo código do catálogo tem template, e o CI falha se aparecer um diagnóstico sem template nos desafios (mostra o genérico + registra lacuna).
