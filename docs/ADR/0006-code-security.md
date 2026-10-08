# ADR-0006: Proteger a execução de código com defesa em camadas e sandbox do sistema operacional

- **Status:** Aceito
- **Data:** 2026-10-08
- **Documentos afetados:** `CODE_EXECUTION_SECURITY.md`, `SECURITY.md`

## Contexto
O §47 exige bloquear filesystem, processos, rede, reflexão perigosa, carregamento arbitrário de assemblies, APIs não autorizadas, acesso ao SO e execução externa, e diz para "não assumir que `AssemblyLoadContext` sozinho fornece sandbox". O .NET moderno não tem CAS.

## Decisão
Sete camadas:
1. **L1** allowlist por símbolo (Roslyn) + proibições sintáticas.
2. **L2** verificação de IL do assembly emitido (System.Reflection.Metadata).
3. **L3** orçamentos instrumentados (laços, recursão, memória, saída, comandos), não capturáveis pelo aluno.
4. **L4** processo separado, sem handles herdados, com watchdog.
5. **L5** sandbox do SO: **Job Object** (MVP) + **AppContainer sem capabilities** ou token restrito + *Low IL* (Fase 9).
6. **L6** host trata o Worker como hostil; Application valida comandos com as regras da UI.
7. **L7** nunca executar automaticamente código de terceiros; pacotes oficiais assinados.

**Sem L5 completo, não existe compartilhamento de código.** Corpus de ataques no CI.

## Alternativas consideradas
| Alternativa | Por que não |
|---|---|
| Só allowlist (estilo Space Engineers) | um bug na lista = acesso total; não contém DoS nativo |
| Só processo isolado | feedback pobre ("o processo morreu"), sem pedagogia |
| Contêiner/VM (Windows Sandbox, Hyper-V) | pesado demais para um jogo de consumo; requisitos de SO |

## Consequências
**Positivas:** segurança proporcional ao risco, com mensagens pedagógicas.
**Negativas:** P/Invoke complexo (AppContainer), matriz de testes por versão do Windows.
**Riscos:** L1/L2 incompletas (contidas por L5).

## Gatilhos de revisão
Introdução de compartilhamento/mods · suporte a macOS/Linux · descoberta de bypass.
