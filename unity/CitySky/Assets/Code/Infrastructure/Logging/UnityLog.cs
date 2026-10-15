using CitySky.Application;
using UnityEngine;

namespace CitySky.Infrastructure
{
    /// <summary>Adaptador da porta <see cref="ILog"/> para o console da Unity (F1-42).</summary>
    public sealed class UnityLog : ILog
    {
        public void Info(string message) => Debug.Log(message);

        public void Warning(string message) => Debug.LogWarning(message);

        public void Error(string message) => Debug.LogError(message);
    }
}
