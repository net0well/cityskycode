# Sistema de Desafios

> **Status:** v1.0 · **Data:** 2026-10-08 · **MVP:** sim
> Relacionados: `VALIDATION_SYSTEM.md`, `HINT_SYSTEM.md`, `ERROR_SYSTEM.md`, `LEARNING_ENGINE.md`.

---

## 1. Tipos de desafio
| Tipo | Onde roda | Validação | Exemplo |
|---|---|---|---|
| **Lição guiada** | cidade da campanha (Script) + checagem | estado da cidade após a execução | "construa 3 casas com isolamento bom" |
| **Laboratório** | cenário *headless* determinístico | testes ocultos + métricas + estrutura | "implemente `CalculateEnergyConsumption`" |
| **Missão de problema** (§27) | nasce de um problema da cidade, resolvida em Laboratório e **aplicada** à cidade | Laboratório + aplicação | "coleta de lixo lenta" → algoritmo de mais próximo |
| **Refatoração** | código legado fornecido | testes (comportamento preservado) + estrutura (CSK4) | "divida a `CityManager`" |
| **Escreva o teste** | código com bug fornecido | testes do aluno devem **falhar no código com bug** e **passar no correto** (mutantes) | "pegue o bug da tarifa" |
| **Parsons** (iniciantes) | arrastar linhas para ordenar | ordem/estrutura | lição 1–3 (opcional) |

## 2. Formato de conteúdo (`content/challenges/<id>/`)
```
energy-consumption-01/
├── challenge.json        # metadados, critérios, regras
├── narrative.pt-BR.md    # problema, teoria curta, explicação final
├── starter/Program.cs    # código inicial (pode ter lacunas)
├── starter/House.cs
├── tests/HiddenTests.cs  # testes ocultos (compilados junto no Worker)
├── tests/VisibleTests.cs # testes que o aluno vê (Fase 17)
├── solution/             # solução de referência (CI)
├── wrong/                # soluções erradas comuns + feedback esperado (CI)
│   ├── returns-zero.cs
│   └── expected.json
└── hints.pt-BR.json      # Hint 1–3, solução parcial, completa, explicação
```

### 2.1 `challenge.json`
```json
{
  "id": "energy-consumption-01",
  "version": 2,
  "title": "Calculando o consumo",
  "concepts": ["methods.return", "methods.parameters"],
  "prerequisites": ["objects.properties"],
  "mode": "laboratory",
  "scenario": { "seed": 1207, "preset": "small-street-6-houses", "ticks": 11520 },
  "entry": { "type": "EnergyCalculator", "method": "CalculateEnergyConsumption" },
  "budgets": { "timeMs": 10000, "budgetTicks": 2000000 },
  "criteria": [
    { "id": "c1", "kind": "tests", "suite": "HiddenTests", "weight": 3 },
    { "id": "c2", "kind": "cityMetric", "metric": "energy.balanceKw", "op": ">=", "value": 0, "weight": 1 },
    { "id": "c3", "kind": "structure", "rule": "MethodReturnsValue(CalculateEnergyConsumption)", "weight": 1 }
  ],
  "passThreshold": 1.0,
  "unlocksOnPass": ["concept:methods.return", "building:SolarPanels"],
  "applyToCity": { "kind": "formula", "target": "House.EnergyConsumption" }
}
```

## 3. Ciclo de vida de uma tentativa
```
Abrir desafio → narrativa + previsão (PRIMM) → editar
  → Executar (opcional, modo de experimentação: roda o cenário e mostra a cidade-laboratório)
  → Validar → ValidationReport (5 níveis) → feedback
     ├─ falhou → mensagens + oferta de dica (HINT_SYSTEM)
     └─ passou → explicação + comparação com a referência + desbloqueios + aplicar na cidade
```

## 4. "Aplicar na cidade"
Desafios que resolvem um problema real são **instalados** na cidade:
| `applyToCity.kind` | Efeito |
|---|---|
| `formula` | substitui uma fórmula derivada (ex.: o consumo das casas passa a usar o método do aluno, avaliado no Worker por projeto de casa, em cache) |
| `policy` | registra um comportamento (Fase 9: `IBehaviorEvaluator`) |
| `script` | executa um script uma vez (ex.: construir infraestrutura) |
| `unlock` | apenas desbloqueia (prédio, API, ferramenta) |

A instalação passa pelas mesmas validações e custos. Se o código instalado depois falhar em runtime, entra o *fallback* para a regra padrão + problema "seu sistema falhou" → oportunidade de depuração.

## 5. Garantia de qualidade do conteúdo (CI)
- `CitySky.Content.Tests` carrega **todos** os desafios e verifica:
  - a solução de referência passa com nota 1,0;
  - cada solução em `wrong/` falha **com o feedback esperado** (`expected.json`);
  - o código inicial compila (ou falha só onde a lacuna é intencional);
  - as dicas existem nos 6 níveis;
  - os pré-requisitos referenciam conceitos existentes, e o grafo continua acíclico;
  - o cenário é determinístico (2 execuções → mesmo hash).
- Revisão pedagógica humana antes de publicar o desafio.

## 6. Anti-trapaça (escopo realista)
Os testes ocultos ficam no disco do jogador (o jogo é local), então trapacear é possível. Não tentamos impedir. Mitigamos o incentivo: sem ranking no MVP, o domínio é medido por variações e revisões, e o feedback valoriza o processo. Se um dia houver ranking, a validação passa a ser feita num servidor (fora de escopo).
