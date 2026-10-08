# Gameplay

> **Status:** v1.0 · **Data:** 2026-10-08 · **MVP:** seções 1–4

---

## 1. Controles (padrão, todos remapeáveis)
| Ação | Teclado/Mouse |
|---|---|
| Pan | WASD / setas / arrastar com o botão direito |
| Zoom | roda do mouse (ao cursor) |
| Rotacionar / inclinar | Q/E, botão do meio + arrastar |
| Selecionar | clique esquerdo |
| Focar | duplo clique |
| Ferramentas | R (rua), H (casa), M (mercado), P (usina), X (demolir) |
| Overlays | 1–9 |
| Velocidade | Espaço (pausa/retomar), `+`/`-` (1x ↔ 2x ↔ 3x ↔ 5x ↔ 10x) |
| Marcadores de câmera | Ctrl+F5..F8 salva, F5..F8 retorna |
| Ajuda | F1 (com o editor em foco: **dica** contextual) |
| Painel de código | ` (crase) |
| Executar código | Ctrl+Enter |
| Desfazer construção | Ctrl+Z (até 5 s) |

## 2. Primeira sessão (roteiro do MVP)
| Min | O que acontece | Sistema/conceito |
|---|---|---|
| 0–3 | Câmera sobrevoa o terreno, a prefeitura e a rua inicial. "Sua cidade precisa crescer." | câmera, mundo |
| 3–6 | Tutorial de construção: puxar uma rua de 150 m e colocar 2 casas pela UI. Lotes aparecem. | ruas, lotes, custo |
| 6–9 | Primeiros moradores chegam a pé. Clique num cidadão: o Inspector mostra nome, casa e "procurando emprego". | cidadãos, Inspector |
| 9–12 | Construir o mercado. Moradores são contratados, ganham salário e fazem compras. O painel de dinheiro mostra o fluxo. | economia |
| 12–15 | A engenheira-chefe: "Prefeito(a), dá para fazer isso **mais rápido**." Abre o painel de código. **Lição 1**: `city.Log("Bom dia, cidade!");` | M0 |
| 15–22 | **Lição 2**: ler o tesouro numa variável e calcular quantas casas cabem no orçamento. | M1 |
| 22–30 | **Lição 3**: `var house = new House(); city.Build(house);`. **A casa aparece.** Momento-chave. | M2: objeto |
| 30–40 | A usina fica no limite à noite. Overlay de Energia em vermelho, moradores reclamam. **Lição 4**: isolamento reduz o consumo; tentar `EnergyConsumption = 0` gera `CS0200` → encapsulamento. | M2: propriedade, energia |
| 40–50 | **Lição 5**: escolher o lote (sobrecarga de `Build`), propriedade × método. | M2: método |
| 50–60 | **Lição 6 (Laboratório)**: implementar `CalculateEnergyConsumption()`. Passou → instalar na cidade. | M2: retorno |
| 60–75 | **Lição 7**: classe `EnergyAuditor` lista as casas sem energia e sugere reformas. | M2: classe |
| 75–90 | **★ Crise de energia**: um apagão em cadeia. O jogador combina orçamento, construção e código para equilibrar a rede. | integração |

## 3. Interações principais
- **Construir:** escolher a ferramenta → fantasma com custo e validade (verde/vermelho) → clique para confirmar → aparece no tick seguinte.
- **Inspecionar:** clique → Inspector com abas (`DEBUG_VISUALIZATION.md`).
- **Investigar um problema:** notificação → clique → câmera foca + overlay relevante + causas → ações sugeridas (UI) e, quando houver, missão de código.
- **Programar:** painel de código → editar → Executar (Script) ou Validar (Laboratório) → console + efeitos na cidade.

## 4. Feedback
| Evento | Feedback |
|---|---|
| Construção | animação curta de "pop", som sutil, débito no tesouro com animação |
| Código executado | console "Build succeeded", efeito destacado na cidade (*highlight* nos objetos alterados) |
| Erro | marcador no editor, mensagem em 4 partes, objeto afetado destacado (se houver) |
| Desafio aprovado | explicação, comparação, desbloqueio com animação e som de recompensa |
| Problema detectado | toast com ícone e cor (sempre com texto), termômetro reage |

## 5. Pós-MVP: sandbox (Fase 10+)
Zoneamento e crescimento privado, marcos de cidade, problemas emergentes com missões compatíveis com o domínio do jogador, automação com políticas (`city.Policies`), relatórios e overlays próprios.

## 6. Pós-MVP: exemplo de missão completa (§74)
Ver `GARBAGE_SYSTEM.md` §5 e `LEARNING_PATH.md` §4: do `GarbageCollector.Collect` até Strategy + teste de regressão, ao longo das Fases 12–17.
