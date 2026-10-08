# Sistema de Mapa e Terreno

> **Status:** v1.0 · **Data:** 2026-10-08 · **MVP:** sim (terreno fixo 2×2 km, grade de análise, seleção) · Inclui o §10 (Terreno)

---

## 1. Intenção (§8, §10)
O mapa é um **sistema real**: espaço navegável, camadas de informação, seleção de áreas e um terreno que **afeta** a cidade.

## 2. Estrutura espacial
| Estrutura | Resolução | Uso |
|---|---|---|
| **Heightmap** (Unity Terrain na apresentação + cópia inteira em mm no domínio) | 1 m (MVP: 2049² para 2 km) | elevação, inclinação, validade de construção |
| **Lotes** | 8 × 24 m | construção e zoneamento |
| **Grade de análise** | **32 m** (MVP: 64 × 64 células = 2 km) | overlays, campos (poluição, valor do terreno, cobertura), estatísticas |
| **Bairros** | polígonos | Fase 10 (MVP: grade automática de setores para relatórios) |
| **Chunks de renderização** | 256 m | combinação de malhas (ruas, terreno), culling |

O domínio guarda **somente o necessário** (alturas amostradas, máscara construível, corpos d'água) em tipos próprios. A Unity Terrain é a **apresentação** do relevo.

## 3. Terreno (§10)
| Recurso | MVP | Fase |
|---|---|---|
| Elevação, montanhas suaves | ✅ (heightmap fixo, feito à mão) | — |
| Lagos | ✅ decorativo | 12: simulado (água) |
| Rios | — | 12 (direção do fluxo: contaminação a jusante) |
| Praias / mar | — | 19 |
| Vegetação | ✅ árvores instanciadas (Terrain trees) | 10: remoção ao construir, parques |
| Áreas construíveis / proibidas | ✅ máscara | — |
| Edição de terreno pelo jogador | — | 19 |

### Terreno afeta a cidade (§10)
| Inclinação | Efeito |
|---|---|
| 0–5% | normal |
| 5–12% | construção +15–40% de custo; ruas +20%; valor do terreno −5% |
| 12–25% | só prédios pequenos; ruas não permitidas; risco de deslizamento (evento, Fase 19) |
| > 25% | não construível |

## 4. Interações do jogador (§8)
Aproximar, afastar, rotacionar e mover a câmera (`CAMERA_SYSTEM.md`) · selecionar objeto (raycast em colisores simplificados ou em *picking buffer*, Fase 18) · **selecionar área** (retângulo/laço → estatísticas da seleção, Fase 10) · visualizar bairros, ruas, terrenos, zonas e infraestrutura (camadas, ver `MAP_OVERLAYS.md`).

## 5. Seleção e picking
- MVP: colisores simples (caixas) em prédios, *capsules* em agentes, malha de colisão por chunk para ruas e terreno.
- Fase 18: *picking* por ID em buffer de GPU (sem colisores para 100k objetos).

## 6. Persistência
O terreno do MVP é um asset fixo (não salvo). A partir da Fase 19 (edição), as diferenças em relação à base são salvas.

## 7. Testes
Cálculo de inclinação, máscara construível, conversão mundo ↔ célula ↔ lote, consistência domínio × apresentação (amostras de altura iguais).
