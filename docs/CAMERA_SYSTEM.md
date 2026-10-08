# Sistema de Câmera

> **Status:** v1.0 · **Data:** 2026-10-08 · **MVP:** sim

---

## 1. Requisitos (§35)
Pan, zoom, rotação, inclinação, foco, seleção, seguir cidadão, seguir veículo, visão geral e visão de bairro.

## 2. Modelo (câmera de estratégia "orbital")
```
alvo (ponto no chão) ── yaw (rotação) ── pitch (inclinação) ── distância (zoom)
posição = alvo + esfera(yaw, pitch) × distância
```
| Parâmetro | Faixa (MVP) |
|---|---|
| Distância | 15 m – 1.500 m (curva exponencial no scroll) |
| Inclinação | 20° – 85° (acopla ao zoom: perto mais rasante, longe mais "planta") |
| Rotação | livre (Q/E, botão do meio) |
| Pan | WASD/setas, arrastar com o botão direito, borda da tela (opcional, desligada por padrão) |

- **Suavização:** mola criticamente amortecida (SmoothDamp) por parâmetro.
- **Zoom ao cursor:** o ponto sob o cursor permanece fixo.
- **Limites:** o alvo fica restrito à área do mapa, e a câmera não atravessa o terreno (altura mínima sobre o heightmap).

## 3. Modos
| Modo | Comportamento | MVP |
|---|---|---|
| Livre | padrão | ✅ |
| Foco | duplo clique → anima até o objeto | ✅ |
| Seguir cidadão/veículo | alvo = posição interpolada do agente dedicado (`SIMULATION_LOD.md` §4); ESC sai | ✅ cidadão |
| Visão geral | atalho para o enquadramento da cidade inteira | ✅ |
| Visão de bairro | enquadra o polígono do bairro | Fase 10 |
| Marcadores | Ctrl+F5..F8 salva, F5..F8 retorna (1–9 são dos overlays) | Fase 10 |
| Cinemático / foto | trilhos, DOF/tilt-shift | Fase 20 (avaliar Cinemachine com ficha de dependência) |

## 4. Implementação
- `StrategyCameraRig` (MonoBehaviour fino) + `CameraController` (C# testável, sem MonoBehaviour) que recebe ações do **Input System** e produz o estado alvo.
- Rebinding de teclas (acessibilidade), sensibilidade ajustável, inversão de eixos.
- A câmera informa a apresentação (LOD de agentes, culling) por meio de `CameraInfo`. **Nunca** informa o domínio (ver `SIMULATION_LOD.md`).

## 5. Pós-processamento ligado à câmera
- *Tilt-shift* sutil (DOF) em zoom médio, para o efeito "maquete" (opcional, desligável).
- Distância de sombra e de LOD adaptadas ao zoom (`GRAPHICS.md`).

## 6. Testes
`CameraController` com testes EditMode (limites, zoom ao cursor, transição de foco); PlayMode para o seguir.
