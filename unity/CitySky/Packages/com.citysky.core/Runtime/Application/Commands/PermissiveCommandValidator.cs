using CitySky.Domain;

namespace CitySky.Application
{
    /// <summary>
    /// Validador do esqueleto (Null Object): aceita qualquer comando. As regras reais (custo, lote livre,
    /// desbloqueio) entram nas fases seguintes, plugadas por composição.
    /// </summary>
    public sealed class PermissiveCommandValidator : ICommandValidator
    {
        public static readonly PermissiveCommandValidator Instance = new PermissiveCommandValidator();

        public CommandResult Validate(ICityCommand command, CityState state) => CommandResult.Ok;
    }
}
