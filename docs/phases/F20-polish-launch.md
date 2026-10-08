# Fase 20 — Polimento, piloto escolar e lançamento (Windows)

> **Status:** Intenção de design · **Bloco:** E · **Atualizado:** 2026-10-08
> **Entrada:** gate da Fase 19 · **Saída:** lançamento no Windows pela Microsoft Store e itch.io (e Steam, se o dono do produto decidir), com piloto escolar concluído e material do professor publicado

---

## 1. Objetivo
Transformar o jogo num **produto educacional lançável**: acessível, localizado, rápido, com material para professores, nome definitivo e validado numa turma real ao longo de semanas.

## 2. Por que nesta posição
É a última fase do roadmap principal. Os itens aqui (marca, piloto longo, material do professor, publicação) só fazem sentido com o jogo completo.

## 3. Escopo
**Inclui:**
- **Acessibilidade:** avaliação de leitor de tela no UI Toolkit, redução de movimento, legendas, rebinding completo, auditoria do `UI_UX.md` §5.
- **Localização:** inglês (tabelas prontas desde a F1).
- **Áudio adaptativo** e onboarding final; presets Médio e Ultra; modo foto/cinemático (avaliar Cinemachine com ficha de dependência).
- **Desempenho final** em PCs reais de escola.
- **Marca e nome** (risco R17): busca de marca e nome definitivo antes de publicar.
- **Piloto escolar** com turma real (várias semanas) e **material do professor** por módulo (plano de aula, objetivos, gabarito comentado, competências).
- **Mapeamento à norma de Computação** complementar à BNCC.
- **Publicação Windows:** Microsoft Store (MSIX assinado pela Store) e itch.io; política de privacidade em linguagem simples.
- **Steam** (decisão do dono do produto; Steam Direct US$ 100, devolvido após US$ 1.000 em vendas): Steamworks.NET atrás de `IPlatformServices`, Steam Cloud, conquistas, **Workshop** com o formato `.citysky` e o mesmo sandbox.
- Revisão da licença Unity (Personal enquanto receita + financiamento < US$ 200 mil em 12 meses).

## 4. Entregáveis
Build de lançamento · página da Store e do itch.io · material do professor · relatório do piloto · (opcional) página da Steam com Workshop.

## 5. Gate de saída
- [ ] Piloto escolar concluído com relatório e mudanças aplicadas.
- [ ] Nome definitivo aprovado; nenhuma marca de terceiros nos assets.
- [ ] Certificação da Microsoft Store aprovada; instalação sem admin confirmada em PC de escola.
- [ ] Auditoria de acessibilidade e de privacidade (LGPD) concluída.
- [ ] Se Steam: conteúdo do Workshop nunca executa ao assinar; L5 completo na versão publicada.

## 6. Riscos
R17 (marca), R18 (LGPD), R23 (bloqueio em escola), R27 (custos de plataforma), R30 (política da Store para código dinâmico), R29 (Windows 10 nas escolas).

## 7. Documentos e ADRs
`UI_UX.md`, `AUDIO.md`, `CLASSROOM.md`, `SECURITY.md` §2, `SHARING_SYSTEM.md` §5, `SAVE_SYSTEM.md` (Steam Cloud), ADR-0017, ADR-0018, `README.md` (nome).
