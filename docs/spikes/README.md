# Spikes

> **Status:** planos prontos, nenhum spike executado · **Data:** 2026-10-08 · Plataforma: **Windows** (ADR-0018)
> Spike = experimento curto, com prazo fixo, para responder **uma** pergunta de risco. O código de spike é **descartável**: fica em `spikes/<id>/` e nunca é copiado para `unity/` ou `dotnet/src/` sem ser reescrito.
> Plano da fase: [`docs/phases/F00-research-and-spikes.md`](../phases/F00-research-and-spikes.md).

---

## 1. Spikes da Fase 0 (ordem recomendada)
Primeiro o que não depende da Unity (dá para começar já), depois o que depende.

| Ordem | Spike | Plano | Depende de | Prazo | Bloqueia | Status |
|---|---|---|---|---|---|---|
| 1 | **S6** Sandbox Windows (Job Object + AppContainer/LPAC/token restrito, Worker .NET 10) | [S6](S6-sandbox-windows.md) | só .NET 10 | 3 d | S1a, S7, F1 | Planejado |
| 2 | **S1a** Code Host: Roslyn compila → Worker executa com orçamentos | [S1a](S1a-codehost-pipeline.md) | S6 | 3 d | S1b, F1 | Planejado |
| 3 | **S4a** Domínio headless: 100k cidadãos em SoA (.NET 10) | [S4a](S4a-headless-domain.md) | só .NET 10 | 2 d | S4b, F5 | Planejado |
| 4 | **S2b** Janela PhotinoX + Monaco + completions do Roslyn | [S2b](S2b-photinox-monaco.md) | .NET 10 + Node | 2 d | S2a, F1 | Planejado |
| 5 | **S1b** Player Unity IL2CPP ↔ Code Host (named pipe; fallback TCP) | [S1b](S1b-il2cpp-ipc.md) | Unity, S1a | 2 d | S7, F1 | Planejado |
| 6 | **S2a** UnityWebBrowser no painel do jogo + **decisão do host** | [S2a](S2a-unitywebbrowser.md) | Unity, S2b | 2 d | F1 | Planejado |
| 7 | **S3** Orçamento de renderização URP (5k prédios, 20k árvores, 1k agentes) | [S3](S3-urp-render-budget.md) | Unity + **PC com GPU integrada** | 3 d | gate F2 | Planejado |
| 8 | **S4b** Fator Mono/IL2CPP + hash entre runtimes | [S4b](S4b-mono-il2cpp-factor.md) | S4a, Unity | 1 d | F5 | Planejado |
| 9 | **S5** Malha de ruas e interseções em runtime | [S5](S5-road-mesh.md) | Unity | 3 d | F3 | Planejado |
| 10 | **S7** MSIX sem admin + Microsoft Store + política de código dinâmico | [S7](S7-msix-store.md) | S1b, S6, Partner Center | 2 d | F1 | Planejado |

Total estimado: **~23 dias de trabalho** de uma pessoa. Os spikes 1–4 rodam em paralelo com a instalação da Unity.

**Correspondência com o roadmap:** o `DEVELOPMENT_ROADMAP.md` agrupa por pergunta (S1 = S1a + S1b; S2 = S2a + S2b; S4 = S4a + S4b). **Gate da Fase 0:** S1, S2, S6 e S7 resolvidos (ou com alternativa aceita em ADR). S3, S4 e S5 não bloqueiam a Fase 1, mas precisam terminar antes da fase que dependem deles (tabela acima).

## 2. Pré-requisitos (checados em 2026-10-08 na máquina de desenvolvimento)
| Item | Situação |
|---|---|
| .NET SDK 10 | ✅ 10.0.400 |
| Git 2.55 + Git LFS 3.7 | ✅ |
| Node.js (bundle do Monaco) | ✅ v24 |
| WebView2 Runtime (PhotinoX no Windows) | ✅ 154 |
| Visual Studio 2026 Community | ✅ instalado. **Confirmar as cargas de trabalho** "Desenvolvimento de jogos com Unity" e "Desenvolvimento para desktop com C++" (o IL2CPP exige o compilador C++ da Microsoft) |
| CWM.RoslynNavigator (dotnet-claude-kit) | ✅ 0.8.0. Rodar `dotnet tool update -g CWM.RoslynNavigator` |
| **Unity Hub + Unity 6.6 com Windows Build Support (IL2CPP)** | ❌ **não instalado.** Necessário para S1b, S2a, S3, S4b e S5 |
| **Conta Unity (licença Personal, gratuita)** | ⏳ criar e ativar no Unity Hub |
| **Repositório Git** | ❌ a pasta ainda não é um repositório |
| **Pasta fora do OneDrive** | ⚠️ o projeto está em `OneDrive\Desktop`. Unity (`Library/` com dezenas de milhares de arquivos) e Git dentro de pasta sincronizada causam lentidão, arquivos bloqueados e risco de corromper o `.git`. **Mover para, por exemplo, `C:\dev\cityskycode`** |
| **PC com GPU integrada (Intel Iris Xe ou similar)** | ❌ a máquina de desenvolvimento (Radeon RX 7600, 32 GB) está muito acima do mínimo. O S3 precisa de um notebook ou PC de escola real |
| **VM Windows 10 22H2** (S6, S7) | ⏳ criar no Hyper-V (o Windows 11 Pro tem Hyper-V) |
| **Usuário Windows padrão (sem admin)** para testes (S6, S7) | ⏳ criar |
| Conta Microsoft Partner Center (individual, gratuita, com verificação de identidade) | ⏳ só para o S7 |
| Espaço em disco | ✅ 86 GB livres em C: (Unity + módulos ≈ 15 GB; projeto + Library ≈ 5–10 GB) |

## 3. Estrutura
```
spikes/
├── S6-sandbox-windows/      (código descartável)
├── S1a-codehost-pipeline/
├── ...
docs/spikes/
├── README.md                (este índice)
├── TEMPLATE.md              (modelo de plano + relatório)
└── S6-sandbox-windows.md    (plano agora; resultado preenchido ao terminar)
```

## 4. Regras
- Cada spike responde **uma** pergunta, com critério de sucesso definido **antes** de começar (tabela no plano).
- O prazo é fixo: estourou, para, registra o que aprendeu e decide (estender com justificativa, mudar de abordagem ou aceitar o risco).
- Todo spike termina preenchendo as seções de resultado do próprio plano e, se mudar uma decisão, com ADR novo ou atualizado.
- Ferramental: dotnet-claude-kit nos spikes .NET (`/plan` antes, `/security-scan` no S6 e no S1a). Os overrides do `CLAUDE.md` valem.

## 5. Spikes de fases futuras
Não têm plano detalhado ainda (para não envelhecer); o plano é escrito no início da fase.

| Spike | Fase | Pergunta |
|---|---|---|
| **S8** Tráfego meso × micro | 11 | Numa cidade de 10k, o modelo mesoscópico autoritativo é bom o bastante em "sensação", e quanto custam os dois em CPU e determinismo? (revisão do ADR-0010) |
| **S9** Ponte domínio → ECS | 11 | Quanto custa sincronizar 2.000 agentes visíveis por frame do domínio para entidades ECS? |
| **S10** Thread de simulação dedicada | 18 | O *double buffer* de read models mantém determinismo e cabe no orçamento de memória e latência? |
| **S11** Unity 7 / CoreCLR | pós-MVP | A Unity 7 estável permite C# moderno no código compartilhado sem quebrar o determinismo e os builds? (gatilho do ADR-0001) |
| **S12** Sandbox Linux | 21+ | Worker .NET 10 com `no_new_privs` + seccomp-bpf + Landlock, sem *user namespaces*, contém o corpus? |
