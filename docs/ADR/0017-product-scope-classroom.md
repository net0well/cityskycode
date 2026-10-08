# ADR-0017: Produto educacional para sala de aula, com compartilhamento e custo zero

- **Status:** Aceito
- **Data:** 2026-10-08
- **Decisores:** dono do produto (respostas às perguntas em aberto da `CRITICAL_ANALYSIS.md` §7)
- **Documentos afetados:** `CLASSROOM.md`, `SHARING_SYSTEM.md`, `MVP.md`, `DEVELOPMENT_ROADMAP.md`, `TECH_STACK.md`, `CODE_EDITOR.md`, `ASSET_PIPELINE.md`, `GRAPHICS.md`, `SECURITY.md`, `CODE_EXECUTION_SECURITY.md`, `RISKS.md`, `FUTURE_FEATURES.md`, ADR-0007

## Contexto
A análise crítica deixou cinco perguntas de produto em aberto. Em 2026-10-08 o dono do produto respondeu:

| Pergunta | Resposta |
|---|---|
| Hardware mínimo | seguir a recomendação (GPU integrada Intel Iris Xe, 8 GB, 30 FPS no preset Baixo) |
| Uso em sala de aula | **sim** |
| Compartilhamento de código/cidades entre jogadores | **sim** |
| Orçamento para assets e ferramentas pagas | **zero**: somente recursos gratuitos |
| Meta | **produto educacional** |

## Decisão
1. **Hardware mínimo** confirmado como no `PERFORMANCE.md` §1. O piloto escolar mede em PCs reais de escola.
2. **Sala de aula é requisito de primeira classe** (`CLASSROOM.md`):
   - perfis de aluno locais no mesmo PC (desde o MVP);
   - lições de até 20 min, para caberem numa aula de 45–50 min;
   - exportação de progresso por arquivo (Fase 9);
   - painel do professor **local**, que importa os arquivos da turma, sem servidor (Fase 13);
   - pacotes de desafio do professor (Fase 13);
   - material do professor e alinhamento com a norma de Computação complementar à BNCC, Resolução CNE/CEB nº 1/2022 (Fase 20).
3. **Compartilhamento é requisito** (`SHARING_SYSTEM.md`):
   - por **arquivo** (`.citysky`), sem servidor;
   - contém **somente código-fonte** e estado de cidade, nunca binários;
   - conteúdo importado **nunca** executa sozinho;
   - entra na **Fase 9**, condicionado ao sandbox L5 completo (AppContainer/token restrito + Job Object) e ao corpus de pacotes maliciosos no CI.
4. **Custo zero**:
   - **sai a Vuplex** (paga). Os candidatos do spike S2 passam a ser UnityWebBrowser (MIT, CEF), WebView2 numa janela nativa acoplada (runtime gratuito e atualizado pelo Windows) e o editor nativo mínimo (ADR-0007 revisado);
   - **arte e áudio CC0** (Kenney, Quaternius, Poly Haven, ambientCG), mais assets gratuitos da Asset Store só onde a licença permitir;
   - ferramentas gratuitas (Unity Personal, VS Code/Visual Studio Community, Blender, GitHub);
   - **distribuição pela Microsoft Store** como pacote MSIX: o cadastro individual é gratuito desde set/2025, e a Store assina o pacote, o que evita bloqueios de SmartScreen/Smart App Control em escolas. itch.io como canal secundário.
5. **Produto educacional:**
   - nenhuma coleta de dados (LGPD, possivelmente menores);
   - nenhuma conta de usuário;
   - acessibilidade como requisito;
   - revisão de nome e marca antes do lançamento;
   - licença Unity Personal enquanto receita + financiamento < US$ 200 mil em 12 meses.

## Alternativas consideradas
| Alternativa | Por que não |
|---|---|
| Compartilhamento online (galeria, turmas na nuvem) | exige servidor, moderação e custos recorrentes; incompatível com orçamento zero. Fica em `FUTURE_FEATURES.md` |
| Compartilhar assemblies compilados | binário não analisável: anula L1/L2. Só código-fonte, recompilado e analisado no destino |
| Manter a Vuplex | paga |
| Certificado de assinatura comercial | pago. A SignPath Foundation (gratuita) exige projeto 100% OSI, o que um build Unity provavelmente não atende |

## Consequências
**Positivas:** produto viável sem orçamento; LGPD simples (nada sai da máquina); escolas instalam pela Store.
**Negativas:**
- visual limitado ao estilo dos kits CC0 (low-poly estilizado);
- risco técnico maior no editor (opções gratuitas menos maduras);
- painel do professor sem nuvem (troca de arquivos manual);
- **laboratórios com Chromebook ou Linux** não são atendidos enquanto o jogo for só para Windows.

**Riscos:** ver `RISKS.md` R15, R17, R18, R21–R25.

## Gatilhos de revisão
Surgimento de orçamento · demanda de escolas por recursos online · resultados do spike S2 · piloto escolar revelando hardware abaixo do mínimo.

## Referências
- Registro gratuito na Microsoft Store (set/2025): https://blogs.windows.com/windowsdeveloper/2025/09/10/free-developer-registration-for-individual-developers-on-microsoft-store/
- UnityWebBrowser (changelog): https://projects.voltstro.dev/UnityWebBrowser/latest/changelog/
- Kenney (assets CC0): https://kenney.itch.io/kenney-jam-resources
- Resolução CNE/CEB nº 1/2022: https://www.lex.com.br/resolucao-cne-ceb-no-1-de-4-de-outubro-de-2022/
- SignPath Foundation (assinatura gratuita para OSS): https://signpath.org/about
