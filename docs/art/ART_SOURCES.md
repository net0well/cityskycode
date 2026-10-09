# Fontes de Arte e Ferramentas Gráficas Gratuitas

> **Status:** v1.0 (pesquisa de 2026-10-09; cada item passa por avaliação na cena de teste antes de entrar) · **ADRs:** [0002](../ADR/0002-render-pipeline.md), [0017](../ADR/0017-product-scope-classroom.md) · Relacionados: [`GRAPHICS.md`](../GRAPHICS.md), [`ASSET_PIPELINE.md`](../ASSET_PIPELINE.md), [`TECH_STACK.md`](../TECH_STACK.md)
> Pedido do dono do produto (2026-10-08): *"o jogo deve ter bons gráficos tipo Cities: Skylines… deve ter alguns assets free… algum framework para gráficos… podemos usar coisas estilo Low-Poly"*.

---

## 1. O que dá para alcançar sem custo (expectativa honesta)

*Cities: Skylines* impressiona por cinco coisas: **densidade** (milhares de prédios, carros e árvores), **luz** (sol, sombras suaves, noite com janelas acesas), **câmera de maquete** (tilt-shift, cores levemente saturadas), **vida** (trânsito, pedestres, fumaça) e **leitura clara** (overlays, ícones). Quatro dessas cinco não dependem de comprar arte: vêm de tecnologia gratuita da Unity e de direção de arte.

O que **não** dá para igualar com orçamento zero é a **variedade e o realismo dos modelos** (CS tem equipes de artistas e milhares de prédios únicos). Por isso o alvo continua sendo o do `GRAPHICS.md`: **low-poly estilizado, limpo e muito bem iluminado**. Pense em uma cidade de maquete bonita, não em fotografia. É o estilo que o dono do produto aprovou, e ele também roda no hardware de escola (Iris Xe).

| O que faz CS bonito | Equivalente gratuito no City: SkyCode |
|---|---|
| Densidade de prédios e props | kits CC0 modulares + variação por instância (cor, telhado, andares) + GPU Resident Drawer + **Mesh LOD** automático |
| Sombras e luz do sol | URP: sol direcional com cascatas, SSAO, céu procedural, exposição por hora do dia |
| Noite com janelas acesas | emissivos por instância + Forward+ para luzes próximas + bloom suave |
| Tilt-shift e cores de maquete | Volumes do URP: profundidade de campo (Bokeh), ajuste de cor, LUT, curvas |
| Neblina, atmosfera | fog do URP + fog volumétrico gratuito (avaliar, §3) |
| Água | shader estilizado gratuito ou próprio em Shader Graph (§3) |
| Trânsito e pedestres | nossa simulação + agentes instanciados (Fase 5/11) |
| Árvores e parques | pacotes CC0 de natureza + Terrain trees + impostores (Fase 10/18) |
| Overlays legíveis | nosso sistema de overlays (ADR-0013) com paletas para daltonismo |

## 2. "Frameworks" gráficos gratuitos da própria Unity (já incluídos, sem dependência extra)

| Recurso | Para quê | Fase |
|---|---|---|
| **URP** (Forward+, SRP Batcher, Render Graph) | pipeline (ADR-0002) | 1 |
| **GPU Resident Drawer** + **GPU Occlusion Culling** | milhares de prédios e árvores com poucas chamadas de desenho | 2 |
| **STP** (upscaling) | resolução interna menor no preset Baixo (GPU integrada) | 2 |
| **Mesh LOD** (Unity 6.2+) | **gera os LODs na importação**, sem criar objetos novos e com pouca memória; resolve a falta de LOD dos kits gratuitos | 2 |
| **SSAO**, **Decals**, **Volumes** (ajuste de cor, LUT, bloom, profundidade de campo, vinheta) | aparência de maquete, ruas com marcas e desgaste | 2 |
| **Shader Graph** | água, janelas noturnas, ondas de vento nas árvores, efeito tilt-shift próprio | 2 |
| **VFX Graph** | fumaça da usina, chuva perto da câmera, partículas de dinheiro | 2 / 19 |
| **Terrain** + **Terrain Tools** | relevo, pintura, árvores | 2 |
| **Splines** | ferramenta de ruas (pré-visualização) | 3 / 11 |
| **ProBuilder** + **Polybrush** | prédios especiais simples (prefeitura), ajustes de cenário dentro da Unity | 2–4 |
| **Cinemachine** | modo cinemático e foto (Fase 20; avaliar com ficha de dependência) | 20 |
| Projeto de exemplo **Universal 3D Sample** e o e-book gratuito *URP shaders & VFX (Unity 6)* | referência de configuração de luz, LUT, decals e SSAO | 2 |

## 3. Ferramentas de terceiros gratuitas (candidatas; avaliar antes de adotar)

Cada uma precisa da ficha de dependência (`TECH_STACK.md` §6), de um teste na cena de avaliação (S3/F2) e do registro de licença.

| Ferramenta | O que faz | Licença / custo | Observação |
|---|---|---|---|
| **Unity URP Volumetric Light** (CristianQiu, GitHub/OpenUPM) | fog volumétrico integrado aos Volumes do URP | código aberto (conferir o arquivo LICENSE) | compatível com Unity 2022.3 até 6000.5 segundo o autor; testar na 6.6 |
| **Volumetric Fog for URP – Lite** (Mirza Beig, Asset Store) | fog volumétrico rápido, inclusive em hardware fraco | gratuito (EULA da Asset Store) | boa candidata para o preset Alto |
| **LUMINATE** (Asset Store) | fog volumétrico, nuvens volumétricas e céu físico no URP | gratuito (EULA da Asset Store) | última versão 2.2.0 (ago/2023): conferir compatibilidade com a 6.6; nuvens volumétricas são caras para Iris Xe (só preset Alto) |
| **Stylized Water Shader** (Alexander Ameye) | água estilizada: gradiente por profundidade, espuma, refração, cáusticas, ondas | gratuito (termos próprios) | ótimo ponto de partida; alternativa: shader próprio em Shader Graph |
| **TinyPlay URPShadersCollection** (GitHub) | coleção de shaders URP (inclui água cartoon) | aberto (conferir LICENSE) | usar como referência de estudo |
| **Simple Dome Clouds** (Xwin Studio) | nuvens leves em domo (UV animada) | gratuito para uso comercial | ideal para o preset Baixo |
| **IMP** (GitHub, Usnul) | gera impostores (billboards) a partir de modelos | **CC0** | antigo (Unity 2017–2018): precisaria portar os shaders para URP; avaliar na Fase 18 |

**Descartadas por custo (orçamento zero):** Amplify Impostors, Rawteous, Crest para URP (vendido na Asset Store), Altos Sky and Clouds, Buto, Stylized Water 2, Simple Atmospheric Fog.

## 4. Arte gratuita

### 4.1 CC0 (domínio público: pode tudo, inclusive versionar em repositório público)
| Fonte | Conteúdo útil para a cidade |
|---|---|
| **Kenney – City Kit (Roads)** v2.1 | ruas, cruzamentos, placas e semáforos (90 arquivos) |
| **Kenney – City Kit (Commercial / Suburban / Industrial)** | prédios comerciais, casas, indústria (inclui solar e eólica) |
| **Kenney – Car Kit** e outros kits 3D | carros, caminhões, ônibus |
| **Quaternius – LowPoly Buildings Pack** | prédios **modulares** com atlas de textura trocável (paletas) |
| **Quaternius – LowPoly Modular Street Pack** | rua modular, sinalização, pontes |
| **Quaternius – natureza e personagens** | árvores, pedras, pessoas rigadas e animadas |
| **Poly Haven** | céus HDRI, texturas e alguns modelos realistas |
| **ambientCG** | materiais PBR (asfalto, concreto, grama) |
| **Kenney Audio**, Freesound (filtro CC0) | sons de UI e ambiente |

### 4.2 Asset Store gratuitos (EULA padrão da Unity Asset Store)
Podem ser usados no jogo, inclusive comercialmente. **Não podem ser redistribuídos**: ficam fora de repositório público (o nosso é privado, então pode versionar) e precisam ser baixados pela conta Unity do dono do projeto ("Add to My Assets").

| Pacote | Conteúdo | Atenção |
|---|---|---|
| **POLYGON – Starter Pack** (Synty) | amostra do estilo POLYGON (prédios, props, personagens) | estilo diferente do Kenney; precisa converter materiais para URP |
| **Simple City** (LowPoly SimpleCity Pack) | escola, hospital, banco, polícia, bombeiros, lojas, carros, ônibus, postes, semáforos; compatível com URP | **conferir o preço atual** na página antes de contar com ele |
| **Low Poly Simple Urban City** | 90+ modelos urbanos, gratuito | a ficha lista URP como não compatível: converter os materiais |
| **Low poly trees – free nature pack** (Nebula) | 6 espécies de árvores, tocos, galhos; ~300 polígonos por malha; feito para URP | é uma amostra de um pacote pago maior |

> Pesquisa feita por buscas; **preços e licenças mudam**. Antes de baixar, confirmar o preço e a licença na página oficial e registrar em `docs/art/ASSET_LICENSES.md`.

## 5. Regras para não virar "colcha de retalhos"
1. **Uma família principal de estilo:** Kenney City Kits + Quaternius (ambos low-poly com cores chapadas). Outros pacotes entram só se conversarem com esse estilo.
2. **Material e paleta próprios:** todos os modelos passam a usar o **atlas de cores do projeto** (uma textura pequena de paleta) e a mesma lista de shaders. Isso unifica pacotes de fontes diferentes e ainda reduz materiais para o GPU Resident Drawer.
3. **LOD sempre:** Mesh LOD na importação; o linter de assets reprova o que não tiver.
4. **Licença registrada antes do primeiro commit** do asset.
5. **Nada de marca de terceiros** dentro dos modelos (ex.: cruz vermelha em hospital).

## 6. Recomendação
- **Arte base:** Kenney City Kits (ruas, comercial, residencial, industrial, carros) + Quaternius (prédios modulares, rua, natureza, pessoas). Tudo CC0.
- **Tecnologia:** só recursos da Unity no MVP (URP, GRD, Mesh LOD, SSAO, Volumes, Shader Graph). Água e janelas noturnas em Shader Graph próprio (o shader do Ameye como referência).
- **Avaliar na cena de teste (S3 e F2-34):** fog volumétrico (CristianQiu ou Mirza Lite) só no preset Alto; nuvens em domo (Simple Dome Clouds) no Baixo.
- **Asset Store gratuito:** só se preencher uma lacuna que os kits CC0 não cobrem (ex.: prédios de serviço do Simple City), depois de conferir preço e licença.
- **Ação do dono do produto:** adicionar à conta Unity os pacotes gratuitos da Asset Store que forem aprovados (eu não consigo baixá-los sem a conta).

## 7. Referências (consultadas em 2026-10-09)
- Unity Manual 6.2 – gerar Mesh LODs na importação: https://docs.unity3d.com/6000.2/Documentation/Manual/lod/mesh-lod-generate-lods.html
- Unity Manual 6.4 – introdução ao Mesh LOD: https://docs.unity3d.com/Manual/lod/mesh-lod-introduction.html
- Unity Manual – SSAO no URP: https://docs.unity3d.com/Manual/urp/add-ssao-renderer-feature-to-renderer.html
- Unity Manual – decals no URP: https://docs.unity.cn/Manual/urp/renderer-feature-decal-landing.html
- Unity – e-book *Create popular shaders and visual effects with URP (Unity 6)*: https://unity.com/vn/resources/create-shaders-visual-effects-urp-unity-6
- Fórum Unity – tilt-shift com pós-processamento: https://discussions.unity.com/t/how-to-achieve-tilt-shift-with-unity-s-post-processing-stack/759156
- Unity URP Volumetric Light (OpenUPM): https://openupm.com/packages/com.cqf.urpvolumetricfog/
- 80.lv – Volumetric Fog gratuito de Mirza Beig: https://80.lv/articles/grab-mirza-beig-s-free-volumetric-fog-asset-for-unity-urp/
- LUMINATE (Asset Store): https://assetstore-fallback.unity.com/packages/vfx/shaders/luminate-volumetrics-215100
- Simple Dome Clouds: https://xwin-studio.itch.io/simple-dome-clouds
- Stylized Water Shader gratuito (fórum Unity): https://discussions.unity.com/t/stylized-water-shader-free/780666
- TinyPlay URPShadersCollection: https://github.com/TinyPlay/URPShadersCollection
- IMP (impostores, CC0): https://github.com/Usnul/IMP
- Crest (introdução; versões pagas por pipeline): https://crest.readthedocs.io/en/stable/about/introduction.html
- Kenney City Kit (Roads) 2.1: https://gamedev.net/news/5156-city-kit-roads/
- Quaternius – LowPoly Buildings Pack: https://opengameart.org/content/lowpoly-buildings-pack
- Quaternius – LowPoly Modular Street: https://quaternius.itch.io/lowpoly-modular-street
- Synty – POLYGON Starter Pack: https://assetstore.unity.com/packages/3d/props/polygon-starter-pack-156819
- LowPoly SimpleCity Pack: https://assetstore.unity.com/packages/3d/environments/lowpoly-simplecity-pack-295278
- Low Poly Simple Urban City (gratuito): https://assetstore-fallback.unity.com/packages/3d/environments/urban/free-low-poly-simple-urban-city-3d-asset-pack-239474
- Low poly trees – free nature pack: https://assetstore.unity.com/packages/3d/vegetation/trees/low-poly-trees-free-nature-pack-300824
