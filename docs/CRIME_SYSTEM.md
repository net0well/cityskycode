# Criminalidade e Segurança (Polícia e Bombeiros)

> **Status:** v1.0 (intenção de design) · **Data:** 2026-10-08 · **Fase:** 13 · **MVP:** não

---

## 1. Criminalidade
- **Risco por célula/bairro** (campo na grade): f(desemprego, infelicidade, renda baixa, iluminação noturna, densidade) − cobertura policial.
- **Ocorrências:** sorteadas (RNG determinístico) pelo risco, à noite com mais peso (`DAY_NIGHT_SYSTEM.md`). Afetam *Safety*, o comércio (perdas) e o valor do terreno.
- **Abordagem responsável:** criminalidade é modelada como **consequência de condições sociais e de serviço**, não de "tipos de pessoas". Nenhum atributo demográfico de cidadão aumenta o risco individual. Essa é uma escolha ética explícita para um jogo educacional.

## 2. Polícia
`PoliceStation` (modelo genérico de serviço): cobertura por tempo de resposta, viaturas (veículos de serviço), eficiência por verba. Reduz o risco e resolve ocorrências.

## 3. Bombeiros e incêndios
- `FireStation` + caminhões. Incêndios surgem por risco (prédios antigos, indústria, eventos, seca) e se propagam por vizinhança se não forem atendidos a tempo.
- Despacho: unidade mais próxima com veículo livre; tempo de resposta decide os danos.
- Liga-se ao `EVENT_SYSTEM.md` (incêndio como evento).

## 4. Overlays
Criminalidade (risco), cobertura policial, cobertura de bombeiros, risco de incêndio.

## 5. Pedagogia
State (ocorrência: aberta → atendida → resolvida), Mediator (despacho), filas de prioridade (ocorrências por gravidade).
