# Sistema de Saúde

> **Status:** v1.0 (intenção de design) · **Data:** 2026-10-08 · **Fase:** 13 · **MVP:** não

---

## 1. Componentes
`Clinic` (atenção básica), `Hospital` (com **composição** de departamentos: `EmergencyDepartment`, `Pharmacy`, `Staff`, como no exemplo do §41), `Ambulance` (veículo de serviço).

## 2. Modelo
- **Saúde individual** (0–10000) por cidadão: cai com poluição, água contaminada, idade, falta de atendimento, e sobe com atendimento e ambiente saudável.
- **Demanda:** consultas eletivas (pela saúde baixa) + emergências (eventos, acidentes, incêndios).
- **Atendimento:** capacidade (leitos/consultas por dia) × eficiência. Fila quando saturado; tempo de espera afeta a satisfação.
- **Ambulâncias:** despacho para emergências pela unidade mais próxima com veículo livre (Mediator/Dispatcher, Fase 16). O tempo de resposta conta.
- **Óbitos** (Fase 13, com sensibilidade): saúde 0 ou idade avançada → falecimento tratado com discrição (sem gráficos chocantes; público jovem).

## 3. Métricas
Cobertura, tempo médio de espera, tempo de resposta de ambulância, saúde média por bairro.

## 4. Pedagogia
Composição (`Hospital` → departamentos), Builder (configurar hospital complexo), State (paciente: aguardando → em atendimento → alta), Mediator (despacho de emergências).
