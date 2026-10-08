# Fase 19 — Conteúdo

> **Status:** Intenção de design · **Bloco:** E · **Atualizado:** 2026-10-08
> **Entrada:** gates das Fases 17 e 18 · **Saída:** transporte público, clima e eventos dinâmicos funcionando com efeitos limitados e explicáveis; cenários e conquistas jogáveis

---

## 1. Objetivo
Dar profundidade e variedade ao jogo: transporte público (§21), clima (§59), eventos dinâmicos (§40), cenários prontos, mais lições e conquistas ligadas a boas práticas.

## 2. Por que nesta posição
São sistemas de alto valor de experiência e baixo risco arquitetural: usam modificadores (`ModifierStack`), grafos e o modelo meso que já existem. Deixá-los para cá protege o caminho crítico.

## 3. Escopo
**Inclui:**
- Transporte público (`PUBLIC_TRANSPORT.md`): ônibus, paradas, linhas criadas pelo jogador (UI e código), escolha modal por custo generalizado, métricas de passageiros, tempo, cobertura, custo e eficiência; `ITransitScheduler`.
- Clima (`WEATHER_SYSTEM.md`): cadeia de Markov por estação, efeitos via modificadores com teto, visual URP (chuva, superfícies molhadas, neblina, sombra de nuvens).
- Eventos dinâmicos (`EVENT_SYSTEM.md` §B1): enchente, seca, epidemia, greve, crise econômica, onda migratória, sempre como reações plausíveis ao estado.
- Semáforos atuados pela demanda e `ITrafficLightPolicy` do aluno; pontes e túneis; edição de terreno (salva como diferença); universidade, biblioteca, incinerador; necessidade *Social*.
- Cenários ("Cidade falida", "Crise de energia", "Código legado"), conquistas de boas práticas, linha do tempo visual de replay, música adaptativa ao termômetro.
- Mais lições e revisões; mapeamento dos módulos às habilidades da norma de Computação da BNCC (Resolução CNE/CEB nº 1/2022), concluído na F20.

## 4. Entregáveis
Épicos por sistema (transporte, clima, eventos, estradas avançadas, cenários, conquistas, áudio, conteúdo).

## 5. Gate de saída
- [ ] Cada evento e efeito de clima com causa explicável e efeito limitado (testes de modificadores).
- [ ] Transporte público: cobertura e escolha modal em faixas plausíveis nos testes de balanceamento.
- [ ] Cenários com condição de vitória/derrota testada headless.
- [ ] Orçamento de desempenho de 100k mantido.

## 6. Riscos
Explosão combinatória de modificadores (`CRITICAL_ANALYSIS.md` §3), calibração da escolha modal, escopo (cortar o que não couber, sem estender a fase).

## 7. Documentos e ADRs
`PUBLIC_TRANSPORT.md`, `WEATHER_SYSTEM.md`, `EVENT_SYSTEM.md`, `ROAD_SYSTEM.md`, `MAP_SYSTEM.md`, `AUDIO.md`, `PROGRESSION.md`, `CLASSROOM.md` §4.
