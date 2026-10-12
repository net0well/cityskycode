using System;
using CitySky.CodeAnalysis;
using CitySky.CodeHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// Code Host (F1-70): Generic Host + DI. Compila o código do jogador (Roslyn + L1/L2) e o executa num
// Worker isolado, atendendo a Unity por named pipe. Uso: CitySky.CodeHost [--pipe <nome>].
string? pipeName = GetArg(args, "--pipe");

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton(_ => ReferenceSet.Create());
builder.Services.AddSingleton<Pipeline>();
builder.Services.AddSingleton<ISandboxLauncher>(_ => new ProcessSandboxLauncher());
builder.Services.AddSingleton<WorkerPool>();
builder.Services.AddSingleton<RunCoordinator>();
builder.Services.AddSingleton<CodeHostServer>();
builder.Services.AddSingleton(new PipeConfig(pipeName));
builder.Services.AddHostedService<PipeHostService>();

IHost host = builder.Build();
host.Run();
return 0;

static string? GetArg(string[] args, string name)
{
    int i = Array.IndexOf(args, name);
    return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
}
