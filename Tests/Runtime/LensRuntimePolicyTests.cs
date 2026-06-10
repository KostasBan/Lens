using NUnit.Framework;
using UnityEngine;

namespace KostasBan.Lens.Tests
{
    public sealed class LensRuntimePolicyTests
    {
        [TearDown]
        public void TearDown()
        {
            LensRuntimePolicy.ResetToDefault();
        }

        [Test]
        public void DefaultPolicyIsAllowedInEditorTests()
        {
            Assert.IsTrue(LensRuntimePolicy.DefaultIsAllowed);
            Assert.IsTrue(LensRuntimePolicy.IsAllowed);
        }

        [Test]
        public void ExplicitPolicyOverrideCanDisableAndReset()
        {
            LensRuntimePolicy.SetAllowed(false);

            Assert.IsFalse(LensRuntimePolicy.IsAllowed);

            LensRuntimePolicy.ResetToDefault();

            Assert.AreEqual(LensRuntimePolicy.DefaultIsAllowed, LensRuntimePolicy.IsAllowed);
        }

        [Test]
        public void DisabledPolicyPreventsConsoleOpenAndToggle()
        {
            var gameObject = new GameObject("Lens Policy Test");
            var console = gameObject.AddComponent<LensRuntimeConsole>();

            try
            {
                LensRuntimePolicy.SetAllowed(false);

                console.Open();
                Assert.IsFalse(console.IsOpen);

                console.Toggle();
                Assert.IsFalse(console.IsOpen);

                LensRuntimePolicy.SetAllowed(true);
                console.Open();
                Assert.IsTrue(console.IsOpen);

                LensRuntimePolicy.SetAllowed(false);
                console.Close();
                Assert.IsFalse(console.IsOpen);
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }
    }
}
