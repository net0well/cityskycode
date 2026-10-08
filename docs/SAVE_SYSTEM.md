# Sistema de Save (Persistência)

> **Status:** v1.0 · **Data:** 2026-10-08 · **ADR:** [0012](ADR/0012-persistence.md) · **MVP:** sim (JSON compactado, 3 slots + autosave)
> Inclui o §52 (Replay).

---

## 1. O que salvar (§53)
| Item | Onde | Observação |
|---|---|---|
| City / Buildings / Citizens / Economy / SimulationState | `city.json.gz` (estado do domínio) | stores serializados em ordem de ID |
| PlayerCode | `code/*.cs` (texto puro) | legível fora do jogo, bom para portfólio |
| Challenges (estado dos desafios da cidade) | `city.json.gz` | — |
| PlayerProgress / Achievements | `profile/profile.json` | **separado** das cidades: apagar uma cidade não apaga o aprendizado |
| Settings | `settings.json` | por usuário |
| Command log (replay) | `replay/commands.jsonl` | opcional por save (Fase 9) |

**Não se salva:** estado da apresentação (GameObjects, entidades ECS, caches de caminho, read models). Tudo isso é **reconstruído** a partir do domínio. Isso simplifica muito o save e elimina uma classe inteira de bugs.

## 2. Formato do container
```
Saves/<slot>/
├── manifest.json        { schemaVersion, gameVersion, playerApiVersion, createdAt, tick, seed, cityName, hash }
├── city.json.gz         estado do domínio (MVP: JSON + gzip; Fase 18: binário por seção)
├── code/Program.cs …    código do jogador
├── thumbnail.png
└── replay/ (opcional)
```
- **Escrita atômica:** grava em `<slot>.tmp/` e troca por renome. Mantém o anterior como `.bak` até concluir.
- **Integridade:** `hash` (xxHash64) do `city.json` no manifest. Hash divergente → oferece carregar o `.bak`.
- **Autosave:** a cada 5 min reais, em rotação de 3, em thread de I/O. A serialização do estado é feita no tick (cópia rápida dos stores) e a compressão e a escrita fora do main thread.

### Steam Cloud (Fase 20, ADR-0018)
Na versão Steam, a pasta de saves e o perfil sincronizam pelo **Steam Cloud**, via `IPlatformServices`. As versões Microsoft Store e itch.io continuam só com saves locais. O formato é o mesmo em todos os canais, então um save viaja entre eles por arquivo.

## 3. Versionamento
- `schemaVersion` (inteiro) do formato do domínio.
- **Cadeia de migrações** `ISaveMigration { From; To; Migrate(JObject) }` aplicadas em sequência (N → N+1 → ...).
- **Golden saves:** cada versão publicada tem saves de referência em `dotnet/tests/.../GoldenSaves/`. O CI carrega todos, migra e valida invariantes e hash esperado.
- `playerApiVersion`: o código salvo é compilado contra a versão gravada (com *shims*) ou migrado com diff mostrado ao aluno (`PLAYER_API.md` §5).

## 4. Tamanho e desempenho
| Escala | Formato | Tamanho estimado | Tempo de save (alvo) |
|---|---|---|---|
| MVP (50 hab.) | JSON + gzip | < 200 KB | < 50 ms |
| 10k | JSON + gzip | ~5–10 MB | < 500 ms |
| 100k | **binário por seção** (ADR revisado) | ~20–40 MB | < 1 s (fora do main thread) |

## 5. Segurança
- **Saves podem conter código do jogador.** Um save vindo de outra pessoa é conteúdo **não confiável**: o código **nunca** é executado automaticamente ao carregar. O jogo mostra o código e pede confirmação, e a execução é sempre no sandbox (`CODE_EXECUTION_SECURITY.md` L7).
- Desserialização sem polimorfismo arbitrário (`TypeNameHandling.None`), com limites de tamanho e validação de invariantes após carregar (IDs válidos, saldos consistentes, ledger fechando).

## 6. Replay e snapshots (§52)
| Recurso | Implementação | Fase |
|---|---|---|
| Restaurar estado (Laboratório) | snapshot em memória do cenário antes de executar | MVP |
| Testar código e comparar resultados | executar variações a partir do mesmo snapshot *headless* e comparar métricas | MVP (Laboratório) |
| Voltar / executar novamente na cidade | **log de comandos** + snapshots periódicos (keyframes a cada dia TC); para voltar a T, carrega o keyframe ≤ T e re-simula os comandos (determinismo) | Fase 9 |
| Linha do tempo visual | UI de replay com marcadores de eventos e execuções de código | Fase 19 |

O replay só funciona porque a simulação é **determinística** (ADR-0008). O teste de determinismo é a garantia.

## 7. Testes
*Round-trip* (salvar → carregar → hash igual), migrações (golden saves), corrupção (arquivo truncado → `.bak`), save durante autosave concorrente, carregamento de save com código inválido (não executa; mostra diagnóstico).
