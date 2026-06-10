using NUnit.Framework;

namespace KostasBan.Lens.Tests
{
    public sealed class LensEntryDrawerRegistryTests
    {
        [TearDown]
        public void TearDown()
        {
            LensEntryDrawerRegistry.Clear();
        }

        [Test]
        public void RegisterAndResolveCustomDrawer()
        {
            var drawer = new NoopDrawer();

            LensEntryDrawerRegistry.Register("sample", drawer);

            Assert.IsTrue(LensEntryDrawerRegistry.TryGet("sample", out var resolved));
            Assert.AreSame(drawer, resolved);
        }

        [Test]
        public void MissingCustomDrawerFailsClearly()
        {
            Assert.Throws<System.InvalidOperationException>(() => LensEntryDrawerRegistry.GetRequired("missing"));
        }

        [Test]
        public void CustomDrawerExceptionsBubble()
        {
            LensEntryDrawerRegistry.Register("throwing", new ThrowingDrawer());

            var entry = LensEntry.Custom("Custom", "throwing", null, _ => "Display");
            var drawer = new LensEntryDrawer();

            Assert.Throws<System.InvalidOperationException>(() => drawer.Draw(entry, "Custom", new LensConsoleState(), new LensGuiStyles()));
        }

        private sealed class NoopDrawer : ILensEntryDrawer
        {
            public void Draw(LensEntryDrawContext context, LensEntry entry)
            {
            }
        }

        private sealed class ThrowingDrawer : ILensEntryDrawer
        {
            public void Draw(LensEntryDrawContext context, LensEntry entry)
            {
                throw new System.InvalidOperationException("Drawer failed.");
            }
        }
    }
}
