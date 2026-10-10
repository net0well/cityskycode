using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace CitySky.Architecture.Tests
{
    // F1-22. Fitness functions (ARCHITECTURE §12). Falham ao introduzir uma violação proposital.
    // TODO(F1): checagens de IL (Domain sem DateTime.Now/Random/Stopwatch/Guid.NewGuid) via Mono.Cecil —
    // exigem inspecionar call sites; as checagens abaixo são baseadas em reflexão (refs + campos).
    [TestFixture]
    public sealed class ArchitectureTests
    {
        private static readonly string[] SharedAssemblies =
        {
            "CitySky.Domain", "CitySky.Application", "CitySky.Contracts", "CitySky.Education",
        };

        private static Assembly Load(string name) => Assembly.Load(new AssemblyName(name));

        [Test]
        public void Shared_Assemblies_Do_Not_Reference_Unity()
        {
            foreach (string name in SharedAssemblies)
            {
                Assembly asm = Load(name);
                string[] bad = asm.GetReferencedAssemblies()
                    .Select(a => a.Name ?? string.Empty)
                    .Where(n => n.StartsWith("UnityEngine", StringComparison.Ordinal)
                             || n.StartsWith("Unity.", StringComparison.Ordinal))
                    .ToArray();

                Assert.That(bad, Is.Empty, $"{name} não pode referenciar a engine (noEngineReferences): {string.Join(", ", bad)}");
            }
        }

        [Test]
        public void Domain_Has_No_Floating_Point_Fields()
        {
            Assembly domain = Load("CitySky.Domain");
            var offenders = new List<string>();
            foreach (Type type in domain.GetTypes())
            {
                FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (FieldInfo field in fields)
                {
                    if (field.FieldType == typeof(float) || field.FieldType == typeof(double))
                    {
                        offenders.Add($"{type.FullName}.{field.Name} : {field.FieldType.Name}");
                    }
                }
            }

            Assert.That(offenders, Is.Empty,
                "O estado do domínio é inteiro/ponto fixo (determinismo): " + string.Join(", ", offenders));
        }

        [Test]
        public void Contracts_Does_Not_Reference_Domain()
        {
            string[] refs = Load("CitySky.Contracts").GetReferencedAssemblies()
                .Select(a => a.Name ?? string.Empty).ToArray();
            Assert.That(refs, Does.Not.Contain("CitySky.Domain"),
                "Contracts são DTOs do IPC e não dependem do Domain (ARCHITECTURE §4.1).");
        }
    }
}
