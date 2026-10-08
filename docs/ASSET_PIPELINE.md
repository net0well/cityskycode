# Pipeline de Assets

> **Status:** v1.0 · **Data:** 2026-10-08 · **MVP:** LOD + linter + referências diretas · Addressables: Fase 10

---

## 1. Avaliação (§56)
| Tecnologia | Decisão | Quando |
|---|---|---|
| **LOD** (LOD Group: LOD0–LOD2 + impostor/cull) | **Obrigatório para toda malha de mundo** | desde o 1º asset |
| **GPU instancing** | automático via **GPU Resident Drawer** (URP) para MeshRenderers | MVP |
| **Entities Graphics** | agentes ECS (pedestres e veículos) | Fase 11 |
| **Addressables** | catálogo de prédios/props por chave de arquétipo, carregamento sob demanda, grupos por tema | Fase 10 (MVP: referências diretas, YAGNI) |
| **Streaming** | mipmap streaming de texturas (MVP); streaming de conteúdo por região (não necessário: o mapa cabe em memória) | MVP / — |
| **Asset bundles** | só via Addressables; nada de bundles manuais | — |
| Entities content | prefabs ECS via baking/subscenes (o Addressables não carrega entidades diretamente) | Fase 11 |

## 2. Orçamentos por classe de asset (iniciais)
| Classe | LOD0 (tris) | LOD1 | LOD2 | Impostor | Sombra |
|---|---|---|---|---|---|
| Casa pequena | ≤ 2.000 | ≤ 600 | ≤ 150 | sim (> 600 m) | sim |
| Prédio médio | ≤ 8.000 | ≤ 2.000 | ≤ 400 | sim | sim |
| Prop pequeno (banco, lixeira) | ≤ 300 | ≤ 80 | cull > 150 m | — | **não** |
| Árvore | ≤ 3.000 | ≤ 800 | billboard | sim | sim (LOD0–1) |
| Pedestre | ≤ 1.500 | ≤ 400 | impostor/ponto | — | só LOD0 |
| Veículo | ≤ 2.500 | ≤ 700 | ≤ 150 | — | só LOD0 |

Texturas: atlas/*trim sheets*, 1–2K máx. por material, compressão BC7/BC5, mipmaps obrigatórios. Materiais: shaders do projeto (lista permitida), compatíveis com SRP Batcher e GRD.

## 3. Linter de assets (CI + editor)
Script de editor (Unity batch) que reprova:
- malha sem LOD Group ou com LOD fora do orçamento;
- material com shader fora da lista;
- textura sem mipmap ou acima do limite;
- prop pequeno projetando sombra;
- prefab de mundo sem `BuildingVisual`/metadado de arquétipo;
- assets sem licença registrada.

## 4. Organização e convenções
```
Assets/Art/{Buildings,Props,Vegetation,Characters,Vehicles,Terrain,Water,UI}/<Tema>/
Nomes: <classe>_<nome>_<variante>_LOD<n>   ex.: bld_house_small_a_LOD0
```
- Import presets por pasta (escala, normais, compressão).
- `Force Text` + `.meta` versionados. Binários em **Git LFS** (`*.fbx *.png *.psd *.tga *.exr *.wav *.ogg *.ttf *.otf *.mp4 *.unitypackage`).

## 5. Fontes de arte e licenças (orçamento zero, ADR-0017)
- **Somente gratuitos**, com **registro de licença** em `docs/art/ASSET_LICENSES.md` (origem, licença, uso permitido, atribuição):

| Fonte | Licença | Uso |
|---|---|---|
| **Kenney** City Kits (Roads com semáforos e placas, Commercial, Suburban, Industrial com solar e eólica) | CC0 | base da cidade: ruas, prédios, props |
| **Quaternius** | CC0 | personagens (rigados e animados), natureza, veículos |
| **Poly Haven** | CC0 | HDRIs de céu, texturas |
| **ambientCG** | CC0 | materiais PBR |
| Kenney Audio, Freesound (filtro CC0) | CC0 | ambiente e UI |
| Asset Store (gratuitos) | Standard Unity Asset Store EULA | só se necessário. **Não versionar em repositório público** (a licença proíbe redistribuição) |
| Blender (próprio) | — | ajustes, LODs, variações, prédios especiais |

- **Cuidado com marcas dentro de assets CC0:** CC0 não libera marcas de terceiros. Exemplo real: a cruz vermelha é marca protegida; hospitais usam outro símbolo.
- Os kits têm **estilo low-poly estilizado**. A direção de arte adota esse estilo de forma coerente (ver `GRAPHICS.md` §1), e todos os assets passam pelo linter (LOD e orçamento).
- **Proibido:** extrair, copiar ou imitar de perto assets e marcas de *Cities: Skylines* ou de outros jogos (§4).
- **Variação procedural:** cores, telhados, andares e props de fachada por seed do lote (mais variedade com menos assets).

## 6. Vegetação (§57)
- MVP: Terrain trees/details (instanciados pela própria Terrain).
- Fase 10: vegetação urbana (parques, árvores de calçada) via GRD com LOD e impostores; remoção ao construir; densidade por preset.
- **Nunca** milhares de GameObjects individuais com scripts.

## 7. Água (§58)
Shader estilizado próprio (ver `GRAPHICS.md` §2). Rios, lagos e mar compartilham o shader com parâmetros. Reflexo via probe/céu (SSR do URP quando disponível). Transparência por profundidade. Ondas por normal map + deslocamento leve de vértices (só perto da câmera).
