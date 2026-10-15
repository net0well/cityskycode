using CitySky.CodeAnalysis;
using NUnit.Framework;

namespace CitySky.Security.Tests
{
    // F1-75 — corpus de rejeição em tempo de compilação (L1) e no IL (L2).
    [TestFixture]
    public sealed class PipelineRejectionTests
    {
        private static Pipeline _pipeline = null!;

        [OneTimeSetUp]
        public void Setup() => _pipeline = new Pipeline(ReferenceSet.Create());

        [TestCase("System.IO.File.ReadAllText(\"x\");", TestName = "arquivo")]
        [TestCase("System.IO.File.WriteAllText(\"x\", \"y\");", TestName = "arquivo-escrita")]
        [TestCase("var c = new System.Net.Http.HttpClient();", TestName = "rede")]
        [TestCase("System.Diagnostics.Process.Start(\"calc\");", TestName = "processo")]
        [TestCase("var a = typeof(string).Assembly;", TestName = "reflexao-assembly")]
        [TestCase("System.Threading.Thread.Sleep(1000);", TestName = "thread")]
        [TestCase("var e = System.Environment.GetEnvironmentVariable(\"PATH\");", TestName = "environment")]
        public void Banned_Api_Rejected_At_L1(string snippet)
        {
            CompileResult result = _pipeline.Compile(snippet);
            Assert.That(result.Ok, Is.False);
            Assert.That(result.Stage, Is.EqualTo("L1"));
            Assert.That(result.Findings, Is.Not.Empty);
        }

        [Test]
        public void File_Access_Also_Rejected_At_L2_Alone()
        {
            // Pula L1 de propósito: o IL emitido ainda referencia System.IO.File → L2 pega sozinho.
            CompileResult result = _pipeline.Compile("System.IO.File.ReadAllText(\"x\");", enforceL1: false);
            Assert.That(result.Ok, Is.False);
            Assert.That(result.Stage, Is.EqualTo("L2"));
            Assert.That(result.Findings, Is.Not.Empty);
        }

        [Test]
        public void Unsafe_And_Pointers_Rejected()
        {
            CompileResult result = _pipeline.Compile("unsafe { int x = 1; int* p = &x; }");
            Assert.That(result.Ok, Is.False);
            Assert.That(result.Findings, Has.Some.Matches<Finding>(f => f.Code == "CSK1004"));
        }

        [Test]
        public void Valid_Program_Compiles()
        {
            CompileResult result = _pipeline.Compile("var city = CitySky.CityGame.Current; city.Log(\"Olá, cidade!\");");
            Assert.That(result.Ok, Is.True, string.Join(" | ", result.Diagnostics));
            Assert.That(result.Stage, Is.EqualTo("emitted"));
            Assert.That(result.Assembly, Is.Not.Null);
        }
    }
}
