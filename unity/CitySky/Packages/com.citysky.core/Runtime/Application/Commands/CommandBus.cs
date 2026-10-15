using System;
using CitySky.Domain;

namespace CitySky.Application
{
    /// <summary>
    /// Recebe comandos (da UI ou do código do jogador), valida e enfileira para o tick alvo (F1-41).
    /// Única porta de entrada de mutação do estado — toda mutação é um comando validado (regra inviolável).
    /// </summary>
    public sealed class CommandBus
    {
        private readonly Simulation _simulation;
        private readonly ICommandValidator _validator;

        public CommandBus(Simulation simulation, ICommandValidator? validator = null)
        {
            _simulation = simulation ?? throw new ArgumentNullException(nameof(simulation));
            _validator = validator ?? PermissiveCommandValidator.Instance;
        }

        /// <summary>Submete um comando para aplicar no próximo tick.</summary>
        public CommandResult Submit(ICityCommand command) => Submit(command, _simulation.CurrentTick);

        /// <summary>Submete um comando para aplicar no tick alvo. Inválido → não entra na fila.</summary>
        public CommandResult Submit(ICityCommand command, long targetTick)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            CommandResult result = _validator.Validate(command, _simulation.State);
            if (result.Success)
            {
                _simulation.Commands.Enqueue(command, targetTick);
            }

            return result;
        }
    }
}
