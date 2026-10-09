# Spike S1a: Pipeline do Code Host (Roslyn → Worker com orçamentos)

- **Status:** **Concluído** (hipótese confirmada; 10/10 casos do corpus passaram)
- **Prazo:** 3 dias · **Ordem:** 2º · **Início / fim:** 2026-10-09 → 2026-10-09 (meio dia)
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
| Compilar e executar "hello" com Host quente (p95 de 100 execuções) | ≤ 1,5 s | ✅ **ponta a ponta p95 39 ms** (compilar+carregar+executar) |
| Primeira compilação após iniciar o Host (frio) | ≤ 4 s | ✅ **232 ms** o primeiro programa do aluno; aquecimento do Host (uma vez, em segundo plano) ≈ 12 s nesta CPU de 2014 (ver "Resultado") |
| `for (int i = 0; i < 10; i--) {}` → `BudgetExceeded` com a linha certa | ≤ 2 s, linha correta | ✅ `budget/Loop` na **linha 2** (linha do `for`) |
| Recursão infinita → detectada pela profundidade (256) **antes** do `StackOverflow`, com a linha | sim | ✅ `budget/Recursion` na linha da função (inclui corpo-expressão `=>`) |
| Se o rewriter for contornado, `StackOverflow` mata só o Worker; o Host recicla e responde | sim | ✅ `goto` para trás (laço não instrumentado) → **watchdog** encerra o Worker em 2 s, o Host recicla e segue |
| `try { laço } catch (Exception) { }` não engole o `BudgetExceeded` | sim | ✅ o filtro no `catch` deixa o estouro propagar |
| `var l = new List<byte[]>(); while (true) l.Add(new byte[1 << 20]);` → contido com mensagem | sim | ✅ `budget/Memory` |
| `System.IO.File.ReadAllText("x")` rejeitado na **L1** (símbolo) | `CSK1001`, nada emitido | ✅ `CSK1001` em L1, nada emitido |
| O mesmo acesso, com a L1 desligada, rejeitado na **L2** (verificação de IL) | sim | ✅ `CSK1001` em L2 |
| 200 execuções seguidas no mesmo Worker: memória cresce ≤ 10% (ALC descarrega) | sim, ou reciclar a cada N | ✅ *working set* do Worker **45 → 34 MB**; heap gerenciado 0,4 MB; 6 reciclagens (só após violação) |
| Linhas e colunas reportadas batem com o código original (após o rewriter) | sim | ✅ NullReference reportada na **linha 4** (exata), com a compilação em modo Debug |

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
- **Ambiente:** Intel Xeon E5-2670 v3 (2014, 12/24 núcleos), 32 GB, Windows 11 25H2, .NET 10.0.11, build Release. Pacote de referência do .NET 10 em `Microsoft.NETCore.App.Ref` **10.0.11**; `Microsoft.CodeAnalysis.CSharp` **5.9.0**.
- **Quatro projetos** em `spikes/S1a-codehost-pipeline/` (`dotnet run` no `CitySky.Spike.CodeHost`):
  - **Runtime** (`__Budget`): contador de iterações, profundidade (para a recursão) e amostragem de alocação; a exceção de orçamento não é capturável pelo aluno (estado marcado + filtro no `catch`).
  - **Contracts**: protocolo `[uint32 tamanho][json]` por stdin/stdout, com limite de tamanho.
  - **Worker**: processo que recebe o binário, carrega num `AssemblyLoadContext` coletável, executa o ponto de entrada com os orçamentos, redireciona a saída de console para um buffer limitado, classifica o resultado (ok/budget/exception) e descarrega o contexto.
  - **CodeHost**: pipeline Parse → **L1 (lista por símbolo)** → **reescrita de orçamento** → compilação da árvore instrumentada → emissão PE+PDB → **L2 (verificação do IL)**, mais o gerenciador do Worker (cronômetro de parede como rede de segurança e reciclagem).
- **Corpus de 10 casos**, cada um com um resultado verificável. Comando: `dotnet run -c Release --project src/CitySky.Spike.CodeHost -- --out results/s1a-results.json`.

## Resultado
**Corpus: 10/10 passaram.** hello (ok) · laço regressivo (`budget/Loop`, linha 2) · recursão infinita (`budget/Recursion`) · laço dentro de try/catch (não engolido) · crescimento de memória (`budget/Memory`) · `goto` para trás (watchdog) · `System.IO.File` barrado em L1 · o mesmo barrado em L2 com L1 desligada · NullReference na linha exata · saída de console truncada no limite.

**Tempos** (depois do aquecimento do Host):
| Medida | Valor |
|---|---|
| Ponta a ponta do "hello" quente (compilar + carregar + executar), p50 / p95 / máx (50 execuções) | **27 / 39 / 48 ms** |
| Primeiro programa do aluno após o aquecimento (compilação) | 232 ms |
| Carga do binário no Worker | 1–2 ms |
| **Aquecimento do Host** (carregar o pacote de referência + primeiro uso do Roslyn), uma vez | **≈ 12 s** nesta CPU de 2014 |
| 200 execuções: *working set* do Worker (primeira → última) | 45 → 34 MB (estável) |

**Resposta à pergunta:** sim. O Roslyn 5.9 no .NET 10 analisa pela lista, instrumenta, emite e entrega a um Worker separado que executa com laço, recursão, memória e saída contidos, e reporta a linha original. O custo em regime (39 ms p95) fica muito abaixo do alvo de 1,5 s.

### Achados e decisões
1. **`goto` para trás cria um laço que o rewriter de laços não vê** → só o cronômetro de parede (2 s) o pega. **Decisão:** barrar `goto` na L1 em produção (não há necessidade pedagógica no MVP), ou instrumentar os rótulos alvo de `goto` para trás. Registrado como item da Fase 1.
2. **Métodos e funções locais com corpo-expressão (`=>`)** precisam ser instrumentados (convertidos para bloco com Enter/try/finally); senão a recursão por `=>` estoura a pilha nativa de verdade. **Feito no spike** — é obrigatório na Fase 1.
3. **A verificação de IL (L2) precisa liberar os atributos de infraestrutura que o compilador emite** no assembly (`DebuggableAttribute`, `TargetFrameworkAttribute`, `Assembly*Attribute`) e os tipos de suporte (`CompilerServices.*`). No produto, isso é uma seção "infra" da `allowlist.json`. Sem isso, **todo** programa é rejeitado em L2.
4. **Compilar o código do aluno com otimização desligada (Debug)** dá o mapeamento de linha exato para as exceções de runtime — importante para o feedback pedagógico. Diverge do `CODE_EXECUTION.md` §4 (que dizia Release); **atualizar o documento**. O custo é desprezível em programas pequenos.
5. **Referenciar a BCL inteira do pacote de referência** e filtrar por símbolo (L1) e por IL (L2) funciona, confirmando a lição do §4.1 ("restringir referências não basta"). `System.IO.File` vive em `System.Runtime`, que é referenciado, e mesmo assim é barrado.
6. **A Player API precisa ser um assembly real na pasta do Worker** (como no produto). O contexto de carga do aluno a resolve explicitamente da pasta, porque ela não está no `deps.json` do Worker.
7. **Preservação de linha na reescrita:** as chamadas de orçamento são inseridas **na mesma linha** das construções (sem criar linhas novas), e o número de linha vai como argumento. As linhas do original são preservadas — não foi preciso `#line`.
8. **Aquecimento de ~12 s** nesta CPU antiga é maior que o esperado (o doc estimava 1–3 s). Mitigações para a Fase 1: publicar com **ReadyToRun**, referenciar só as fachadas necessárias (não as ~160 do pacote) e aquecer em segundo plano na abertura do jogo. Medir de novo numa máquina mais nova e numa Iris Xe de escola.

## Decisão
- [x] Hipótese confirmada: seguir com a arquitetura do ADR-0005/0006
- [ ] Hipótese refutada
- [ ] Inconclusivo

**ADR criado ou atualizado:** nenhum ADR novo. Atualizações de documento para a Fase 1: `CODE_EXECUTION.md` §4 (compilar o aluno em Debug, não Release; seção "infra" da allowlist; barrar `goto` no MVP), `CODE_EXECUTION_SECURITY.md` (L3 cobre corpo-expressão; L2 com atributos de infraestrutura), `MVP.md` §6 e `CODE_EXECUTION.md` §8 (tempos medidos). Esta foi a validação sem a Unity; o IPC com o player IL2CPP é o S1b, e o Job Object/AppContainer é o S6.

## Riscos que sobraram
- **R04 (sandbox burlado):** o spike valida L1–L4 (lista por símbolo, verificação de IL, orçamentos, processo separado com watchdog). **L5 (Job Object + AppContainer) é o S6** e é a camada que de fato contém um atacante — ainda pendente (bloqueado só pela ativação da licença não; o S6 é só .NET e pode rodar).
- **Aquecimento de 12 s** até medir numa máquina nova/escola (achado 8).
- **`goto` e corpo-expressão** precisam da decisão/implementação definitiva na Fase 1 (achados 1 e 2).
- **Descarregamento do ALC:** no spike, a reciclagem a cada violação + o GC mantêm a memória estável; falta o teste de descarregamento com `WeakReference` sob carga longa (Fase 1).
