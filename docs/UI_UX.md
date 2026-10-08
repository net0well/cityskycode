# UI e UX

> **Status:** v1.0 · **Data:** 2026-10-08 · **MVP:** sim (HUD, ferramentas, Inspector, painel de código, lições)
> "A UI deve ter aparência de software profissional" (§61).

---

## 1. Tecnologia
- **UI Toolkit** (runtime): UXML + USS com **variáveis de tema** (tokens de cor, espaçamento, tipografia), data binding de read models.
- **Gráficos** (séries, barras, pizza, Sankey simples): VisualElements customizados com `Painter2D`.
- **Editor de código:** Monaco em webview, exibido num painel dedicado (`CODE_EDITOR.md`).
- **Localização:** Unity Localization (tabelas pt-BR, depois en). **Nenhuma string fixa.**

## 2. Layout
```
┌───────────────────────────────────────────────────────────────────────┐
│ 👥 2.340  💰 $124.000 (+$1.250/dia)  😊 78%  🌡️ 🟢 Saudável  📅 D12 08:30 ⏸ ▶ ▶▶ ▶▶▶ │  ← barra superior
├──────┬──────────────────────────────────────────────────┬─────────────┤
│ 🛣️    │                                                  │ INSPECTOR   │
│ 🏠    │                                                  │ Casa #1029  │
│ 🏪    │                  CIDADE (3D)                     │ ...         │
│ ⚡    │                                                  │             │
│ 🧨    │                                                  │             │
│ ──   │                                                  │             │
│ 🗺️    │  [overlays 1–9]                                  │             │
├──────┴──────────────────────────────────────────────────┴─────────────┤
│ { } CÓDIGO  │ Program.cs | EnergyAuditor.cs        ▶ ■ 🧪 💡 │ CONSOLE  │  ← painel acoplável
│             │ (Monaco)                                    │ Build ok │
└───────────────────────────────────────────────────────────────────────┘
```
- Modos de layout: **Cidade** (painel de código minimizado), **Lado a lado**, **Código em foco** (cidade em miniatura).
- Painéis: Orçamento, População, Energia, Problemas, Lições/Árvore de conhecimento, Termômetro, Configurações.

## 3. Componentes (§61)
| Componente | Descrição | MVP |
|---|---|---|
| Painéis e menus | barra superior, ferramentas, configurações | ✅ |
| Gráficos | orçamento (7 dias), população, energia por hora | ✅ (básicos) |
| Inspector | `DEBUG_VISUALIZATION.md` §2 | ✅ |
| Overlays | barra + legenda | ✅ |
| Árvore de conhecimento | grafo de conceitos com estado (bloqueado/disponível/dominado) | ✅ (trilha linear no MVP) |
| Editor | Monaco | ✅ |
| Console | estilo IDE, clicável (§62) | ✅ |
| Estatísticas | relatórios detalhados | Fase 10 |
| Notificações | toasts com link para a causa ("Apagão no bairro Norte → ver") | ✅ |

## 4. Princípios de UX
1. **Toda métrica é clicável** e leva à explicação (causa → overlay → objeto).
2. **Feedback em < 100 ms** para ações de UI. Comandos mostram pré-visualização (fantasma + custo) antes de confirmar.
3. **Pausa ao programar** (configurável) e ao abrir lições.
4. **Consistência de vocabulário** com o glossário e com a Player API (o que a UI chama de "Isolamento" é `Insulation` no código, e o hover mostra os dois).
5. **Desfazer** (Ctrl+Z) para ações de construção recentes (Command pattern), com reembolso integral em até 5 s.
6. **Atalhos** de teclado para tudo (lista em F1).

## 5. Acessibilidade
- Escala de UI (80–150%) e de fonte do editor.
- Paletas para daltonismo (protanopia, deuteranopia, tritanopia) em overlays e no termômetro (ícone + cor, nunca só cor).
- Alto contraste.
- Rebinding completo (Input System).
- Redução de movimento (sem tilt-shift, sem balanço de câmera).
- Legendas para todo áudio relevante.
- Leitor de tela: o Monaco tem suporte. Para o UI Toolkit, avaliar na Fase 20.
- Sem limites de tempo em desafios.

## 6. Estilo visual da UI
Profissional e "IDE-like" nos painéis de engenharia (tema escuro, monoespaçada para código, cores semânticas: erro/aviso/sucesso), mais amigável nos painéis de cidade (ícones, ilustrações). Tokens compartilhados garantem coerência.

## 7. Testes
EditMode: binding de read models → elementos; snapshots de layout em resoluções 1366×768, 1920×1080 e 2560×1440; teste de "nenhuma string sem chave de localização".
