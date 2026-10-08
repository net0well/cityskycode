# Áudio

> **Status:** v1.0 (intenção de design) · **Data:** 2026-10-08 · **MVP:** básico (UI + ambiente simples)

---

## 1. Objetivos
Reforçar "cidade viva", dar **feedback** às ações e ao código, e não cansar em sessões longas de programação.

## 2. Camadas
| Camada | Conteúdo | Dirigido por | Fase |
|---|---|---|---|
| **Ambiente urbano** | loops por densidade e zona (vozes distantes, trânsito, pássaros, obras), mixados conforme a posição e o zoom da câmera | read models (densidade na área visível) | MVP (básico) |
| **Eventos** | apagão (zumbido caindo), construção concluída, sirene | eventos de domínio | MVP (poucos) |
| **UI** | cliques, confirmação, erro **sutil** | UI | MVP |
| **Código** | "build succeeded" discreto, desafio aprovado (recompensa), erro de compilação **não punitivo** | Code Host → console | MVP |
| **Música** | adaptativa ao termômetro (calma no saudável, tensa no crítico), volume reduzido no modo código | `CityHealth` | Fase 19 |
| **Clima/dia-noite** | chuva, vento, grilos à noite | clima/calendário | Fase 19 |

## 3. Tecnologia
Áudio nativo da Unity (Audio Mixer, snapshots por modo). **FMOD/Wwise adiados** (§65: o problema ainda não exige). Reavaliar se o áudio adaptativo crescer.

## 4. Acessibilidade
Volumes separados (ambiente, música, UI, notificações). Todo som com informação tem um **equivalente visual** (toast, ícone). Opção "sem sons de erro".

## 5. Assets
Licenças registradas como na arte. Formatos OGG (loops) e WAV (UI curtos), via Git LFS.
