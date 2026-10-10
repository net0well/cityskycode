# Spike S3 — código

Spike descartável. Relatório, resultados e checklist: [`docs/spikes/S3-urp-render-budget.md`](../../docs/spikes/S3-urp-render-budget.md).

Status: **parcial** — teto validado no RX 7600 (FPS p5 ≥ 269, GPU ~0,3 ms, CPU main ~3 ms, mem ~580 MB, GRD engajado); **mínimo Iris Xe pendente** e agentes são cápsulas rígidas (animação *skinned* não medida).

## Conteúdo versionado
- `unity/Assets/Editor/S3Build.cs` — cria o URP asset (Forward+, GRD, render scale 0,67, sombra curta), inclui o `URP/Lit` nos *Always Included Shaders*, liga `enableFrameTimingStats`, builda IL2CPP.
- `unity/Assets/Scripts/PerfRunner.cs` — spawna a cena (prédios/árvores/agentes/luzes), roteiriza a câmera (3 zooms × dia/noite) e grava CSV + JSON por `FrameTimingManager`.
- `unity/Packages/manifest.json` — dependências (URP 17.6.0).

Projeto Unity completo fora do repo em `C:\dev\unity-spikes\S3`; `Library/`, `s3-out/` e `results/*.{json,csv,log}` ignorados.

## Reproduzir
```powershell
$u = "C:\Unity\Editor\6000.6.5f1\Editor\Unity.exe"
& $u -batchmode -nographics -projectPath C:\dev\unity-spikes\S3 `
     -executeMethod S3Build.BuildPlayer -backend IL2CPP -logFile build.log
$exe = "C:\dev\unity-spikes\S3\s3-out\player-IL2CPP\S3Player.exe"
& $exe -screen-fullscreen 0 -screen-width 1920 -screen-height 1080 -frames 240 -outdir .\results
# argumentos: -buildings N -trees N -agents N -frames N -warmup S
# GPU ms só é reportado em D3D11 (-force-d3d11) nesta máquina
```
Para fechar o gate: rodar o mesmo `S3Player.exe` num PC com **Intel Iris Xe** e conferir FPS p5 ≥ 30 em todos os segmentos.
