using System;
using System.IO.Pipes;
using System.Runtime.Versioning;
using System.Security.AccessControl;
using System.Security.Principal;

namespace CitySky.CodeHost;

/// <summary>
/// Cria o <see cref="NamedPipeServerStream"/> da sessão do Code Host. No Windows aplica uma ACL que
/// concede acesso só ao usuário atual (e ao SYSTEM), de modo que outra conta na mesma máquina não
/// consiga se conectar ao Code Host (F1-50; CODE_EXECUTION_SECURITY). Fora do Windows (canário Linux)
/// cria o pipe sem ACL do Windows — os named pipes do Unix já ficam no namespace do usuário.
/// </summary>
internal static class PipeServerFactory
{
    public static NamedPipeServerStream CreateSessionServer(string pipeName)
    {
        if (OperatingSystem.IsWindows())
        {
            return CreateWindowsUserOnly(pipeName);
        }

        return new NamedPipeServerStream(
            pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
    }

    [SupportedOSPlatform("windows")]
    private static NamedPipeServerStream CreateWindowsUserOnly(string pipeName)
    {
        var security = new PipeSecurity();
        using var identity = WindowsIdentity.GetCurrent();
        SecurityIdentifier user = identity.User
            ?? throw new InvalidOperationException("Sem SID do usuário atual para montar a ACL do pipe.");
        security.AddAccessRule(new PipeAccessRule(user, PipeAccessRights.FullControl, AccessControlType.Allow));

        var localSystem = new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null);
        security.AddAccessRule(new PipeAccessRule(localSystem, PipeAccessRights.FullControl, AccessControlType.Allow));

        return NamedPipeServerStreamAcl.Create(
            pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous,
            inBufferSize: 0, outBufferSize: 0, pipeSecurity: security);
    }
}
