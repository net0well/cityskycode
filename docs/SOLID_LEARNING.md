# Aprendizagem de SOLID

> **Status:** v1.0 (intenção de design) · **Data:** 2026-10-08 · **Fase:** 15 · **MVP:** não

---

## 1. Abordagem
SOLID é ensinado **pela dor**: o jogo entrega ao aluno o **"código legado da prefeitura"**, um sistema que funciona mas é difícil de mudar. A cidade cresce, os pedidos de mudança chegam, e o aluno sente o custo. Só então o princípio aparece como alívio (§42, §71, §72).

> Nossa própria arquitetura **nunca** tem uma `CityManager` que faz tudo. Ela existe só como conteúdo (`content/legacy/CityManager.cs`).

## 2. A vilã: `CityManager`
Classe fornecida com ~400 linhas: calcula impostos, paga salários, distribui energia, coleta lixo, envia caminhões e gera relatórios. Testes de comportamento (ocultos) garantem que qualquer refatoração **preserve** o funcionamento.

## 3. Um princípio, uma dor

| Princípio | Dor provocada pela cidade | Desafio | Validação (CSK4 + testes) |
|---|---|---|---|
| **SRP** (responsabilidade única) | "Mudamos a regra de impostos e o lixo parou de ser coletado" (mudança num ponto quebra outro) | extrair `TaxService`, `EnergyService`, `GarbageService` da `CityManager` | comportamento preservado; nenhuma classe do aluno com > N responsabilidades (coesão LCOM) |
| **OCP** (aberto/fechado) | "Chegou um novo tipo de usina, e você precisa mexer em 6 `switch`es" | introduzir abstração para fontes de energia; adicionar `WindFarm` **sem editar** classes existentes | teste: adicionar `WindFarm` só com arquivo novo; diff das classes antigas = vazio |
| **LSP** (substituição) | "O `ElectricBus : Bus` lança exceção em `Refuel()`, e a garagem quebra" | redesenhar a hierarquia (capacidade de reabastecer como composição/interface) | nenhuma sobrescrita lança `NotSupportedException`; testes da garagem passam para todos os subtipos |
| **ISP** (segregação de interfaces) | "A casa solar é obrigada a implementar `ConsumeGas()` de `IUtilityConsumer`" | dividir em `IPowerConsumer`, `IWaterConsumer`, `IGasConsumer` | nenhuma implementação vazia ou com exceção; interfaces com ≤ K membros |
| **DIP** (inversão de dependências) | "Não dá para testar a coleta de lixo sem a frota real de caminhões" | `GarbageService` depender de `IVehicleFleet`; criar fake nos testes | `GarbageService` não referencia tipo concreto de frota; teste do aluno com fake passa |

## 4. Sequência pedagógica
1. **Missão funcional** com a `CityManager` (o aluno só usa).
2. **Pedido de mudança** que dói (a dor é medida: arquivos alterados, testes quebrados).
3. **Tentativa livre**.
4. **Apresentação do princípio** com o vocabulário formal.
5. **Refatoração guiada** (code actions da Fase 15: extrair interface/classe).
6. **Novo pedido de mudança** que agora é fácil (a recompensa é sentir a diferença).
7. **Revisão espaçada** com variações.

## 5. Métricas mostradas ao aluno
Antes e depois da refatoração: linhas por classe, dependências, arquivos tocados por mudança, cobertura de testes. Os números aparecem como "relatório do engenheiro da prefeitura" e servem para conversa, não para nota.
