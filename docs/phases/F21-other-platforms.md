# Fase 21+ — Outras plataformas

> **Status:** Intenção de design (**reavaliar após o lançamento no Windows**, ADR-0018) · **Atualizado:** 2026-10-08
> **Entrada:** lançamento Windows concluído + decisão do dono do produto · **Saída:** a definir na reavaliação

---

## 1. Objetivo
Levar o jogo a "todos os computadores" que conseguem rodá-lo, começando pelo que mais importa para escolas: **Linux** (laboratórios escolares, Steam Deck), depois **macOS**. Chromebook só com uma versão web, que seria praticamente outro produto.

## 2. Por que nesta posição
Decisão do dono do produto: primeiro fazer funcionar no Windows. A arquitetura foi mantida portável desde a F1 (interfaces de plataforma, CA1416, canário Linux no CI), então portar significa **implementar interfaces**, não reescrever.

## 3. Escopo (por plataforma)
| Plataforma | Trabalho principal |
|---|---|
| **Linux x64 / Steam Deck** | **Spike S12** (sandbox Linux): `no_new_privs` + seccomp-bpf + Landlock + rlimits, sem depender de *user namespaces*; `IIpcTransport` com Unix domain socket; editor dentro do jogo (janelas separadas são ruins no Gaming Mode); texto ≥ 12 px a 1280×800; Steam Input; alvo "Playable" no Deck; build nativo (sem Proton) |
| **macOS** | Seatbelt no Worker, hardened runtime com entitlement de JIT do .NET, notarização (Apple Developer ~US$ 99/ano, decisão de custo do dono do produto); PhotinoX sobre WKWebView |
| **Chromebook / web** | pesquisa: Unity Web + Roslyn em WebAssembly + execução em Web Worker; limites de memória e threads, sem Burst |
| **Windows ARM64** | barato de incluir já no ciclo Windows se o custo de teste for baixo (ADR-0018) |

## 4. Gate de saída
A definir na reavaliação. Mínimo: corpus de segurança verde no sandbox da plataforma e as metas de desempenho do hardware mínimo equivalente.

## 5. Riscos
R26 (dívida de portabilidade), R28 (Steam Deck), R27 (custos de plataforma), R21 (escolas com Linux/Chromebook).

## 6. Documentos e ADRs
ADR-0018, `CODE_EXECUTION_SECURITY.md` §3 L5 (Linux/macOS), `CODE_EDITOR.md`, `TECH_STACK.md`, `FUTURE_FEATURES.md`.
