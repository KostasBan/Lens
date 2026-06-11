using NUnit.Framework;

namespace KostasBan.Lens.Tests
{
    public sealed class LensConsoleStateTests
    {
        [Test]
        public void OpenCloseAndToggleUpdateVisibility()
        {
            var state = new LensConsoleState();

            state.Open();
            Assert.IsTrue(state.IsOpen);

            state.Toggle();
            Assert.IsFalse(state.IsOpen);

            state.Close();
            Assert.IsFalse(state.IsOpen);
        }

        [Test]
        public void SectionsStartExpandedAndCanToggle()
        {
            var state = new LensConsoleState();

            Assert.IsTrue(state.IsSectionExpanded("Build"));

            state.ToggleSection("Build");

            Assert.IsFalse(state.IsSectionExpanded("Build"));
        }

        [Test]
        public void SearchDetectsNonEmptyText()
        {
            var state = new LensConsoleState();

            Assert.IsFalse(state.HasSearch);

            state.SearchText = "flags";

            Assert.IsTrue(state.HasSearch);
        }

        [Test]
        public void NumberDraftPersistsUserText()
        {
            var state = new LensConsoleState();

            state.SetNumberDraft("Feature/Multiplier", "abc");

            Assert.AreEqual("abc", state.GetNumberDraft("Feature/Multiplier", 1.25f));
        }

        [Test]
        public void ActionConfirmationCanBeRequestedAndCleared()
        {
            var state = new LensConsoleState();

            state.RequestActionConfirmation("Actions/Unlock");

            Assert.IsTrue(state.IsActionConfirmationPending("Actions/Unlock"));

            state.ClearActionConfirmation();

            Assert.IsFalse(state.IsActionConfirmationPending("Actions/Unlock"));
        }

        [Test]
        public void MultiSelectDraftReusesArrayWhenLengthMatches()
        {
            var state = new LensConsoleState();
            var first = state.GetMultiSelectDraft("Flags/Options", new[] { true, false, true });

            state.ResetMultiSelectDraft("Flags/Options", new[] { false, true, false });
            var second = state.GetMultiSelectDraft("Flags/Options", new[] { true, true, true });

            Assert.AreSame(first, second);
            CollectionAssert.AreEqual(new[] { false, true, false }, second);
        }
    }
}
