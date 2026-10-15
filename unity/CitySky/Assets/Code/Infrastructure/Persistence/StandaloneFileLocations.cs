using System.IO;
using CitySky.Application;

namespace CitySky.Infrastructure
{
    /// <summary>
    /// Implementação Standalone de <see cref="IFileLocations"/> (F1-53): subpastas sob
    /// <c>UnityEngine.Application.persistentDataPath</c> (gravável sem admin — ADR-0017), montadas com
    /// <c>Path.Combine</c> e criadas sob demanda.
    /// </summary>
    public sealed class StandaloneFileLocations : IFileLocations
    {
        public StandaloneFileLocations()
        {
            // UnityEngine.Application qualificado (ver StandalonePlatformServices).
            string root = UnityEngine.Application.persistentDataPath;
            SavesDirectory = Ensure(Path.Combine(root, "saves"));
            ProfilesDirectory = Ensure(Path.Combine(root, "profiles"));
            LogsDirectory = Ensure(Path.Combine(root, "logs"));
        }

        public string SavesDirectory { get; }

        public string ProfilesDirectory { get; }

        public string LogsDirectory { get; }

        private static string Ensure(string directory)
        {
            Directory.CreateDirectory(directory);
            return directory;
        }
    }
}
