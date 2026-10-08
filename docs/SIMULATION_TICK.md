# Simulação em Tick

> **Status:** v1.0 (valores iniciais, a calibrar em playtest/profiling) · **Data:** 2026-10-08 · **ADR:** [0008](ADR/0008-simulation-tick.md) · **MVP:** sim

---

## 1. Problema
O briefing (§33–34) propõe frequências em **Hz reais** (Economia 1 Hz, Tráfego 10–20 Hz...). Isso tem dois defeitos:
1. **O resultado depende da velocidade.** Em 10x, a economia a 1 Hz real faz 10x menos passos por dia de jogo.
2. **O resultado depende do frame rate**, se os sistemas forem acoplados ao `Update()`.

Precisamos de um tempo **discreto, numerado e determinístico**.

## 2. Modelo: um tick, dois relógios

- **Tick:** passo fixo e indivisível da simulação, numerado (`long`). Tudo no domínio é função do número do tick.
- **Tempo de Mobilidade (TM):** escala "física" na qual veículos andam e semáforos trocam. **1 tick = 0,25 s TM.**
- **Tempo de Calendário (TC):** escala do relógio e do calendário (dia/noite, turnos, economia). **1 s TM = R s TC**, com **R = 30** (parâmetro de balanceamento).

| Grandeza | Valor |
|---|---|
| 1 tick | 0,25 s TM = 7,5 s TC |
| 1 minuto TC | 8 ticks |
| 1 hora TC | **480 ticks** |
| 1 dia TC | **11.520 ticks** |
| Em 1x | 4 ticks/s real → 1 hora TC = 2 min reais; **1 dia = 48 min reais** |
| Em 10x | 40 ticks/s → 1 dia = 4,8 min reais |

### 2.1 Por que dois relógios? O problema clássico da escala temporal
Em city builders, um dia de jogo dura minutos reais. Se os veículos andassem em tempo de calendário, um carro a 50 km/h atravessaria a cidade em um piscar de olhos. Se o calendário andasse em tempo físico, um dia duraria 24 h reais.

**Decisão:** a mobilidade roda em TM (velocidades visualmente plausíveis), o calendário em TC (comprimido), e a conversão é fixa (R). Consequência aceita: um trajeto de 2 km a pé (~24 min TM) "dura" ~12 h TC.

**Mitigação no design (a calibrar):**
- velocidade de mobilidade com um multiplicador de jogo (`MobilitySpeedScale`, padrão 3x, por exemplo pedestre a 4 m/s visuais);
- cidades do MVP pequenas (≤ 1 km);
- agendas planejadas com folga (o cidadão sai de casa quando a viagem estimada termina no horário).

Com `MobilitySpeedScale = 3`, 1 km a pé leva ~4 min TM ≈ 2 h TC. Ainda é longo, mas é um compromisso comum no gênero. **Este é um ponto explícito para playtest** (Fases 5 e 11): ajustar R, a escala de mobilidade e o tamanho das cidades.

## 3. Tabela de sistemas (períodos em ticks)

| Sistema | Período | Ticks | Freq. real em 1x | em 10x | Observação |
|---|---|---:|---:|---:|---|
| Renderização | por frame | — | 60 FPS | 60 FPS | independente |
| Movimento visual (interpolação) | por frame | — | 60 | 60 | posição = f(viagem, t + α) |
| Microssimulação **visual** de veículos (ECS, Fase 11) | passo fixo de apresentação | — | 30 Hz | 30 Hz (dt maior, LOD) | não autoritativa |
| Aplicação de comandos | todo tick | 1 | 4 Hz | 40 Hz | — |
| Viagens: progresso e chegadas | todo tick | 1 | 4 Hz | 40 Hz | — |
| Planejamento de viagens / pathfinding | todo tick, com orçamento | 1 | 4 Hz | 40 Hz | máx. N buscas/tick |
| Semáforos (fases em TM) | todo tick | 1 | — | — | fase verde = 30 s TM |
| Tráfego meso | 1 s TM | 4 | 1 Hz | 10 Hz | substitui os "10–20 Hz" do briefing |
| Agenda do cidadão (decisão) | 15 min TC por cidadão | baldes de 120 | — | — | 1/120 dos cidadãos por tick |
| Necessidades | 1 h TC por cidadão | baldes de 480 | — | — | 1/480 por tick |
| Felicidade | 1 h TC por cidadão | baldes de 480 | — | — | fase deslocada das necessidades |
| Rede elétrica | 1 h TC ou quando *dirty* | 480 | 1/2 min | 1/12 s | *dirty* = mudança na rede |
| Emprego | 1 h TC | 480 | | | |
| Economia: liquidação horária | 1 h TC | 480 | | | operações de empresas |
| Economia: salários, impostos, contas | 1 dia TC | 11.520 | 1/48 min | 1/4,8 min | — |
| Fechamento de orçamento | 1 dia TC | 11.520 | | | relatório diário |
| Migração e crescimento | 1 dia TC (06:00) | 11.520 | | | substitui o "1/Hour" do briefing |
| Detecção de problemas | 1 h TC | 480 | | | |
| Saúde da cidade | 1 h TC + EMA | 480 | | | |
| Eventos / Clima (pós-MVP) | 1 h TC | 480 | | | |
| Autosave | 5 min reais | — | | | camada de aplicação |

> **Comparação com o briefing:** "Citizen Needs 1–5 Hz" vira "cada cidadão 1x por hora de jogo, em baldes". O custo total por segundo real é menor e **não depende da velocidade**: por dia de jogo, o trabalho é sempre o mesmo.

## 4. Laço do runner (Unity)
```csharp
// SimulationRunner.Update(realDeltaSeconds) — chamado pela Presentation 1x por frame
_due += realDeltaSeconds * Speed.TicksPerSecond;   // 0 (pausa), 4, 8, 12, 20, 40
int ran = 0;
long start = _clock.Timestamp();
while (_due >= 1.0 && ran < MaxTicksPerFrame && _clock.ElapsedMs(start) < SimBudgetMs)
{
    _simulation.RunTick(++_tick);
    _due -= 1.0;
    ran++;
}
if (_due > MaxBacklogTicks) { _due = MaxBacklogTicks; EffectiveSpeedLimited = true; }
Alpha = _due;   // 0..1 → interpolação visual
```
- `MaxTicksPerFrame = 12`, `SimBudgetMs = 6` ms (alvo médio ≤ 2 ms no MVP).
- Se a máquina não acompanha, a **velocidade efetiva cai** e a UI mostra "7,2x de 10x". O jogo nunca entra em "spiral of death" e **nunca pula ticks**: o resultado continua idêntico, só fica mais lento.
- `realDeltaSeconds` limitado (ex.: 0,1 s) para não acumular backlog após pausas do SO.

## 5. Velocidades
| Opção | Ticks/s real | Dia de jogo |
|---|---:|---|
| Pausa | 0 | — |
| 1x | 4 | 48 min |
| 2x | 8 | 24 min |
| 3x | 12 | 16 min |
| 5x | 20 | 9,6 min |
| 10x | 40 | 4,8 min |

Em **Laboratórios** (*headless*), a simulação roda "o mais rápido possível", tipicamente milhares de ticks por segundo.

## 6. Comandos e o tick
- Comando recebido durante o frame → `targetTick = tickAtual + 1`.
- Execução de script do jogador (MVP): a simulação **pausa**, o script roda contra o snapshot do tick T, e os comandos resultantes entram em T+1 em ordem de emissão.
- Replays reaplicam a mesma lista `(targetTick, seq, comando)`.

## 7. Calendário
- O dia começa às 00:00. Fases: **Manhã** 06–12, **Dia** 12–17, **Tarde/Entardecer** 17–20, **Noite** 20–06 (ver `DAY_NIGHT_SYSTEM.md`).
- Semana de 7 dias (fins de semana a partir da Fase 10). Mês de 30 dias para relatórios.
- Jogo novo começa às 07:00 do dia 1.

## 8. Testes
- `SimulationClock`: conversões tick ↔ TM ↔ TC (casos de borda, overflow em 100 anos de jogo).
- Determinismo independente da velocidade: rodar 2 dias em 1x e em 10x (mesmos comandos nos mesmos ticks) → hashes iguais.
- Scheduler: cada sistema executa exatamente `floor(T/period)` vezes em T ticks, nas fases corretas.
- Runner: com orçamento estourado, nenhum tick é pulado e a velocidade efetiva é reportada.

## 9. Questões em aberto
- Valor final de **R** e de `MobilitySpeedScale` (playtest das Fases 5/11).
- O tick de 0,25 s TM é suficiente para a microssimulação visual? Sim, porque ela é da apresentação e roda no próprio passo.
