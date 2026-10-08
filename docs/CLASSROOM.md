# Sala de Aula

> **Status:** v1.0 · **Data:** 2026-10-08 · **ADR:** [0017](ADR/0017-product-scope-classroom.md) · **MVP:** parcial (perfis, tamanho das lições, playtest em sala)
> O CitySkyCode é um **produto educacional** que será usado em sala de aula, **sem servidor e sem orçamento**. Tudo aqui funciona offline, com arquivos.

---

## 1. Restrições reais de uma escola
| Restrição | Consequência no design |
|---|---|
| Aula de 45–50 min, com tempo de entrada e saída | lição ≤ 20 min; salvar a qualquer momento; retomar em < 30 s |
| Vários alunos no mesmo PC (turnos) | **perfis locais** por aluno, sem conta e sem e-mail |
| PCs fracos e às vezes antigos | preset Baixo (Iris Xe, 8 GB, 30 FPS) como linha de base; medir no piloto |
| Sem permissão de administrador | instalação por usuário (MSIX/Store); AppContainer não exige admin |
| Internet instável ou bloqueada | 100% offline |
| TI da escola bloqueia executáveis não assinados (SmartScreen/Smart App Control) | distribuição pela **Microsoft Store** (pacote assinado pela Microsoft); instrução para a TI pré-aprovar |
| Laboratórios com Chromebook ou Linux | **não atendidos** enquanto só houver Windows (risco R21). Linux na Fase 21+ (ADR-0018); Chromebook só com uma futura versão web |
| Alunos menores de idade (LGPD) | nenhum dado sai do PC; apelido no lugar do nome; nada de telemetria remota |

## 2. Funcionalidades por fase
| Fase | Funcionalidade |
|---|---|
| **MVP (8)** | seleção de **perfil local** na abertura (apelido + avatar; PIN opcional de 4 dígitos, que é conveniência e não segurança); lições dimensionadas para ≤ 20 min; **playtest em sala real** como gate |
| **9** | **exportação de progresso** (`.cityskyprogress`, JSON: perfil, domínio por conceito, lições, dicas usadas, erros mais comuns; sem código nem cidade); **compartilhamento por arquivo** (`SHARING_SYSTEM.md`) |
| **13** | **painel do professor local**: modo "Professor" importa os arquivos da turma (pasta) e mostra a tabela aluno × conceito, as lições concluídas, os erros frequentes da turma e quem travou; exporta CSV. **Pacote de turma**: o professor define quais lições e prédios ficam liberados e o ritmo. **Pacotes de desafio do professor**: criados num editor simples e compartilhados por arquivo; o conteúdo é **não confiável** e roda no sandbox |
| **20** | **piloto escolar** com turma real; **material do professor** por módulo (plano de aula, objetivos, gabarito comentado, competências); **alinhamento com a norma de Computação** complementar à BNCC (Resolução CNE/CEB nº 1/2022, anexo de competências e habilidades); distribuição pela Microsoft Store (escolas não usam a Steam, que exige conta com idade mínima) |
| **21+** | suporte a Linux para laboratórios escolares (ADR-0018) |

## 3. Integridade do progresso (sem servidor)
- O arquivo de progresso leva um **hash** do conteúdo para detectar corrupção acidental. **Não há proteção real contra edição**: sem servidor, qualquer segredo no cliente pode ser extraído.
- O painel do professor deve ser usado como **evidência de aprendizagem**, não como prova. Para avaliar, o professor propõe um desafio em sala e observa a validação ao vivo.

## 4. Alinhamento curricular
- A Resolução CNE/CEB nº 1/2022 define normas de Computação como complemento à BNCC, com competências e habilidades em anexo, e prazo de implementação pelas redes.
- **Tarefa de conteúdo (Fase 19–20):** mapear cada módulo de `LEARNING_PATH.md` às habilidades do anexo (ainda não analisado nesta documentação) e publicar a tabela no material do professor.
- O foco do jogo (programação textual em C#, POO, engenharia de software) atende melhor ao **ensino médio e técnico** do que aos anos iniciais. O público-alvo do material do professor deve declarar isso.

## 5. Privacidade (LGPD)
- Perfis só com apelido. Nenhum dado pessoal é exigido.
- Arquivos exportados ficam sob controle do aluno e da escola, e o jogo nunca envia nada pela rede.
- Qualquer telemetria futura exige consentimento explícito do responsável legal (art. 14) e revisão de privacidade (`SECURITY.md` §2).

## 6. Fora de escopo (sem orçamento)
Turmas na nuvem, contas, sincronização, integração com Google Classroom/Moodle por API, ranking online. Ficam em `FUTURE_FEATURES.md`. A troca de arquivos (pendrive, pasta compartilhada, anexo no ambiente virtual da escola) cobre o essencial.

## 7. Testes
- Perfis: dois alunos no mesmo PC não veem o progresso um do outro; trocar de perfil não perde o estado.
- Exportação/importação de progresso: *round-trip*, arquivo corrompido (hash), versão antiga (migração).
- Painel: importar 40 arquivos (uma turma) em < 2 s.
- Playtest em sala: protocolo em `docs/playtests/` (tempo por lição, abandono, ajuda pedida ao professor).
