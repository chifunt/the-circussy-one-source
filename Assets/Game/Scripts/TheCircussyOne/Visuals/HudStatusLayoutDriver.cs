using TheCircussyOne.Config;
using TheCircussyOne.Rules;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheCircussyOne.Visuals
{
    internal readonly struct HudStatusLayoutElements
    {
        public HudStatusLayoutElements(
            VisualElement xpStrip,
            VisualElement xpTrack,
            VisualElement xpSegments,
            VisualElement experienceFill,
            VisualElement experiencePulse,
            Label levelLabel,
            VisualElement healthCluster,
            VisualElement healthTrack,
            VisualElement healthFill,
            VisualElement healthSegments,
            Label healthLabel,
            VisualElement ticketsCluster,
            VisualElement ticketsIcon,
            Label ticketsLabel)
        {
            XpStrip = xpStrip;
            XpTrack = xpTrack;
            XpSegments = xpSegments;
            ExperienceFill = experienceFill;
            ExperiencePulse = experiencePulse;
            LevelLabel = levelLabel;
            HealthCluster = healthCluster;
            HealthTrack = healthTrack;
            HealthFill = healthFill;
            HealthSegments = healthSegments;
            HealthLabel = healthLabel;
            TicketsCluster = ticketsCluster;
            TicketsIcon = ticketsIcon;
            TicketsLabel = ticketsLabel;
        }

        public VisualElement XpStrip { get; }
        public VisualElement XpTrack { get; }
        public VisualElement XpSegments { get; }
        public VisualElement ExperienceFill { get; }
        public VisualElement ExperiencePulse { get; }
        public Label LevelLabel { get; }
        public VisualElement HealthCluster { get; }
        public VisualElement HealthTrack { get; }
        public VisualElement HealthFill { get; }
        public VisualElement HealthSegments { get; }
        public Label HealthLabel { get; }
        public VisualElement TicketsCluster { get; }
        public VisualElement TicketsIcon { get; }
        public Label TicketsLabel { get; }
    }

    internal static class HudStatusLayoutDriver
    {
        public static void Apply(
            HudStatusLayoutElements elements,
            HudVisualConfig config,
            HudXpGradientDriver xpGradient,
            int maxHealthForSegments,
            float scale)
        {
            if (config == null)
            {
                return;
            }

            ApplyXp(elements, config, xpGradient, scale);
            ApplyHealth(elements, config, maxHealthForSegments, scale);
            ApplyTickets(elements, config, scale);
        }

        public static void ConfigureHealthSegments(VisualElement healthSegments, HudVisualConfig config, int maxHealth, float scale)
        {
            HudBarSegmentDriver.ConfigureHealthSegments(healthSegments, config, Mathf.Max(0, maxHealth), scale);
        }

        private static void ApplyXp(HudStatusLayoutElements elements, HudVisualConfig config, HudXpGradientDriver xpGradient, float scale)
        {
            if (elements.XpStrip != null)
            {
                elements.XpStrip.style.height = Px(config.xpStripHeight, scale);
                elements.XpStrip.style.paddingLeft = Px(config.xpSideMargin, scale);
                elements.XpStrip.style.paddingRight = Px(config.xpSideMargin, scale);
                elements.XpStrip.style.paddingTop = Px(config.xpTopPadding, scale);
            }

            if (elements.XpTrack != null)
            {
                elements.XpTrack.style.left = Px(config.xpSideMargin, scale);
                elements.XpTrack.style.right = Px(config.xpSideMargin, scale);
                elements.XpTrack.style.top = Px(config.xpTopPadding, scale);
                elements.XpTrack.style.height = Px(config.xpTrackHeight, scale);
                elements.XpTrack.style.backgroundColor = config.xpTrackColor;
                SetBorderColor(elements.XpTrack, config.xpBorderColor);
                SetRadius(elements.XpTrack, config.xpCornerRadius, scale);
            }

            if (elements.ExperienceFill != null)
            {
                elements.ExperienceFill.style.backgroundColor = config.xpFillColor;
                xpGradient?.Apply(elements.ExperienceFill, config);
                SetRadius(elements.ExperienceFill, Mathf.Max(0f, config.xpCornerRadius - 1f), scale);
            }

            if (elements.ExperiencePulse != null)
            {
                elements.ExperiencePulse.style.backgroundColor = config.xpLevelUpPulseColor;
                elements.ExperiencePulse.style.opacity = 0f;
                SetRadius(elements.ExperiencePulse, Mathf.Max(0f, config.xpCornerRadius - 1f), scale);
            }

            HudBarSegmentDriver.ConfigureXpSegments(elements.XpSegments, config, scale);
            ApplyLevelBadge(elements.LevelLabel, config, scale);
        }

        private static void ApplyLevelBadge(Label levelLabel, HudVisualConfig config, float scale)
        {
            if (levelLabel == null)
            {
                return;
            }

            levelLabel.style.left = Px(config.healthOffset.x, scale);
            levelLabel.style.top = Px(CenteredRowTop(config, config.levelBadgeHeight) + config.levelBadgeOffset.y, scale);
            levelLabel.style.width = Px(config.levelBadgeWidth, scale);
            levelLabel.style.minWidth = Px(config.levelBadgeWidth, scale);
            levelLabel.style.height = Px(config.levelBadgeHeight, scale);
            levelLabel.style.paddingLeft = Px(config.levelBadgeHorizontalPadding, scale);
            levelLabel.style.paddingRight = Px(config.levelBadgeHorizontalPadding, scale);
            levelLabel.style.fontSize = Px(config.levelBadgeFontSize, scale);
            levelLabel.style.backgroundColor = config.levelBadgeColor;
            levelLabel.style.color = config.levelBadgeTextColor;
            levelLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            levelLabel.style.borderLeftWidth = 1f * scale;
            levelLabel.style.borderRightWidth = 1f * scale;
            levelLabel.style.borderTopWidth = 1f * scale;
            levelLabel.style.borderBottomWidth = 1f * scale;
            SetBorderColor(levelLabel, config.xpFillColor);
            SetRadius(levelLabel, config.xpCornerRadius, scale);
        }

        private static void ApplyHealth(HudStatusLayoutElements elements, HudVisualConfig config, int maxHealthForSegments, float scale)
        {
            if (elements.HealthCluster != null)
            {
                elements.HealthCluster.style.left = Px(HealthBarLeft(config), scale);
                elements.HealthCluster.style.top = Px(CenteredRowTop(config, config.healthHeight), scale);
                elements.HealthCluster.style.width = Px(config.healthWidth, scale);
                elements.HealthCluster.style.height = Px(config.healthHeight, scale);
            }

            if (elements.HealthTrack != null)
            {
                elements.HealthTrack.style.backgroundColor = config.healthTrackColor;
                SetBorderColor(elements.HealthTrack, config.healthFillColor.WithAlpha(0.48f));
                SetRadius(elements.HealthTrack, config.healthCornerRadius, scale);
            }

            if (elements.HealthFill != null)
            {
                elements.HealthFill.style.backgroundColor = config.healthFillColor;
                SetRadius(elements.HealthFill, Mathf.Max(0f, config.healthCornerRadius - 1f), scale);
            }

            if (elements.HealthLabel != null)
            {
                float textOffset = Px(config.healthTextVerticalOffset, scale);
                elements.HealthLabel.style.top = textOffset;
                elements.HealthLabel.style.bottom = -textOffset;
                elements.HealthLabel.style.height = StyleKeyword.Null;
                elements.HealthLabel.style.fontSize = Px(config.healthFontSize, scale);
                elements.HealthLabel.style.color = config.healthTextColor;
                elements.HealthLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            }

            ConfigureHealthSegments(elements.HealthSegments, config, maxHealthForSegments, scale);
        }

        private static void ApplyTickets(HudStatusLayoutElements elements, HudVisualConfig config, float scale)
        {
            if (elements.TicketsCluster != null)
            {
                elements.TicketsCluster.style.left = Px(HealthBarLeft(config) + config.healthWidth + config.ticketsOffsetFromHealth.x, scale);
                elements.TicketsCluster.style.top = Px(CenteredRowTop(config, config.ticketsHeight) + config.ticketsOffsetFromHealth.y, scale);
                elements.TicketsCluster.style.right = StyleKeyword.Null;
                elements.TicketsCluster.style.height = Px(config.ticketsHeight, scale);
            }

            if (elements.TicketsIcon != null)
            {
                elements.TicketsIcon.style.width = Px(config.ticketsIconWidth, scale);
                elements.TicketsIcon.style.minWidth = Px(config.ticketsIconWidth, scale);
                elements.TicketsIcon.style.height = Px(config.ticketsIconHeight, scale);
                elements.TicketsIcon.style.minHeight = Px(config.ticketsIconHeight, scale);
                elements.TicketsIcon.style.marginRight = Px(config.ticketsSpacing, scale);
                elements.TicketsIcon.style.position = Position.Relative;
                elements.TicketsIcon.style.top = Px(config.ticketsIconYOffset, scale);
            }

            if (elements.TicketsLabel != null)
            {
                elements.TicketsLabel.style.minWidth = Px(config.ticketsNumberMinWidth, scale);
                elements.TicketsLabel.style.fontSize = Px(config.ticketsNumberFontSize, scale);
                elements.TicketsLabel.style.color = config.ticketsNumberColor;
            }
        }

        private static float Px(float value, float scale)
        {
            return value * scale;
        }

        private static float HealthBarLeft(HudVisualConfig config)
        {
            return config.healthOffset.x + Mathf.Max(1f, config.levelBadgeWidth) + Mathf.Max(0f, config.levelBadgeOffset.x);
        }

        private static float CenteredRowTop(HudVisualConfig config, float elementHeight)
        {
            float rowHeight = Mathf.Max(
                Mathf.Max(1f, config.levelBadgeHeight),
                Mathf.Max(Mathf.Max(1f, config.healthHeight), Mathf.Max(1f, config.ticketsHeight)));
            return config.healthOffset.y + (rowHeight - Mathf.Max(1f, elementHeight)) * 0.5f;
        }

        private static void SetRadius(VisualElement element, float radius, float scale)
        {
            float scaled = Mathf.Max(0f, radius * scale);
            element.style.borderTopLeftRadius = scaled;
            element.style.borderTopRightRadius = scaled;
            element.style.borderBottomLeftRadius = scaled;
            element.style.borderBottomRightRadius = scaled;
        }

        private static void SetBorderColor(VisualElement element, Color color)
        {
            element.style.borderLeftColor = color;
            element.style.borderRightColor = color;
            element.style.borderTopColor = color;
            element.style.borderBottomColor = color;
        }
    }
}
