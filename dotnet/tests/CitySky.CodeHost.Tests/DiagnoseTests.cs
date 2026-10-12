using CitySky.CodeAnalysis;
using CitySky.Contracts;
using NUnit.Framework;

namespace CitySky.CodeHost.Tests
{
    // F1-93: diagnósticos ao digitar (compila sem executar) — erros do compilador + achados da allowlist.
    [TestFixture]
    public sealed class DiagnoseTests
    {
        private static Pipeline _pipeline = null!;

        [OneTimeSetUp]
        public void Setup() => _pipeline = new Pipeline(ReferenceSet.Create());

        [Test]
        public void Reports_Compiler_Error_With_Line()
        {
            var diags = _pipeline.Diagnose("int x = ;");
            Assert.That(diags, Has.Some.Matches<Diagnostic>(d => d.Severity == DiagnosticSeverity.Error && d.StartLine == 1));
        }

        [Test]
        public void Flags_Banned_Api_As_CSK()
        {
            var diags = _pipeline.Diagnose("System.IO.File.ReadAllText(\"x\");");
            Assert.That(diags, Has.Some.Matches<Diagnostic>(d => d.Id == "CSK1001"));
        }

        [Test]
        public void Valid_Program_Has_No_Errors()
        {
            var diags = _pipeline.Diagnose("var city = CitySky.CityGame.Current; city.Log(\"ok\");");
            Assert.That(diags, Has.None.Matches<Diagnostic>(d => d.Severity == DiagnosticSeverity.Error));
        }
    }
}
