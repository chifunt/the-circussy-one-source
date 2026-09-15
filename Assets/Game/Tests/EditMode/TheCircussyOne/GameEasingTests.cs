using NUnit.Framework;
using TheCircussyOne.Rules;

public sealed class GameEasingTests
{
    [Test]
    public void PresetsClampAndPreserveEndpoints()
    {
        foreach (EasePreset preset in System.Enum.GetValues(typeof(EasePreset)))
        {
            var settings = new EaseSettings(preset);

            Assert.That(GameEasing.Evaluate01(settings, -1f), Is.EqualTo(0f).Within(0.0001f), preset.ToString());
            Assert.That(GameEasing.Evaluate01(settings, 1f), Is.EqualTo(1f).Within(0.0001f), preset.ToString());
            Assert.That(GameEasing.Evaluate01(settings, 2f), Is.EqualTo(1f).Within(0.0001f), preset.ToString());
        }
    }

    [Test]
    public void ShapeChangesNonLinearCurve()
    {
        float soft = GameEasing.Evaluate01(new EaseSettings(EasePreset.OutCubic, 0.5f), 0.5f);
        float sharp = GameEasing.Evaluate01(new EaseSettings(EasePreset.OutCubic, 2f), 0.5f);

        Assert.That(soft, Is.Not.EqualTo(sharp).Within(0.0001f));
    }

    [Test]
    public void ExponentialSmoothingWithDefaultShapeMatchesPreviousFormula()
    {
        float sharpness = 9f;
        float deltaTime = 0.016f;

        float expected = 1f - UnityEngine.Mathf.Exp(-sharpness * deltaTime);
        float actual = GameEasing.SmoothingWeight(sharpness, deltaTime, EaseSettings.Exponential);

        Assert.That(actual, Is.EqualTo(expected).Within(0.0001f));
    }
}
