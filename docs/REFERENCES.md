# Referências (pesquisa da Fase 0)

> Fontes consultadas em **2026-10-08** para fundamentar as decisões. Verificar novamente antes de revisar ADRs, porque versões e roadmaps mudam.

## Unity: engine, versões e roadmap
- Unity — *Render Pipelines strategy for 2026* (URP recomendado; HDRP em manutenção; Built-in em depreciação a partir da 6.5): https://unity.com/topics/render-pipelines-strategy-for-2026
- Unity Discussions — *Render Pipelines strategy for 2026*: https://discussions.unity.com/t/render-pipelines-strategy-for-2026/1710004
- Unity Discussions — *CoreCLR, Scripting, and .NET Update, October 2026* (Unity 7 alpha com editor CoreCLR ainda em C# 9/.NET Standard 2.1; players CoreCLR experimentais na 6.7 LTS): https://discussions.unity.com/t/coreclr-scripting-and-net-update-october-2026/1738338
- Unity Discussions — *CoreCLR, Scripting, and Serialization Update, June 2026*: https://discussions.unity.com/t/coreclr-scripting-and-serialization-update-june-2026/1723299
- Unity Discussions — *Path to CoreCLR, 2026: Upgrade Guide*: https://discussions.unity.com/t/path-to-coreclr-2026-upgrade-guide/1714279
- Unity Manual — compilador C# (C# 9.0): https://docs.unity3d.com/6000.0/Documentation/Manual/csharp-compiler.html
- Unity Manual — Scripting backends / IL2CPP: https://docs.unity3d.com/Manual/scripting-backends.html
- Unity Manual — Limitações do IL2CPP (6.7): https://docs.unity.com/en-us/engine/6000.7/manual/programming-environment/scripting-backends/il2cpp/scripting-restrictions.md
- Unity — LTS vs Update releases: https://unity.com/releases/lts-vs-tech-stream
- Unity Discussions — *ECS Development Status, December 2025*: https://discussions.unity.com/t/ecs-development-status-december-2025/1699284
- Unity Discussions — *ECS Development Status for Unity 6.6*: https://discussions.unity.com/t/ecs-development-status-for-unity-6-6/1724480
- Entities package (6.5): https://docs.unity3d.com/Packages/com.unity.entities@6.5/manual/index.html
- Digital Production — *Unity's 2026 Roadmap: CoreCLR, Verified Packages, Fewer Surprises*: https://digitalproduction.com/2025/11/26/unitys-2026-roadmap-coreclr-verified-packages-fewer-surprises/
- Unity — preços e planos (Personal até US$ 200 mil): https://unity.com/pricing
- URP para criadores avançados (Unity 6): https://unity.com/resources/introduction-to-urp-advanced-creators-unity-6

## City builders: lições técnicas
- Paavo Huhtala — *Why Cities: Skylines 2 performs poorly* (renderer próprio, sem Entities Graphics, LOD e sombras): https://blog.paavo.me/cities-skylines-2-performance/
- TechRadar — Colossal Order confirma falta de LODs nos personagens: https://www.techradar.com/gaming/pc-gaming/no-cities-skylines-2s-performance-issues-arent-because-of-the-citizens-teeth-developer-confirms
- Paradox Forum — *Cities: Skylines Dev Diary 7: Simulation*: https://admin-forum.paradoxplaza.com/forum/developer-diary/cities-skylines-dev-diary-7-simulation.828492
- Steam Workshop — limites internos de CS1 (Citizens 1M / CitizenInstances 65k): https://steamcommunity.com/workshop/filedetails/discussion/531738447/481115363859488292

## Execução segura de código .NET
- Microsoft Learn — App com suporte a plugins (isolamento por SO/virtualização para código não confiável): https://learn.microsoft.com/dotnet/core/tutorials/creating-app-with-plugin-support
- Microsoft Learn — `AssemblyLoadContext`: https://learn.microsoft.com/dotnet/api/system.runtime.loader.assemblyloadcontext
- Microsoft (legado) — *How to: Run Partially Trusted Code in a Sandbox* (CAS não suportado no .NET Core+; não é fronteira de segurança): https://msdn.microsoft.com/en-us/library/bb763046
- MSDN Magazine (2013) — hospedagem de plugins em processo separado: https://msdn.microsoft.com/en-in/magazine/dn342875.aspx
- Microsoft Learn — *Implementing an AppContainer*: https://learn.microsoft.com/windows/win32/secauthz/implementing-an-appcontainer
- Microsoft Learn — `UpdateProcThreadAttribute` (SECURITY_CAPABILITIES, HANDLE_LIST): https://learn.microsoft.com/windows/win32/api/processthreadsapi/nf-processthreadsapi-updateprocthreadattribute
- Chromium — *Sandbox FAQ* (tokens restritos, Job Objects, níveis de integridade): https://chromium.googlesource.com/chromium/src.git/+show/main/docs/design/sandbox_faq.md
- Space Engineers Wiki — *The Anatomy of a Script* (allowlist + contador de instruções em scripts C# de jogadores): https://spaceengineers.wiki.gg/wiki/Scripting/The_Anatomy_of_a_Script

## .NET e Roslyn
- NuGet — Microsoft.CodeAnalysis.CSharp 5.0.0: https://www.nuget.org/packages/Microsoft.CodeAnalysis.CSharp/5.0.0
- .NET Blog — .NET 11 RC1 (GA prevista para nov/2026; STS): https://devblogs.microsoft.com/dotnet/dotnet-11-rc-1/
- RoslynPad (referência de editor sobre Roslyn): https://github.com/roslynpad/roslynpad

## Editor / webview
- Vuplex 3D WebView (avaliado e descartado por custo): https://developer.vuplex.com/
- UnityWebBrowser (CEF, open source, MIT): https://projects.voltstro.dev/UnityWebBrowser/latest/
- UnityWebBrowser, changelog (2.1.1, mar/2024, CEF 122): https://projects.voltstro.dev/UnityWebBrowser/latest/changelog/

## Produto educacional, custo zero e distribuição (ADR-0017)
- Windows Developer Blog: registro gratuito de desenvolvedor individual na Microsoft Store (set/2025): https://blogs.windows.com/windowsdeveloper/2025/09/10/free-developer-registration-for-individual-developers-on-microsoft-store/
- SignPath Foundation (assinatura gratuita só para projetos 100% open source): https://signpath.org/about
- Kenney (kits de assets CC0): https://kenney.itch.io/kenney-jam-resources
- Resolução CNE/CEB nº 1, de 4 de outubro de 2022 (Computação na Educação Básica, complemento à BNCC): https://www.lex.com.br/resolucao-cne-ceb-no-1-de-4-de-outubro-de-2022/
- Parecer CNE/CEB nº 2/2022: https://www.gov.br/mec/pt-br/cne/pdf/bncc-ensino-medio/parecer-cne-ceb-no-2-2022-aprovado-em-17-de-fevereiro-de-2022

## Plataformas e Steam (ADR-0018)
- Steamworks: Steam Direct fee (US$ 100 por app): https://partner.steamgames.com/doc/gettingstarted/appfee
- TechRaptor: notarização obrigatória para jogos macOS na Steam (2019): https://techraptor.net/content/apple-notarization-mac-gaming
- Android Authority: fim da Steam para Chromebook (jan/2026): https://www.androidauthority.com/steam-chromebook-3585553/
- PhotinoX (fork mantido do Photino.NET, Apache-2.0): https://packages.nuget.org/packages/PhotinoX
- Ubuntu: restrição de user namespaces via AppArmor: https://discourse.ubuntu.com/t/understanding-apparmor-user-namespace-restriction/58007
- Steamworks: compatibilidade com Steam Deck: https://partner.steamgames.com/doc/steamdeck/compat
- Monaco Editor: https://microsoft.github.io/monaco-editor/

## Planos dos spikes (consultados em 2026-10-08)
- Help Net Security (jun/2026): ESU de Windows 10 para consumidores estendido até 12/out/2027 (R29): https://www.helpnetsecurity.com/2026/06/26/microsoft-windows-10-free-security-updates-esu-program/
- Microsoft Q&A: Windows 10 ESU além de out/2026 (escolas e licenciamento comercial): https://learn.microsoft.com/en-au/answers/questions/5899411/windows-10-esu-beyond-october-2026
- Microsoft Store Policies (cláusula 10.2.2, inclusão dinâmica de código; ler a versão vigente no S7, R30): https://learn.microsoft.com/windows/uwp/publish/store-policies
- Fórum AutoHotkey: interpretação da comunidade da cláusula 10.2.2 para apps com interpretador (não oficial): https://www.autohotkey.com/boards/viewtopic.php?p=550020
- UnityWebBrowser: changelog (último release documentado 2.1.1, 22/mar/2024, CEF 122.1.13): https://projects.voltstro.dev/UnityWebBrowser/latest/changelog/

## Ferramental de IA
- dotnet-claude-kit (GitHub): https://github.com/codewithmukesh/dotnet-claude-kit
- Guia do dotnet-claude-kit (instalação, comandos, MCP): https://codewithmukesh.com/blog/dotnet-claude-kit-guide/

## Pedagogia (conceitos)
- Kapur, M. — *Productive Failure*.
- Sweller, J. — *Cognitive Load Theory* (efeito de exemplos resolvidos, *fading*).
- Sentance, S. et al. — *PRIMM* (Predict-Run-Investigate-Modify-Make).
- Corbett & Anderson — *Bayesian Knowledge Tracing*.
- Lei nº 13.709/2018 (LGPD), art. 14: dados de crianças e adolescentes.
