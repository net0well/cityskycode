# Fase 14 — Currículo de POO

> **Status:** Intenção de design · **Bloco:** D · **Atualizado:** 2026-10-08
> **Entrada:** gate da Fase 10 (os tipos de prédio e de veículo precisam existir); pode correr em paralelo às Fases 11–13 · **Saída:** Módulos M5–M7 com missões nascidas dos sistemas da cidade e pré/pós-teste mostrando ganho

---

## 1. Objetivo
Ensinar encapsulamento, composição, herança, polimorfismo e interfaces **com problemas que a cidade já tem** (§41, §72), nunca como aula avulsa.

## 2. Por que nesta posição
O princípio "problema primeiro" (§71) exige que a dor exista antes do conceito. Herança precisa de vários tipos de prédio de serviço; interfaces precisam de veículos que calculam rotas de formas diferentes (§74). Esses sistemas chegam nas Fases 10–13.

## 3. Escopo
**Inclui:**
- **M5 Encapsulamento e composição:** modificadores de acesso, invariantes, validação; `Hospital` → departamentos (composição, §41).
- **M6 Herança e polimorfismo:** `Building` → `ResidentialBuilding`/`CommercialBuilding`/`ServiceBuilding`; `virtual CalculateMaintenanceCost()`; `abstract`; missão do lixo etapa 3 (tipos de veículo: lixo, ônibus, polícia, bombeiros).
- **M7 Interfaces:** `IRouteStrategy` por tipo de veículo (missão do lixo etapa 4), `IMapOverlay` preparado.
- Player API: tipos-base abertos para herança pedagógica (exceção ao `sealed`, override O7), registro explícito de instâncias (`CRITICAL_ANALYSIS.md` D7: declarar classe não cria nada).
- Analyzer pedagógico `CSK2005` (`if/else if` sobre tipo → sugere polimorfismo).
- Editor: ir para definição (código do aluno + "metadata as source" da Player API).
- Arco "da POO ao orientado a dados", parte 1: o jogo mostra os limites da herança (LSP) e prepara a composição.
- Pré/pós-teste conceitual curto (`LEARNING_DESIGN.md` §9).

**Não inclui:** SOLID formal (F15), padrões (F16).

## 4. Entregáveis
Missões M5–M7 com pacotes de desafio completos · extensões da Player API · `CSK2005` · ir para definição · instrumento de pré/pós-teste.

## 5. Gate de saída
- [ ] Todos os desafios M5–M7 passam no `CitySky.Content.Tests`.
- [ ] Playtest: ganho mensurável entre pré e pós-teste; nenhuma missão com abandono > 30%.
- [ ] Mudanças na Player API revisadas por `security-auditor` e `get_public_api`.

## 6. Riscos
R09 (sobrecarga), herança ensinada × engine que prefere composição (tratado como arco narrativo, `CRITICAL_ANALYSIS.md` §4.9), herança de tipos-base da API abrindo superfície de ataque (revisão de segurança dos membros `virtual`).

## 7. Documentos e ADRs
`LEARNING_PATH.md`, `BUILDING_SYSTEM.md` §5, `VEHICLE_SYSTEM.md` §5, `PLAYER_API.md` §3, `CODE_ANALYSIS.md`, `CODE_EDITOR.md`.
