# Análise de Código (Roslyn)

> **Status:** v1.0 · **Data:** 2026-10-08 · **ADRs:** 0005, 0006 · **MVP:** sim (famílias CSK1, CSK2 básicas, CSK3)
> Projeto: `dotnet/src/CitySky.CodeAnalysis` (.NET 10, C# 14).

---

## 1. Papéis da análise
1. **Segurança** (allowlist): impedir o uso de APIs proibidas antes da execução.
2. **Pedagogia**: transformar código "que funciona, mas ensina errado" em conversa (ex.: número mágico, método gigante, `switch` sobre tipo).
3. **Progressão**: bloquear recursos ainda não desbloqueados, com explicação.
4. **Validação estrutural de desafios**: verificar se a solução usa o conceito ensinado (§48, *Architecture*).

## 2. Famílias de diagnósticos (prefixo `CSK`)
| Faixa | Família | Severidade | Exemplo |
|---|---|---|---|
| CSK1xxx | **Segurança/allowlist** | Erro (bloqueia) | `CSK1001` API não disponível na cidade; `CSK1002` finalizador proibido; `CSK1003` `[ModuleInitializer]` proibido; `CSK1004` código `unsafe`/`extern` |
| CSK2xxx | **Pedagógicos (qualidade)** | Aviso/Info | `CSK2001` número mágico em regra de cidade; `CSK2002` método com mais de 40 linhas; `CSK2003` variável não usada (complementa `CS0168`/`CS0219`); `CSK2004` nome não descritivo (`x`, `a1`); `CSK2005` `if/else if` encadeado sobre tipo (sugere polimorfismo, *a partir da Fase 14*) |
| CSK3xxx | **Progressão/desbloqueio** | Erro (didático) | `CSK3001` recurso bloqueado: "LINQ será desbloqueado na lição Coleções" |
| CSK4xxx | **Estruturais de desafio** | Erro/Aviso (só em Laboratório) | `CSK4001` falta classe que implemente `IRouteStrategy`; `CSK4002` dependência proibida de tipo concreto (DIP); `CSK4003` classe com responsabilidades demais (SRP, por heurística de coesão) |
| CSK5xxx | **Determinismo** (Laboratório) | Erro | `CSK5001` `DateTime.Now`/`Random` sem seed (usar `city.Random`/`city.Clock`) |

Cada diagnóstico tem: id, título pt-BR, mensagem com parâmetros, **conceito relacionado** (id do grafo de conceitos), link para a lição e dica curta. Ver `ERROR_SYSTEM.md`.

## 3. Implementação
- `DiagnosticAnalyzer`s registrados numa `CompilationWithAnalyzers` (Roslyn padrão). Não são distribuídos como pacote NuGet: são carregados pelo Code Host.
- **Allowlist analyzer:** `RegisterOperationAction` (invocações, referências a propriedades/campos, criação de objetos, conversões, operadores) + `RegisterSymbolAction` (declarações: finalizadores, atributos) + `RegisterSyntaxNodeAction` (`unsafe`, `stackalloc`, `fixed`, `extern`).
- **Forma canônica** de símbolos: `SymbolDisplayFormat` fixo, com tipos genéricos por definição (`System.Collections.Generic.List`1`). A mesma forma é usada pelo verificador de IL.
- **Regras estruturais (CSK4):** expressas como **Specifications** compostas (o próprio padrão ensinado na Fase 16):
```csharp
var rule = Spec.TypeImplements("CitySky.Routing.IRouteStrategy")
              .And(Spec.NoTypeSwitchOver("CitySky.Vehicle"))
              .And(Spec.MethodBodyMaxStatements("CalculateRoute", 30));
```
  As regras ficam declaradas no `challenge.json` (ver `CHALLENGE_SYSTEM.md`) e são interpretadas pelo Code Host.
- **Métricas pedagógicas** (para feedback, não para nota): complexidade ciclomática, linhas por método, profundidade de aninhamento, acoplamento entre classes do aluno (LCOM simplificado).

## 4. Desempenho
- Análise incremental por documento alterado. Analyzers caros (CSK4) só rodam ao **validar** o desafio, não a cada tecla.
- Meta: diagnósticos de digitação ≤ 400 ms, contando IPC e renderização.

## 5. Testes
- Cada analyzer tem testes "código → diagnósticos esperados" (NUnit, com `CSharpAnalyzerTest`-like helper próprio).
- Paridade L1 × L2: para cada caso do corpus de segurança, L1 **ou** L2 rejeita, e o teste falha se só L2 pegar (sinal de lacuna em L1).
- Snapshot das mensagens pt-BR (golden files) para evitar regressões de texto.

## 6. Ferramentas de desenvolvimento (para nós)
O mesmo Roslyn que analisa o aluno ajuda a equipe: o MCP **CWM.RoslynNavigator** do dotnet-claude-kit (`detect_antipatterns`, `get_diagnostics`, `find_dead_code`) é usado na **nossa** base de código. São usos diferentes, com a mesma tecnologia.
