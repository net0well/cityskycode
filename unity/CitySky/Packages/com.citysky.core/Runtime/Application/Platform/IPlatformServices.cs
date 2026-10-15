namespace CitySky.Application
{
    /// <summary>
    /// Serviços da plataforma host, por trás de uma interface (ADR-0018; F1-53). Esqueleto da Fase 1:
    /// nome da plataforma e versão do app. Implementação Standalone no Infrastructure (Unity).
    /// </summary>
    public interface IPlatformServices
    {
        string PlatformName { get; }

        string ApplicationVersion { get; }
    }
}
