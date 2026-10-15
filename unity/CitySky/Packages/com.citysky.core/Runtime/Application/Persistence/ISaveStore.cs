namespace CitySky.Application
{
    /// <summary>
    /// Grava e lê saves do esqueleto (F1-101). Cada save é um subdiretório com <c>manifest.json</c>
    /// (resumo legível: esquema, tick, seed, hash) e <c>city.json.gz</c> (estado comprimido). Escrita
    /// atômica com <c>.bak</c>. A reconstrução do estado + conferência de hash fica no <see cref="SnapshotService"/>.
    /// </summary>
    public interface ISaveStore
    {
        void Save(string slot, CitySnapshot snapshot);

        /// <summary>Lê o save; retorna <c>null</c> se o slot não existe.</summary>
        CitySnapshot? Load(string slot);

        bool Exists(string slot);
    }
}
