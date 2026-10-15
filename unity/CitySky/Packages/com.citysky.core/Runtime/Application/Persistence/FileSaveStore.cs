using System.IO;
using System.IO.Compression;
using System.Text;

namespace CitySky.Application
{
    /// <summary>
    /// <see cref="ISaveStore"/> em disco (F1-101): cada slot é uma pasta com <c>manifest.json</c> (resumo
    /// legível) e <c>city.json.gz</c> (estado comprimido). Escrita atômica — grava um <c>.tmp</c> e troca
    /// com <c>File.Replace</c>, deixando a versão anterior como <c>.bak</c>. Só System.IO (sem deps, O6);
    /// o <i>onde</i> vem do <see cref="IFileLocations"/> (Infrastructure decide a raiz por SO).
    /// </summary>
    public sealed class FileSaveStore : ISaveStore
    {
        private const string ManifestFile = "manifest.json";
        private const string CityFile = "city.json.gz";

        private readonly IFileLocations _locations;

        public FileSaveStore(IFileLocations locations)
        {
            _locations = locations ?? throw new System.ArgumentNullException(nameof(locations));
        }

        public void Save(string slot, CitySnapshot snapshot)
        {
            if (snapshot == null)
            {
                throw new System.ArgumentNullException(nameof(snapshot));
            }

            string dir = SlotDir(slot);
            Directory.CreateDirectory(dir);

            string json = SnapshotJson.Write(snapshot);
            WriteAtomic(Path.Combine(dir, ManifestFile), Encoding.UTF8.GetBytes(json));
            WriteAtomic(Path.Combine(dir, CityFile), Gzip(json));
        }

        public CitySnapshot? Load(string slot)
        {
            string path = Path.Combine(SlotDir(slot), CityFile);
            if (!File.Exists(path))
            {
                return null;
            }

            string json = Gunzip(File.ReadAllBytes(path));
            return SnapshotJson.Read(json);
        }

        public bool Exists(string slot) => File.Exists(Path.Combine(SlotDir(slot), CityFile));

        private string SlotDir(string slot)
        {
            if (string.IsNullOrEmpty(slot))
            {
                throw new System.ArgumentException("Nome do slot vazio.", nameof(slot));
            }

            return Path.Combine(_locations.SavesDirectory, slot);
        }

        private static void WriteAtomic(string path, byte[] bytes)
        {
            string tmp = path + ".tmp";
            File.WriteAllBytes(tmp, bytes);
            if (File.Exists(path))
            {
                File.Replace(tmp, path, path + ".bak"); // troca atômica; versão anterior vira .bak
            }
            else
            {
                File.Move(tmp, path);
            }
        }

        private static byte[] Gzip(string text)
        {
            byte[] raw = Encoding.UTF8.GetBytes(text);
            using var output = new MemoryStream();
            using (var gz = new GZipStream(output, CompressionLevel.Optimal, leaveOpen: true))
            {
                gz.Write(raw, 0, raw.Length);
            }

            return output.ToArray();
        }

        private static string Gunzip(byte[] compressed)
        {
            using var input = new MemoryStream(compressed);
            using var gz = new GZipStream(input, CompressionMode.Decompress);
            using var output = new MemoryStream();
            gz.CopyTo(output);
            return Encoding.UTF8.GetString(output.ToArray());
        }
    }
}
