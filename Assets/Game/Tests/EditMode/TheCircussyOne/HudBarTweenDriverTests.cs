using NUnit.Framework;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;
using TheCircussyOne.Visuals;

namespace TheCircussyOne.Tests
{
    public sealed class HudBarTweenDriverTests
    {
        [Test]
        public void StartCreatesTweenFromClampedValues()
        {
            var state = new HudBarTweenState();

            bool started = HudBarTweenDriver.Start(ref state, -0.5f, 1.5f, new HudBarTweenSettings(true, 0.3f, EaseSettings.Linear));

            Assert.That(started, Is.True);
            Assert.That(state.Active, Is.True);
            Assert.That(state.From, Is.Zero);
            Assert.That(state.To, Is.EqualTo(1f));
            Assert.That(state.Duration, Is.EqualTo(0.3f).Within(0.001f));
        }

        [Test]
        public void StartReturnsFalseForDisabledOrUnchangedValues()
        {
            var state = new HudBarTweenState { Active = true };

            bool disabled = HudBarTweenDriver.Start(ref state, 0f, 1f, new HudBarTweenSettings(false, 0.3f, EaseSettings.Linear));
            Assert.That(disabled, Is.False);
            Assert.That(state.Active, Is.False);

            bool unchanged = HudBarTweenDriver.Start(ref state, 0.5f, 0.5f, new HudBarTweenSettings(true, 0.3f, EaseSettings.Linear));
            Assert.That(unchanged, Is.False);
            Assert.That(state.Active, Is.False);
        }

        [Test]
        public void TickAppliesInterpolatedValueAndCompletesAtDuration()
        {
            var state = new HudBarTweenState();
            float applied = -1f;
            HudBarTweenDriver.Start(ref state, 0f, 1f, new HudBarTweenSettings(true, 0.2f, EaseSettings.Linear));

            HudBarTweenDriver.Tick(ref state, value => applied = value, 0.1f);

            Assert.That(state.Active, Is.True);
            Assert.That(applied, Is.EqualTo(0.5f).Within(0.001f));

            HudBarTweenDriver.Tick(ref state, value => applied = value, 0.1f);

            Assert.That(state.Active, Is.False);
            Assert.That(applied, Is.EqualTo(1f).Within(0.001f));
        }
    }
}
