# Marco F1-112: abre o jogo (player Unity) com o editor EMBUTIDO como painel lateral direito.
# Monta editor-web (Monaco) + publica o Code Host + constrói o player (se faltar) + abre o editor em
# modo --unity (reparenta a janela do editor como filha da janela do jogo, técnica do spike S2a).
#
# Uso:  pwsh scripts/run-embedded.ps1 [-Rebuild] [-Backend Mono|IL2CPP]
param(
  [switch]$Rebuild,
  [ValidateSet('Mono','IL2CPP')] [string]$Backend = 'Mono'
)
$ErrorActionPreference = 'Stop'
$root  = Split-Path $PSScriptRoot -Parent
$unity = 'C:\Unity\Editor\6000.6.5f1\Editor\Unity.exe'
$proj  = "$root\unity\CitySky"
$out   = "$proj\Build\player"
$exe   = "$out\CitySkyPlayer.exe"

Write-Host '1/4  Bundle do editor (Monaco)...' -ForegroundColor Cyan
Push-Location "$root\editor-web"
if (-not (Test-Path node_modules)) { npm install }
npm run build
Pop-Location

Write-Host '2/4  Publicando o Code Host...' -ForegroundColor Cyan
dotnet publish "$root\dotnet\src\CitySky.CodeHost\CitySky.CodeHost.csproj" -c Release `
  -o "$root\unity\CitySky\Assets\StreamingAssets\codehost" --nologo -v q

if ($Rebuild -or -not (Test-Path $exe)) {
  Write-Host "3/4  Construindo o player Unity ($Backend)..." -ForegroundColor Cyan
  $log = Join-Path $env:TEMP 'citysky-player-build.log'
  & $unity -batchmode -nographics -silent-crashes -projectPath $proj `
    -executeMethod CitySky.Build.CityPlayerBuild.BuildPlayer -backend $Backend -out $out -logFile $log
  if (-not (Test-Path $exe)) { Write-Error "Build do player falhou; veja $log"; exit 2 }
} else {
  Write-Host '3/4  Player já existe (use -Rebuild para reconstruir).' -ForegroundColor DarkGray
}

Write-Host '4/4  Abrindo o jogo com o editor embutido...' -ForegroundColor Cyan
dotnet build "$root\dotnet\src\CitySky.EditorHost\CitySky.EditorHost.csproj" -c Release --nologo -v q
$editorHost = (Get-ChildItem "$root\dotnet\src\CitySky.EditorHost\bin\Release" -Recurse -Filter CitySky.EditorHost.exe)[0].FullName
& $editorHost `
  --unity    $exe `
  --codehost "$root\unity\CitySky\Assets\StreamingAssets\codehost\CitySky.CodeHost.exe" `
  --web      "$root\unity\CitySky\Assets\StreamingAssets\editor-web"
