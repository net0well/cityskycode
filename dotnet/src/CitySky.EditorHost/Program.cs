using System;
using System.IO;
using System.Threading.Tasks;
using CitySky.Contracts;
using CitySky.EditorHost;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Photino.NET;

// Host do editor: janela PhotinoX/WebView2 com o bundle Monaco (editor-web), falando com o Code Host.
// Uso: CitySky.EditorHost [--codehost <exe>] [--web <dir>]
string codeHostExe = ArgValue("--codehost") ?? Path.Combine(AppContext.BaseDirectory, "codehost", "CitySky.CodeHost.exe");
string webRoot = Path.GetFullPath(ArgValue("--web") ?? Path.Combine(AppContext.BaseDirectory, "editor-web"));

var json = new JsonSerializerSettings
{
    ContractResolver = new CamelCasePropertyNamesContractResolver(),
    NullValueHandling = NullValueHandling.Ignore,
    Formatting = Formatting.None,
};

using var codeHost = new CodeHostConnection(codeHostExe);

var window = new PhotinoWindow();
window
    .SetTitle("City: SkyCode — Editor")
    .SetUseOsDefaultSize(false)
    .SetSize(1000, 720)
    .SetContextMenuEnabled(false)
    .SetUserDataFolder(Path.Combine(Path.GetTempPath(), "CitySkyEditorHost"))
    .RegisterCustomSchemeHandler("app", ServeFile)
    .RegisterWebMessageReceivedHandler((_, e) => OnMessage(e.Message))
    .Load("app://localhost/index.html");

new PhotinoApplication().Run(window);
return 0;

void OnMessage(string raw)
{
    JObject message;
    try { message = JObject.Parse(raw); }
    catch (JsonException) { return; }

    string type = message.Value<string>("type") ?? string.Empty;

    // O painel pede estados de janela (expandir/normal). Na janela standalone isso maximiza o SO.
    if (type == "panelState")
    {
        string state = message.Value<string>("state") ?? "normal";
        window.Invoke(() => window.SetMaximized(state == "expanded"));
        return;
    }

    string source = message.Value<string>("source") ?? string.Empty;

    // Chamadas ao Code Host bloqueiam (round-trip do pipe); rodam fora do thread de UI.
    _ = Task.Run(() =>
    {
        string? reply = type switch
        {
            "run" => Reply("runResult", codeHost.Run(source)),
            "diagnostics" => JsonConvert.SerializeObject(new { type = "diagnostics", items = codeHost.GetDiagnostics(source) }, json),
            _ => null,
        };
        if (reply is not null)
        {
            window.Invoke(() => window.SendWebMessage(reply));
        }
    });
}

string Reply(string type, RunResult result) =>
    JsonConvert.SerializeObject(new { type, success = result.Success, console = result.Console, error = result.Error }, json);

Stream? ServeFile(object sender, string scheme, string url, out string contentType)
{
    string relative = Uri.UnescapeDataString(new Uri(url).AbsolutePath).TrimStart('/');
    if (relative.Length == 0)
    {
        relative = "index.html";
    }

    string fullPath = Path.GetFullPath(Path.Combine(webRoot, relative));
    contentType = Path.GetExtension(fullPath).ToLowerInvariant() switch
    {
        ".html" => "text/html",
        ".js" => "text/javascript",
        ".css" => "text/css",
        ".json" => "application/json",
        ".woff" => "font/woff",
        ".woff2" => "font/woff2",
        ".ttf" => "font/ttf",
        ".svg" => "image/svg+xml",
        _ => "application/octet-stream",
    };

    if (!fullPath.StartsWith(webRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) || !File.Exists(fullPath))
    {
        return null;
    }

    return File.OpenRead(fullPath);
}

string? ArgValue(string name)
{
    string[] args = Environment.GetCommandLineArgs();
    int i = Array.IndexOf(args, name);
    return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
}
