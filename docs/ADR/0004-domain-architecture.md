# ADR-0004: Camadas limpas, domínio orientado a dados e shared source

- **Status:** Aceito
- **Data:** 2026-10-08
- **Documentos afetados:** `ARCHITECTURE.md`, `DOMAIN_MODEL.md`, `CODING_STANDARDS.md`

## Contexto
O domínio precisa ser independente da Unity (§30) e rodar também no Code Host (.NET 10) para validar desafios *headless*. Precisa escalar a 100k+ cidadãos com o GC do Mono/Boehm (não geracional) e ser determinístico.

## Decisão
1. **Camadas:** Presentation → Application → Domain. Infrastructure implementa portas. Presentation lê read models e envia comandos, sem referenciar o Domain.
2. **Domain e Application** em C# 9 / `netstandard2.1`, **sem dependências externas**, num pacote UPM local (`com.citysky.core`) com `noEngineReferences: true`.
3. **Shared source:** os mesmos `.cs` são compilados pela Unity e por projetos SDK em `dotnet/` (via `<Compile Include>` com `LangVersion 9`), o que dá testes rápidos, Code Host e Roslyn MCP sobre o mesmo código.
4. **Orientado a dados:** stores SoA + IDs fortemente tipados, sem `class` por entidade de alto volume.
5. **Estado autoritativo sem ponto flutuante** (inteiros/ponto fixo); dinheiro em centavos (`long`).
6. **Toda mutação por comando**, com eventos de domínio em buffer por tick.
7. **Pure DI** com composition root único; sem singletons nem service locator.

## Alternativas consideradas
| Alternativa | Por que não |
|---|---|
| DLL pré-compilada do domínio importada na Unity | depuração e iteração piores; dois passos de build |
| Domínio como objetos ricos (uma classe por cidadão) | GC, escala, serialização, determinismo |
| DI container (VContainer/Zenject) | dependência sem problema comprovado (§65); reavaliar com ADR |
| Vertical Slice puro | a simulação exige um núcleo de domínio compartilhado; usamos *feature folders* dentro da Application |

## Consequências
**Positivas:** domínio portátil, testável em ms, determinístico e analisável pelo dotnet-claude-kit.
**Negativas:** C# 9 no núcleo; disciplina de orientação a dados (menos "OO" no núcleo, explicada como conteúdo).
**Riscos:** divergência de comportamento Mono × CoreCLR. Mitigação: testes de hash cruzados.

## Gatilhos de revisão
Unity com C# moderno estável · necessidade de dependência externa no domínio.
