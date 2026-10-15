using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using NUnit.Framework;

namespace CitySky.CodeHost.Tests
{
    // F1-50: o named pipe da sessão é criado com ACL que só deixa o usuário atual (e o SYSTEM) conectar.
    [TestFixture]
    public sealed class PipeAclTests
    {
        [Test]
        public void SessionServer_Accepts_Connection_From_Same_User()
        {
            string name = "citysky-acl-" + Guid.NewGuid().ToString("N");
            using var server = PipeServerFactory.CreateSessionServer(name);
            using var client = new NamedPipeClientStream(".", name, PipeDirection.InOut, PipeOptions.Asynchronous);

            System.Threading.Tasks.Task wait = server.WaitForConnectionAsync();
            client.Connect(5000);
            Assert.That(wait.Wait(5000), Is.True, "o mesmo usuário deve conseguir conectar apesar da ACL");
        }

        [Test]
        public void SessionServer_Grants_Only_CurrentUser_And_System()
        {
            if (!OperatingSystem.IsWindows())
            {
                Assert.Ignore("ACL de pipe é específica do Windows.");
                return;
            }

            string name = "citysky-acl2-" + Guid.NewGuid().ToString("N");
            using var server = PipeServerFactory.CreateSessionServer(name);

            PipeSecurity security = server.GetAccessControl();
            AuthorizationRuleCollection rules = security.GetAccessRules(true, false, typeof(SecurityIdentifier));

            var allowed = new HashSet<string>(StringComparer.Ordinal);
            foreach (PipeAccessRule rule in rules)
            {
                if (rule.AccessControlType == AccessControlType.Allow)
                {
                    allowed.Add(((SecurityIdentifier)rule.IdentityReference).Value);
                }
            }

            using var me = WindowsIdentity.GetCurrent();
            string system = new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null).Value;
            Assert.That(allowed, Does.Contain(me.User!.Value));
            Assert.That(allowed, Does.Contain(system));
            Assert.That(allowed.Count, Is.EqualTo(2), "a ACL deve conceder acesso só ao usuário atual e ao SYSTEM");
        }
    }
}
