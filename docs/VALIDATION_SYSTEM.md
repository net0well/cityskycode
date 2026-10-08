# Sistema de Validação

> **Status:** v1.0 · **Data:** 2026-10-08 · **MVP:** níveis 1–4 + estrutura básica · **ADR:** 0005
> "Não validar somente texto" (§48).

---

## 1. Os cinco níveis
| Nível | Pergunta | Mecanismo | Saída típica |
|---|---|---|---|
| **1. Sintaxe** | O código é C# válido? | `CSharpSyntaxTree` | `CS1002: ; esperado` → "Faltou terminar a instrução com `;` na linha 3" |
| **2. Compilação** | Compila contra a Player API? Respeita allowlist e desbloqueios? | `CSharpCompilation` + analyzers CSK1/CSK3 + verificação de IL | `CS0103`, `CSK1001` |
| **3. Runtime** | Executa sem exceções e dentro do orçamento? | Worker + orçamentos | `NullReferenceException` na linha 8, laço infinito na linha 12 |
| **4. Comportamento** | Produz o resultado correto? | testes ocultos (NUnit-like) + métricas do cenário *headless* + propriedades | "Para uma casa com 4 moradores e isolamento bom, esperávamos 3,1 kW e o método retornou 0" |
| **5. Arquitetura** | Usa o conceito ensinado e respeita as restrições de projeto? | regras estruturais CSK4 (Specifications sobre o modelo semântico) | "A classe `Router` decide pelo tipo do veículo com `if`. Use `IRouteStrategy`" |

Níveis são **sequenciais**: um nível só roda se o anterior passou. Exceção: o nível 5 pode emitir **avisos** mesmo com o nível 4 aprovado (sugestões de melhoria sem bloquear).

## 2. `ValidationReport`
```csharp
public sealed class ValidationReport
{
    public string ChallengeId { get; init; }
    public ValidationLevel ReachedLevel { get; init; }     // até onde chegou
    public bool Passed { get; init; }
    public double Score { get; init; }                     // 0–1, ponderado pelos critérios
    public IReadOnlyList<CriterionResult> Criteria { get; init; }
    public IReadOnlyList<Diagnostic> Diagnostics { get; init; }      // níveis 1–3
    public IReadOnlyList<BehaviorFailure> Failures { get; init; }    // nível 4: esperado × obtido + contexto da cidade
    public IReadOnlyList<ArchitectureFinding> Findings { get; init; } // nível 5
    public RunMetrics Metrics { get; init; }               // tempo, ticks de orçamento, alocação
    public CityDelta CityDelta { get; init; }              // o que mudou no cenário (para a explicação)
}
```
(No Code Host, com C# 14; nos Contracts, é espelhado em DTO compatível com C# 9.)

## 3. Técnicas de validação de comportamento
| Técnica | Uso |
|---|---|
| **Testes ocultos** (exemplos) | entradas e saídas fixas, incluindo casos de borda |
| **Propriedades** | invariantes ("consumo nunca negativo", "mais isolamento ⇒ consumo ≤") testadas com entradas geradas por seed fixa |
| **Métricas do cenário** | após N ticks: "saldo de energia ≥ 0", "lixo acumulado ≤ 10%" |
| **Comparação com referência** | mesmo cenário, solução do aluno × referência, com tolerância |
| **Mutantes** (testes do aluno, Fase 17) | os testes do aluno precisam matar ≥ X% dos mutantes do código-alvo |
| **Desempenho** (Fase 18) | orçamento de ticks/tempo em cenários grandes (complexidade) |

## 4. Feedback por nível (contrato com `ERROR_SYSTEM.md`)
Toda falha vira uma mensagem com 4 partes (§51):
```
Problema na cidade:   A estação de energia não está funcionando.
Problema detectado:   O método CalculateEnergyConsumption() retornou 0 para uma casa com 4 moradores.
Conceito relacionado: Métodos e retorno.
Dica:                 O método precisa informar quanto de energia está sendo consumido.
```

## 5. Determinismo e justiça
- O cenário usa seed fixa, e `city.Random`/`city.Clock` são determinísticos. Fontes não determinísticas geram `CSK5001`.
- O mesmo código sempre produz o mesmo relatório. Isso é testado no CI com as soluções de referência.

## 6. Desempenho
Validação típica de um desafio do MVP: ≤ 3 s (compilação quente + 1 dia de jogo *headless* com 6 casas).

## 7. Testes do próprio sistema
- Golden reports (JSON) para soluções certas e erradas de cada desafio.
- Teste de que nenhuma informação dos testes ocultos vaza na mensagem antes do nível de dica correspondente.
