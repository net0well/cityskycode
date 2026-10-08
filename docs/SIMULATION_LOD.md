# LOD de Simulação

> **Status:** v1.0 · **Data:** 2026-10-08 · **ADR:** [0009](ADR/0009-citizen-simulation.md) · **MVP:** parcial (S0/S1 e R0)

---

## 1. A correção principal: **a câmera não muda a simulação**

O briefing (§54–55) propõe "cidadão próximo → simulação detalhada; distante → simplificada; muito distante → estatística". **Não adotamos a distância da câmera como critério de simulação.**

| Problema | Consequência se a câmera definisse a simulação |
|---|---|
| Efeito observador | O engarrafamento some quando o jogador afasta a câmera |
| Determinismo | Duas execuções com os mesmos comandos divergem conforme a câmera |
| Replay | Rever a partida com outra câmera produz outra história |
| Validação de desafios | Os Laboratórios rodam *headless* (sem câmera). O resultado validado seria diferente do da cidade real |
| Justiça pedagógica | "Meu código funcionou quando eu estava olhando" |

**Princípio:** separar **o que acontece** (LOD de **Simulação**, por estado/atividade, independente da câmera) de **como é mostrado** (LOD de **Apresentação**, pela câmera).

Isso preserva a intuição do briefing (pedestre visível ≠ cidadão totalmente simulado o tempo todo) sem o efeito observador.

---

## 2. LOD de simulação (camadas S)

| Nível | Quem | O que é simulado | Custo |
|---|---|---|---|
| **S0: Em viagem** | cidadãos com `Trip` ativa | caminho (no cache), horário de partida, chegada prevista, progresso no modelo meso | O(1) por tick por viagem (progresso) + custo por link (meso) |
| **S1: Em atividade** | cidadãos em casa, no trabalho ou em compras | máquina de estados da agenda, necessidades e felicidade **em baldes** (1x/h TC) | ~N/480 por tick |
| **S2: Coorte** (Fase 18, se necessário) | populações agregadas: visitantes, *commuters* externos, excedente acima do limite de individualização | contagens por domicílio-modelo/bairro, fluxos estatísticos | O(bairros) |

Notas:
- **Todo** cidadão individual está em S0 ou S1, independentemente da câmera.
- S2 só existe se a memória ou a CPU exigirem (escala acima de 100k). As regras de S2 são calibradas para produzir os **mesmos agregados** que S0/S1 (validado por teste estatístico *headless*).
- **Limite de viagens concorrentes** (ex.: 20.000 na Fase 18): viagens excedentes são "virtuais". Existem no modelo meso como fluxo, sem `Trip` individual, e são contabilizadas da mesma forma. É o equivalente ao limite de *CitizenInstances* de *Cities: Skylines* (65k instâncias ativas para até 1M de cidadãos).

## 3. LOD de apresentação (camadas R, por câmera)

| Nível | Distância típica | Representação |
|---|---|---|
| **R0** | < 80 m | modelo completo, animação esqueletal (MVP: GameObject; Fase 11+: ECS) |
| **R1** | 80–300 m | low-poly, animação em textura (VAT) ou sem animação, sem sombra |
| **R2** | 300–1.000 m | impostor/billboard ou ponto instanciado |
| **R3** | > 1.000 m | não renderizado; densidade aparece em overlays e como "vida" agregada (pontos de luz noturnos) |

- **Orçamento de agentes visíveis:** a apresentação escolhe, entre as viagens S0 em links visíveis, até `MaxVisibleAgents` (MVP: 50; Fase 11: 2.000; Fase 18: ~10.000), priorizando distância e seleção do jogador.
- **Agentes cosméticos:** perto da câmera, a apresentação pode adicionar pessoas paradas em praças e ciclistas decorativos. Eles são marcados como **cosméticos** e nunca aparecem em estatísticas nem no Inspector.

## 4. "Seguir cidadão" sem quebrar o modelo
Quando o jogador segue um cidadão, a apresentação cria um agente **dedicado** ligado à `Trip` do domínio. A posição é `Path.Evaluate(progress(t + α))`, calculada com o progresso **autoritativo**. O agente chega exatamente quando o domínio diz que chegou. Se o cidadão está em S1 (dentro de um prédio), a câmera foca o prédio e o Inspector mostra a atividade.

## 5. Por que isso funciona com o tráfego
O tráfego autoritativo é **mesoscópico** (densidade e velocidade por link, ver `TRAFFIC_SYSTEM.md`). A microssimulação de veículos (frear, acelerar, mudar de faixa, parar no vermelho) é **visual** e alimentada pelo estado de cada link: se o link tem fila de 120 m, a apresentação mostra uma fila de ~120 m de carros. Ver o congestionamento continua sendo **verdade**, mas o que se vê não altera o resultado.

## 6. Contratos
```csharp
// Application → Presentation (por frame)
public readonly struct AgentSnapshotItem
{
    public readonly TripId Trip;
    public readonly CitizenId Citizen;
    public readonly TravelMode Mode;
    public readonly PathRef Path;
    public readonly long DepartTick;
    public readonly long ExpectedArriveTick;
    public readonly int ProgressFixed;      // Q16.16 ao longo do caminho
}

public interface IAgentPresentation
{
    void Sync(ReadOnlySpan<AgentSnapshotItem> activeTrips, float alpha, in CameraInfo camera);
    void Follow(CitizenId citizen);
}
```
(`ReadOnlySpan` está disponível em netstandard2.1.)

## 7. Testes
- **Teste de independência da câmera:** rodar a mesma cidade *headless* e no PlayMode com 3 trajetórias de câmera diferentes → hashes de estado iguais.
- **Teste estatístico S2 × S0/S1** (Fase 18): agregados (emprego, consumo, viagens/h) dentro de ±3%.
- **Orçamento visual:** com 10.000 viagens ativas, a apresentação mantém ≤ `MaxVisibleAgents` e o FPS-alvo.

## 8. Riscos
- Jogadores podem notar que veículos visuais "não são" cidadãos específicos. Mitigação: seguir cidadão e o Inspector sempre mostram dados reais; agentes cosméticos são poucos e próximos da câmera.
- A calibragem de S2 é trabalhosa. Mitigação: só ativar se o perfilamento exigir.
