using System.IO;
using CitySky.Application;
using CitySky.Application.Tests.Helpers;
using CitySky.Domain;
using NUnit.Framework;

namespace CitySky.Application.Tests.Persistence
{
    // F1-101: save/load do esqueleto — round-trip com hash igual, corrupção detectada, escrita com .bak.
    [TestFixture]
    public sealed class SaveLoadTests
    {
        private const ulong Seed = 0xC175C0DE5EEDUL;

        private static CityState RunSome()
        {
            var sim = new HeadlessSimulation(Seed);
            sim.RunTicks(SimulationClock.TicksPerDay + 137); // estado não-trivial (heartbeat/daycounter > 0)
            return sim.State;
        }

        [Test]
        public void Capture_Then_Restore_Preserves_State()
        {
            CityState original = RunSome();
            CitySnapshot snap = SnapshotService.Capture(original);
            CityState restored = SnapshotService.Restore(snap);

            Assert.That(restored.ComputeHash(), Is.EqualTo(original.ComputeHash()));
            Assert.That(restored.WorldSeed, Is.EqualTo(original.WorldSeed));
            Assert.That(restored.Clock.Tick, Is.EqualTo(original.Clock.Tick));
            Assert.That(restored.Heartbeat, Is.EqualTo(original.Heartbeat));
            Assert.That(restored.DayCounter, Is.EqualTo(original.DayCounter));
        }

        [Test]
        public void FileStore_RoundTrips_With_Same_Hash()
        {
            using var locations = new TempFileLocations();
            var store = new FileSaveStore(locations);
            CityState original = RunSome();
            ulong originalHash = original.ComputeHash();

            store.Save("slot1", SnapshotService.Capture(original));
            CitySnapshot? loaded = store.Load("slot1");

            Assert.That(loaded, Is.Not.Null);
            CityState restored = SnapshotService.Restore(loaded!);
            Assert.That(restored.ComputeHash(), Is.EqualTo(originalHash));
            Assert.That(File.Exists(Path.Combine(locations.SavesDirectory, "slot1", "manifest.json")), Is.True);
            Assert.That(File.Exists(Path.Combine(locations.SavesDirectory, "slot1", "city.json.gz")), Is.True);
        }

        [Test]
        public void Second_Save_Leaves_A_Bak()
        {
            using var locations = new TempFileLocations();
            var store = new FileSaveStore(locations);
            CitySnapshot snap = SnapshotService.Capture(RunSome());

            store.Save("slot1", snap);
            store.Save("slot1", snap); // sobrescreve → versão anterior vira .bak

            string dir = Path.Combine(locations.SavesDirectory, "slot1");
            Assert.That(File.Exists(Path.Combine(dir, "city.json.gz.bak")), Is.True);
            Assert.That(File.Exists(Path.Combine(dir, "manifest.json.bak")), Is.True);
        }

        [Test]
        public void Load_Missing_Slot_Returns_Null()
        {
            using var locations = new TempFileLocations();
            var store = new FileSaveStore(locations);
            Assert.That(store.Exists("nope"), Is.False);
            Assert.That(store.Load("nope"), Is.Null);
        }

        [Test]
        public void Restore_Rejects_Corrupted_Hash()
        {
            CityState original = RunSome();
            CitySnapshot good = SnapshotService.Capture(original);
            var tampered = new CitySnapshot(good.SchemaVersion, good.WorldSeed, good.Tick, good.Heartbeat + 1, good.DayCounter, good.Hash);
            Assert.That(() => SnapshotService.Restore(tampered), Throws.TypeOf<SnapshotException>());
        }

        [Test]
        public void Restore_Rejects_Incompatible_Schema()
        {
            CitySnapshot snap = SnapshotService.Capture(RunSome());
            var future = new CitySnapshot(snap.SchemaVersion + 1, snap.WorldSeed, snap.Tick, snap.Heartbeat, snap.DayCounter, snap.Hash);
            Assert.That(() => SnapshotService.Restore(future), Throws.TypeOf<SnapshotException>());
        }

        [Test]
        public void SnapshotJson_RoundTrips()
        {
            CitySnapshot snap = SnapshotService.Capture(RunSome());
            CitySnapshot back = SnapshotJson.Read(SnapshotJson.Write(snap));
            Assert.That(back.SchemaVersion, Is.EqualTo(snap.SchemaVersion));
            Assert.That(back.WorldSeed, Is.EqualTo(snap.WorldSeed));
            Assert.That(back.Tick, Is.EqualTo(snap.Tick));
            Assert.That(back.Heartbeat, Is.EqualTo(snap.Heartbeat));
            Assert.That(back.DayCounter, Is.EqualTo(snap.DayCounter));
            Assert.That(back.Hash, Is.EqualTo(snap.Hash));
        }
    }
}
