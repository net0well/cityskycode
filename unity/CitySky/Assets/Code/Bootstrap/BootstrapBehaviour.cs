using CitySky.Application;
using CitySky.Infrastructure;
using UnityEngine;

namespace CitySky.Bootstrap
{
    /// <summary>
    /// Ponto de entrada na cena Bootstrap: monta a raiz de composição e avança a simulação a cada frame,
    /// com o orçamento de tempo real. É a única "cola" MonoBehaviour do esqueleto (sem regra de negócio). F1-43.
    /// </summary>
    public sealed class BootstrapBehaviour : MonoBehaviour
    {
        [SerializeField] private long _worldSeed = 1;

        private CompositionRoot _root = null!;
        private ISimulationBudget _budget = null!;
        private GameSpeed _speed = GameSpeed.Normal;

        private void Awake()
        {
            _budget = new StopwatchSimulationBudget();
            _root = new CompositionRoot((ulong)_worldSeed, new UnityLog());
            UnityEngine.Application.runInBackground = true;
        }

        private void Update()
        {
            _root.Advance(Time.unscaledDeltaTime, _speed, _budget);
        }

        /// <summary>Troca a velocidade do jogo (ligado à UI depois).</summary>
        public void SetSpeed(GameSpeed speed) => _speed = speed;
    }
}
