namespace CitySky.Contracts
{
    /// <summary>Constantes do protocolo Unity ↔ Code Host (CODE_EXECUTION §6). F1-60.</summary>
    public static class Protocol
    {
        /// <summary>Versão do protocolo (handshake rejeita versões diferentes).</summary>
        public const int Version = 1;

        /// <summary>Limite de 8 MB por mensagem (enquadramento).</summary>
        public const int MaxMessageBytes = 8 * 1024 * 1024;
    }
}
