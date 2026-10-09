# Fase 2 — Mundo: terreno, câmera, iluminação e seleção

> **Status:** Planejada · **Bloco:** B (Vertical Slice) · **Atualizado:** 2026-10-08
> **Entrada:** gate da Fase 1; relatório do S3 disponível antes do gate desta fase · **Saída:** orçamento de FPS do S3 mantido na cena real, no hardware mínimo · **Estimativa:** 4 semanas

---

## 1. Objetivo
Um mundo navegável e bonito o bastante para o playtest: terreno de 2 × 2 km, câmera de estratégia, ciclo dia/noite, seleção com Inspector genérico e HUD com controle de velocidade.

## 2. Por que agora
Tudo o que vem depois (ruas, casas, cidadãos) precisa de um chão, de uma câmera e de uma forma de selecionar. Também é aqui que a direção de arte com kits CC0 (ADR-0017) é posta à prova no hardware mínimo, antes de haver centenas de assets.

## 3. Escopo
**Inclui:** terreno fixo (heightmap feito à mão), máscara construível, lago decorativo, árvores do Terrain, `TerrainInfo` no domínio, grade de análise de 32 m, câmera (`CAMERA_SYSTEM.md` modos do MVP), dia/noite (`DAY_NIGHT_SYSTEM.md`, `LIGHTING.md`), presets Baixo e Alto, paleta e importação dos kits, linter de assets v0, registro de licenças, seleção e Inspector genérico, HUD superior.
**Não inclui:** edição de terreno (F19), clima (F19), marcadores de câmera e visão de bairro (F10), presets Médio/Ultra (F20), seleção de área (F10).

## 4. Entregáveis
- Cena `World` aditiva sobre a *Bootstrap*.
- `docs/art/ART_DIRECTION.md` (paleta, materiais, referências) e `docs/art/ASSET_LICENSES.md` (registro obrigatório).
- Cena de referência `perf-render` (reescrita a partir do S3) rodando no CI noturno.

## 5. Backlog

### E1 — Terreno e grade
| ID | Tarefa | Onde | Depende de | Pronto quando |
|---|---|---|---|---|
| F2-01 | Heightmap 2049² (2 km, 1 m) com relevo suave e área construível central de 1 × 1 km; lago decorativo | Assets/City/Terrain | — | revisão visual; inclinações dentro do esperado |
| F2-02 | `TerrainInfo` no domínio: alturas amostradas em mm, máscara construível, cálculo de inclinação (faixas do `MAP_SYSTEM.md` §3) | Domain | F1 | testes de inclinação e máscara |
| F2-03 | Exportador editor → domínio (asset binário de alturas lido pelo Application) e teste de consistência Unity × domínio | Infrastructure / testes | F2-01, F2-02 | amostras de altura iguais (tolerância 1 cm) |
| F2-04 | `AnalysisGrid` 64 × 64 (32 m) e conversões mundo ↔ célula | Domain | F2-02 | testes de conversão nas bordas |
| F2-05 | Árvores e detalhes do Terrain com densidade por preset | Presentation | F2-01 | custo dentro do orçamento do S3 |

### E2 — Câmera
| ID | Tarefa | Onde | Depende de | Pronto quando |
|---|---|---|---|---|
| F2-10 | `CameraController` (C# puro, testável): alvo, yaw, pitch acoplado ao zoom, distância 15–1.500 m, limites do mapa, altura mínima sobre o terreno | Presentation | — | testes EditMode de limites e de zoom ao cursor |
| F2-11 | `StrategyCameraRig` (MonoBehaviour fino) + mapa de ações do Input System (pan, zoom, rotação, inclinação, foco, visão geral) com suavização | Presentation | F2-10 | comportamento conforme `GAMEPLAY.md` §1 |
| F2-12 | Foco por duplo clique e visão geral | Presentation | F2-11, F2-30 | animação até o objeto |
| F2-13 | Base de *rebinding* e sensibilidade (acessibilidade) | Presentation | F2-11 | tecla trocada persiste em `settings.json` |
| F2-14 | `CameraInfo` para a apresentação (LOD, culling). **Nunca** chega ao domínio | Presentation | F2-10 | teste de arquitetura: Domain não conhece câmera |

### E3 — Iluminação e dia/noite
| ID | Tarefa | Onde | Depende de | Pronto quando |
|---|---|---|---|---|
| F2-20 | Configuração do URP: Forward+, GPU Resident Drawer, GPU Occlusion Culling, SRP Batcher, STP; presets **Baixo** e **Alto** (`GRAPHICS.md` §3) | Settings | S3 | presets selecionáveis em runtime |
| F2-21 | `DayNightController`: lê a hora do read model, interpola sol, cor, exposição, céu procedural; SH e *reflection probe* atualizados por fase do dia, não por frame | Presentation | F1-41 | 0 alocação por frame; transições nas horas certas |
| F2-22 | Emissivos noturnos (parâmetro global "noite" nos shaders de janela e poste) e SSAO no Alto | Presentation | F2-21 | janelas acendem às 18:30 |
| F2-23 | Distância de sombra acoplada ao zoom | Presentation | F2-11 | medição no S3 mantida |

### E4 — Direção de arte e assets
| ID | Tarefa | Onde | Depende de | Pronto quando |
|---|---|---|---|---|
| F2-30 | Paleta, materiais base (lista permitida de shaders, compatíveis com SRP Batcher e GRD) e `ART_DIRECTION.md` | `docs/art/`, Assets/Materials | — | revisão pelo dono do produto |
| F2-31 | Importar Kenney City Kits e Quaternius com *import presets* por pasta; **Mesh LOD** na importação (Unity 6.2+); Blender só onde o gerador falhar; trocar materiais pelo atlas de paleta do projeto | Assets/Art | F2-30 | todos os modelos com LOD; ≤ 60 materiais únicos |
| F2-32 | `ASSET_LICENSES.md` com origem, licença e atribuição de cada asset | `docs/art/` | F2-31 | nenhum asset sem registro |
| F2-33 | Linter de assets v0 (batch): LOD Group e orçamento, shader na lista, textura com mipmap, prop pequeno sem sombra, licença registrada | `tools/`, Editor | F2-31 | CI reprova um asset fora da regra |
| F2-34 | Avaliar os complementos gráficos gratuitos de `docs/art/ART_SOURCES.md` §3 (fog volumétrico, nuvens em domo, água estilizada) na cena `perf-render`, com ficha de dependência e custo por preset | Assets, `docs/art/` | F2-50 | decisão registrada por item (adotar no preset Alto / Baixo / descartar) |

### E5 — Seleção, Inspector e HUD
| ID | Tarefa | Onde | Depende de | Pronto quando |
|---|---|---|---|---|
| F2-40 | Seleção por raycast em colisores simples (caixas, cápsulas, malha por chunk) | Presentation | — | clique devolve a entidade certa |
| F2-41 | Inspector genérico (UI Toolkit) com abas Estado/Propriedades, lendo read models `*Details` | Presentation / Application | F2-40 | abre para um objeto de teste |
| F2-42 | HUD superior: data e hora, velocidade (⏸ ▶ ▶▶ ▶▶▶), velocidade efetiva ("7,2x de 10x") e atalhos Espaço/`+`/`-` | Presentation | F1-40 | mudar a velocidade não muda o hash (F1-111) |
| F2-43 | Tokens de tema USS (cores, espaçamento, tipografia) e escala de UI 80–150% | UI | — | troca de escala sem quebrar layout em 1366×768 |

### E6 — Desempenho
| ID | Tarefa | Onde | Depende de | Pronto quando |
|---|---|---|---|---|
| F2-50 | Cena `perf-render` (reescrita do S3) + câmera roteirizada + captura via `FrameTimingManager` em CSV | Tests/Performance | S3 | roda em batch e gera CSV |
| F2-51 | Medição no PC com Iris Xe, preset Baixo, 1080p | — | F2-50 | registro em `docs/perf/` |

## 6. Conteúdo pedagógico
Nenhum novo. O Inspector com abas "Estado" e "Propriedades" prepara a lição de encapsulamento (parâmetro × derivado), que só ganha dados na Fase 4.

## 7. Testes e qualidade
EditMode do `CameraController`; testes de domínio do terreno e da grade; consistência Unity × domínio; performance (GC alloc por frame = 0 em câmera, HUD e dia/noite); linter de assets no CI.

## 8. Gate de saída
- [ ] Cena `World` a **≥ 30 FPS** (p5) no Iris Xe, preset Baixo, 1080p, nos 3 zooms do S3, de dia e de noite.
- [ ] ≥ 60 FPS no hardware recomendado, preset Alto.
- [ ] 0 alocação por frame em regime (câmera parada e em movimento).
- [ ] Todos os assets passam no linter e têm licença registrada.
- [ ] Testes de domínio e EditMode verdes; hash determinístico intacto.

## 9. Riscos
| Risco | Ref. | Mitigação nesta fase |
|---|---|---|
| GPU como gargalo | R05 | presets e orçamento do S3; linter de assets |
| Visual limitado pelos kits CC0 | R24 | paleta própria, iluminação, variação por instância |
| Custo de arte | R14 | só o necessário para o MVP |

## 10. Documentos e ADRs a atualizar
`MAP_SYSTEM.md`, `CAMERA_SYSTEM.md`, `DAY_NIGHT_SYSTEM.md`, `LIGHTING.md`, `GRAPHICS.md` (status e números reais), `ASSET_PIPELINE.md` (linter real), `PERFORMANCE.md` (medições).

## 11. Ferramental
O grosso do trabalho é Unity (Perfil U). `/tdd` para `TerrainInfo`, grade e `CameraController`. `performance-analyst` revisa as capturas.

## 12. Encerramento
- [ ] Gate verificado e registrado
- [ ] `/health-check`
- [ ] `RISKS.md` revisado (R05, R14, R24)
- [ ] Velocidade real registrada
