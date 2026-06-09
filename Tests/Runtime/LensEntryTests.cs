using System;
using NUnit.Framework;

namespace KostasBan.Lens.Tests
{
    public sealed class LensEntryTests
    {
        [Test]
        public void ReadOnlyEntryPreservesExistingConstructor()
        {
            var entry = new LensEntry("Coins", "120");

            Assert.AreEqual("Coins", entry.Key);
            Assert.AreEqual("120", entry.Value);
            Assert.AreEqual(LensEntryKind.ReadOnly, entry.Kind);
        }

        [Test]
        public void ToggleEntryReadsAndWritesThroughCallbacks()
        {
            var value = false;
            var entry = LensEntry.Toggle("Flag", () => value, next => value = next);

            entry.SetBoolValue(true);

            Assert.IsTrue(value);
            Assert.AreEqual("True", entry.Value);
        }

        [Test]
        public void TextEntryReadsAndWritesThroughCallbacks()
        {
            var value = "Development";
            var entry = LensEntry.Text("Environment", () => value, next => value = next);

            entry.SetTextValue("Staging");

            Assert.AreEqual("Staging", value);
            Assert.AreEqual("Staging", entry.Value);
        }

        [Test]
        public void NumberEntryFormatsInvariantValue()
        {
            var value = 1.25f;
            var entry = LensEntry.Number("Multiplier", () => value, next => value = next);

            entry.SetNumberValue(2.5f);

            Assert.AreEqual(2.5f, value);
            Assert.AreEqual("2.5", entry.Value);
        }

        [Test]
        public void ButtonEntryExecutesActionOnlyWhenInvoked()
        {
            var invoked = false;
            var entry = LensEntry.Button("Unlock Content", () => invoked = true);

            Assert.AreEqual("Unlock Content", entry.Value);
            Assert.IsFalse(invoked);

            entry.ExecuteAction();

            Assert.IsTrue(invoked);
        }

        [Test]
        public void FactoryMethodsRejectMissingCallbacks()
        {
            Assert.Throws<ArgumentNullException>(() => LensEntry.Toggle("Flag", null, _ => { }));
            Assert.Throws<ArgumentNullException>(() => LensEntry.Text("Text", () => string.Empty, null));
            Assert.Throws<ArgumentNullException>(() => LensEntry.Number("Number", () => 0f, null));
            Assert.Throws<ArgumentNullException>(() => LensEntry.Button("Action", null));
        }
    }
}
