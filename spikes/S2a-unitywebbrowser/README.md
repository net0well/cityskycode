# Spike S2a — código

Spike descartável. Relatório, matriz de decisão e checklist humano: [`docs/spikes/S2a-unitywebbrowser.md`](../../docs/spikes/S2a-unitywebbrowser.md).

Decisão: host do editor = **(B') WebView2 como janela-filha** do player Unity. UWB rejeitado.

## Conteúdo versionado
- `src/CitySky.Spike.EditorHostS2a/` — host do editor (PhotinoX/WebView2) que lança o player Unity real, reparenta o editor como `WS_CHILD` no painel direito e mede composição/estados/digitação/memória. Reaproveita `Native.cs`, `ProcessTree.cs` e o bundle Monaco (`wwwroot`, gerado pelo S2b).
- `unity/Assets/` — cena 3D colorida em movimento (`SceneDriver`, `Spinner`) + build do player (`S2aBuild`).
- `results/screenshots/` — capturas de referência (editor composto sobre a cena, por estado + fullscreen).

O projeto Unity completo fica fora do repo em `C:\dev\unity-spikes\S2a`; `wwwroot/`, `bin/`, `obj/` e `results/*.json` são ignorados.

## Reproduzir
```powershell
# 1) Bundle Monaco (reusa o do S2b) → wwwroot do host
#    (já construído em spikes/S2b-photinox-monaco; copie wwwroot para src/.../wwwroot)
# 2) Build do player Unity (IL2CPP)
$u = "C:\Unity\Editor\6000.6.5f1\Editor\Unity.exe"
& $u -batchmode -nographics -projectPath C:\dev\unity-spikes\S2a `
     -executeMethod S2aBuild.BuildPlayer -backend IL2CPP -logFile build.log
# 3) Build do host e execução (objetivo: JSON + capturas; --hold para o checklist humano)
dotnet build spikes/S2a-unitywebbrowser/src/CitySky.Spike.EditorHostS2a -c Release
$h = (gci "spikes/S2a-unitywebbrowser/src/CitySky.Spike.EditorHostS2a/bin/Release" -Recurse -Filter *.exe)[0].FullName
& $h --unity "C:\dev\unity-spikes\S2a\s2a-out\player-IL2CPP\S2aPlayer.exe" `
     --out results\s2a-IL2CPP.json --shots results\screenshots   # some --hold para inspeção manual
```
