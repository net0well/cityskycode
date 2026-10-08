# Ciclo Dia/Noite

> **Status:** v1.0 · **Data:** 2026-10-08 · **MVP:** sim (fases do dia + iluminação + agenda)

---

## 1. Fases (§60)
| Fase | Horário (TC) | Simulação | Visual |
|---|---|---|---|
| **Morning** (manhã) | 06:00–12:00 | deslocamento para o trabalho (pico), comércio abre às 08:00 | sol nascendo, luz quente → neutra |
| **Day** (dia) | 12:00–17:00 | trabalho, compras | sol alto, luz neutra |
| **Evening** (tarde/entardecer) | 17:00–20:00 | volta para casa (pico), compras | luz alaranjada, postes acendem às 18:30 |
| **Night** (noite) | 20:00–06:00 | sono (22:00–06:00), comércio fechado (MVP) | céu escuro, janelas e postes emissivos |

## 2. Efeitos (§60)
| Aspecto | Efeito |
|---|---|
| Iluminação | rotação do sol, temperatura de cor, intensidade, exposição (Volume) |
| Tráfego | picos manhã/tarde (agenda dos cidadãos) |
| Comércio | horário de funcionamento (MVP: 08:00–20:00) |
| Trabalho | turnos (MVP: diurno; Fase 13: noturno em serviços essenciais) |
| Cidadãos | agenda (dormir, trabalhar, comprar) |
| Energia | consumo residencial maior à noite, comércio de dia; solar só de dia |
| Criminalidade (Fase 13) | risco noturno maior; postes reduzem |

## 3. Implementação
- **Domain:** `CalendarSystem` publica a fase e a hora (TC). Curvas de consumo por hora em tabela inteira.
- **Presentation:** `DayNightController` lê o relógio do read model e interpola: ângulo do sol, gradiente do céu (procedural), ambiente (SH atualizado ao trocar de fase, não por frame), exposição e emissivos (parâmetro global de "noite" nos shaders das janelas, com variação por instância para janelas acesas e apagadas).
- **Desempenho:** iluminação 100% dinâmica (cidade construída pelo jogador → **sem GI baked**). Ver `LIGHTING.md`.

## 4. Testes
Transições de fase nos horários exatos, curvas de consumo por hora, agenda × fase.
