using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheCircussyOne.Visuals
{
    internal sealed class UiCelebrationEffectController
    {
        private const int RayCount = 16;
        private const int ConfettiCount = 120;
        private const float FallbackWidth = 1920f;
        private const float FallbackHeight = 1080f;

        private static readonly Color[] ConfettiColors =
        {
            new(1f, 0.17f, 0.13f, 1f),
            new(1f, 0.88f, 0.25f, 1f),
            new(0.1f, 0.78f, 1f, 1f),
            new(0.21f, 0.95f, 0.34f, 1f),
            new(0.82f, 0.28f, 1f, 1f),
            new(1f, 0.96f, 0.72f, 1f),
            new(1f, 0.44f, 0.16f, 1f)
        };

        private readonly List<RayRecord> rays = new();
        private readonly List<ConfettiRecord> confetti = new();
        private readonly System.Random random = new(23917);

        private VisualElement rayLayer;
        private VisualElement confettiLayer;
        private bool active;
        private int showCount;

        public void Bind(VisualElement rayLayer, VisualElement confettiLayer = null)
        {
            if (this.rayLayer == rayLayer
                && this.confettiLayer == confettiLayer)
            {
                return;
            }

            this.rayLayer = rayLayer;
            this.confettiLayer = confettiLayer;

            EnsureRays();
            EnsureConfetti();
            if (!active)
            {
                HideImmediate();
            }
        }

        public void Show()
        {
            active = true;
            showCount++;
            SetVisible(rayLayer, true);
            SetVisible(confettiLayer, true);
            ResetRays();
            ResetConfetti();
            Tick(0f, Time.unscaledTime);
        }

        public void HideImmediate()
        {
            active = false;
            SetVisible(rayLayer, false);
            SetVisible(confettiLayer, false);
        }

        public void Tick(float deltaTime, float unscaledTime)
        {
            if (!active)
            {
                return;
            }

            TickRays(unscaledTime);
            TickConfetti(unscaledTime);
        }

        private void EnsureRays()
        {
            if (rayLayer == null)
            {
                return;
            }

            rayLayer.Clear();
            rays.Clear();
            for (int i = 0; i < RayCount; i++)
            {
                var ray = new VisualElement { name = $"upgrade-ray-{i}", pickingMode = PickingMode.Ignore };
                ray.AddToClassList("upgrade-ray");
                ray.AddToClassList("ui-ray");
                rayLayer.Add(ray);
                rays.Add(new RayRecord(ray));
            }
        }

        private void EnsureConfetti()
        {
            if (confettiLayer == null)
            {
                return;
            }

            confettiLayer.Clear();
            confetti.Clear();
            for (int i = 0; i < ConfettiCount; i++)
            {
                var piece = new VisualElement { name = $"upgrade-confetti-{i}", pickingMode = PickingMode.Ignore };
                piece.AddToClassList("upgrade-confetti-piece");
                piece.AddToClassList("ui-confetti-piece");
                confettiLayer.Add(piece);
                confetti.Add(new ConfettiRecord(piece));
            }
        }

        private void ResetRays()
        {
            for (int i = 0; i < rays.Count; i++)
            {
                RayRecord record = rays[i];
                record.Angle = (360f / RayCount) * i + Range(-7f, 7f);
                record.LengthScale = Range(0.9f, 1.35f);
                record.Thickness = Range(42f, 116f);
                record.Alpha = Range(0.055f, 0.135f);
                record.Speed = Range(-4f, 4f);
                record.Phase = Range(0f, Mathf.PI * 2f);
            }
        }

        private void ResetConfetti()
        {
            for (int i = 0; i < confetti.Count; i++)
            {
                ConfettiRecord record = confetti[i];
                record.X01 = Range(0.02f, 0.98f);
                record.Delay = Range(0f, 1f);
                record.Speed = Range(0.12f, 0.36f);
                record.Sway = Range(10f, 72f);
                record.SwaySpeed = Range(1.4f, 3.9f);
                record.Rotation = Range(0f, 360f);
                record.RotationSpeed = Range(-110f, 110f);
                record.Width = Range(5f, 12f);
                record.Height = Range(8f, 21f);
                record.Color = ConfettiColors[(i + showCount) % ConfettiColors.Length];
                record.Phase = Range(0f, Mathf.PI * 2f);
                ApplyRadius(record.Element, Range(0f, 1f) > 0.72f ? Mathf.Max(record.Width, record.Height) : 2f);
            }
        }

        private void TickRays(float unscaledTime)
        {
            float width = ResolveSize(rayLayer, axis: 0, FallbackWidth);
            float height = ResolveSize(rayLayer, axis: 1, FallbackHeight);
            float length = Mathf.Max(width, height) * 1.28f;
            float centerX = width * 0.5f;
            float centerY = height * 0.5f;

            for (int i = 0; i < rays.Count; i++)
            {
                RayRecord record = rays[i];
                float wave = 0.5f + 0.5f * Mathf.Sin(unscaledTime * 1.35f + record.Phase);
                float thickness = record.Thickness * Mathf.Lerp(0.72f, 1.1f, wave);
                float angle = record.Angle + unscaledTime * record.Speed;
                Color color = new(1f, 0.74f, 0.18f, record.Alpha * Mathf.Lerp(0.6f, 1.25f, wave));

                record.Element.style.left = centerX - length * 0.5f;
                record.Element.style.top = centerY - thickness * 0.5f;
                record.Element.style.width = length * record.LengthScale;
                record.Element.style.height = thickness;
                record.Element.style.rotate = new Rotate(angle);
                record.Element.style.backgroundColor = color;
                record.Element.style.opacity = 1f;
            }
        }

        private void TickConfetti(float unscaledTime)
        {
            float width = ResolveSize(confettiLayer, axis: 0, FallbackWidth);
            float height = ResolveSize(confettiLayer, axis: 1, FallbackHeight);
            float fallDistance = height + 140f;

            for (int i = 0; i < confetti.Count; i++)
            {
                ConfettiRecord record = confetti[i];
                float progress = Mathf.Repeat(unscaledTime * record.Speed + record.Delay, 1f);
                float eased = progress * progress * (3f - 2f * progress);
                float x = record.X01 * width + Mathf.Sin(unscaledTime * record.SwaySpeed + record.Phase) * record.Sway;
                float y = -70f + eased * fallDistance;
                float fadeIn = Mathf.Clamp01(progress * 8f);
                float fadeOut = Mathf.Clamp01((1f - progress) * 5f);
                float alpha = Mathf.Min(fadeIn, fadeOut);

                Color color = record.Color;
                color.a = alpha * 0.95f;
                record.Element.style.left = x;
                record.Element.style.top = y;
                record.Element.style.width = record.Width;
                record.Element.style.height = record.Height;
                record.Element.style.rotate = new Rotate(record.Rotation + unscaledTime * record.RotationSpeed);
                record.Element.style.backgroundColor = color;
                record.Element.style.opacity = alpha;
            }
        }

        private float Range(float min, float max)
        {
            return Mathf.Lerp(min, max, (float)random.NextDouble());
        }

        private static void SetVisible(VisualElement element, bool visible)
        {
            if (element != null)
            {
                element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private static float ResolveSize(VisualElement element, int axis, float fallback)
        {
            if (element == null)
            {
                return fallback;
            }

            float value = axis == 0 ? element.resolvedStyle.width : element.resolvedStyle.height;
            return float.IsNaN(value) || value <= 1f ? fallback : value;
        }

        private static void ApplyRadius(VisualElement element, float radius)
        {
            element.style.borderTopLeftRadius = radius;
            element.style.borderTopRightRadius = radius;
            element.style.borderBottomLeftRadius = radius;
            element.style.borderBottomRightRadius = radius;
        }

        private sealed class RayRecord
        {
            public readonly VisualElement Element;
            public float Angle;
            public float LengthScale;
            public float Thickness;
            public float Alpha;
            public float Speed;
            public float Phase;

            public RayRecord(VisualElement element)
            {
                Element = element;
            }
        }

        private sealed class ConfettiRecord
        {
            public readonly VisualElement Element;
            public float X01;
            public float Delay;
            public float Speed;
            public float Sway;
            public float SwaySpeed;
            public float Rotation;
            public float RotationSpeed;
            public float Width;
            public float Height;
            public Color Color;
            public float Phase;

            public ConfettiRecord(VisualElement element)
            {
                Element = element;
            }
        }
    }
}
