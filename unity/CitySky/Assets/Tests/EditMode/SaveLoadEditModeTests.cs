using System.IO;
using CitySky.Application;
using CitySky.Domain;
using NUnit.Framework;

namespace CitySky.EditModeTests
{
    // F1-101 no runtime da Unity (Mono): save/load round-trip com hash igual, espelhando o teste .NET.
    public sealed class SaveLoadEditModeTests
    {
        // IFileLocations temporário e isolado (não suja o persistentDataPath).
        private sealed class TempLocations : IFileLocations
        {
            public TempLocations(string root)
            {
                SavesDirectory = Path.Combine(root, "saves");
                ProfilesDirectory = Path.Combine(root, "profiles");
                LogsDirectory = Path.Combine(root, "logs");
                Directory.CreateDirectory(SavesDirectory);
            }

            public string SavesDirectory { get; }
            public string ProfilesDirectory { get; }
            public string LogsDirectory { get; }
        }

        [Test]
        public void Save_Then_Load_Preserves_Hash_In_Unity_Runtime()
        {
            string root = Path.Combine(UnityEngine.Application.temporaryCachePath, "citysky-saveload-" + System.Guid.NewGuid().ToString("N"));
            try
            {
                var sim = new HeadlessSimulation(0xC175C0DE5EEDUL);
                sim.RunTicks(SimulationClock.TicksPerDay + 137);
                ulong originalHash = sim.State.ComputeHash();

                var store = new FileSaveStore(new TempLocations(root));
                store.Save("slot1", SnapshotService.Capture(sim.State));
                CitySnapshot loaded = store.Load("slot1");

                Assert.That(loaded, Is.Not.Null);
                CityState restored = SnapshotService.Restore(loaded);
                Assert.That(restored.ComputeHash(), Is.EqualTo(originalHash));
            }
            finally
            {
                try { if (Directory.Exists(root)) { Directory.Delete(root, true); } }
                catch { /* pasta temporária */ }
            }
        }
    }
}
