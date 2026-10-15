# Revisão da Player API v0 (F1-81)

> Data: 2026-10-15 · Escopo: `CitySky.PlayerApi` v0 (superfície pública que o código do jogador enxerga).
> Método: inspeção da superfície pública (equivalente a `get_public_api`) + revisão de segurança manual
> (o `security-auditor` automático roda no PR quando o ambiente estiver disponível).

## Superfície pública
| Tipo | Membros | Observação |
|---|---|---|
| `CityGame` (static) | `Current : City` | Ponto de entrada. `Current` devolve um singleton imutável (`static readonly`). |
| `City` (sealed) | `Log(string)`, `Log(object)` | Construtor **internal** — o jogador não cria `City`, só obtém via `CityGame.Current`. |
| `ICityGateway` | `Log(string)` | **Público de propósito**: o jogador pode escrever um fake para testar o próprio código (Gateway/Adapter). |
| ~~`LiveCityGateway`~~ | `Log(string)` | Agora **internal** (antes público) — ver achado 1. |

## Achados
1. **(Baixo, corrigido)** `LiveCityGateway` era `public`: o jogador podia instanciá-lo. Era inócuo (só
   `Console.WriteLine`), mas é superfície desnecessária. **Tornado `internal`** nesta revisão; o acesso
   continua por `CityGame.Current`, e `ICityGateway` segue público para fakes de teste.
2. **(Informativo)** `CityGame.Current` é um singleton (`static readonly City`). Aceitável: cada execução
   roda em um sandbox isolado (um processo/ALC por run), então não há estado compartilhado entre jogadores.
   Não é `static` mutável (regra do CLAUDE.md respeitada).
3. **(Informativo)** `Log(object)` chama `value?.ToString()`. Um `ToString` do jogador que entre em laço
   ou lance exceção roda **dentro** do sandbox (orçamentos de tempo/memória do Worker), então não ameaça
   o Host. Null-safe.

## Segurança
- A superfície **não muta estado do jogo** (v0 só tem `Log`); não há comando exposto ainda.
- O isolamento real (o que da BCL o jogador pode chamar) é responsabilidade da **allowlist** (L1/L2) e do
  corpus `CitySky.Security.Tests` (F1-75), não da Player API.
- **Regra para evoluir:** cada novo membro que mude a cidade entra como **comando validado**, com caso no
  corpus de segurança e revisão do `security-auditor` (CLAUDE.md). O jogador nunca deve ter poder que a UI
  não tenha.

## Conclusão
Superfície v0 mínima e segura; um ajuste aplicado (achado 1). Pronta para o marco F1-112.
