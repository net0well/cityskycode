# Segurança (geral)

> **Status:** v1.0 · **Data:** 2026-10-08
> Execução de código do jogador: ver `CODE_EXECUTION_SECURITY.md` (documento principal de segurança).

---

## 1. Superfícies
| Superfície | Risco | Controles |
|---|---|---|
| **Código do jogador** | execução arbitrária | `CODE_EXECUTION_SECURITY.md` (L1–L7) |
| **IPC Unity ↔ Code Host** | outro processo local se conectar ou injetar mensagens | named pipe com nome aleatório + ACL do usuário atual; ou TCP loopback com token de 256 bits no handshake; validação de esquema; limites de tamanho |
| **Webview (Chromium)** | conteúdo remoto, XSS, navegação | conteúdo **somente local**; CSP `default-src 'self'`; navegação externa bloqueada; sem *popups*; bridge com mensagens tipadas e validadas; webview atualizada (patches de segurança do Chromium são responsabilidade do fornecedor, que é um critério de escolha no spike S2) |
| **Saves de terceiros** | código embutido, desserialização | nunca executar automaticamente; `TypeNameHandling.None`; validação pós-carga |
| **Conteúdo/mods (futuro)** | código e assets maliciosos | assinatura de pacotes oficiais; conteúdo comunitário sempre no sandbox completo; revisão |
| **Cadeia de suprimentos** | pacotes NuGet, npm ou Unity comprometidos | versões fixadas (CPM, `package-lock.json`, `manifest.json`), `dotnet list package --vulnerable`, `/outdated` e `/security-scan` (dotnet-claude-kit), `npm audit`, revisão de novas dependências (ficha do `TECH_STACK.md`) |
| **Binários distribuídos** | adulteração; bloqueio por SmartScreen/Smart App Control em escolas | pacote **MSIX publicado na Microsoft Store**, assinado pela Microsoft sem custo (ADR-0017, spike S7); certificado comercial descartado por custo |
| **Pacotes compartilhados `.citysky`** | código malicioso, zip bomb, zip slip | `SHARING_SYSTEM.md` §4 |
| **Arquivos de progresso** | edição para "colar" | sem servidor não há proteção real; o progresso é evidência, não prova (`CLASSROOM.md` §3) |
| **Atualizações** | MITM | via plataforma de distribuição (Steam/itch), sem atualizador próprio |

## 2. Privacidade e LGPD
- O público inclui **adolescentes e possivelmente crianças**, em sala de aula (confirmado, ADR-0017). A LGPD (Lei 13.709/2018, art. 14) exige tratamento no **melhor interesse** e, para crianças, consentimento específico de um responsável.
- **MVP:** nenhuma coleta remota. Progresso, código e telemetria ficam **somente no disco local**.
- **Futuro (opt-in):** telemetria anônima e agregada, com consentimento explícito (e do responsável, quando aplicável), política de privacidade em linguagem simples, opção de exportar e apagar dados.
- Nomes de cidadãos são gerados de tabelas, sem dados reais. O jogador não precisa de conta.
- Um tutor com IA (futuro) exigiria uma avaliação de privacidade específica (envio de código para um serviço externo).

## 3. Segredos
O jogo não tem segredos de servidor. Ferramentas de CI usam *secrets* do GitHub (licença Unity). **Nunca** commitar chaves; *pre-commit* com detecção de segredos (hook do dotnet-claude-kit e `/security-scan`).

## 4. Processo
- Revisão do `security-auditor` em PRs que tocam as superfícies acima.
- Registro de incidentes locais (Worker enviando mensagens inválidas, violações L2 sem L1) em log para análise.
- Antes de qualquer recurso de compartilhamento: revisão completa de segurança (checklist em `CODE_EXECUTION_SECURITY.md` §7), *fuzzing* do protocolo e teste de contenção em Windows 10 e 11.
