using System;
using Sirenix.OdinInspector;
using UnityEngine;
using TheCircussyOne.Runtime;
using TheCircussyOne.Rules;

namespace TheCircussyOne.Config
{
    [CreateAssetMenu(menuName = "The Circussy One/VFX Visual Config", fileName = "VfxVisualConfig")]
    [InfoBox("PROJECTED VISUAL: particle tuning applies to generated VFX prefabs. Gameplay damage, pickup, and movement rules live elsewhere.")]
    public sealed class VfxVisualConfig : SerializedScriptableObject
    {
        private const string Tabs = "VFX Visuals";

        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Workflow"), LabelWidth(190), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.VisualColor")]
        public bool autoApplyOnChange = true;

        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Workflow"), LabelWidth(190)]
        public bool enabled = true;

        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Move Dust"), LabelWidth(190)]
        public bool playerMoveDustEnabled = true;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Move Dust"), LabelWidth(190), SuffixLabel("particles/sec")]
        [Min(0f)] public float playerMoveDustMaxEmissionRate = 18f;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Move Dust"), LabelWidth(190), SuffixLabel("local u")]
        public Vector3 playerMoveDustLocalOffset = new(0f, 0.08f, -0.32f);
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Move Dust"), LabelWidth(190)]
        public Color playerMoveDustColor = new(0.72f, 0.92f, 1f, 0.42f);
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Move Dust"), LabelWidth(190), SuffixLabel("sec")]
        [Min(0.01f)] public float playerMoveDustLifetime = 0.46f;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Move Dust"), LabelWidth(190), SuffixLabel("u")]
        [Min(0.001f)] public float playerMoveDustStartSize = 0.22f;

        [TabGroup(Tabs, "World"), BoxGroup(Tabs + "/World/Ambient Dust"), LabelWidth(190), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.VisualColor")]
        public bool worldAmbientDustEnabled = true;
        [TabGroup(Tabs, "World"), BoxGroup(Tabs + "/World/Ambient Dust"), LabelWidth(190), SuffixLabel("particles/sec")]
        [Min(0f)] public float worldAmbientDustEmissionRate = 16f;
        [TabGroup(Tabs, "World"), BoxGroup(Tabs + "/World/Ambient Dust"), LabelWidth(190)]
        [Min(1)] public int worldAmbientDustMaxParticles = 220;
        [TabGroup(Tabs, "World"), BoxGroup(Tabs + "/World/Ambient Dust"), LabelWidth(190), SuffixLabel("u")]
        [Min(1f)] public float worldAmbientDustFollowRadius = 34f;
        [TabGroup(Tabs, "World"), BoxGroup(Tabs + "/World/Ambient Dust"), LabelWidth(190), SuffixLabel("min/max u")]
        public Vector2 worldAmbientDustHeightRange = new(0.5f, 8f);
        [TabGroup(Tabs, "World"), BoxGroup(Tabs + "/World/Ambient Dust"), LabelWidth(190), SuffixLabel("world u")]
        public Vector3 worldAmbientDustFollowOffset = Vector3.zero;
        [TabGroup(Tabs, "World"), BoxGroup(Tabs + "/World/Ambient Dust"), LabelWidth(190)]
        public Color worldAmbientDustColor = new(0.95f, 0.72f, 0.38f, 0.16f);
        [TabGroup(Tabs, "World"), BoxGroup(Tabs + "/World/Ambient Dust"), LabelWidth(190), SuffixLabel("min/max u")]
        public Vector2 worldAmbientDustSizeRange = new(0.025f, 0.09f);
        [TabGroup(Tabs, "World"), BoxGroup(Tabs + "/World/Ambient Dust"), LabelWidth(190), SuffixLabel("min/max sec")]
        public Vector2 worldAmbientDustLifetimeRange = new(7f, 13f);
        [TabGroup(Tabs, "World"), BoxGroup(Tabs + "/World/Ambient Dust"), LabelWidth(190), SuffixLabel("u/sec")]
        [Min(0f)] public float worldAmbientDustDriftSpeed = 0.08f;
        [TabGroup(Tabs, "World"), BoxGroup(Tabs + "/World/Ambient Dust"), LabelWidth(190)]
        [Min(0f)] public float worldAmbientDustNoiseStrength = 0.12f;

        [TabGroup(Tabs, "World"), BoxGroup(Tabs + "/World/Floor Haze"), LabelWidth(190), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.VisualColor"), InlineProperty, HideLabel]
        public WorldAtmosphereLayerSettings worldFloorHaze = WorldAtmosphereLayerSettings.FloorHazeDefault();

        [TabGroup(Tabs, "World"), BoxGroup(Tabs + "/World/God Ray Dust"), LabelWidth(190), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.VisualColor"), InlineProperty, HideLabel]
        public WorldAtmosphereLayerSettings worldGodRayDust = WorldAtmosphereLayerSettings.GodRayDustDefault();

        [TabGroup(Tabs, "Projectile"), BoxGroup(Tabs + "/Projectile/Muzzle Puff"), LabelWidth(190), InlineProperty, HideLabel]
        public VfxOneShotSettings projectileMuzzlePuff = VfxOneShotSettings.Default(
            enabled: true,
            color: new Color(1f, 0.86f, 0.25f, 0.76f),
            burstCount: 14,
            duration: 0.15f,
            lifetime: 0.24f,
            speed: 1.8f,
            size: 0.13f,
            radius: 0.06f,
            additive: true);

        [TabGroup(Tabs, "Projectile"), BoxGroup(Tabs + "/Projectile/Explosion"), LabelWidth(190), InlineProperty, HideLabel]
        public VfxOneShotSettings projectileExplosion = VfxOneShotSettings.Default(
            enabled: true,
            color: new Color(1f, 0.48f, 0.12f, 0.88f),
            burstCount: 48,
            duration: 0.32f,
            lifetime: 0.46f,
            speed: 4.8f,
            size: 0.18f,
            radius: 0.38f,
            additive: true);

        [TabGroup(Tabs, "Projectile"), BoxGroup(Tabs + "/Projectile/Bounce Burst"), LabelWidth(190), InlineProperty, HideLabel]
        public VfxOneShotSettings projectileBounceBurst = VfxOneShotSettings.Default(
            enabled: true,
            color: new Color(0.32f, 0.9f, 1f, 0.9f),
            burstCount: 26,
            duration: 0.18f,
            lifetime: 0.32f,
            speed: 4.1f,
            size: 0.105f,
            radius: 0.16f,
            additive: true);

        [TabGroup(Tabs, "Projectile"), BoxGroup(Tabs + "/Projectile/Explosion/Radius Overlay"), LabelWidth(190), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.VisualColor")]
        public bool projectileExplosionOverlayEnabled = true;
        [TabGroup(Tabs, "Projectile"), BoxGroup(Tabs + "/Projectile/Explosion/Radius Overlay"), LabelWidth(190), SuffixLabel("sec"), Min(0.01f)]
        public float projectileExplosionOverlayDuration = 0.28f;
        [TabGroup(Tabs, "Projectile"), BoxGroup(Tabs + "/Projectile/Explosion/Radius Overlay"), LabelWidth(190), SuffixLabel("x"), Range(0f, 1f)]
        public float projectileExplosionOverlayStartRadiusFraction = 0.06f;
        [TabGroup(Tabs, "Projectile"), BoxGroup(Tabs + "/Projectile/Explosion/Radius Overlay"), LabelWidth(190), SuffixLabel("u"), Min(0.001f)]
        public float projectileExplosionOverlayRingThickness = 0.08f;
        [TabGroup(Tabs, "Projectile"), BoxGroup(Tabs + "/Projectile/Explosion/Radius Overlay"), LabelWidth(190)]
        public Color projectileExplosionOverlayRingColor = new(1f, 0.68f, 0.16f, 0.58f);
        [TabGroup(Tabs, "Projectile"), BoxGroup(Tabs + "/Projectile/Explosion/Radius Overlay"), LabelWidth(190)]
        public Color projectileExplosionOverlaySphereColor = new(1f, 0.38f, 0.08f, 0.12f);
        [TabGroup(Tabs, "Projectile"), BoxGroup(Tabs + "/Projectile/Explosion/Radius Overlay"), LabelWidth(190), InlineProperty]
        public EaseSettings projectileExplosionOverlayExpandEase = EaseSettings.OutCubic;
        [TabGroup(Tabs, "Projectile"), BoxGroup(Tabs + "/Projectile/Explosion/Radius Overlay"), LabelWidth(190), InlineProperty]
        public EaseSettings projectileExplosionOverlayFadeEase = EaseSettings.InCubic;

        [TabGroup(Tabs, "Fire Hoop"), BoxGroup(Tabs + "/Fire Hoop/Area Telegraph"), LabelWidth(190), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.VisualColor")]
        public bool fireHoopAreaTelegraphEnabled = true;
        [TabGroup(Tabs, "Fire Hoop"), BoxGroup(Tabs + "/Fire Hoop/Area Telegraph"), LabelWidth(190)]
        public Color fireHoopAreaTelegraphFillColor = new(1f, 0.22f, 0.04f, 0.035f);
        [TabGroup(Tabs, "Fire Hoop"), BoxGroup(Tabs + "/Fire Hoop/Area Telegraph"), LabelWidth(190)]
        public Color fireHoopAreaTelegraphRingColor = new(1f, 0.42f, 0.04f, 0.2f);
        [TabGroup(Tabs, "Fire Hoop"), BoxGroup(Tabs + "/Fire Hoop/Area Telegraph"), LabelWidth(190), SuffixLabel("u"), Min(0.01f)]
        public float fireHoopAreaTelegraphRingWidth = 0.16f;
        [TabGroup(Tabs, "Fire Hoop"), BoxGroup(Tabs + "/Fire Hoop/Area Telegraph"), LabelWidth(190), Range(0f, 1f)]
        public float fireHoopAreaTelegraphPulseAlphaBoost = 0.14f;
        [TabGroup(Tabs, "Fire Hoop"), BoxGroup(Tabs + "/Fire Hoop/Area Telegraph"), LabelWidth(190), SuffixLabel("x"), Min(0f)]
        public float fireHoopAreaTelegraphPulseRingWidthBoost = 0.45f;
        [TabGroup(Tabs, "Fire Hoop"), BoxGroup(Tabs + "/Fire Hoop/Area Telegraph"), LabelWidth(190), InlineProperty]
        public EaseSettings fireHoopAreaTelegraphPulseEase = EaseSettings.OutCubic;

        [TabGroup(Tabs, "Fire Hoop"), BoxGroup(Tabs + "/Fire Hoop/Ground Sampling"), LabelWidth(190), SuffixLabel("samples"), Min(8)]
        public int fireHoopAreaTelegraphSampleCount = 48;
        [TabGroup(Tabs, "Fire Hoop"), BoxGroup(Tabs + "/Fire Hoop/Ground Sampling"), LabelWidth(190), SuffixLabel("sec"), Min(0.01f)]
        public float fireHoopAreaTelegraphRefreshSeconds = 0.08f;
        [TabGroup(Tabs, "Fire Hoop"), BoxGroup(Tabs + "/Fire Hoop/Ground Sampling"), LabelWidth(190), SuffixLabel("u"), Min(0f)]
        public float fireHoopAreaTelegraphGroundClearance = 0.035f;
        [TabGroup(Tabs, "Fire Hoop"), BoxGroup(Tabs + "/Fire Hoop/Ground Sampling"), LabelWidth(190), SuffixLabel("u"), Min(0.01f)]
        public float fireHoopAreaTelegraphProbeHeight = 3f;
        [TabGroup(Tabs, "Fire Hoop"), BoxGroup(Tabs + "/Fire Hoop/Ground Sampling"), LabelWidth(190), SuffixLabel("u"), Min(0.01f)]
        public float fireHoopAreaTelegraphProbeDepth = 8f;
        [TabGroup(Tabs, "Fire Hoop"), BoxGroup(Tabs + "/Fire Hoop/Ground Sampling"), LabelWidth(190)]
        public LayerMask fireHoopAreaTelegraphGroundMask = ~0;

        [TabGroup(Tabs, "Enemy"), BoxGroup(Tabs + "/Enemy/Hit Sparks"), LabelWidth(190), InlineProperty, HideLabel]
        public VfxOneShotSettings enemyHitSparks = VfxOneShotSettings.Default(
            enabled: true,
            color: new Color(1f, 0.78f, 0.22f, 0.92f),
            burstCount: 18,
            duration: 0.18f,
            lifetime: 0.28f,
            speed: 3.8f,
            size: 0.08f,
            radius: 0.09f,
            additive: true);

        [TabGroup(Tabs, "Enemy"), BoxGroup(Tabs + "/Enemy/Knife Hit Sparks"), LabelWidth(190), InlineProperty, HideLabel]
        public VfxOneShotSettings knifeHitSparks = VfxOneShotSettings.Default(
            enabled: true,
            color: new Color(1f, 0.96f, 0.72f, 0.88f),
            burstCount: 10,
            duration: 0.11f,
            lifetime: 0.18f,
            speed: 3.5f,
            size: 0.065f,
            radius: 0.045f,
            additive: true);

        [TabGroup(Tabs, "Enemy"), BoxGroup(Tabs + "/Enemy/Death Burst"), LabelWidth(190), InlineProperty, HideLabel]
        public VfxOneShotSettings enemyDeathBurst = EnemyDeathBurstDefault();

        [TabGroup(Tabs, "Pickup"), BoxGroup(Tabs + "/Pickup/Attract Trail"), LabelWidth(190)]
        public bool pickupAttractTrailEnabled = true;
        [TabGroup(Tabs, "Pickup"), BoxGroup(Tabs + "/Pickup/Attract Trail"), LabelWidth(190), SuffixLabel("particles/sec")]
        [Min(0f)] public float pickupAttractTrailEmissionRate = 24f;
        [TabGroup(Tabs, "Pickup"), BoxGroup(Tabs + "/Pickup/Attract Trail"), LabelWidth(190)]
        public Color pickupAttractTrailColor = new(0.42f, 0.9f, 1f, 0.72f);
        [TabGroup(Tabs, "Pickup"), BoxGroup(Tabs + "/Pickup/Attract Trail"), LabelWidth(190), SuffixLabel("sec")]
        [Min(0.01f)] public float pickupAttractTrailLifetime = 0.24f;
        [TabGroup(Tabs, "Pickup"), BoxGroup(Tabs + "/Pickup/Attract Trail"), LabelWidth(190), SuffixLabel("u")]
        [Min(0.001f)] public float pickupAttractTrailStartSize = 0.075f;

        [TabGroup(Tabs, "Pickup"), BoxGroup(Tabs + "/Pickup/Collect Pop"), LabelWidth(190), InlineProperty, HideLabel]
        public VfxOneShotSettings xpPickupCollectPop = VfxOneShotSettings.Default(
            enabled: true,
            color: new Color(0.45f, 0.92f, 1f, 0.86f),
            burstCount: 22,
            duration: 0.24f,
            lifetime: 0.36f,
            speed: 2.6f,
            size: 0.11f,
            radius: 0.12f,
            additive: true);

        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Damage Burst"), LabelWidth(190), InlineProperty, HideLabel]
        public VfxOneShotSettings playerDamageBurst = VfxOneShotSettings.Default(
            enabled: true,
            color: new Color(1f, 0.08f, 0.08f, 0.84f),
            burstCount: 28,
            duration: 0.24f,
            lifetime: 0.35f,
            speed: 2.8f,
            size: 0.14f,
            radius: 0.32f,
            additive: true);

        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Jump Takeoff"), LabelWidth(190), InlineProperty, HideLabel]
        public VfxOneShotSettings playerJumpTakeoff = VfxOneShotSettings.Default(
            enabled: true,
            color: new Color(0.74f, 0.92f, 1f, 0.68f),
            burstCount: 24,
            duration: 0.22f,
            lifetime: 0.36f,
            speed: 2.6f,
            size: 0.16f,
            radius: 0.34f,
            additive: false);

        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Jump Landing"), LabelWidth(190), InlineProperty, HideLabel]
        public VfxOneShotSettings playerJumpLand = VfxOneShotSettings.Default(
            enabled: true,
            color: new Color(0.86f, 0.94f, 1f, 0.72f),
            burstCount: 30,
            duration: 0.24f,
            lifetime: 0.40f,
            speed: 3.0f,
            size: 0.18f,
            radius: 0.42f,
            additive: false);

        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Jump Trail"), LabelWidth(190)]
        public bool playerJumpTrailEnabled = true;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Jump Trail"), LabelWidth(190), SuffixLabel("particles/sec")]
        [Min(0f)] public float playerJumpTrailEmissionRate = 26f;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Jump Trail"), LabelWidth(190), SuffixLabel("local u")]
        public Vector3 playerJumpTrailLocalOffset = new(0f, 0.12f, -0.08f);
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Jump Trail"), LabelWidth(190)]
        public Color playerJumpTrailColor = new(0.62f, 0.88f, 1f, 0.62f);
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Jump Trail"), LabelWidth(190), SuffixLabel("sec")]
        [Min(0.01f)] public float playerJumpTrailLifetime = 0.32f;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Jump Trail"), LabelWidth(190), SuffixLabel("u")]
        [Min(0.001f)] public float playerJumpTrailStartSize = 0.09f;

        [TabGroup(Tabs, "Level Up"), BoxGroup(Tabs + "/Level Up/Burst"), LabelWidth(190), InlineProperty, HideLabel]
        public VfxOneShotSettings levelUpBurst = VfxOneShotSettings.Default(
            enabled: true,
            color: new Color(0.36f, 1f, 0.72f, 0.92f),
            burstCount: 64,
            duration: 0.55f,
            lifetime: 0.82f,
            speed: 3.5f,
            size: 0.16f,
            radius: 0.85f,
            additive: true);

        [TabGroup(Tabs, "Pooling"), BoxGroup(Tabs + "/Pooling/Capacity"), LabelWidth(190)]
        [Min(0)] public int oneShotPrewarmCount = 0;

        [TabGroup(Tabs, "Preview"), ShowInInspector, ReadOnly, LabelText("One-Shots"), PropertyOrder(100)]
        private string OneShotSummary => enabled
            ? $"Hit {enemyHitSparks.burstCount}, knife {knifeHitSparks.burstCount}, bounce {projectileBounceBurst.burstCount}, death {enemyDeathBurst.burstCount}, explosion {projectileExplosion.burstCount}, level {levelUpBurst.burstCount}"
            : "Disabled";

        public VfxOneShotSettings SettingsFor(VfxEffectId effectId)
        {
            return effectId switch
            {
                VfxEffectId.ProjectileMuzzlePuff => projectileMuzzlePuff,
                VfxEffectId.ProjectileExplosion => projectileExplosion,
                VfxEffectId.ProjectileBounceBurst => projectileBounceBurst,
                VfxEffectId.EnemyHitSparks => enemyHitSparks,
                VfxEffectId.KnifeHitSparks => knifeHitSparks,
                VfxEffectId.EnemyDeathBurst => enemyDeathBurst,
                VfxEffectId.XpPickupCollectPop => xpPickupCollectPop,
                VfxEffectId.PlayerDamageBurst => playerDamageBurst,
                VfxEffectId.LevelUpBurst => levelUpBurst,
                VfxEffectId.PlayerJumpTakeoff => playerJumpTakeoff,
                VfxEffectId.PlayerJumpLand => playerJumpLand,
                _ => null
            };
        }

        public bool EnsureWorkflowDefaults()
        {
            bool changed = false;
            changed |= EnsureMinimum(ref playerMoveDustMaxEmissionRate, 18f, 0f);
            changed |= EnsureMinimum(ref playerMoveDustLifetime, 0.46f, 0.01f);
            changed |= EnsureMinimum(ref playerMoveDustStartSize, 0.22f, 0.001f);
            changed |= EnsureMinimum(ref worldAmbientDustEmissionRate, 16f, 0f);
            changed |= EnsureMinimum(ref worldAmbientDustMaxParticles, 220, 1);
            changed |= EnsureMinimum(ref worldAmbientDustFollowRadius, 34f, 1f);
            changed |= EnsureRange(ref worldAmbientDustHeightRange, new Vector2(0.5f, 8f), 0f, 0.1f);
            changed |= EnsureRange(ref worldAmbientDustSizeRange, new Vector2(0.025f, 0.09f), 0.001f, 0.001f);
            changed |= EnsureRange(ref worldAmbientDustLifetimeRange, new Vector2(7f, 13f), 0.02f, 0.01f);
            changed |= EnsureMinimum(ref worldAmbientDustDriftSpeed, 0.08f, 0f);
            changed |= EnsureMinimum(ref worldAmbientDustNoiseStrength, 0.12f, 0f);
            changed |= EnsureAtmosphereLayer(ref worldFloorHaze, WorldAtmosphereLayerSettings.FloorHazeDefault());
            changed |= EnsureAtmosphereLayer(ref worldGodRayDust, WorldAtmosphereLayerSettings.GodRayDustDefault());
            changed |= EnsureMinimum(ref pickupAttractTrailEmissionRate, 24f, 0f);
            changed |= EnsureMinimum(ref pickupAttractTrailLifetime, 0.24f, 0.01f);
            changed |= EnsureMinimum(ref pickupAttractTrailStartSize, 0.075f, 0.001f);
            changed |= EnsureMinimum(ref playerJumpTrailEmissionRate, 26f, 0f);
            changed |= EnsureMinimum(ref playerJumpTrailLifetime, 0.32f, 0.01f);
            changed |= EnsureMinimum(ref playerJumpTrailStartSize, 0.09f, 0.001f);
            changed |= EnsureOneShot(ref projectileMuzzlePuff, VfxOneShotSettings.Default(new Color(1f, 0.86f, 0.25f, 0.76f)));
            changed |= EnsureOneShot(
                ref projectileExplosion,
                VfxOneShotSettings.Default(
                    enabled: true,
                    color: new Color(1f, 0.48f, 0.12f, 0.88f),
                    burstCount: 48,
                    duration: 0.32f,
                    lifetime: 0.46f,
                    speed: 4.8f,
                    size: 0.18f,
                    radius: 0.38f,
                    additive: true));
            changed |= EnsureOneShot(
                ref projectileBounceBurst,
                VfxOneShotSettings.Default(
                    enabled: true,
                    color: new Color(0.32f, 0.9f, 1f, 0.9f),
                    burstCount: 26,
                    duration: 0.18f,
                    lifetime: 0.32f,
                    speed: 4.1f,
                    size: 0.105f,
                    radius: 0.16f,
                    additive: true));
            changed |= EnsureMinimum(ref projectileExplosionOverlayDuration, 0.28f, 0.01f);
            changed |= EnsureMinimum(ref projectileExplosionOverlayStartRadiusFraction, 0.06f, 0f);
            changed |= EnsureMaximum(ref projectileExplosionOverlayStartRadiusFraction, 0.06f, 1f);
            changed |= EnsureMinimum(ref projectileExplosionOverlayRingThickness, 0.08f, 0.001f);
            changed |= EnsureEaseDefault(ref projectileExplosionOverlayExpandEase, EaseSettings.OutCubic);
            changed |= EnsureEaseDefault(ref projectileExplosionOverlayFadeEase, EaseSettings.InCubic);
            changed |= EnsureMinimum(ref fireHoopAreaTelegraphRingWidth, 0.16f, 0.01f);
            changed |= EnsureMinimum(ref fireHoopAreaTelegraphPulseAlphaBoost, 0.14f, 0f);
            changed |= EnsureMaximum(ref fireHoopAreaTelegraphPulseAlphaBoost, 0.14f, 1f);
            changed |= EnsureMinimum(ref fireHoopAreaTelegraphPulseRingWidthBoost, 0.45f, 0f);
            changed |= EnsureEaseDefault(ref fireHoopAreaTelegraphPulseEase, EaseSettings.OutCubic);
            changed |= EnsureMinimum(ref fireHoopAreaTelegraphSampleCount, 48, 8);
            changed |= EnsureMinimum(ref fireHoopAreaTelegraphRefreshSeconds, 0.08f, 0.01f);
            changed |= EnsureMinimum(ref fireHoopAreaTelegraphGroundClearance, 0.035f, 0f);
            changed |= EnsureMinimum(ref fireHoopAreaTelegraphProbeHeight, 3f, 0.01f);
            changed |= EnsureMinimum(ref fireHoopAreaTelegraphProbeDepth, 8f, 0.01f);
            if (fireHoopAreaTelegraphGroundMask.value == 0)
            {
                fireHoopAreaTelegraphGroundMask = GameLayers.EnvironmentMaskExcludingGameplay;
                changed = true;
            }

            changed |= EnsureOneShot(ref enemyHitSparks, VfxOneShotSettings.Default(new Color(1f, 0.78f, 0.22f, 0.92f)));
            changed |= EnsureOneShot(
                ref knifeHitSparks,
                VfxOneShotSettings.Default(
                    enabled: true,
                    color: new Color(1f, 0.96f, 0.72f, 0.88f),
                    burstCount: 10,
                    duration: 0.11f,
                    lifetime: 0.18f,
                    speed: 3.5f,
                    size: 0.065f,
                    radius: 0.045f,
                    additive: true));
            changed |= EnsureOneShot(ref enemyDeathBurst, EnemyDeathBurstDefault());
            changed |= EnsureOneShot(ref xpPickupCollectPop, VfxOneShotSettings.Default(new Color(0.45f, 0.92f, 1f, 0.86f)));
            changed |= EnsureOneShot(ref playerDamageBurst, VfxOneShotSettings.Default(new Color(1f, 0.08f, 0.08f, 0.84f)));
            changed |= EnsureOneShot(ref levelUpBurst, VfxOneShotSettings.Default(new Color(0.36f, 1f, 0.72f, 0.92f)));
            changed |= EnsureOneShot(ref playerJumpTakeoff, VfxOneShotSettings.Default(new Color(0.74f, 0.92f, 1f, 0.68f)));
            changed |= EnsureOneShot(ref playerJumpLand, VfxOneShotSettings.Default(new Color(0.86f, 0.94f, 1f, 0.72f)));
            return changed;
        }

        private static VfxOneShotSettings EnemyDeathBurstDefault()
        {
            return VfxOneShotSettings.Default(
                enabled: true,
                color: new Color(1f, 0.28f, 0.62f, 1f),
                burstCount: 72,
                duration: 0.42f,
                lifetime: 0.78f,
                speed: 5.8f,
                size: 0.24f,
                radius: 0.42f,
                additive: true);
        }

        private void OnValidate()
        {
            EnsureWorkflowDefaults();
        }

        private static bool EnsureMinimum(ref float value, float defaultValue, float minimum)
        {
            if (value >= minimum)
            {
                return false;
            }

            value = defaultValue;
            return true;
        }

        private static bool EnsureMinimum(ref int value, int defaultValue, int minimum)
        {
            if (value >= minimum)
            {
                return false;
            }

            value = defaultValue;
            return true;
        }

        private static bool EnsureMaximum(ref float value, float defaultValue, float maximum)
        {
            if (value <= maximum)
            {
                return false;
            }

            value = defaultValue;
            return true;
        }

        private static bool EnsureRange(ref Vector2 value, Vector2 defaultValue, float minimum, float minimumSpan)
        {
            if (value.x >= minimum && value.y >= value.x + minimumSpan)
            {
                return false;
            }

            value = defaultValue;
            return true;
        }

        private static bool EnsureEaseDefault(ref EaseSettings settings, EaseSettings defaultValue)
        {
            if (settings.shape > 0f)
            {
                return false;
            }

            settings = defaultValue;
            return true;
        }

        private static bool EnsureOneShot(ref VfxOneShotSettings settings, VfxOneShotSettings defaultSettings)
        {
            if (settings == null)
            {
                settings = defaultSettings;
                return true;
            }

            return settings.EnsureDefaults();
        }

        private static bool EnsureAtmosphereLayer(ref WorldAtmosphereLayerSettings settings, WorldAtmosphereLayerSettings defaultSettings)
        {
            if (settings == null)
            {
                settings = defaultSettings;
                return true;
            }

            return settings.EnsureDefaults(defaultSettings);
        }
    }

    public enum WorldAtmosphereLayerId
    {
        AmbientDust = 0,
        FloorHaze = 1,
        GodRayDust = 2
    }

    [Serializable]
    public sealed class WorldAtmosphereLayerSettings
    {
        [LabelWidth(180)] public bool enabled = true;
        [LabelWidth(180), SuffixLabel("particles/sec"), Min(0f)] public float emissionRate = 8f;
        [LabelWidth(180), Min(1)] public int maxParticles = 140;
        [LabelWidth(180), SuffixLabel("u"), Min(1f)] public float followRadius = 40f;
        [LabelWidth(180), SuffixLabel("min/max u")] public Vector2 heightRange = new(0.05f, 1.2f);
        [LabelWidth(180), SuffixLabel("world u")] public Vector3 followOffset = Vector3.zero;
        [LabelWidth(180)] public Color color = new(0.48f, 0.43f, 0.34f, 0.09f);
        [LabelWidth(180), SuffixLabel("min/max u")] public Vector2 sizeRange = new(0.85f, 2.4f);
        [LabelWidth(180), SuffixLabel("min/max sec")] public Vector2 lifetimeRange = new(6f, 12f);
        [LabelWidth(180), SuffixLabel("u/sec"), Min(0f)] public float driftSpeed = 0.16f;
        [LabelWidth(180), Min(0f)] public float noiseStrength = 0.16f;
        [LabelWidth(180)] public int sortingOrder = 1;

        public static WorldAtmosphereLayerSettings FloorHazeDefault()
        {
            return new WorldAtmosphereLayerSettings
            {
                enabled = true,
                emissionRate = 8f,
                maxParticles = 140,
                followRadius = 42f,
                heightRange = new Vector2(0.05f, 1.25f),
                followOffset = Vector3.zero,
                color = new Color(0.48f, 0.43f, 0.34f, 0.09f),
                sizeRange = new Vector2(0.85f, 2.4f),
                lifetimeRange = new Vector2(6f, 12f),
                driftSpeed = 0.16f,
                noiseStrength = 0.16f,
                sortingOrder = 1
            };
        }

        public static WorldAtmosphereLayerSettings GodRayDustDefault()
        {
            return new WorldAtmosphereLayerSettings
            {
                enabled = true,
                emissionRate = 10f,
                maxParticles = 160,
                followRadius = 38f,
                heightRange = new Vector2(2.4f, 12f),
                followOffset = Vector3.zero,
                color = new Color(1f, 0.74f, 0.42f, 0.1f),
                sizeRange = new Vector2(0.025f, 0.1f),
                lifetimeRange = new Vector2(7f, 14f),
                driftSpeed = 0.06f,
                noiseStrength = 0.14f,
                sortingOrder = 3
            };
        }

        public bool EnsureDefaults(WorldAtmosphereLayerSettings defaults)
        {
            if (defaults == null)
            {
                return false;
            }

            bool changed = false;
            changed |= EnsureMinimum(ref emissionRate, defaults.emissionRate, 0f);
            changed |= EnsureMinimum(ref maxParticles, defaults.maxParticles, 1);
            changed |= EnsureMinimum(ref followRadius, defaults.followRadius, 1f);
            changed |= EnsureRange(ref heightRange, defaults.heightRange, 0f, 0.1f);
            changed |= EnsureRange(ref sizeRange, defaults.sizeRange, 0.001f, 0.001f);
            changed |= EnsureRange(ref lifetimeRange, defaults.lifetimeRange, 0.02f, 0.01f);
            changed |= EnsureMinimum(ref driftSpeed, defaults.driftSpeed, 0f);
            changed |= EnsureMinimum(ref noiseStrength, defaults.noiseStrength, 0f);
            return changed;
        }

        private static bool EnsureMinimum(ref float value, float defaultValue, float minimum)
        {
            if (value >= minimum)
            {
                return false;
            }

            value = defaultValue;
            return true;
        }

        private static bool EnsureMinimum(ref int value, int defaultValue, int minimum)
        {
            if (value >= minimum)
            {
                return false;
            }

            value = defaultValue;
            return true;
        }

        private static bool EnsureRange(ref Vector2 value, Vector2 defaultValue, float minimum, float minimumSpan)
        {
            if (value.x >= minimum && value.y >= value.x + minimumSpan)
            {
                return false;
            }

            value = defaultValue;
            return true;
        }
    }

    [Serializable]
    public sealed class VfxOneShotSettings
    {
        [LabelWidth(180)] public bool enabled = true;
        [LabelWidth(180)] public bool additive = true;
        [LabelWidth(180)] public Color color = Color.white;
        [LabelWidth(180), Min(1)] public int burstCount = 12;
        [LabelWidth(180), SuffixLabel("sec"), Min(0.01f)] public float duration = 0.2f;
        [LabelWidth(180), SuffixLabel("sec"), Min(0.01f)] public float lifetime = 0.35f;
        [LabelWidth(180), SuffixLabel("u/sec"), Min(0f)] public float speed = 2f;
        [LabelWidth(180), SuffixLabel("u"), Min(0.001f)] public float startSize = 0.1f;
        [LabelWidth(180), SuffixLabel("u"), Min(0f)] public float shapeRadius = 0.1f;
        [LabelWidth(180)] public float gravityModifier = 0f;
        [LabelWidth(180), Min(1)] public int maxParticles = 128;
        [LabelWidth(180), Min(0)] public int sortingOrder = 60;

        public static VfxOneShotSettings Default(Color color)
        {
            return Default(
                enabled: true,
                color: color,
                burstCount: 12,
                duration: 0.2f,
                lifetime: 0.35f,
                speed: 2f,
                size: 0.1f,
                radius: 0.1f,
                additive: true);
        }

        public static VfxOneShotSettings Default(
            bool enabled,
            Color color,
            int burstCount,
            float duration,
            float lifetime,
            float speed,
            float size,
            float radius,
            bool additive)
        {
            return new VfxOneShotSettings
            {
                enabled = enabled,
                color = color,
                burstCount = burstCount,
                duration = duration,
                lifetime = lifetime,
                speed = speed,
                startSize = size,
                shapeRadius = radius,
                additive = additive,
                maxParticles = Mathf.Max(64, burstCount * 2),
                sortingOrder = additive ? 70 : 60
            };
        }

        public bool EnsureDefaults()
        {
            bool changed = false;
            changed |= EnsureMinimum(ref burstCount, 12, 1);
            changed |= EnsureMinimum(ref duration, 0.2f, 0.01f);
            changed |= EnsureMinimum(ref lifetime, 0.35f, 0.01f);
            changed |= EnsureMinimum(ref speed, 2f, 0f);
            changed |= EnsureMinimum(ref startSize, 0.1f, 0.001f);
            changed |= EnsureMinimum(ref shapeRadius, 0.1f, 0f);
            changed |= EnsureMinimum(ref maxParticles, 64, 1);
            return changed;
        }

        private static bool EnsureMinimum(ref int value, int defaultValue, int minimum)
        {
            if (value >= minimum)
            {
                return false;
            }

            value = defaultValue;
            return true;
        }

        private static bool EnsureMinimum(ref float value, float defaultValue, float minimum)
        {
            if (value >= minimum)
            {
                return false;
            }

            value = defaultValue;
            return true;
        }
    }
}
