# ADR-0002: Usar URP em vez de HDRP

- **Status:** Aceito (diverge do briefing, que propunha HDRP)
- **Data:** 2026-10-08
- **Documentos afetados:** `GRAPHICS.md`, `LIGHTING.md`, `ASSET_PIPELINE.md`, `TECH_STACK.md`

## Contexto
- O briefing propôs HDRP para iluminação, volumetria, água e atmosfera.
- A **estratégia oficial da Unity para 2026** coloca o HDRP em **manutenção** (sem novas funcionalidades; apenas estabilidade, regressões e Switch 2), recomenda **URP para qualquer projeto novo** e concentra no URP a evolução visual (luz física, exposição, céu físico, GI em tempo real, SSR).
- City builders são tipicamente limitados pela **GPU** (geometria, draw calls, sombras). A análise técnica de *Cities: Skylines II* (Unity + HDRP + DOTS) mostrou que a simulação escalou e a renderização foi o gargalo.
- O público (estudantes) usa com frequência notebooks com GPU integrada.

## Decisão
Usar **URP** com Forward+, GPU Resident Drawer, GPU Occlusion Culling, SRP Batcher, Render Graph, STP, SSAO e Decals. Água, nuvens e fog volumétrico serão shaders estilizados próprios (ou assets com ficha de dependência). Direção de arte **estilizada-realista**.

## Alternativas consideradas
| Alternativa | Prós | Contras |
|---|---|---|
| HDRP | água, volumetria e céu físico nativos; fidelidade | modo manutenção; hardware alto; caro em cenas densas |
| Built-in | simples | em depreciação desde a 6.5 |
| URP **(adotado)** | investimento ativo, escalável, GRD/occlusion, amplo hardware | menos recursos de alta fidelidade nativos |

## Consequências
**Positivas:** roadmap alinhado à Unity; roda em GPU integrada; desempenho previsível.
**Negativas:** trabalho próprio de shaders (água, atmosfera); visual de "fotorrealismo" fora de alcance (e fora do objetivo).
**Riscos:** algum recurso anunciado para o URP atrasar. Mitigação: soluções próprias ou assets.

## Gatilhos de revisão
Unity revertendo a estratégia · necessidade visual comprovada (playtest) que o URP não atenda.

## Referências
https://unity.com/topics/render-pipelines-strategy-for-2026 · https://blog.paavo.me/cities-skylines-2-performance/
