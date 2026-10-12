using CitySky.Domain;

namespace CitySky.Application
{
    /// <summary>
    /// Valida um comando antes de entrar na fila (padrão Strategy). As mesmas regras valem para a UI e
    /// para o código do jogador — o código nunca tem poder que a UI não tenha (regra inviolável).
    /// </summary>
    public interface ICommandValidator
    {
        CommandResult Validate(ICityCommand command, CityState state);
    }
}
