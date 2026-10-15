using System.IO;
using CitySky.Application;

namespace CitySky.EditModeTests
{
    /// <summary>IFileLocations temporário e isolado para testes (não suja o persistentDataPath).</summary>
    internal sealed class TempLocations : IFileLocations
    {
        public TempLocations(string root)
        {
            SavesDirectory = Path.Combine(root, "saves");
            ProfilesDirectory = Path.Combine(root, "profiles");
            LogsDirectory = Path.Combine(root, "logs");
            Directory.CreateDirectory(SavesDirectory);
        }

        public string SavesDirectory { get; }

        public string ProfilesDirectory { get; }

        public string LogsDirectory { get; }
    }
}
