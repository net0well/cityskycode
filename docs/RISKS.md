# Registro de Riscos

> **Status:** vivo · **Atualizado:** 2026-10-08 · Revisar ao fim de cada fase.
> Análise detalhada: `CRITICAL_ANALYSIS.md`. Escala: P (probabilidade) e I (impacto) de 1 a 5. Exposição = P × I.

| ID | Risco | Categoria | P | I | Exp. | Mitigação | Gatilho/indicador | Dono | Status |
|---|---|---|---|---|---|---|---|---|---|
| R01 | Escopo excede a capacidade da equipe | Produção | 5 | 5 | 25 | MVP mínimo, gates, regra §75, roadmap orientado a risco | fase atrasa > 50% | PO | Aberto |
| R02 | IPC/Process não funciona no IL2CPP | Técnico | 2 | 5 | 10 | spike S1; fallback TCP loopback; em último caso, Mono no player | S1 falha | Arquitetura | Aberto |
| R03 | Monaco em webview com problemas (input, IME, memória) | Técnico | 3 | 4 | 12 | spike S2; `ICodeEditorHost`; fallback nativo | S2 falha | UI | Aberto |
| R04 | Sandbox burlado (allowlist incompleta) | Segurança | 3 | 4 | 12 | camadas L1–L7; corpus; `security-auditor`; sem compartilhamento antes de L5 | violação L2 sem L1 | Segurança | Aberto |
| R05 | GPU como gargalo (geometria/sombras) | Performance | 4 | 4 | 16 | URP + GRD + occlusion; LOD obrigatório; linter; spike S3 | FPS abaixo do alvo em cena de referência | Gráficos | Aberto |
| R06 | Mono/IL2CPP muito mais lento que os benchmarks .NET | Performance | 3 | 3 | 9 | medir no player; Burst via portas; time-slicing | S4 (fator Mono) | Simulação | Aberto |
| R07 | Determinismo quebra entre runtimes | Técnico | 3 | 4 | 12 | inteiros/ponto fixo; testes de hash cruzados; ordem fixa | teste de hash falha | Simulação | Aberto |
| R08 | Malha de interseções de estradas (artefatos, custo) | Técnico | 4 | 3 | 12 | spike S5; MVP só com segmentos retos | S5 falha | Gráficos | Aberto |
| R09 | Sobrecarga cognitiva (cidade + código) | Pedagógico | 4 | 4 | 16 | pausa, Laboratórios, *fading*, playtests | abandono > 30% numa lição | Pedagogia | Aberto |
| R10 | Código vira trapaça (atalhos mágicos) | Design | 3 | 4 | 12 | grandezas derivadas, mesmas regras da UI, revisão de cada membro da Player API | playtest acha atalho | Design | Aberto |
| R11 | Mudanças da Unity (preço, CoreCLR, ECS, HDRP) | Negócio/Técnico | 3 | 3 | 9 | domínio independente; versões fixadas; revisões de ADR | anúncio da Unity | Arquitetura | Aberto |
| R12 | Balanceamento instável (laços de realimentação) | Design | 4 | 3 | 12 | amortecimento, testes de balanceamento *headless* | indicadores fora das faixas | Design | Aberto |
| R13 | Escala temporal (viagens "longas" no calendário) incomoda | Design | 3 | 2 | 6 | calibrar R e a escala de mobilidade em playtest | feedback de playtest | Design | Aberto |
| R14 | Custo de arte (orçamento zero) | Produção | 4 | 3 | 12 | kits CC0 (Kenney, Quaternius), estilo low-poly coerente, variação procedural | backlog de arte | Arte | Aberto |
| R15 | Host gratuito do editor imaturo (UWB desatualizado; janela PhotinoX menos imersiva) | Dependência | 3 | 3 | 9 | `ICodeEditorHost`; spike S2 com 3 candidatos; fallback nativo | S2 falha | UI | Aberto |
| R16 | Doc rot (documentação desatualizada) | Processo | 4 | 2 | 8 | DoD com docs; status em cada doc; revisões por fase | divergência doc × código | Todos | Aberto |
| R17 | Nome/marca semelhante a *Cities: Skylines* (agora com lançamento confirmado: produto educacional). O nome definido, **City: SkyCode**, repete o formato "palavra: Sky…" e fica ainda mais próximo | Legal | 4 | 3 | 12 | busca de marca antes da Fase 20; nome definitivo antes de publicar na Store | início da Fase 20 | PO | Aberto |
| R18 | LGPD/menores (uso em sala de aula confirmado) | Legal | 3 | 4 | 12 | nenhum dado sai do PC; perfis só com apelido; exportações controladas pela escola; telemetria só com consentimento do responsável | qualquer proposta de coleta | PO | Aberto |
| R19 | Overrides do dotnet-claude-kit ignorados por agentes (C# 10+ no código Unity) | Processo | 3 | 2 | 6 | `LangVersion 9` no csproj compartilhado (o compilador barra); `CLAUDE.md` com overrides | build Unity falha | Arquitetura | Aberto |
| R20 | Tráfego meso não "parece" vivo o bastante | Design | 3 | 3 | 9 | microssimulação visual rica; spike comparativo na Fase 11 | playtest | Simulação | Aberto |
| R21 | Laboratórios escolares com Chromebook ou Linux (o jogo é só Windows até a Fase 21) | Produto | 4 | 4 | 16 | levantar o parque de máquinas das escolas-alvo antes do piloto; escolher escola piloto com Windows; Linux na Fase 21+; Chromebook só via versão web futura | escola piloto sem Windows | PO | Aberto |
| R22 | Chromium desatualizado no UnityWebBrowser | Segurança | 3 | 3 | 9 | conteúdo 100% local, CSP, navegação bloqueada; preferir a janela PhotinoX (WebView2 Evergreen no Windows) se o S2 permitir | S2 / CVEs do Chromium | Segurança | Aberto |
| R23 | Bloqueio por SmartScreen/Smart App Control em PCs de escola | Distribuição | 4 | 4 | 16 | MSIX assinado pela Microsoft Store (gratuito); guia de pré-aprovação para a TI; spike S7 | S7 falha | PO | Aberto |
| R24 | Visual limitado aos kits CC0 (estilo low-poly) | Arte | 4 | 2 | 8 | direção de arte coerente com os kits; variação procedural; paleta e iluminação próprias | playtest visual | Arte | Aberto |
| R25 | Compartilhamento usado para circular código malicioso entre alunos | Segurança | 3 | 4 | 12 | L5 completo antes do recurso; nunca executar automaticamente; corpus de pacotes maliciosos; revisão do `security-auditor` | Fase 9 | Segurança | Aberto |
| R26 | Dívida de portabilidade: código acoplado ao Windows torna o port caro na Fase 21+ | Arquitetura | 3 | 3 | 9 | interfaces de plataforma desde a Fase 1; CA1416; testes .NET num runner Linux | violação do CA1416 / canário falha | Arquitetura | Aberto |
| R27 | Custos de plataforma (Steam US$ 100, Apple ~US$ 99/ano) contra o orçamento zero | Negócio | 3 | 2 | 6 | Windows primeiro; itch.io/Microsoft Store gratuitos; a taxa da Steam é devolvida após US$ 1.000 | decisão de lançar na Steam/macOS | PO | Aberto |
| R28 | Steam Deck: programar sem teclado e janelas separadas no Gaming Mode | Produto | 3 | 2 | 6 | alvo "Playable"; editor dentro do jogo no Deck; teclado físico recomendado | Fase 21+ | UI | Aberto |
| R29 | Windows 10 nas escolas: fim do suporte em 14/out/2025; ESU para consumidores estendido até 12/out/2027; escolas dependem de ESU comercial/educacional. PCs de laboratório podem ficar sem atualizações, ou a escola pode migrar para Linux/Chromebook | Produto | 3 | 3 | 9 | testar Win10 22H2 nos spikes S6/S7; levantar a versão do Windows das escolas-alvo antes do playtest; decidir até quando o Win10 fica na matriz de testes | S7; levantamento da escola piloto | PO | Aberto |
| R30 | Microsoft Store recusar ou restringir um app que compila e executa código do usuário (política 10.2.2, inclusão dinâmica de código) | Distribuição | 2 | 4 | 8 | ler a política vigente no S7; declarar a funcionalidade na listagem; consultar o suporte do Partner Center; alternativa: itch.io + guia para a TI | S7 | PO | Aberto |
