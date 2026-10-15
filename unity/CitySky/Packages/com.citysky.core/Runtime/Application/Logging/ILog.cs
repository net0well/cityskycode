namespace CitySky.Application
{
    /// <summary>
    /// Porta de log da camada de aplicação (adaptada pela Infrastructure: Unity <c>Debug.Log</c> + arquivo).
    /// O domínio NÃO loga — ele emite eventos e métricas (ARCHITECTURE §10). F1-42.
    /// </summary>
    public interface ILog
    {
        void Info(string message);

        void Warning(string message);

        void Error(string message);
    }
}
