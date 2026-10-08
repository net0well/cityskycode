# ADR-0012: Salvar só o estado do domínio, num container versionado com migrações

- **Status:** Aceito
- **Data:** 2026-10-08
- **Documentos afetados:** `SAVE_SYSTEM.md`

## Contexto
O §53 pede save versionado de cidade, progresso, código, desafios, prédios, cidadãos, economia, simulação, conquistas e configurações. O §52 pede replay e restauração.

## Decisão
1. Salvar **somente o estado do domínio** (stores) + código do aluno (texto) + manifest. Apresentação, ECS e caches são reconstruídos.
2. Container por pasta/slot: `manifest.json`, `city.json.gz` (MVP; binário por seção quando > ~10 MB), `code/`, `thumbnail.png`, `replay/`.
3. `schemaVersion` + **cadeia de migrações** testada com **golden saves**.
4. **Perfil de aprendizagem separado** dos saves de cidade.
5. Escrita atômica, hash de integridade, autosave em rotação.
6. Replay = log de comandos + keyframes, possível graças ao determinismo (ADR-0008).
7. Código de saves de terceiros **nunca** é executado automaticamente.

## Alternativas consideradas
| Alternativa | Por que não |
|---|---|
| Serializar GameObjects/cenas | acoplado à Unity; frágil; enorme |
| Banco embutido (SQLite) | dependência sem ganho para snapshot inteiro |
| Binário desde o MVP | perde a depurabilidade cedo demais |

## Consequências
**Positivas:** saves pequenos, robustos e depuráveis; código do aluno legível fora do jogo.
**Negativas:** precisa reconstruir a apresentação no load (tempo de carga).

## Gatilhos de revisão
Saves > 10 MB ou save > 500 ms → formato binário.
