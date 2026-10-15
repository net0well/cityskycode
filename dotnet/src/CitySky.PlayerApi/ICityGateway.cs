namespace CitySky
{
    /// <summary>
    /// Ponte entre o código do jogador e o jogo (padrão Gateway/Adapter). No modo "ao vivo" as chamadas
    /// viram comandos/consultas validados; nos testes do jogador, uma implementação falsa.
    /// </summary>
    public interface ICityGateway
    {
        /// <summary>Escreve uma mensagem no console do jogo.</summary>
        void Log(string message);
    }
}
