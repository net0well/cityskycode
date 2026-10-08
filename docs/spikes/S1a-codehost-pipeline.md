# Spike S1a: Pipeline do Code Host (Roslyn → Worker com orçamentos)

- **Status:** Planejado
- **Prazo:** 3 dias · **Ordem:** 2º
- **Depende de:** S6 (launcher com Job Object) · **Bloqueia:** S1b, Fase 1 (E8)
- **Código:** `spikes/S1a-codehost-pipeline/` (descartável)
- **ADRs relacionados:** [ADR-0005](../ADR/0005-code-execution.md), [ADR-0006](../ADR/0006-code-security.md) · Documentos: [`CODE_EXECUTION.md`](../CODE_EXECUTION.md) §4, [`CODE_ANALYSIS.md`](../CODE_ANALYSIS.md)

## Pergunta
O Roslyn 5 no .NET 10 consegue analisar o código do aluno contra uma allowlist mínima, instrumentá-lo com orçamentos, emiti-lo em memória e entregá-lo a um Worker em processo separado (Job Object) que o executa num `AssemblyLoadContext` coletável, com laço infinito, recursão e memória contidos e mensagens com a **linha original**, em **≤ 1,5 s** com o Host aquecido?

## Hipótese
Sim. Com o Host quente, compilar um arquivo pequeno custa dezenas a poucas centenas de milissegundos; o Worker pré-iniciado carrega o assembly em menos de 10 ms. O primeiro uso do Roslyn (frio) custa 1–3 s. O risco técnico está no rewriter (cobrir todos os laços e corpos de método, inclusive lambdas e funções locais) e no descarregamento do ALC.

## Por que importa
É o núcleo do produto. Se o pipeline for lento demais ou se os orçamentos não forem confiáveis, o AC-03 (≤ 1,5 s), o AC-08 (laço) e o AC-09 (recursão) caem, e a experiência "escrevo e a cidade muda" fica comprometida.

## Critério de sucesso (definido antes de começar)
| Métrica | Alvo | Resultado |
|---|---|---|
| Compilar e executar "hello" com Host quente (p95 de 100 execuções) | ≤ 1,5 s | |
| Primeira compilação após iniciar o Host (frio) | ≤ 4 s | |
| `for (int i = 0; i < 10; i--) {}` → `BudgetExceeded` com a linha certa | ≤ 2 s, linha correta | |
| Recursão infinita → detectada pela profundidade (256) **antes** do `StackOverflow`, com a linha | sim | |
| Se o rewriter for contornado, `StackOverflow` mata só o Worker; o Host recicla e responde | sim | |
| `try { laço } catch (Exception) { }` não engole o `BudgetExceeded` | sim | |
| `var l = new List<byte[]>(); while (true) l.Add(new byte[1 << 20]);` → contido com mensagem | sim | |
| `System.IO.File.ReadAllText("x")` rejeitado na **L1** (símbolo) | `CSK1001`, nada emitido | |
| O mesmo acesso, com a L1 desligada, rejeitado na **L2** (verificação de IL) | sim | |
| 200 execuções seguidas no mesmo Worker: memória cresce ≤ 10% (ALC descarrega) | sim, ou reciclar a cada N | |
| Linhas e colunas reportadas batem com o código original (após o rewriter) | sim | |

## Escopo
**Inclui:** Host console .NET 10 (sem Generic Host ainda); parse C# 14; referências curadas; analyzer de allowlist mínimo (`RegisterOperationAction`); rewriter de orçamento (`__Budget.Tick(line)`, `Enter/Exit`, filtros em `catch`); emit + Portable PDB em memória; verificação de IL com `System.Reflection.Metadata`; Worker (do S6, só Job Object) com protocolo `[uint32 length][json]` por stdin/stdout; `Console.SetOut` redirecionado; ALC coletável.
**Não inclui:** serviços de linguagem do editor (S2b), IPC com a Unity (S1b), Player API real, AppContainer (vem do S6, ligado depois).

## Ambiente
Windows 11, .NET SDK 10.0.400, `Microsoft.CodeAnalysis.CSharp` 5.x (última estável no NuGet no dia do spike), Newtonsoft.Json.

## Plano de execução
**Dia 1 — Compilação**
1. Referências: assemblies de **referência** do `Microsoft.NETCore.App.Ref` (pasta `packs/` do SDK, copiados no build) **ou** o pacote `Basic.Reference.Assemblies.Net100` (MIT). Registrar qual funcionou e o tamanho resultante: isso vira item da ficha de dependência.
2. `CSharpSyntaxTree.ParseText` com `LanguageVersion.CSharp14`; `CSharpCompilation` com `OutputKind.ConsoleApplication` (top-level statements), `AllowUnsafe = false`, `Deterministic = true`, `OptimizationLevel.Release`.
3. Analyzer de allowlist mínimo: permitir `System.Object`, `System.String`, `System.Int32`, `System.Console.WriteLine`, `System.Math`, `List<T>`; negar o resto com `CSK1001`.
4. Rewriter (`CSharpSyntaxRewriter`): `__Budget.Tick(line)` no início de todo corpo de laço (`for`, `foreach`, `while`, `do`) e de todo corpo de método, lambda e função local; `Enter/Exit` em `try/finally` para a profundidade; `catch (Exception)` e `catch` sem tipo ganham filtro que relança exceções de orçamento; `#line` para preservar as linhas.

**Dia 2 — Execução**
1. Emit para `MemoryStream` (PE + PDB); verificação de IL: percorrer `TypeReference`, `MemberReference`, `MethodSpecification`, atributos; rejeitar `ModuleRef`/`ImplMap`.
2. Worker: `AssemblyLoadContext(isCollectible: true)`, `LoadFromStream(pe, pdb)`, invocar o *entry point* (`Program.<Main>$`), capturar saída, aplicar orçamentos; watchdog de 2 s no Host com *kill*.
3. Mensagens com linha: mapear a exceção para a linha original via PDB.

**Dia 3 — Medições e casos**
1. Rodar a tabela de critérios; medir frio/quente; teste de descarregamento (`WeakReference` para o ALC + ciclos de GC).
2. Casos difíceis do rewriter: `goto` para trás, recursão mútua, recursão via lambda, laço dentro de `finally`, propriedades com corpo (getter recursivo).
3. `/security-scan` no código; relatório.

## Método de medição
`Stopwatch` em cada estágio (parse, analyze, rewrite, compile, IL verify, IPC, load, run) com média e p95 de 100 execuções; memória do Worker por `Process.WorkingSet64` e `GC.GetTotalMemory` reportado pelo próprio Worker; tempo frio medido com o Host recém-iniciado (3 repetições).

## Armadilhas conhecidas
- Assemblies de referência modernos concentram muitos tipos em `System.Runtime` (inclusive `System.IO.File`): restringir referências **não basta**, por isso L1 e L2 são obrigatórias.
- `goto` cria laços que o rewriter de laços não vê. Opções: instrumentar rótulos alvo de `goto` para trás ou proibir `goto` na L1 (não há necessidade pedagógica). Registrar a decisão.
- O ALC não descarrega se algo mantiver referência (delegates em estáticos, `Console.Out` apontando para um writer do assembly). Reciclar o Worker a cada N execuções é a rede de segurança.
- `GC.GetAllocatedBytesForCurrentThread` não mede alocações de outras threads, que estão proibidas pela L1 de qualquer forma.

## Saídas esperadas
- Números reais para `MVP.md` §6 e `CODE_EXECUTION.md` §8.
- Decisão sobre as referências da compilação (pasta do SDK × pacote) e sobre `goto`.
- Desenho do rewriter validado → reescrito na Fase 1 (F1-71, F1-72).

---
*As seções abaixo são preenchidas ao terminar.*

## O que foi feito

## Resultado

## Decisão
- [ ] Hipótese confirmada: seguir com a arquitetura documentada
- [ ] Hipótese refutada: alternativa adotada = …
- [ ] Inconclusivo: próximo passo = …

**ADR criado ou atualizado:**

## Riscos que sobraram
