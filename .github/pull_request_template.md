<!-- F1-08. Preencha antes de pedir revisão. -->

## O que muda
<!-- Resumo curto da mudança. -->

## Checklist
- [ ] **Pertence à fase atual?** Tarefa: `Fn-xx` (ver `docs/phases/`). Se não pertence, está no roadmap e **não** foi implementado aqui.
- [ ] Docs atualizados (doc do sistema afetado, `PLAYER_API.md` se a API mudou, ADR se a decisão mudou).
- [ ] `/verify` e `/arch-check` sem violações.
- [ ] Testes cobrem a mudança; `dotnet test dotnet/CitySky.slnx` verde (CI Windows + canário Linux).
- [ ] Um tipo por arquivo, em pastas (`CODING_STANDARDS.md` §2). Commits atômicos citando o ID da tarefa.
- [ ] Sem dependência nova sem justificativa; fonte compartilhada (`com.citysky.core`) continua sem deps externas.

## Notas de revisão
<!-- Pontos de atenção, decisões, o que testar manualmente. -->
