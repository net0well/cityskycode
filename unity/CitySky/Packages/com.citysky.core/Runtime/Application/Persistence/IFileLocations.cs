namespace CitySky.Application
{
    /// <summary>
    /// Pastas do produto (saves, perfis, logs) — por trás de uma interface porque dependem do SO e de
    /// estar empacotado ou não (ADR-0018; F1-53). A implementação Standalone (Unity) as monta sob
    /// <c>Application.persistentDataPath</c> com <c>Path.Combine</c>. Os testes usam uma pasta temporária.
    /// </summary>
    public interface IFileLocations
    {
        /// <summary>Pasta onde ficam os saves (um subdiretório por save).</summary>
        string SavesDirectory { get; }

        /// <summary>Pasta dos perfis locais (sala de aula; sem conta — ADR-0017).</summary>
        string ProfilesDirectory { get; }

        /// <summary>Pasta de logs.</summary>
        string LogsDirectory { get; }
    }
}
