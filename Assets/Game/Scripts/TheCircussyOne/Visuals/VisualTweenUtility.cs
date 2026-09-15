using PrimeTween;
using UnityEngine;
using TheCircussyOne.Rules;

namespace TheCircussyOne.Visuals
{
    internal static class VisualTweenUtility
    {
        public static Tween Scale(Transform target, Vector3 endValue, float duration, EaseSettings ease, int cycles = 1, CycleMode cycleMode = CycleMode.Restart)
        {
            Vector3 startValue = target.localScale;
            return Tween.Custom(
                target,
                0f,
                1f,
                Mathf.Max(0f, duration),
                (transform, rawProgress) =>
                {
                    float progress = GameEasing.Evaluate01(ease, rawProgress);
                    transform.localScale = Vector3.LerpUnclamped(startValue, endValue, progress);
                },
                ease: Ease.Linear,
                cycles: cycles,
                cycleMode: cycleMode);
        }

        public static Tween LocalPositionY(Transform target, float endValue, float duration, EaseSettings ease, int cycles = 1, CycleMode cycleMode = CycleMode.Restart)
        {
            float startValue = target.localPosition.y;
            return Tween.Custom(
                target,
                0f,
                1f,
                Mathf.Max(0f, duration),
                (transform, rawProgress) =>
                {
                    float progress = GameEasing.Evaluate01(ease, rawProgress);
                    Vector3 position = transform.localPosition;
                    position.y = Mathf.LerpUnclamped(startValue, endValue, progress);
                    transform.localPosition = position;
                },
                ease: Ease.Linear,
                cycles: cycles,
                cycleMode: cycleMode);
        }
    }
}
