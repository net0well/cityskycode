using CitySky.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CitySky.Infrastructure
{
    /// <summary>
    /// Serialização do protocolo no lado Unity (Newtonsoft, via com.unity.nuget.newtonsoft-json).
    /// Espelha exatamente as opções do Code Host (<c>CitySky.CodeHost.ProtocolSerializer</c>): camelCase,
    /// <c>TypeNameHandling.None</c>, ignora nulos, sem formatação. O golden JSON (CitySky.Contracts.Tests)
    /// garante que as duas pontas produzem o mesmo byte — essencial para o IL2CPP. F1-74.
    /// </summary>
    public static class ProtocolJson
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            TypeNameHandling = TypeNameHandling.None,
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.None,
        };

        public static string Serialize<T>(Envelope<T> envelope) => JsonConvert.SerializeObject(envelope, Settings);

        public static Envelope<T> Deserialize<T>(string json) => JsonConvert.DeserializeObject<Envelope<T>>(json, Settings);
    }
}
