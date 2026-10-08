# Sistema de Compartilhamento

> **Status:** v1.0 · **Data:** 2026-10-08 · **ADRs:** [0017](ADR/0017-product-scope-classroom.md), [0006](ADR/0006-code-security.md) · **Fase:** 9 · **MVP:** não
> **Pré-requisito inegociável:** sandbox **L5 completo** (AppContainer ou token restrito + Job Object) e corpus de pacotes maliciosos no CI. Sem isso, o recurso não é liberado.

---

## 1. O que se compartilha
| Tipo | Conteúdo | Uso típico |
|---|---|---|
| **Projeto de código** | arquivos `.cs` do aluno | mostrar uma solução, pedir ajuda, revisão entre colegas |
| **Cidade** | save (estado do domínio) + código instalado | exibir a cidade, desafios entre colegas |
| **Solução de desafio** | código + id e versão do desafio | o professor avalia em sala |
| **Pacote de desafios** (professor) | `challenge.json`, starter, testes, dicas (`CHALLENGE_SYSTEM.md`) | atividades da turma |
| **Progresso** | ver `CLASSROOM.md` (formato próprio) | professor acompanha a turma |

## 2. Formato `.citysky` (zip)
```
manifest.json   { "type": "code|city|solution|challenge-pack", "schemaVersion": 1,
                  "playerApiVersion": 3, "gameVersion": "x.y.z", "author": "<apelido>",
                  "createdAt": "...", "files": [{ "path": "...", "sha256": "..." }] }
code/*.cs
city/city.json.gz      (opcional)
challenges/<id>/...    (pacote de desafios)
thumbnail.png          (opcional, ≤ 512 KB)
```
**Regras:**
- **Somente código-fonte, nunca binários.** Nada de `.dll`, `.exe` ou scripts de outras linguagens. O código é **recompilado e analisado** no destino (L1/L2 sempre se aplicam).
- Lista fechada de extensões (`.json`, `.cs`, `.gz`, `.png`, `.md`).
- Limites: 20 MB compactado, 100 MB descompactado, 2.000 arquivos, 200 KB por `.cs`.

## 3. Fluxo de importação
```
Arquivo .citysky
  → validação do zip (limites, "zip slip": nenhum caminho fora da pasta de destino, extensões, hashes do manifest)
  → validação do JSON (esquema, sem tipos polimórficos)
  → PRÉ-VISUALIZAÇÃO: código em modo somente leitura, miniatura da cidade, autor, avisos
  → o usuário decide:
       • "Abrir para ler"           → nada executa
       • "Carregar cidade"          → o estado carrega; comportamentos de código importado ficam DESATIVADOS
       • "Executar no laboratório"  → compila + analisa + executa no Worker (L1–L6)
  → comportamentos importados só são ativados na cidade após revisão explícita ("Ativar código de <autor>")
```

## 4. Segurança (resumo; detalhe em `CODE_EXECUTION_SECURITY.md`)
| Ameaça | Controle |
|---|---|
| Código malicioso no pacote | sandbox L1–L6, sem execução automática |
| Zip bomb / arquivos gigantes | limites de tamanho e contagem, descompactação em streaming com contador |
| Zip slip (caminho `../`) | normalização + verificação de prefixo do destino |
| JSON malicioso (desserialização) | `TypeNameHandling.None`, esquema, limites de profundidade e tamanho |
| Save com estado inconsistente | validação de invariantes após o carregamento (`SAVE_SYSTEM.md`) |
| Conteúdo impróprio (nomes, comentários) | o jogo não tem texto livre além do código e de nomes curtos. Em sala, a moderação é do professor; nomes de cidade e apelidos passam por um filtro simples de palavras |

**Testes:** corpus `CitySky.Security.Tests/Packages`, com pacotes de zip bomb, zip slip, binário disfarçado, JSON profundo, manifest com hash errado e código com cada ataque do corpus de código.

## 5. Transporte
Por arquivo (pendrive, pasta compartilhada da escola, anexo no ambiente virtual ou e-mail). **Sem servidor** (ADR-0017).

**Steam Workshop (Fase 20, ADR-0018):** quando houver versão Steam, o Workshop vira o canal online. A Valve hospeda o conteúdo, sem servidor nosso. Os itens do Workshop usam o mesmo formato `.citysky`, o mesmo fluxo de importação (§3) e o mesmo sandbox. Nunca são executados automaticamente ao assinar o item. Uma galeria própria continua em `FUTURE_FEATURES.md`.

## 6. Pedagogia
- **Revisão de código entre colegas:** importar a solução de um colega, ler, comentar (comentários no próprio código) e comparar métricas no Laboratório.
- **Integridade acadêmica:** o pacote registra autor e hashes. Detecção de similaridade entre soluções da turma (no painel do professor) fica para depois da Fase 13.
