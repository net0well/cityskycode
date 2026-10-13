# Monta e abre o "City: SkyCode — Editor" (Monaco + Code Host + Worker no Job Object).
# Uso:  pwsh scripts/run-editor.ps1
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent

Write-Host '1/3  Bundle do editor (Monaco)...' -ForegroundColor Cyan
Push-Location "$root\editor-web"
if (-not (Test-Path node_modules)) { npm install }
npm run build
Pop-Location

Write-Host '2/3  Publicando o Code Host...' -ForegroundColor Cyan
dotnet publish "$root\dotnet\src\CitySky.CodeHost\CitySky.CodeHost.csproj" -c Release `
  -o "$root\unity\CitySky\Assets\StreamingAssets\codehost" --nologo -v q

Write-Host '3/3  Abrindo o editor...' -ForegroundColor Cyan
dotnet build "$root\dotnet\src\CitySky.EditorHost\CitySky.EditorHost.csproj" -c Release --nologo -v q
$exe = (Get-ChildItem "$root\dotnet\src\CitySky.EditorHost\bin\Release" -Recurse -Filter CitySky.EditorHost.exe)[0].FullName
& $exe `
  --codehost "$root\unity\CitySky\Assets\StreamingAssets\codehost\CitySky.CodeHost.exe" `
  --web "$root\unity\CitySky\Assets\StreamingAssets\editor-web"
