namespace CitySky.CodeHost;

/// <summary>
/// Inicia o processo Worker (padrão Factory + Strategy de plataforma). No produto o Worker roda dentro
/// de um Job Object com limites rígidos (F1-51, spike S6); a implementação v0 usa um processo com stdio
/// redirecionado. A abstração deixa trocar o lançador sem tocar no resto (ADR-0018).
/// </summary>
public interface ISandboxLauncher
{
    ISandboxProcess Launch();
}
