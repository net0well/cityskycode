using System.Collections.Generic;
using CitySky.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace CitySky.Infrastructure
{
    /// <summary>
    /// Relay entre o editor-web (Monaco) e o Code Host (F1-52/F1-92). Recebe as mensagens do editor
    /// ({run, diagnostics, ready, panelState}) e devolve a resposta no formato que o editor espera
    /// ({runResult, diagnostics}). O host da janela (WebView2-filho) só transporta os bytes.
    /// </summary>
    public sealed class EditorBridge
    {
        private static readonly JsonSerializerSettings CamelCase = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore,
        };

        private readonly ICodeHost _host;

        public EditorBridge(ICodeHost host)
        {
            _host = host;
        }

        /// <summary>Processa uma mensagem do editor; devolve a resposta JSON a enviar de volta, ou <c>null</c>.</summary>
        public string? HandleMessage(string json)
        {
            JObject message;
            try
            {
                message = JObject.Parse(json);
            }
            catch (JsonException)
            {
                return null;
            }

            string? type = message.Value<string>("type");
            string source = message.Value<string>("source") ?? string.Empty;

            switch (type)
            {
                case "run":
                    RunResult result = _host.Run(source);
                    return JsonConvert.SerializeObject(
                        new { type = "runResult", success = result.Success, console = result.Console, error = result.Error },
                        CamelCase);

                case "diagnostics":
                    IReadOnlyList<Diagnostic> items = _host.GetDiagnostics(source);
                    return JsonConvert.SerializeObject(new { type = "diagnostics", items }, CamelCase);

                default:
                    return null; // ready / panelState: sem resposta pela ponte (o host da janela cuida do painel)
            }
        }
    }
}
