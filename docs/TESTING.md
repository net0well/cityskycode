# Testes

> **Status:** v1.0 · **Data:** 2026-10-08 · **ADR:** [0016](ADR/0016-test-framework.md) · **MVP:** Parte A (sim), Parte B (Fase 17)
> Duas partes: **(A)** como **nós** testamos o projeto; **(B)** testes como **conteúdo** para o jogador (§44).

---

# Parte A: Testes do projeto (engenharia)

## A1. Framework: NUnit em todo lugar
- **Unity:** Unity Test Framework (NUnit), EditMode e PlayMode. `com.unity.test-framework.performance` para testes de desempenho.
- **.NET (`dotnet/tests`):** **NUnit 4** com modelo de *constraints* (`Assert.That(x, Is.EqualTo(y))`), que é compatível com o estilo do NUnit da Unity e com o que o aluno aprende.
- **Override do dotnet-claude-kit:** o skill `testing` recomenda xUnit v3 + WebApplicationFactory + Testcontainers (padrão de APIs web). Aqui **não se aplica** (ADR-0016). O resto do skill vale: AAA obrigatório, testar comportamento e não implementação, nomes descritivos.

## A2. Pirâmide (adaptada a jogo + simulação)
| Camada | Onde roda | Velocidade | O que cobre | Meta |
|---|---|---|---|---|
| **Unidade de domínio** | `dotnet test` | ms | regras (consumo, felicidade, ledger, pathfinding) | maioria dos testes |
| **Invariantes/propriedades** | `dotnet test` | s | conservação de dinheiro, sem órfãos, faixas válidas, após cada tick de cenários | todo sistema |
| **Determinismo** | `dotnet test` + Unity EditMode | s | hash de estado: .NET 10 × Mono × (CI noturno) IL2CPP | obrigatório |
| **Contrato (IPC)** | `dotnet test` | ms | golden JSON de cada mensagem | toda mensagem |
| **Code Host / pipeline** | `dotnet test` | s | parse, análise, compilação, execução, orçamentos | toda etapa |
| **Segurança (corpus)** | `dotnet test` (Windows) | s | `CODE_EXECUTION_SECURITY.md` §5 | todo PR do escopo |
| **Conteúdo** | `dotnet test` | s | desafios: referência passa, erradas falham com o feedback certo | todo desafio |
| **Arquitetura** | `dotnet test` + `/arch-check` | s | Domain sem Unity, sem `DateTime.Now`, dependências entre projetos | sempre |
| **Balanceamento** | CI noturno (*headless*) | min | cidades roteirizadas por 1–10 anos dentro de faixas | por marco |
| **Unity EditMode** | Unity batch | s | adaptadores, read models → views, ferramentas | camada de apresentação |
| **Unity PlayMode / integração** | Unity batch (Windows) | min | fluxo ponta a ponta: UI → comando → domínio → view; Unity + Code Host real | fluxos críticos do MVP |
| **Desempenho** | Unity Performance Testing + BenchmarkDotNet | min | orçamentos (`PERFORMANCE.md`) com limiares | sistemas quentes |
| **Playtest** | humanos | — | aprendizagem e diversão | a cada fase |

## A3. Convenções
```csharp
[TestFixture]
public sealed class HouseEnergyTests
{
    [Test]
    public void EnergyConsumption_WithExcellentInsulation_IsLowerThanBasic()
    {
        // Arrange
        var basic = HouseRules.Consumption(residents: 4, Insulation.Basic, solar: false);
        // Act
        var excellent = HouseRules.Consumption(residents: 4, Insulation.Excellent, solar: false);
        // Assert
        Assert.That(excellent, Is.LessThan(basic));
    }
}
```
- Nome: `Metodo_Cenario_Resultado`. AAA com comentários. Um comportamento por teste.
- **Test Data Builders** para cenários: `new CityBuilder().WithRoad(200).WithHouses(6).WithPowerPlant(400).Build()`.
- Sem `Thread.Sleep`, sem tempo real, sem ordem entre testes.
- Teste de domínio no código compartilhado é escrito em C# 14 (o projeto de teste é .NET 10), mas só pode chamar APIs C# 9 do domínio.

## A4. Teste de determinismo (modelo)
```csharp
[Test]
public void SameSeedAndCommands_ProduceSameStateHash()
{
    var a = HeadlessSimulation.Create(seed: 42, preset: "mvp-village");
    var b = HeadlessSimulation.Create(seed: 42, preset: "mvp-village");
    var script = CommandScript.Load("fixtures/mvp-3-days.json");
    a.Run(script, ticks: 3 * 11_520);
    b.Run(script, ticks: 3 * 11_520);
    Assert.That(a.StateHash(), Is.EqualTo(b.StateHash()));
    Assert.That(a.StateHash(), Is.EqualTo(Golden.Hash("mvp-3-days")));   // também comparado no Mono (UTF)
}
```

## A5. CI
| Gatilho | Jobs |
|---|---|
| Todo PR | `dotnet build` + `dotnet test` (exceto balanceamento) + `/verify`, que pode ser rodado localmente pelos agentes |
| PR tocando `unity/**` | Unity EditMode + PlayMode (GameCI, licença em *secrets*) |
| PR tocando execução de código | + corpus de segurança (runner Windows) |
| Noturno | balanceamento, determinismo IL2CPP (build de player de teste), benchmarks com histórico |

## A6. Cobertura
Meta de **comportamento**, não de porcentagem: toda regra do domínio documentada tem teste. `get_test_coverage_map` (Roslyn MCP) ajuda a achar lacunas.

---

# Parte B: Testes como conteúdo para o jogador (Fase 17)

## B1. O que o aluno aprende
`[Test]`, AAA, `Assert.That`, nomes de teste, casos de borda, fakes/stubs (com DIP), "teste que pega bug", TDD leve (vermelho → verde → refatora).

## B2. Como funciona tecnicamente
- O aluno escreve classes com métodos `[Test]` (API NUnit: `nunit.framework` referenciado na compilação e na allowlist, só para atributos e `Assert`/`Is`/`Has`).
- **Descoberta e execução dentro do Worker** por reflexão própria (não usamos o runner do NUnit, que é pesado e acessa o sistema de arquivos). Cada teste roda com orçamento.
- Resultado no console estilo IDE: ✔/✘ por teste, mensagem do `Assert`, linha.

## B3. Tipos de desafio
| Desafio | Validação |
|---|---|
| "Escreva um teste para o cálculo da conta de luz" | o teste do aluno **passa** na implementação correta |
| "Pegue o bug" | o teste do aluno **falha** na implementação com bug e **passa** na corrigida |
| "Mate os mutantes" | o conjunto de testes do aluno precisa falhar em ≥ 80% das variações erradas (mutantes) geradas do código-alvo (ex.: `<` ↔ `<=`, `+` ↔ `-`, constante alterada) |
| "Corrija sem quebrar" | testes visíveis + ocultos passam após a correção |
| "TDD": implemente a partir dos testes | testes fornecidos ficam verdes; o nível 5 checa que não há lógica "especial para o teste" |

## B4. Ligação com a cidade
Bugs reais "acontecem" na cidade: a tarifa cobra em dobro de quem tem painel solar, os moradores reclamam e o termômetro cai. O aluno reproduz o bug com um teste, corrige, e a cidade se recupera.
