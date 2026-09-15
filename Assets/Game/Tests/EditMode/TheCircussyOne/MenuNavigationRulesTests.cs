using NUnit.Framework;
using TheCircussyOne.Rules;

public sealed class MenuNavigationRulesTests
{
    [Test]
    public void DirectionFromAxisRequiresConfiguredThreshold()
    {
        Assert.That(MenuNavigationRules.DirectionFromAxis(0.69f, 0.7f), Is.EqualTo(0));
        Assert.That(MenuNavigationRules.DirectionFromAxis(0.71f, 0.7f), Is.EqualTo(1));
        Assert.That(MenuNavigationRules.DirectionFromAxis(-0.71f, 0.7f), Is.EqualTo(-1));
    }

    [Test]
    public void HeldDirectionWaitsBeforeFirstRepeat()
    {
        var state = new MenuNavigationRepeatState();

        Assert.That(MenuNavigationRules.ShouldTrigger(ref state, 1, 0f, 0.34f, 0.24f), Is.True);
        Assert.That(MenuNavigationRules.ShouldTrigger(ref state, 1, 0.2f, 0.34f, 0.24f), Is.False);
        Assert.That(MenuNavigationRules.ShouldTrigger(ref state, 1, 0.34f, 0.34f, 0.24f), Is.True);
        Assert.That(MenuNavigationRules.ShouldTrigger(ref state, 1, 0.45f, 0.34f, 0.24f), Is.False);
        Assert.That(MenuNavigationRules.ShouldTrigger(ref state, 1, 0.58f, 0.34f, 0.24f), Is.True);
    }

    [Test]
    public void DirectionChangeTriggersImmediately()
    {
        var state = new MenuNavigationRepeatState();

        Assert.That(MenuNavigationRules.ShouldTrigger(ref state, 1, 1f), Is.True);
        Assert.That(MenuNavigationRules.ShouldTrigger(ref state, -1, 1.1f), Is.True);
        Assert.That(state.HeldDirection, Is.EqualTo(-1));
    }

    [Test]
    public void NeutralDirectionResetsHeldState()
    {
        var state = new MenuNavigationRepeatState();

        Assert.That(MenuNavigationRules.ShouldTrigger(ref state, 1, 1f), Is.True);
        Assert.That(MenuNavigationRules.ShouldTrigger(ref state, 0, 1.1f), Is.False);

        Assert.That(state.HeldDirection, Is.EqualTo(0));
        Assert.That(MenuNavigationRules.ShouldTrigger(ref state, 1, 1.2f), Is.True);
    }

    [Test]
    public void RequiresNeutralReleaseBlocksHeldInputUntilNeutral()
    {
        var state = new MenuNavigationRepeatState();

        MenuNavigationRules.RequireNeutralRelease(ref state);

        Assert.That(MenuNavigationRules.ShouldTrigger(ref state, 1, 1f), Is.False);
        Assert.That(state.RequiresNeutralRelease, Is.True);

        Assert.That(MenuNavigationRules.ShouldTrigger(ref state, 0, 1.1f), Is.False);
        Assert.That(state.RequiresNeutralRelease, Is.False);
        Assert.That(MenuNavigationRules.ShouldTrigger(ref state, 1, 1.2f), Is.True);
    }

    [Test]
    public void NextClampedIndexMovesWithoutWrapping()
    {
        Assert.That(MenuNavigationRules.NextClampedIndex(0, 3, 1), Is.EqualTo(1));
        Assert.That(MenuNavigationRules.NextClampedIndex(1, 3, -1), Is.EqualTo(0));
        Assert.That(MenuNavigationRules.NextClampedIndex(0, 3, -1), Is.EqualTo(0));
        Assert.That(MenuNavigationRules.NextClampedIndex(2, 3, 1), Is.EqualTo(2));
    }
}
