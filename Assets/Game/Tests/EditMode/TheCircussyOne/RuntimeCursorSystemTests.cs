using System.Reflection;
using NUnit.Framework;
using TheCircussyOne.Content;
using TheCircussyOne.Rules;
using TheCircussyOne.Runtime;
using TheCircussyOne.Visuals;
using UnityEngine;
using UnityEngine.UIElements;

public sealed class RuntimeCursorSystemTests
{
    [Test]
    public void CursorRulesLockAndHideDuringGameplay()
    {
        RuntimeCursorPolicy policy = RuntimeCursorRules.Evaluate(menuOpen: false, ControlInputMode.MouseKeyboard);

        Assert.That(policy.LockMode, Is.EqualTo(CursorLockMode.Locked));
        Assert.That(policy.Visible, Is.False);
        Assert.That(policy.TransparentCursor, Is.True);
    }

    [Test]
    public void CursorRulesUnlockAndShowForMouseKeyboardMenus()
    {
        RuntimeCursorPolicy policy = RuntimeCursorRules.Evaluate(menuOpen: true, ControlInputMode.MouseKeyboard);

        Assert.That(policy.LockMode, Is.EqualTo(CursorLockMode.None));
        Assert.That(policy.Visible, Is.True);
        Assert.That(policy.TransparentCursor, Is.False);
    }

    [Test]
    public void CursorRulesUnlockAndHideForGamepadMenus()
    {
        RuntimeCursorPolicy policy = RuntimeCursorRules.Evaluate(menuOpen: true, ControlInputMode.Gamepad);

        Assert.That(policy.LockMode, Is.EqualTo(CursorLockMode.None));
        Assert.That(policy.Visible, Is.False);
        Assert.That(policy.TransparentCursor, Is.True);
    }

    [Test]
    public void InputModeRulesPreferMouseKeyboardWhenPointerOrKeyboardIsActive()
    {
        ControlInputMode mode = ControlInputModeRules.Resolve(
            ControlInputMode.Gamepad,
            new ControlInputActivity(mouseKeyboardActive: true, gamepadActive: true));

        Assert.That(mode, Is.EqualTo(ControlInputMode.MouseKeyboard));
    }

    [Test]
    public void InputModeRulesSwitchToGamepadWhenOnlyGamepadIsActive()
    {
        ControlInputMode mode = ControlInputModeRules.Resolve(
            ControlInputMode.MouseKeyboard,
            new ControlInputActivity(mouseKeyboardActive: false, gamepadActive: true));

        Assert.That(mode, Is.EqualTo(ControlInputMode.Gamepad));
    }

    [Test]
    public void InputModeRulesKeepCurrentModeWithoutActivity()
    {
        ControlInputMode mode = ControlInputModeRules.Resolve(
            ControlInputMode.Gamepad,
            new ControlInputActivity(mouseKeyboardActive: false, gamepadActive: false));

        Assert.That(mode, Is.EqualTo(ControlInputMode.Gamepad));
    }

    [Test]
    public void CursorLockBehaviourDoesNotForceLockEveryFrame()
    {
        BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        Assert.That(typeof(CursorLockBehaviour).GetMethod("Update", flags), Is.Null);
        Assert.That(typeof(CursorLockBehaviour).GetMethod("LateUpdate", flags), Is.Null);
        Assert.That(typeof(CursorLockBehaviour).GetMethod("OnGUI", flags), Is.Null);
    }

    [Test]
    public void RuntimeCursorTreatsRewardRevealAsMenu()
    {
        var gameObject = new GameObject("TCO Test Reward Reveal Cursor View");
        RewardRevealView view = gameObject.AddComponent<RewardRevealView>();
        view.ConfigureForTests(new VisualElement(), new VisualElement(), new VisualElement(), new Label(), new Label(), new Label(), new Label(), new Label(), new Button());
        ItemDefinition item = TheCircussyOneTestObjects.CreateItemDefinition("rubber_soles", "Rubber Soles");
        view.Show(RewardRevealFrame.ForItem(item, 1, 1));
        var system = new RuntimeCursorSystem(new ControlInputModeTracker(), null, rewardReveal: view);

        MethodInfo method = typeof(RuntimeCursorSystem).GetMethod("IsMenuOpen", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(method, Is.Not.Null);

        Assert.That((bool)method.Invoke(system, null), Is.True);

        TheCircussyOneTestObjects.Destroy(item);
        Object.DestroyImmediate(gameObject);
    }
}
