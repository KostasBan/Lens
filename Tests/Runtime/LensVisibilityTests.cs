using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace KostasBan.Lens.Tests
{
    public sealed class LensVisibilityTests
    {
        LensRuntimeConsole console;
        readonly List<bool> changes = new List<bool>();
        [SetUp] public void Setup()
        {
            LensRuntimePolicy.SetAllowed(true);
            console = new GameObject("Synthetic console").AddComponent<LensRuntimeConsole>();
            changes.Clear();console.VisibilityChanged += changes.Add;
        }
        [TearDown] public void Cleanup()
        { if(console)Object.DestroyImmediate(console.gameObject);LensRuntimePolicy.ResetToDefault(); }
        [Test] public void OpenCloseAndToggleNotifyOnlyOnTransitions()
        {
            console.Open();console.Open();console.Close();console.Close();console.Toggle();console.Toggle();
            Assert.That(changes,Is.EqualTo(new[]{true,false,true,false}));
        }
        [Test] public void DisableClosesImmediatelyAndCannotReopenUntilEnabled()
        {
            console.Open();console.enabled=false;console.Open();console.Toggle();
            Assert.That(console.IsOpen,Is.False);Assert.That(changes,Is.EqualTo(new[]{true,false}));
            console.enabled=true;console.Open();Assert.That(console.IsOpen,Is.True);
        }
        [Test] public void DestroyNotifiesClosureExactlyOnce()
        {
            console.Open();Object.DestroyImmediate(console.gameObject);
            Assert.That(changes,Is.EqualTo(new[]{true,false}));
        }
        [Test] public void DenialClosesSynchronouslyAndBlocksActivation()
        {
            console.Open();LensRuntimePolicy.SetAllowed(false);console.Open();console.Toggle();
            Assert.That(console.IsOpen,Is.False);Assert.That(changes,Is.EqualTo(new[]{true,false}));
            LensRuntimePolicy.SetAllowed(true);Assert.That(console.IsOpen,Is.False);
        }
        [Test] public void HostCanOwnActivationAndDisableNativeSharing()
        {
            console.ToggleKey=KeyCode.None;console.ShowFloatingButton=false;console.AllowNativeShare=false;
            Assert.That(console.ToggleKey,Is.EqualTo(KeyCode.None));Assert.That(console.ShowFloatingButton,Is.False);
            Assert.That(console.AllowNativeShare,Is.False);console.Open();Assert.That(console.IsOpen,Is.True);
        }
    }
}
