using System;
using System.IO;

namespace CitySky.CodeHost;

/// <summary>Processo Worker em execução, com os fluxos binários do canal Host ↔ Worker.</summary>
public interface ISandboxProcess : IDisposable
{
    Stream StandardInput { get; }

    Stream StandardOutput { get; }

    bool HasExited { get; }

    void Kill();
}
