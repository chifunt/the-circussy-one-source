using System.Collections.Generic;
using TheCircussyOne.Rules;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheCircussyOne.Visuals
{
    [System.Serializable]
    public sealed class MarqueeButtonEffectSettings
    {
        public bool enabled = true;
        [Min(2f)] public float bulbSize = 4f;
        [Min(8f)] public float bulbSpacing = 23f;
        [Min(4f)] public float edgeInset = 15f;
        [Range(0f, 1f)] public float idleOpacity = 0.18f;
        [Range(0f, 1f)] public float activeOpacity = 0.86f;
        [Range(0f, 1f)] public float chaseOpacity = 1f;
        [Min(0f)] public float chaseCyclesPerSecond = 0.9f;
        [Range(0.04f, 0.45f)] public float chaseWidth = 0.22f;
        [Min(1f)] public float hoverScale = 1.045f;
        [Range(0.75f, 1f)] public float pressedScale = 0.96f;
        [Min(0.01f)] public float scaleSeconds = 0.16f;
        [Min(0.01f)] public float pressedSeconds = 0.08f;
        public bool pointerHoverActivates = true;
        public bool focusActivates = true;
        public EaseSettings scaleEase = EaseSettings.OutBack;
        public Color idleTint = new(0.8f, 0.48f, 0.16f, 0.85f);
        public Color activeTint = new(1f, 0.88f, 0.34f, 1f);
        public Color chaseTint = new(1f, 0.98f, 0.75f, 1f);
    }

    public sealed class MarqueeButtonEffectController : System.IDisposable
    {
        private const int MaxBulbsPerRail = 32;

        private readonly MarqueeButtonEffectSettings settings;
        private readonly List<Record> records = new();
        private readonly Dictionary<VisualElement, Record> recordsByTarget = new();

        public MarqueeButtonEffectController(MarqueeButtonEffectSettings settings)
        {
            this.settings = settings ?? new MarqueeButtonEffectSettings();
        }

        public void Register(VisualElement target)
        {
            if (!settings.enabled || target == null || recordsByTarget.ContainsKey(target))
            {
                return;
            }

            var record = new Record(target, settings);
            record.Root = CreateRoot();
            record.TopRail = CreateRail("top");
            record.RightRail = CreateRail("right");
            record.BottomRail = CreateRail("bottom");
            record.LeftRail = CreateRail("left");

            record.Root.Add(record.TopRail);
            record.Root.Add(record.RightRail);
            record.Root.Add(record.BottomRail);
            record.Root.Add(record.LeftRail);
            target.Add(record.Root);

            record.RegisterCallbacks();
            records.Add(record);
            recordsByTarget.Add(target, record);
        }

        public void Pulse(VisualElement target)
        {
            if (target != null && recordsByTarget.TryGetValue(target, out Record record))
            {
                record.PressedPulseRemaining = settings.pressedSeconds;
                SetScaleTarget(record, settings.pressedScale);
            }
        }

        public void Tick(float deltaTime, float unscaledTime)
        {
            if (!settings.enabled)
            {
                return;
            }

            for (int i = records.Count - 1; i >= 0; i--)
            {
                Record record = records[i];
                if (record.Target == null)
                {
                    records.RemoveAt(i);
                    continue;
                }

                ApplyRailLayout(record);
                EnsureBulbs(record);
                TickBulbs(record, unscaledTime);
                TickScale(record, deltaTime);
            }
        }

        public void Clear()
        {
            for (int i = 0; i < records.Count; i++)
            {
                Record record = records[i];
                record.UnregisterCallbacks();
                if (record.Target != null)
                {
                    record.Target.style.scale = new Scale(Vector3.one);
                }

                record.Root?.RemoveFromHierarchy();
            }

            records.Clear();
            recordsByTarget.Clear();
        }

        public void Dispose()
        {
            Clear();
        }

        private static VisualElement CreateRoot()
        {
            var root = new VisualElement { name = "marquee-effect-root", pickingMode = PickingMode.Ignore };
            root.AddToClassList("marquee-effect-root");
            return root;
        }

        private static VisualElement CreateRail(string side)
        {
            var rail = new VisualElement { name = $"marquee-bulb-rail-{side}", pickingMode = PickingMode.Ignore };
            rail.AddToClassList("marquee-bulb-rail");
            rail.AddToClassList($"marquee-bulb-rail--{side}");
            return rail;
        }

        private void ApplyRailLayout(Record record)
        {
            float bulbSize = Mathf.Max(3f, settings.bulbSize);
            float edgeInset = Mathf.Max(4f, settings.edgeInset);
            float edgePosition = Mathf.Max(1f, bulbSize * 0.5f);

            record.TopRail.style.left = edgeInset;
            record.TopRail.style.right = edgeInset;
            record.TopRail.style.top = edgePosition;
            record.TopRail.style.height = bulbSize;

            record.RightRail.style.top = edgeInset;
            record.RightRail.style.right = edgePosition;
            record.RightRail.style.bottom = edgeInset;
            record.RightRail.style.width = bulbSize;

            record.BottomRail.style.left = edgeInset;
            record.BottomRail.style.right = edgeInset;
            record.BottomRail.style.bottom = edgePosition;
            record.BottomRail.style.height = bulbSize;

            record.LeftRail.style.top = edgeInset;
            record.LeftRail.style.left = edgePosition;
            record.LeftRail.style.bottom = edgeInset;
            record.LeftRail.style.width = bulbSize;
        }

        private void EnsureBulbs(Record record)
        {
            float width = record.Target.resolvedStyle.width;
            float height = record.Target.resolvedStyle.height;
            if (float.IsNaN(width) || float.IsNaN(height) || width < 24f || height < 24f)
            {
                return;
            }

            int horizontalCount = ResolveBulbCount(width);
            int verticalCount = ResolveBulbCount(height);
            if (record.HorizontalBulbCount == horizontalCount && record.VerticalBulbCount == verticalCount)
            {
                ApplyBulbVisuals(record);
                return;
            }

            record.ClearBulbs();
            List<BulbVisual> topBulbs = AddBulbs(record.TopRail, horizontalCount);
            List<BulbVisual> rightBulbs = AddBulbs(record.RightRail, verticalCount);
            List<BulbVisual> bottomBulbs = AddBulbs(record.BottomRail, horizontalCount);
            List<BulbVisual> leftBulbs = AddBulbs(record.LeftRail, verticalCount);

            record.OrderedBulbs.AddRange(topBulbs);
            record.OrderedBulbs.AddRange(rightBulbs);
            for (int i = bottomBulbs.Count - 1; i >= 0; i--)
            {
                record.OrderedBulbs.Add(bottomBulbs[i]);
            }

            for (int i = leftBulbs.Count - 1; i >= 0; i--)
            {
                record.OrderedBulbs.Add(leftBulbs[i]);
            }

            record.HorizontalBulbCount = horizontalCount;
            record.VerticalBulbCount = verticalCount;
            ApplyBulbVisuals(record);
        }

        private int ResolveBulbCount(float edgeLength)
        {
            float usableLength = Mathf.Max(1f, edgeLength - settings.edgeInset * 2f);
            return Mathf.Clamp(Mathf.RoundToInt(usableLength / settings.bulbSpacing) + 1, 2, MaxBulbsPerRail);
        }

        private List<BulbVisual> AddBulbs(VisualElement rail, int count)
        {
            var bulbs = new List<BulbVisual>(count);
            for (int i = 0; i < count; i++)
            {
                var bulb = new VisualElement { pickingMode = PickingMode.Ignore };
                bulb.AddToClassList("marquee-bulb");
                rail.Add(bulb);
                bulbs.Add(new BulbVisual(bulb));
            }

            return bulbs;
        }

        private void ApplyBulbVisuals(Record record)
        {
            float bulbSize = Mathf.Max(3f, settings.bulbSize);
            for (int i = 0; i < record.OrderedBulbs.Count; i++)
            {
                BulbVisual bulb = record.OrderedBulbs[i];
                bulb.Root.style.width = bulbSize;
                bulb.Root.style.height = bulbSize;
                SetRadius(bulb.Root, bulbSize * 0.5f);
            }
        }

        private void TickBulbs(Record record, float unscaledTime)
        {
            int bulbCount = record.OrderedBulbs.Count;
            if (bulbCount == 0)
            {
                return;
            }

            bool active = record.IsActive;
            float head = Mathf.Repeat(unscaledTime * settings.chaseCyclesPerSecond, 1f);
            for (int i = 0; i < bulbCount; i++)
            {
                BulbVisual bulb = record.OrderedBulbs[i];
                float opacity = active ? settings.activeOpacity : settings.idleOpacity;
                Color tint = active ? settings.activeTint : settings.idleTint;
                if (active)
                {
                    float position = i / (float)bulbCount;
                    float distance = Mathf.Abs(position - head);
                    distance = Mathf.Min(distance, 1f - distance);
                    float chase = Mathf.Clamp01(1f - distance / Mathf.Max(0.01f, settings.chaseWidth));
                    chase = chase * chase * (3f - 2f * chase);
                    opacity = Mathf.Lerp(opacity, settings.chaseOpacity, chase);
                    tint = Color.Lerp(tint, settings.chaseTint, chase);
                }

                opacity = Mathf.Clamp01(opacity);
                bulb.Root.style.backgroundColor = new StyleColor(WithAlpha(tint, opacity));
            }
        }

        private void TickScale(Record record, float deltaTime)
        {
            bool pressed = record.PointerPressed || record.PressedPulseRemaining > 0f;
            if (record.PressedPulseRemaining > 0f)
            {
                record.PressedPulseRemaining = Mathf.Max(0f, record.PressedPulseRemaining - deltaTime);
            }

            float desiredScale = pressed ? settings.pressedScale : record.IsActive ? settings.hoverScale : 1f;
            SetScaleTarget(record, desiredScale);

            record.ScaleElapsed = Mathf.Min(record.ScaleDuration, record.ScaleElapsed + deltaTime);
            float progress = record.ScaleDuration > 0f ? record.ScaleElapsed / record.ScaleDuration : 1f;
            float eased = GameEasing.Evaluate01(settings.scaleEase, progress);
            record.CurrentScale = Mathf.LerpUnclamped(record.ScaleStart, record.ScaleTarget, eased);
            record.Target.style.scale = new Scale(new Vector3(record.CurrentScale, record.CurrentScale, 1f));
        }

        private void SetScaleTarget(Record record, float targetScale)
        {
            if (Mathf.Abs(record.ScaleTarget - targetScale) < 0.001f)
            {
                return;
            }

            record.ScaleStart = record.CurrentScale;
            record.ScaleTarget = targetScale;
            record.ScaleDuration = Mathf.Max(0.01f, settings.scaleSeconds);
            record.ScaleElapsed = 0f;
        }

        private static Color WithAlpha(Color color, float alpha)
        {
            color.a *= Mathf.Clamp01(alpha);
            return color;
        }

        private static void SetRadius(VisualElement element, float radius)
        {
            element.style.borderTopLeftRadius = radius;
            element.style.borderTopRightRadius = radius;
            element.style.borderBottomLeftRadius = radius;
            element.style.borderBottomRightRadius = radius;
        }

        private sealed class Record
        {
            public readonly VisualElement Target;
            public readonly List<BulbVisual> OrderedBulbs = new();
            public VisualElement Root;
            public VisualElement TopRail;
            public VisualElement RightRail;
            public VisualElement BottomRail;
            public VisualElement LeftRail;
            public int HorizontalBulbCount;
            public int VerticalBulbCount;
            public bool Hovered;
            public bool Focused;
            public bool PointerPressed;
            public float PressedPulseRemaining;
            public float CurrentScale = 1f;
            public float ScaleStart = 1f;
            public float ScaleTarget = 1f;
            public float ScaleElapsed = 1f;
            public float ScaleDuration = 0.01f;
            private readonly MarqueeButtonEffectSettings settings;

            public Record(VisualElement target, MarqueeButtonEffectSettings settings)
            {
                Target = target;
                this.settings = settings;
            }

            public bool IsActive => (Hovered && settings.pointerHoverActivates)
                || (Focused && settings.focusActivates)
                || (Target.ClassListContains("is-focused") && settings.focusActivates)
                || Target.ClassListContains("is-selected");

            public void RegisterCallbacks()
            {
                Target.RegisterCallback<PointerEnterEvent>(OnPointerEnter);
                Target.RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
                Target.RegisterCallback<PointerDownEvent>(OnPointerDown);
                Target.RegisterCallback<PointerUpEvent>(OnPointerUp);
                Target.RegisterCallback<PointerCancelEvent>(OnPointerCancel);
                Target.RegisterCallback<FocusInEvent>(OnFocusIn);
                Target.RegisterCallback<FocusOutEvent>(OnFocusOut);
            }

            public void UnregisterCallbacks()
            {
                Target.UnregisterCallback<PointerEnterEvent>(OnPointerEnter);
                Target.UnregisterCallback<PointerLeaveEvent>(OnPointerLeave);
                Target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
                Target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
                Target.UnregisterCallback<PointerCancelEvent>(OnPointerCancel);
                Target.UnregisterCallback<FocusInEvent>(OnFocusIn);
                Target.UnregisterCallback<FocusOutEvent>(OnFocusOut);
            }

            public void ClearBulbs()
            {
                TopRail.Clear();
                RightRail.Clear();
                BottomRail.Clear();
                LeftRail.Clear();
                OrderedBulbs.Clear();
            }

            private void OnPointerEnter(PointerEnterEvent evt)
            {
                Hovered = true;
            }

            private void OnPointerLeave(PointerLeaveEvent evt)
            {
                Hovered = false;
                PointerPressed = false;
            }

            private void OnPointerDown(PointerDownEvent evt)
            {
                PointerPressed = true;
            }

            private void OnPointerUp(PointerUpEvent evt)
            {
                PointerPressed = false;
            }

            private void OnPointerCancel(PointerCancelEvent evt)
            {
                PointerPressed = false;
            }

            private void OnFocusIn(FocusInEvent evt)
            {
                Focused = true;
            }

            private void OnFocusOut(FocusOutEvent evt)
            {
                Focused = false;
                PointerPressed = false;
            }
        }

        private readonly struct BulbVisual
        {
            public readonly VisualElement Root;

            public BulbVisual(VisualElement root)
            {
                Root = root;
            }
        }
    }
}
