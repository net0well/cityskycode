using System;
using System.IO;
using UnityEngine;

namespace CitySky.Spike.Bench
{
    /// <summary>
    /// No player: roda o benchmark assim que o jogo inicia, escreve o resultado num arquivo
    /// (ao lado do executável) e encerra. Rodar com: Player.exe -batchmode -nographics
    /// </summary>
    public static class BenchRunner
    {
        private const int Citizens = 100_000;
        private const ulong Seed = 42;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void RunOnStart()
        {
            string backend = IsIl2cpp() ? "IL2CPP" : "Mono";
            string line = $"runtime=Player-{backend} unity={Application.unityVersion} {BenchCore.Run(Citizens, Seed)}";

            string dir = Path.GetDirectoryName(Application.dataPath) ?? Application.persistentDataPath;
            string path = Path.Combine(dir, $"s4b-player-{backend}.txt");
            File.WriteAllText(path, line);
            Debug.Log("S4B_RESULT " + line);
            Application.Quit(0);
        }

        private static bool IsIl2cpp()
        {
#if ENABLE_IL2CPP
            return true;
#else
            return false;
#endif
        }
    }
}
