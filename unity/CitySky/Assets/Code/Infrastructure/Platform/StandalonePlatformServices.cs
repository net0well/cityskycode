using CitySky.Application;

namespace CitySky.Infrastructure
{
    /// <summary>Implementação Standalone de <see cref="IPlatformServices"/> (F1-53).</summary>
    public sealed class StandalonePlatformServices : IPlatformServices
    {
        // UnityEngine.Application qualificado: dentro de CitySky.* o nome "Application" resolveria para
        // CitySky.Application (o namespace) antes de UnityEngine.Application.
        public string PlatformName => UnityEngine.Application.platform.ToString();

        public string ApplicationVersion => UnityEngine.Application.version;
    }
}
