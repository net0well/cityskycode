using CitySky.Infrastructure;
using NUnit.Framework;

namespace CitySky.EditModeTests
{
    // F1-52/F1-92: o relay entre o editor-web e o Code Host (lógica pura, com ICodeHost falso).
    public sealed class EditorBridgeTests
    {
        private static EditorBridge NewBridge() => new EditorBridge(new FakeCodeHost());

        [Test]
        public void Run_Message_Returns_RunResult_With_Console()
        {
            string? reply = NewBridge().HandleMessage("{\"type\":\"run\",\"source\":\"city.Log(\\\"oi\\\");\"}");
            Assert.That(reply, Does.Contain("\"type\":\"runResult\""));
            Assert.That(reply, Does.Contain("\"success\":true"));
            Assert.That(reply, Does.Contain("saída fake"));
        }

        [Test]
        public void Diagnostics_Message_Returns_Items()
        {
            string? reply = NewBridge().HandleMessage("{\"type\":\"diagnostics\",\"source\":\"x\"}");
            Assert.That(reply, Does.Contain("\"type\":\"diagnostics\""));
            Assert.That(reply, Does.Contain("CSK1001"));
        }

        [Test]
        public void Ready_And_Unknown_Messages_Have_No_Reply()
        {
            Assert.That(NewBridge().HandleMessage("{\"type\":\"ready\"}"), Is.Null);
            Assert.That(NewBridge().HandleMessage("{\"type\":\"panelState\",\"state\":\"expanded\"}"), Is.Null);
            Assert.That(NewBridge().HandleMessage("não é json"), Is.Null);
        }
    }
}
