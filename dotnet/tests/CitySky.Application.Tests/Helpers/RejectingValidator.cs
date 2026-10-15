using CitySky.Domain;

namespace CitySky.Application.Tests
{
    /// <summary>Validador de teste que rejeita todo comando.</summary>
    internal sealed class RejectingValidator : ICommandValidator
    {
        public CommandResult Validate(ICityCommand command, CityState state) =>
            CommandResult.Fail(CommandError.Rejected);
    }
}
