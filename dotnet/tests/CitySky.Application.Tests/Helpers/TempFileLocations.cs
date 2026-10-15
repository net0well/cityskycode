using System;
using System.IO;
using CitySky.Application;

namespace CitySky.Application.Tests.Helpers
{
    // IFileLocations apontando para uma pasta temporária descartável (uma por teste).
    internal sealed class TempFileLocations : IFileLocations, IDisposable
    {
        public TempFileLocations()
        {
            Root = Path.Combine(Path.GetTempPath(), "citysky-tests-" + Guid.NewGuid().ToString("N"));
            SavesDirectory = Path.Combine(Root, "saves");
            ProfilesDirectory = Path.Combine(Root, "profiles");
            LogsDirectory = Path.Combine(Root, "logs");
            Directory.CreateDirectory(SavesDirectory);
        }

        public string Root { get; }

        public string SavesDirectory { get; }

        public string ProfilesDirectory { get; }

        public string LogsDirectory { get; }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(Root))
                {
                    Directory.Delete(Root, true);
                }
            }
            catch
            {
                // pasta temporária; ignora falha de limpeza
            }
        }
    }
}
