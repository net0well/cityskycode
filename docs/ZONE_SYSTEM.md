# Sistema de Zoneamento

> **Status:** v1.0 (intenção de design) · **Data:** 2026-10-08 · **Fase:** 10 · **MVP:** não (MVP: lotes sem zona; o jogador constrói diretamente)

---

## 1. Tipos (§11)
`Residential` (baixa/média/alta densidade), `Commercial`, `Industrial`, `Office`, `Public`, `Park`, `Infrastructure`.

## 2. Mecânica
- O jogador **pinta zonas** em lotes (gerados ao longo das ruas, ver `ROAD_SYSTEM.md` §6).
- O **setor privado** constrói nos lotes zoneados conforme a **demanda**: prédios surgem e crescem (densificação) e podem ser abandonados.
- Prédios públicos (`Public`, `Park`, `Infrastructure`) continuam sendo construídos pelo jogador.

## 3. Demanda (RCI + O)
Calculada a cada hora TC, com valores de −100 a +100:
```
Residencial = f(vagas de emprego abertas, atratividade (felicidade, serviços), moradia livre ↓)
Comercial   = f(poder de compra dos domicílios, oferta comercial existente ↓, trânsito de clientes)
Industrial  = f(demanda de bens, mão de obra disponível, logística (acesso viário), exportações)
Escritórios = f(população educada (Fase 13), serviços)
```
Cada termo é registrado como **contribuição** (explicabilidade): "Demanda residencial alta porque há 120 vagas abertas".

Sequência típica (§11): demanda residencial alta → casas surgem → população ↑ → demanda comercial ↑ → comércios surgem → empregos ↑.

## 4. Crescimento
- `ZoneGrowthSystem` (1 h TC): escolhe lotes candidatos (Specification: zoneado ∧ acessível ∧ com energia ∧ inclinação ok), ordenados por valor do terreno e acessibilidade, e cria `BuildBuilding` com dono privado (Factory por zona e densidade).
- Ritmo limitado por "capacidade de construção" da cidade (Fase 10: construtoras, §19).
- Abandono: prédio sem energia, sem acesso ou com ocupação/lucro baixo por X dias.

## 5. Valor do terreno
Campo na grade de análise (32 m): acessibilidade + serviços + parques − poluição − ruído − inclinação. Afeta impostos (IPTU), demanda e densificação.

## 6. Pedagogia
Specification (regras de lote), Factory (criação por zona), Observer (notificações de crescimento), "políticas de zoneamento" escritas pelo aluno (Fase 16).

## 7. Testes
Demanda (tabelas de cenário), crescimento só em lotes válidos, determinismo da escolha de lotes, abandono.
