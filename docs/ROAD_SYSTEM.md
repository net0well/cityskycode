# Sistema de Estradas

> **Status:** v1.0 · **Data:** 2026-10-08 · **ADR:** [0011](ADR/0011-road-system.md) · **MVP:** segmentos retos, 1 tipo de rua, interseções automáticas

---

## 1. Intenção
A rede viária é o esqueleto da cidade: define acessibilidade, lotes, rotas, condução de energia (MVP) e trânsito. É um dos sistemas mais difíceis tecnicamente (geometria de interseções).

## 2. Divisão de responsabilidades
| Camada | Responsabilidade |
|---|---|
| **Domain** | grafo (`RoadNode`, `RoadSegment`, `Lane`), geometria autoritativa (`Bezier2` em mm), tipos de rua, regras (conexão, sobreposição, custo), conectividade de faixas, lotes |
| **Application** | ferramentas como comandos (`BuildRoadSegment`, `SplitSegment`, `DemolishRoadSegment`), validação |
| **Presentation** | ferramenta de desenho (snap, pré-visualização), **Unity Splines** para edição e extrusão, geração de malha (pista, calçada, meio-fio, interseções), decals de sinalização |

O domínio **não** usa `UnityEngine.Splines`: a geometria é própria (C# puro) e convertida na apresentação.

## 3. Modelo de dados (§7)
```
Road (RoadSegment)
├── ID                 RoadSegmentId
├── Geometry           Bezier2 (MVP: reta = Bézier degenerada)
├── Lanes              LaneId[] (derivadas do RoadType)
├── SpeedLimit         do RoadType (km/h)
├── Direction          TwoWay | OneWay(start→end)
├── TrafficCapacity    veículos/h por faixa × faixas
├── Connections        nós inicial/final; conectividade de faixas nos nós
├── TrafficLights      no nó (RoadNode.Control = Signal) + plano de fases
└── Sidewalks          esquerda/direita (largura) → grafo de pedestres
```

## 4. Operações e regras
| Operação | MVP | Regra |
|---|---|---|
| Criar rua | ✅ | comprimento ≥ 16 m; sem cruzar prédio; inclinação ≤ 12% |
| Prolongar | ✅ | snap no nó final |
| Conectar / criar cruzamento | ✅ | ao cruzar um segmento existente: divide-o e cria um nó (até 4 ramos no MVP; ângulo mínimo de 30°) |
| Remover | ✅ | demole prédios cujo lote perde o acesso (com confirmação) |
| Largura / faixas / mão única / mão dupla | Fase 11 | troca de `RoadType` (comando `UpgradeSegment`) |
| Adicionar faixa | Fase 11 | via `RoadType` |
| Rotatória | Fase 11+ | nó do tipo `Roundabout` com anel gerado |
| Ponte / túnel | Fase 19 | segmento com elevação e pilares; restrições de terreno |
| Semáforo | Fase 11 | controle do nó + plano de fases (State pattern) |
| Faixa de pedestre | Fase 11 | elemento do nó; afeta o grafo de pedestres e a capacidade |
| Curvas | Fase 11 (3b) | Bézier cúbica com raio mínimo |

## 5. Conectividade de faixas
Gerada automaticamente em cada nó a partir de regras (à direita para a direita etc.). Editável na Fase 11+. Alimenta o pathfinding de veículos e o modelo meso.

## 6. Lotes
Ao criar um segmento, são gerados lotes dos dois lados (frente de 8 m, fundo de 24 m), descartando os que colidem com outros lotes, ruas ou terreno inválido. Lotes guardam `SegmentId` e `Side` (acesso). Ver `ZONE_SYSTEM.md`.

## 7. Terreno
Ao construir, a apresentação suaviza o terreno sob o segmento (Unity Terrain `SetHeights` regional, com orçamento). O domínio registra a inclinação, que entra no custo (+% por inclinação) e na validade. Inclinação alta aumenta custo e risco e reduz o valor do terreno (§10).

## 8. Geração de malha (Presentation)
- Segmento: extrusão de perfil (pista + meio-fio + calçada) ao longo da curva, com UVs contínuas.
- Interseção: polígono calculado pelas bordas dos segmentos recuadas do nó, com triangulação e cantos arredondados. **Ponto técnico de maior risco** (spike S5).
- Sinalização: decals (faixas, setas, faixa de pedestre).
- LOD: malha simplificada a distância; uma malha combinada por *chunk* (ex.: 256 m) para reduzir draw calls.

## 9. Player API
Leitura: `city.GetRoads()`, `Road.Name`, `Road.LengthMeters`. Construir ruas via código fica para a Fase 10 (laços: "construa uma grade de 4×4 quarteirões"), sempre com o custo normal.

## 10. Testes
Domínio: divisão de segmento ao cruzar, ângulo mínimo, conectividade gerada, lotes gerados e descartados, custo por metro e inclinação. Apresentação: testes visuais (snapshots de malha de interseções de referência) no spike S5 e depois em PlayMode.
