using System;
using CitySky.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace CitySky.CodeHost;

/// <summary>
/// Serialização do protocolo Unity ↔ Code Host (Newtonsoft, TypeNameHandling.None). As duas pontas
/// usam estas opções (a Unity espelha no lado Infrastructure). O golden JSON está em CitySky.Contracts.Tests.
/// </summary>
public static class ProtocolSerializer
{
    private static readonly JsonSerializerSettings Settings = new()
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        TypeNameHandling = TypeNameHandling.None,
        NullValueHandling = NullValueHandling.Ignore,
        Formatting = Formatting.None,
    };

    private static readonly JsonSerializer Serializer = JsonSerializer.Create(Settings);

    public static byte[] Serialize<T>(Envelope<T> envelope) =>
        System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(envelope, Settings));

    /// <summary>Lê o envelope de forma "crua": devolve o <c>type</c> e o payload como <see cref="JObject"/>.</summary>
    public static (int Version, string Id, string Type, JObject? Payload) ReadEnvelope(byte[] json)
    {
        string text = System.Text.Encoding.UTF8.GetString(json);
        JObject root = JObject.Parse(text);
        return (
            root.Value<int?>("v") ?? Protocol.Version,
            root.Value<string>("id") ?? string.Empty,
            root.Value<string>("type") ?? string.Empty,
            root["payload"] as JObject);
    }

    public static T? ReadPayload<T>(JObject? payload) where T : class =>
        payload?.ToObject<T>(Serializer);
}
