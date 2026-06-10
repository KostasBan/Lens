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
            Assert.AreEqual("120", entry.DisplayValue);
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
        public void SensitiveEntryKeepsRawValueButDisplaysRedactedValue()
        {
            var entry = new LensEntry("Token", "secret-token", true);

            Assert.IsTrue(entry.IsSensitive);
            Assert.AreEqual("secret-token", entry.Value);
            Assert.AreEqual(LensEntry.DefaultRedactedValue, entry.DisplayValue);
        }

        [Test]
        public void SensitiveMutableEntryDisplaysRedactedValue()
        {
            var value = "secret-token";
            var entry = LensEntry.Text("Token", () => value, next => value = next, true, "[hidden]");

            Assert.AreEqual("secret-token", entry.Value);
            Assert.AreEqual("[hidden]", entry.DisplayValue);
        }

        [Test]
        public void ButtonEntryCanRequireConfirmation()
        {
            var entry = LensEntry.Button("Unlock Content", () => { }, true, "Unlock everything?");

            Assert.IsTrue(entry.RequiresConfirmation);
            Assert.AreEqual("Unlock everything?", entry.ConfirmationMessage);
        }

        [Test]
        public void ButtonActionExceptionsBubble()
        {
            var entry = LensEntry.Button("Fail", () => throw new InvalidOperationException("Action failed."));

            Assert.Throws<InvalidOperationException>(() => entry.ExecuteAction());
        }

        [Test]
        public void InfoTextIsOptionalMetadata()
        {
            var entry = LensEntry.ReadOnly("Flag", "Enabled", "Explains the flag.");

            Assert.IsTrue(entry.HasInfo);
            Assert.AreEqual("Explains the flag.", entry.InfoText);
        }

        [Test]
        public void SliderClampsAndSnapsValues()
        {
            var value = 42f;
            var entry = LensEntry.Slider("Rollout", () => value, next => value = next, 0f, 100f, 5f, "0");

            entry.SetSliderValue(43f);

            Assert.AreEqual(45f, value);
            Assert.AreEqual("45", entry.Value);
        }

        [Test]
        public void SingleSelectUsesOptionLabelsAndCommitsSelection()
        {
            var value = "dev";
            var options = new[]
            {
                new LensOption<string>("dev", "Development"),
                new LensOption<string>("stage", "Staging")
            };
            var entry = LensEntry.SingleSelect("Environment", () => value, next => value = next, options);

            Assert.AreEqual("Development", entry.Value);

            entry.SetSingleOption(1);

            Assert.AreEqual("stage", value);
            Assert.AreEqual("Staging", entry.Value);
        }

        [Test]
        public void MultiSelectFormatsSelectionState()
        {
            var values = new System.Collections.Generic.List<string>();
            var options = new[]
            {
                new LensOption<string>("coins", "Coins"),
                new LensOption<string>("gems", "Gems"),
                new LensOption<string>("boosters", "Boosters")
            };
            var entry = LensEntry.MultiSelect("Rewards", () => values, next =>
            {
                values.Clear();
                values.AddRange(next);
            }, options);

            Assert.AreEqual("None", entry.Value);

            entry.SetMultiOptions(new[] { true, false, false });
            Assert.AreEqual("Coins", entry.Value);

            entry.SetMultiOptions(new[] { true, true, false });
            Assert.AreEqual("Mixed...", entry.Value);

            entry.SetMultiOptions(new[] { true, true, true });
            Assert.AreEqual("Everything", entry.Value);
        }

        [Test]
        public void ProgressFormatsCurrentMaxAndPercent()
        {
            var entry = LensEntry.Progress("Download", () => 50f, () => 100f, "Catalog");

            Assert.AreEqual(0.5f, entry.GetProgressRatio());
            StringAssert.Contains("Catalog: 50 / 100 (50%)", entry.Value);
        }

        [Test]
        public void CustomEntryUsesCallbacks()
        {
            var entry = LensEntry.Custom("Custom", "sample", 12, payload => $"Display {payload}", payload => $"Search {payload}", payload => $"Report {payload}");

            Assert.AreEqual(LensEntryKind.Custom, entry.Kind);
            Assert.AreEqual("sample", entry.CustomTypeId);
            Assert.AreEqual("Display 12", entry.DisplayValue);
            Assert.AreEqual("Search 12", entry.SearchText);
            Assert.AreEqual("Report 12", entry.ReportValue);
        }

        [Test]
        public void FactoryMethodsRejectMissingCallbacks()
        {
            Assert.Throws<ArgumentNullException>(() => LensEntry.Toggle("Flag", null, _ => { }));
            Assert.Throws<ArgumentNullException>(() => LensEntry.Text("Text", () => string.Empty, null));
            Assert.Throws<ArgumentNullException>(() => LensEntry.Number("Number", () => 0f, null));
            Assert.Throws<ArgumentNullException>(() => LensEntry.Button("Action", null));
            Assert.Throws<ArgumentNullException>(() => LensEntry.Slider("Slider", null, _ => { }, 0f, 1f));
            Assert.Throws<ArgumentNullException>(() => LensEntry.SingleSelect("Select", null, _ => { }, Array.Empty<LensOption<string>>()));
            Assert.Throws<ArgumentNullException>(() => LensEntry.MultiSelect<string>("Multi", null, _ => { }, Array.Empty<LensOption<string>>()));
            Assert.Throws<ArgumentNullException>(() => LensEntry.Progress("Progress", null, () => 1f));
            Assert.Throws<ArgumentException>(() => LensEntry.Custom("Custom", string.Empty, null, _ => string.Empty));
        }
    }
}
