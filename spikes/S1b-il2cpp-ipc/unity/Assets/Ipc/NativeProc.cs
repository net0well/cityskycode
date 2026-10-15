using System;
using System.Runtime.InteropServices;

namespace CitySky.Spike.Ipc
{
    /// <summary>
    /// Lançador de processo por P/Invoke CreateProcess. No IL2CPP, System.Diagnostics.Process.Start
    /// falha ("Native error= Success"); CreateProcess nativo funciona (mesmo caminho do S6/ISandboxLauncher).
    /// </summary>
    internal sealed class ChildProcess : IDisposable
    {
        private IntPtr _process;
        private IntPtr _thread;

        private ChildProcess(IntPtr process, IntPtr thread, int pid)
        {
            _process = process;
            _thread = thread;
            Pid = pid;
        }

        public int Pid { get; }

        public bool HasExited
        {
            get
            {
                if (_process == IntPtr.Zero)
                {
                    return true;
                }

                return GetExitCodeProcess(_process, out uint code) && code != StillActive;
            }
        }

        public static ChildProcess Start(string exe, string args)
        {
            var si = new Startupinfo { Cb = Marshal.SizeOf<Startupinfo>() };
            string commandLine = $"\"{exe}\" {args}";
            string workingDir = System.IO.Path.GetDirectoryName(exe) ?? string.Empty;

            bool ok = CreateProcessW(
                exe, commandLine, IntPtr.Zero, IntPtr.Zero, false,
                CreateNoWindow | CreateUnicodeEnvironment, IntPtr.Zero, workingDir, ref si, out ProcessInformation pi);

            if (!ok)
            {
                throw new InvalidOperationException($"CreateProcess falhou: Win32 {Marshal.GetLastWin32Error()}");
            }

            return new ChildProcess(pi.Process, pi.Thread, pi.ProcessId);
        }

        public void WaitForExit(int ms) => WaitForSingleObject(_process, (uint)ms);

        public void Kill()
        {
            try
            {
                if (_process != IntPtr.Zero && !HasExited)
                {
                    TerminateProcess(_process, 1);
                }
            }
            catch
            {
                // já encerrado
            }
        }

        public void Dispose()
        {
            if (_thread != IntPtr.Zero)
            {
                CloseHandle(_thread);
                _thread = IntPtr.Zero;
            }

            if (_process != IntPtr.Zero)
            {
                CloseHandle(_process);
                _process = IntPtr.Zero;
            }
        }

        private const uint StillActive = 259;
        private const uint CreateNoWindow = 0x08000000;
        private const uint CreateUnicodeEnvironment = 0x00000400;

        [StructLayout(LayoutKind.Sequential)]
        private struct Startupinfo
        {
            public int Cb;
            public IntPtr Reserved, Desktop, Title;
            public int X, Y, XSize, YSize, XCountChars, YCountChars, FillAttribute, Flags;
            public short ShowWindow, CbReserved2;
            public IntPtr Reserved2, StdInput, StdOutput, StdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ProcessInformation
        {
            public IntPtr Process;
            public IntPtr Thread;
            public int ProcessId;
            public int ThreadId;
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool CreateProcessW(
            string? applicationName, string commandLine, IntPtr processAttributes, IntPtr threadAttributes,
            bool inheritHandles, uint creationFlags, IntPtr environment, string? currentDirectory,
            ref Startupinfo startupInfo, out ProcessInformation processInformation);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint WaitForSingleObject(IntPtr handle, uint ms);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetExitCodeProcess(IntPtr process, out uint exitCode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool TerminateProcess(IntPtr process, uint exitCode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr handle);
    }
}
