# Sistema de Trânsito

> **Status:** v1.0 (decisão provisória, revisão obrigatória na Fase 11) · **Data:** 2026-10-08 · **ADR:** [0010](ADR/0010-pathfinding.md) · **MVP:** não (MVP só tem pedestres)

---

## 1. Intenção
Veículos com origem, destino e rota, que seguem ruas, respeitam semáforos, param, aceleram, mudam de faixa e sofrem congestionamento (§20). O trânsito afeta tempo de deslocamento, felicidade, economia, poluição e produtividade.

## 2. Decisão central: **meso autoritativo + micro visual**
| Modelo | Prós | Contras |
|---|---|---|
| Micro autoritativo (cada carro é simulado e o resultado vem dele) | emergência rica; "o que você vê é o que acontece" | caríssimo (CS2: ~1.200 sistemas DOTS); float/física → difícil ser determinístico entre Mono, IL2CPP e .NET (Laboratórios); escala limitada |
| **Meso autoritativo + micro visual (adotado)** | barato (custo por link, não por veículo); determinístico (inteiros); roda *headless* nos Laboratórios; escala para 100k viagens | menos emergência microscópica (ex.: um carro bloqueando a faixa não causa um efeito específico) |
| Macro (só fluxos agregados) | baratíssimo | não dá para "ver" o trânsito nem seguir um cidadão |

**Spike da Fase 11:** construir os dois protótipos (meso e micro autoritativos) numa cidade de 10k, comparar custo, determinismo e "sensação". Revisar o ADR-0010.

## 3. Modelo mesoscópico (Domain)
- **Unidade:** link = faixa de um segmento (ou o segmento agregado por direção, na Fase 11).
- **Estado por link (inteiros):** veículos presentes `n`, capacidade `C` (veículos/h), comprimento `L`, velocidade livre `v0`, fila `q`.
- **Tempo de travessia** (função BPR adaptada):
```
t = t0 × (1 + α × (fluxo/C)^β)        t0 = L / v0,  α = 0,15, β = 4  (calibrável)
```
- **Interseções:** capacidade de saída reduzida pela razão de verde do semáforo (g/C) ou pelas regras de prioridade. Se o link de saída está cheio, forma-se **fila** no link de entrada (*spillback*), que pode bloquear o cruzamento anterior. É esse mecanismo que permite engarrafamentos "em cadeia".
- **Viagens:** uma `Trip` de carro percorre a lista de links do caminho. Entrada e saída de cada link são agendadas pelo tempo de travessia corrente. O progresso fica em ponto fixo.
- **Período:** 1 s TM (4 ticks). Ver `SIMULATION_TICK.md`.
- **Custo de rota dinâmico:** tempos médios por link (janela de 1 h TC) alimentam os custos do pathfinding (com versionamento).

## 4. Microssimulação visual (Presentation, ECS + Burst)
- Para links visíveis, a apresentação gera **veículos representativos** compatíveis com `n` e `q` do link: densidade, fila e velocidade.
- Comportamentos visuais: *car-following* (IDM simplificado), parada no vermelho conforme a fase do semáforo do **domínio**, troca de faixa estética, aceleração e frenagem.
- Veículos ligados a viagens **seguidas** pelo jogador ou selecionadas usam o progresso autoritativo (ver `SIMULATION_LOD.md` §4).
- A microssimulação visual **nunca** devolve dados ao domínio.

## 5. Semáforos (State)
`RoadNode.Control = Signal` com plano de fases (verde/amarelo/vermelho por grupo de movimentos) em **segundos TM**. Plano fixo na Fase 11; atuado pela demanda (fila) na Fase 19. Excelente tema para o padrão **State** (Fase 16) e para políticas do jogador (`ITrafficLightPolicy`, Fase 19).

## 6. Efeitos
| Saída | Efeito |
|---|---|
| tempo de deslocamento | necessidade *Transporte* → felicidade; horários de saída |
| atraso de entregas | custo das empresas |
| emissões por link | campo de poluição do ar e ruído |
| produtividade | trabalhadores atrasados → produção menor |

## 7. Overlay e debug
Overlay de **trânsito** (fluxo/C por link: verde → vermelho), "Show Traffic Routes" (rotas que passam por um link selecionado: "de onde vêm esses carros?"), contagem por link no Inspector da rua.

## 8. Desempenho (metas Fase 11/18)
| Escala | Links | Viagens ativas | Custo meso por passo (1 s TM) |
|---|---:|---:|---|
| 10k cidadãos | ~5.000 | ~2.000 | ≤ 0,5 ms |
| 100k cidadãos | ~30.000 | ~20.000 | ≤ 3 ms (Burst via porta, se necessário) |
Veículos visuais: ≤ 2.000 (Fase 11) / ≤ 6.000 (Fase 18) com Entities Graphics.

## 9. Testes
Tempo BPR (tabela), *spillback* (cenário de gargalo), conservação de veículos (entram = saem + presentes), determinismo, testes de balanceamento (picos de manhã e tarde em faixas plausíveis).
