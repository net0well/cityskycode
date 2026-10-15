# Spike S1b — código

Spike descartável. Relatório e decisão: [`docs/spikes/S1b-il2cpp-ipc.md`](../../docs/spikes/S1b-il2cpp-ipc.md).

## Conteúdo versionado
- `src/CitySky.Spike.EchoHost/` — host de eco .NET 10 (faz o papel do Code Host; só ecoa quadros `[uint32 len LE][bytes]`). Modos `--pipe <nome>` e `--tcp <arquivoPorta>`.
- `unity/Assets/Ipc/IpcProbe.cs` — probe que lança o host, mede ida-e-volta (pipe e TCP), 8 MB e recuperação.
- `unity/Assets/Ipc/NativeProc.cs` — lançador por `CreateProcess` nativo (P/Invoke). **Necessário no IL2CPP:** `System.Diagnostics.Process.Start` falha lá.
- `unity/Assets/Editor/IpcBuild.cs` — build de player em batch (`-executeMethod IpcBuild.BuildPlayer -backend IL2CPP|Mono`).

O **projeto Unity completo** (Library/, ProjectSettings/, etc.) fica fora do repo em `C:\dev\unity-spikes\S1b`; aqui só vão os fontes. A saída `publish/` (runtime self-contained, ~150 DLLs) é ignorada — reconstrua com o passo abaixo.

## Reproduzir
```powershell
# 1) Publicar o echo host self-contained e copiar para o StreamingAssets do projeto Unity
dotnet publish spikes/S1b-il2cpp-ipc/src/CitySky.Spike.EchoHost -c Release -r win-x64 `
  -o C:\dev\unity-spikes\S1b\Assets\StreamingAssets\echohost

# 2) Copiar os fontes deste repo para o projeto Unity (Assets/Ipc, Assets/Editor)

# 3) Build do player (IL2CPP) e execução em batch
$u = "C:\Unity\Editor\6000.6.5f1\Editor\Unity.exe"
& $u -batchmode -nographics -projectPath C:\dev\unity-spikes\S1b `
     -executeMethod IpcBuild.BuildPlayer -backend IL2CPP -logFile build.log
& "C:\dev\unity-spikes\S1b\s1b-out\player-IL2CPP\S1bPlayer.exe" -batchmode -nographics
# resultado em s1b-out\player-IL2CPP\s1b-IL2CPP.txt
```
