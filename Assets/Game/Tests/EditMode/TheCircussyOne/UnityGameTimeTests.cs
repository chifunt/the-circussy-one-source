using NUnit.Framework;
using TheCircussyOne.Runtime;

public sealed class UnityGameTimeTests
{
    [Test]
    public void TimeStartsAtZeroForEachRunInstance()
    {
        float now = 120f;
        var time = new UnityGameTime(() => now, () => 0.016f);

        Assert.That(time.Time, Is.EqualTo(0f).Within(0.0001f));

        now = 125.5f;

        Assert.That(time.Time, Is.EqualTo(5.5f).Within(0.0001f));

        var restarted = new UnityGameTime(() => now, () => 0.016f);

        Assert.That(restarted.Time, Is.EqualTo(0f).Within(0.0001f));
    }

    [Test]
    public void PauseTimeDoesNotAdvanceRunClock()
    {
        float now = 10f;
        var pauseState = new RunPauseState();
        var time = new UnityGameTime(() => now, () => 0.016f, pauseState);

        now = 14f;
        pauseState.Pause("test");
        now = 30f;

        Assert.That(time.Time, Is.EqualTo(4f).Within(0.0001f));
        Assert.That(time.DeltaTime, Is.Zero);

        pauseState.Resume("test");
        now = 35f;

        Assert.That(time.Time, Is.EqualTo(9f).Within(0.0001f));
    }
}
