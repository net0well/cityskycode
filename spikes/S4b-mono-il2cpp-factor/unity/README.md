# Projeto Unity do S4b (descartável)

Estes são os scripts do projeto Unity usado no S4b. O projeto completo (com `Library/`) foi
criado fora do repositório, em `C:\dev\unity-spikes\S4b`, para não versionar milhares de arquivos.

- `unity/Domain/CitySky.Spike.Domain.asmdef` — o asmdef sem referência à engine; os `.cs` do domínio
  são **os mesmos do S4a** (`spikes/S4a-headless-domain/src/CitySky.Spike.Domain/`), copiados para `Assets/Domain/`.
- `unity/Bench/` — benchmark rodável na Unity (`BenchCore`) + `BenchRunner` (roda no player ao iniciar).
- `unity/Editor/` — `BenchBuild` (métodos chamados por `-executeMethod`: rodar no Editor e compilar players).

Como reproduzir: criar um projeto Unity 6000.6.5f1 vazio, copiar o domínio do S4a para `Assets/Domain/`
e estes scripts para `Assets/Bench` e `Assets/Editor`, e rodar os comandos do relatório `docs/spikes/S4b-mono-il2cpp-factor.md`.
