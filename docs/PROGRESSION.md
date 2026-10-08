# Progressão

> **Status:** v1.0 · **Data:** 2026-10-08 · **MVP:** sim (simplificado)

---

## 1. Dois eixos de progressão, conectados
| Eixo | Medida | Desbloqueia |
|---|---|---|
| **Cidade** (marcos de população/serviços) | população, saúde da cidade, infraestrutura | prédios, áreas do mapa, sistemas (zoneamento, trânsito, serviços) |
| **Engenharia** (domínio de conceitos) | Mastery por conceito (`LEARNING_ENGINE.md`) | APIs (CSK3), ferramentas (editor avançado, testes), automações e políticas |

**Conexão:** alguns marcos de cidade **exigem** conceitos ("para gerenciar 500 habitantes você precisa automatizar a energia: conclua *Métodos com retorno*"), e alguns conceitos **exigem** cidade ("a lição de herança precisa de pelo menos 3 tipos de prédio de serviço"). Os dois eixos se puxam mutuamente.

## 2. Marcos de cidade (proposta inicial)
| Marco | Condição | Desbloqueios |
|---|---|---|
| Vila (MVP) | início | rua, casa, mercado, usina pequena; lições M0–M2 |
| Bairro | 200 hab. + M2 dominado | zoneamento, laços e coleções, prédios residenciais médios |
| Cidade pequena | 1.000 hab. + M3 | trânsito, veículos, escola, clínica |
| Cidade | 5.000 hab. + M5/M6 | serviços completos, herança/polimorfismo com veículos de serviço |
| Metrópole regional | 20.000 hab. + M8 | transporte público, distritos, políticas avançadas |
| Metrópole | 50.000+ hab. + M9/M10 | grandes projetos, desafios de escala (M12) |

## 3. Recompensas
- **Funcionais**, não cosméticas, como recompensa principal: novas ferramentas, prédios, automações.
- Cosméticas (temas do editor, estilos arquitetônicos) como reforço secundário.
- **Conquistas** (Fase 19): ligadas a boas práticas ("Refatorador: reduziu a `CityManager` a < 100 linhas"), e não só a quantidade.

## 4. Falha e recuperação
- Falência e êxodo são **falhas suaves**: a cidade entra em "intervenção" (empréstimo emergencial com missão de recuperação) em vez de *game over*.
- Em Laboratórios, falhar é esperado e barato: reset instantâneo e comparação de tentativas.

## 5. MVP
Uma trilha linear de 7 lições + desafio final. Os desbloqueios do MVP são só os prédios e as APIs das lições. Os marcos de cidade começam a valer na Fase 10.
