# Iluminação

> **Status:** v1.0 · **Data:** 2026-10-08 · **MVP:** sim

---

## 1. Restrição fundamental: **a cidade é dinâmica**
O jogador constrói e demole a qualquer momento. Por isso **GI baked (lightmaps, APV baked) não é viável** para a cidade. Toda iluminação de mundo é **em tempo real**. (APV baked pode servir só a cenas fixas, como o menu principal, se valer a pena.)

## 2. Componentes
| Componente | Uso |
|---|---|
| **Luz direcional (sol/lua)** | ângulo pela hora TC; cor por curva (temperatura); intensidade; sombras em cascata |
| **Ambiente** | céu procedural → harmônicos esféricos (SH) atualizados **quando a fase do dia muda** (ou a cada X min TC), nunca por frame |
| **SSAO** | sombras de contato e coesão visual |
| **Reflection probe** | 1 probe global do céu, atualizada por fase do dia (*time-sliced*) |
| **Emissivos** | janelas (variação por instância: acesas/apagadas por "ocupação" e hora), letreiros, faróis |
| **Luzes pontuais reais** | postes e faróis **perto da câmera** (Forward+), com LOD de luzes: além de X m só o emissivo |
| **Exposição** | curva por hora + ajuste automático leve (Volume) |
| **Pós** | bloom suave à noite, color grading por fase (LUT), vinheta sutil |

## 3. Curvas por fase do dia
| Fase | Sol | Cor | Ambiente | Emissivos |
|---|---|---|---|---|
| Manhã | baixo a leste | quente (3.500 K) → neutra | azulado | apagando |
| Dia | alto | neutra (5.500–6.500 K) | céu claro | off |
| Entardecer | baixo a oeste | alaranjada (3.000 K) | quente | acendendo (18:30) |
| Noite | lua (fraca, fria) | 8.000 K, baixa intensidade | azul-escuro | on (janelas por ocupação) |

## 4. Sombras
- Distância de sombra **acoplada ao zoom** (perto: alta resolução e distância curta; longe: distância maior e resolução menor).
- **Filtro de shadow casters** no asset (flag no linter): props < 2 m não projetam sombra.
- Sombra de nuvens (*cookie* animado) quando nublado.

## 5. Clima
Chuva/tempestade: sol atenuado, ambiente mais escuro, superfícies molhadas (*wetness* global), fog ↑. Ver `WEATHER_SYSTEM.md`.

## 6. Desempenho
Atualizações caras (SH, reflection probe) são *time-sliced* e acontecem em eventos de fase, não por frame. A meta é que a iluminação custe ≤ 3 ms de GPU no preset Alto (sem contar sombras).
