using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace CitySky.Application
{
    /// <summary>
    /// Serializa o <see cref="CitySnapshot"/> em JSON canônico (campos só inteiros, ordem fixa, cultura
    /// invariante) e o lê de volta. Sem dependência de JSON externo (override O6): a escrita monta a
    /// string à mão e a leitura extrai cada inteiro por chave. Determinístico nos dois runtimes.
    /// </summary>
    public static class SnapshotJson
    {
        public static string Write(CitySnapshot s)
        {
            if (s == null)
            {
                throw new System.ArgumentNullException(nameof(s));
            }

            var b = new StringBuilder(160);
            b.Append('{');
            Field(b, "schemaVersion", s.SchemaVersion.ToString(CultureInfo.InvariantCulture)).Append(',');
            Field(b, "seed", s.WorldSeed.ToString(CultureInfo.InvariantCulture)).Append(',');
            Field(b, "tick", s.Tick.ToString(CultureInfo.InvariantCulture)).Append(',');
            Field(b, "heartbeat", s.Heartbeat.ToString(CultureInfo.InvariantCulture)).Append(',');
            Field(b, "dayCounter", s.DayCounter.ToString(CultureInfo.InvariantCulture)).Append(',');
            Field(b, "hash", s.Hash.ToString(CultureInfo.InvariantCulture));
            b.Append('}');
            return b.ToString();
        }

        public static CitySnapshot Read(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                throw new SnapshotException("Save vazio ou ilegível.");
            }

            int schema = (int)ReadLong(json, "schemaVersion");
            ulong seed = ReadULong(json, "seed");
            long tick = ReadLong(json, "tick");
            long heartbeat = ReadLong(json, "heartbeat");
            long dayCounter = ReadLong(json, "dayCounter");
            ulong hash = ReadULong(json, "hash");
            return new CitySnapshot(schema, seed, tick, heartbeat, dayCounter, hash);
        }

        private static StringBuilder Field(StringBuilder b, string key, string value) =>
            b.Append('"').Append(key).Append("\":").Append(value);

        private static string Match(string json, string key)
        {
            Match m = Regex.Match(json, "\"" + key + "\"\\s*:\\s*(\\d+)");
            if (!m.Success)
            {
                throw new SnapshotException("Campo ausente ou inválido no save: " + key + ".");
            }

            return m.Groups[1].Value;
        }

        private static long ReadLong(string json, string key) =>
            long.Parse(Match(json, key), CultureInfo.InvariantCulture);

        private static ulong ReadULong(string json, string key) =>
            ulong.Parse(Match(json, key), CultureInfo.InvariantCulture);
    }
}
