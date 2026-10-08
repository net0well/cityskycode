# Overlays de Mapa

> **Status:** v1.0 · **Data:** 2026-10-08 · **ADR:** [0013](ADR/0013-map-overlay-system.md) · **MVP:** Energia e Felicidade

---

## 1. Problema com a interface proposta
```csharp
public interface IMapOverlay { Color GetColor(Vector3 position); }   // proposta do briefing (§9)
```
1. `Color`/`Vector3` são tipos **da Unity**: o overlay não roda no domínio nem no Code Host.
2. **Uma chamada por posição** (pixel, vértice ou prédio) é uma armadilha de desempenho. Com overlay do jogador, ainda seria IPC por posição.
3. Mistura **cálculo do dado** com **escolha de cor** (viola SRP, justamente o que o jogo ensina).

## 2. Arquitetura adotada
```
Dado (Domain/Application/Worker)            Apresentação (Unity)
IOverlaySource ──► OverlayField ──────────► OverlayRenderer
 (calcula valores)  (grade 32 m ou            ├─ textura R16 na grade → shader de terreno/ruas
                     valores por entidade)    ├─ cor por instância de prédio (GRD/MPB)
                                              └─ legenda (gradiente/categorias)
```

### Contratos
```csharp
// Domain/Application (C# 9, sem Unity)
public interface IOverlaySource
{
    OverlayId Id { get; }
    OverlayKind Kind { get; }                     // Grid | PerBuilding | PerSegment | Categorical
    OverlayLegend Legend { get; }                 // faixas, unidades, rótulos pt-BR
    void Compute(CityReadModel city, OverlayField output);   // preenche valores (int/Fixed)
    int RefreshPeriodTicks { get; }
}
```
- **Cor** é responsabilidade da apresentação: a legenda define uma rampa (gradiente) ou categorias, e o shader amostra um **LUT**.
- Paletas **amigáveis a daltonismo** (viridis/cividis-like) por padrão, e alternativa de alto contraste.
- Quando o overlay está ativo, a cena base é **dessaturada** (o visual clássico de city builders) e os valores aparecem por cima.

### Versão pedagógica (Player API, Fase 15/16)
```csharp
public interface IMapOverlay
{
    string Name { get; }
    double Evaluate(CellInfo cell);     // 0..1 (ou valor com legenda)
}
// exemplo do aluno
public sealed class ShadeOverlay : IMapOverlay
{
    public string Name => "Sombra de árvores";
    public double Evaluate(CellInfo cell) => Math.Min(1.0, cell.TreeCount / 10.0);
}
```
- Avaliado no **Worker**, em lote, sobre a grade inteira (64×64 = 4.096 células no MVP), a cada N horas TC. Ensina **interface, polimorfismo, abstração e Strategy** (§9) sem matar o desempenho.

## 3. Catálogo
| Overlay (§8) | Tipo | Fase |
|---|---|---|
| Normal | — | MVP |
| **Energia** (abastecido/cortado) | por prédio, categórico | **MVP** |
| **Felicidade** | por prédio | **MVP** |
| População / Densidade | grade | 10 |
| Renda | por prédio | 10 |
| Valor do terreno | grade | 10 |
| Empregos | por prédio | 10 |
| Zonas | por lote, categórico | 10 |
| Tráfego | por segmento | 11 |
| Água / Esgoto | por prédio + rede | 12 |
| Poluição (ar/solo/ruído) | grade | 12 |
| Lixo | por prédio | 12 |
| Saúde / Educação / Crime | grade + cobertura | 13 |

## 4. Interação
Barra de overlays (teclas 1–9), legenda com valores, *hover* mostra o valor da célula ou prédio, clique abre o Inspector com a explicação do valor.

## 5. Desempenho
- Cálculo em baixa frequência (configurado por overlay), só quando o overlay está visível ou há assinante (ex.: o termômetro usa os dados).
- Upload da textura da grade: ≤ 0,1 ms. Cores por instância via propriedade de material compatível com o GPU Resident Drawer.

## 6. Testes
Cada `IOverlaySource` com cenários de valores esperados; legenda consistente; overlay do aluno avaliado com orçamento (lento → aviso e uso do valor anterior).
