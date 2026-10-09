# Gráficos

> **Status:** v1.0 · **Data:** 2026-10-08 · **ADR:** [0002](ADR/0002-render-pipeline.md) · **MVP:** sim (preset Baixo/Alto)
> Relacionados: `LIGHTING.md`, `ASSET_PIPELINE.md`, `PERFORMANCE.md`.

---

## 1. Direção de arte (§4)
> **"Uma cidade que parece viva"**, com visual **moderno, limpo, detalhado, estilizado-realista, agradável de longe e legível de perto**, sem copiar assets, identidade visual ou propriedade intelectual de *Cities: Skylines*.

| Pilar | Tradução prática |
|---|---|
| Legibilidade | silhuetas distintas por tipo de prédio; cores de zona sutis; contraste figura/fundo |
| Vida | movimento constante (pessoas, carros, árvores ao vento), luzes à noite, pequenas animações (fumaça da usina) |
| Estilizado-realista | proporções reais, materiais simplificados (cores sólidas + *trim sheets*), pouco ruído de textura |
| Maquete | tilt-shift opcional, iluminação suave, sombras de contato (SSAO) |
| Desempenho | geometria barata, LOD agressivo, poucos materiais |

Referências de estilo: city builders e "dioramas" estilizados modernos. Paleta e guia em `docs/art/` (Fase 2).

**Fontes gratuitas de arte e ferramentas, e o que dá para alcançar do visual de *Cities: Skylines* sem custo:** [`docs/art/ART_SOURCES.md`](art/ART_SOURCES.md) (pesquisa de 2026-10-09).

**Ajuste por orçamento zero (ADR-0017):** a base de arte são kits **CC0** (Kenney City Kits, Quaternius), de estilo **low-poly estilizado**. O alvo visual passa de "estilizado-realista" para **"low-poly estilizado, limpo e bem iluminado"**. A qualidade vem da iluminação (dia/noite, SSAO, emissivos), da paleta coerente, da variação procedural e da vida da cidade, não da contagem de polígonos. Isso também ajuda no hardware mínimo de escola.

## 2. Pipeline: URP (não HDRP)
Resumo do ADR-0002: HDRP em manutenção (estratégia Unity 2026), URP recomendado para projetos novos e com investimento ativo, alcance de hardware maior (público estudante), lição de CS2 (GPU como gargalo).

### Configuração do URP
| Recurso | Uso |
|---|---|
| **Forward+** | muitas luzes pontuais à noite (postes, janelas próximas) |
| **GPU Resident Drawer** | instancing automático de MeshRenderers (prédios, props, árvores fora do Terrain) via BatchRendererGroup |
| **GPU Occlusion Culling** | prédios escondem o que está atrás, em zoom baixo/rasante |
| **SRP Batcher** | todos os shaders compatíveis |
| **Render Graph** | passes customizados (overlays, outline de seleção) |
| **STP** (upscaling) | presets Baixo/Médio em GPUs integradas |
| **SSAO** | contato e "maquete" |
| **Decals** | marcações de rua, faixas, áreas de zona, manchas de poluição |
| **Volumes** | pós por hora do dia (cor, exposição, bloom suave) |
| **Fog** | fog de distância/altura (URP nativo) + shader próprio se necessário |
| **VFX Graph** | chuva perto da câmera, fumaça, partículas de dinheiro (Money Flow) |

### O que não usamos e como compensamos
| HDRP tinha | Compensação no URP |
|---|---|
| Water System | shader de água estilizado (cor por profundidade, normal maps animados, espuma na borda por profundidade, reflexo do céu/probe) |
| Nuvens volumétricas | céu procedural + camadas 2D animadas + sombra de nuvens (*cookie*) |
| Fog volumétrico | fog de altura + *light shafts* falsos (opcional) |
| Céu físico | céu procedural (gradientes por hora); céu físico do URP quando lançado |

## 3. Presets de qualidade
| Preset | Alvo | Resolução interna | Sombras | Distância de LOD | SSAO | Outros |
|---|---|---|---|---|---|---|
| Baixo | GPU integrada, 30 FPS | 67% + STP | 1 cascata, 400 m | agressiva | off | sem decals distantes |
| Médio | GTX 1050 Ti | 85% + STP | 2 cascatas, 800 m | média | metade | — |
| Alto | GTX 1660 / RX 6600, 60 FPS | 100% | 3 cascatas, 1.200 m | normal | full | — |
| Ultra | RTX 3070+ | 100% (ou DLAA/TAA) | 4 cascatas | estendida | full | densidade de vegetação ↑ |

## 4. Orçamentos de renderização (iniciais; ver `PERFORMANCE.md`)
| Item | Alvo (Alto, cidade de 10k) |
|---|---|
| Draw calls (após BRG/GRD) | ≤ 3.000 |
| Triângulos visíveis | ≤ 6 M |
| Shadow casters | só prédios e árvores grandes; **props pequenos não projetam sombra** |
| Materiais únicos em cena | ≤ 60 (atlas/arrays) |
| Luzes reais | ≤ 64 por frame no Forward+ (o resto é emissivo) |

**Lição de *Cities: Skylines II*:** a análise técnica pública mostrou personagens de ~56k vértices sem LOD, props densos e sombras respondendo por ~72% dos draw calls de um frame. **Toda malha entra com LOD e orçamento, e o linter de assets reprova o que não cumpre** (`ASSET_PIPELINE.md`).

## 5. Elementos (§4)
| Elemento | Técnica |
|---|---|
| Prédios | kits modulares, LOD0–2 + impostor, variação por instância (cor/telhado) via propriedades |
| Ruas | malha gerada por chunk + decals (`ROAD_SYSTEM.md`) |
| Árvores/parques | Terrain trees (MVP) → GRD instanciado com LOD/impostor (Fase 10) |
| Cidadãos | MVP: low-poly animado. Fase 11+: ECS + VAT (animação em textura) + impostores |
| Veículos | ECS + Entities Graphics, LOD, luzes emissivas |
| Água | shader estilizado |
| Infraestrutura | postes, linhas, canos (visíveis só em overlays) |
| Ciclo dia/noite, chuva, neblina, sombras | `LIGHTING.md`, `WEATHER_SYSTEM.md` |

## 6. Overlays e seleção
Overlays dessaturam a cena e aplicam LUT (ver `MAP_OVERLAYS.md`). Seleção com *outline* (pass do Render Graph) e *highlight* de relacionados.

## 7. Riscos
Malha de interseções (spike S5), custo de sombras em zoom rasante, *pop-in* de LOD (*crossfade* com dithering), memória de texturas (streaming de mipmaps).
