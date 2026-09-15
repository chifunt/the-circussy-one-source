using NUnit.Framework;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;
using UnityEngine;

public sealed class ArenaBarrierVisualRulesTests
{
    [Test]
    public void RadiusAppliesConfiguredPadding()
    {
        RunWorldGenerationConfig config = RunWorldGenerationConfig.CreateRuntimeDefault();
        config.arenaBarrierRadiusPadding = 3.5f;

        Assert.That(ArenaBarrierVisualRules.Radius(24f, config), Is.EqualTo(27.5f).Within(0.0001f));

        Object.DestroyImmediate(config);
    }

    [Test]
    public void DynamicRingsMoveUpwardAndWrap()
    {
        RunWorldGenerationConfig config = RunWorldGenerationConfig.CreateRuntimeDefault();
        config.arenaBarrierHeight = 10f;
        config.arenaBarrierDynamicRingSpeed = 3f;
        config.arenaBarrierDynamicRingFadeEase = EaseSettings.Linear;

        ArenaBarrierDynamicRingFrame start = ArenaBarrierVisualRules.DynamicRingFrame(0, 2, 0f, config);
        ArenaBarrierDynamicRingFrame moved = ArenaBarrierVisualRules.DynamicRingFrame(0, 2, 1f, config);
        ArenaBarrierDynamicRingFrame wrapped = ArenaBarrierVisualRules.DynamicRingFrame(0, 2, 4f, config);

        Assert.That(start.Height, Is.EqualTo(0f).Within(0.0001f));
        Assert.That(moved.Height, Is.EqualTo(3f).Within(0.0001f));
        Assert.That(wrapped.Height, Is.EqualTo(2f).Within(0.0001f));

        Object.DestroyImmediate(config);
    }

    [Test]
    public void DynamicRingAlphaFadesWithHeight()
    {
        RunWorldGenerationConfig config = RunWorldGenerationConfig.CreateRuntimeDefault();
        config.arenaBarrierHeight = 10f;
        config.arenaBarrierDynamicRingSpeed = 5f;
        config.arenaBarrierDynamicRingFadeEase = EaseSettings.Linear;
        config.arenaBarrierDynamicRingColor = new Color(1f, 1f, 1f, 0.4f);

        ArenaBarrierDynamicRingFrame bottom = ArenaBarrierVisualRules.DynamicRingFrame(0, 1, 0f, config);
        ArenaBarrierDynamicRingFrame middle = ArenaBarrierVisualRules.DynamicRingFrame(0, 1, 1f, config);

        Assert.That(bottom.Color.a, Is.EqualTo(0.4f).Within(0.0001f));
        Assert.That(middle.Color.a, Is.EqualTo(0.2f).Within(0.0001f));

        Object.DestroyImmediate(config);
    }
}
