using System.IO;
using CitySky.Contracts;
using CitySky.Infrastructure;
using NUnit.Framework;
using UnityEngine;

namespace CitySky.EditModeTests
{
    // F1-74 / semente do marco F1-112: a Unity lança o Code Host real, compila e executa city.Log e
    // recebe o console de volta — de ponta a ponta (Unity → Host → Worker → Unity). Windows-only
    // (lançador nativo); precisa do Code Host publicado em StreamingAssets/codehost.
    [TestFixture]
    [Platform(Include = "Win")]
    public sealed class CodeHostClientTests
    {
        private static string HostExe =>
            Path.Combine(UnityEngine.Application.streamingAssetsPath, "codehost", "CitySky.CodeHost.exe");

        [Test]
        public void City_Log_RoundTrips_Through_Code_Host()
        {
            if (!File.Exists(HostExe))
            {
                Assert.Ignore("Code Host não publicado em StreamingAssets/codehost (dotnet publish).");
            }

            using var client = new CodeHostClient();
            client.Start(HostExe);

            RunResult result = client.Run("var city = CitySky.CityGame.Current; city.Log(\"Olá, prefeito(a)!\");");

            Assert.That(result.Success, Is.True, result.Error);
            Assert.That(result.Console, Does.Contain("Olá, prefeito(a)!"));
        }

        [Test]
        public void Compile_Error_Is_Reported()
        {
            if (!File.Exists(HostExe))
            {
                Assert.Ignore("Code Host não publicado em StreamingAssets/codehost (dotnet publish).");
            }

            using var client = new CodeHostClient();
            client.Start(HostExe);

            RunResult result = client.Run("System.IO.File.ReadAllText(\"x\");");

            Assert.That(result.Success, Is.False);
            Assert.That(result.Error, Is.Not.Null);
        }

        [Test]
        public void Diagnostics_While_Typing_Flag_Banned_Api()
        {
            if (!File.Exists(HostExe))
            {
                Assert.Ignore("Code Host não publicado em StreamingAssets/codehost (dotnet publish).");
            }

            using var client = new CodeHostClient();
            client.Start(HostExe);

            var diags = client.GetDiagnostics("System.IO.File.ReadAllText(\"x\");");
            Assert.That(diags, Has.Some.Matches<Diagnostic>(d => d.Id == "CSK1001"));
        }
    }
}
