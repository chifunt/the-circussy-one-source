using UnityEngine;
using TheCircussyOne.Content;

namespace TheCircussyOne.Config
{
    public enum ConfigWorkflowKind
    {
        LiveRuntime,
        ProjectedVisual,
        StructuralReset
    }

    public readonly struct ConfigWorkflowDescriptor
    {
        public ConfigWorkflowDescriptor(ConfigWorkflowKind kind, string label, string summary, string detail)
        {
            Kind = kind;
            Label = label;
            Summary = summary;
            Detail = detail;
        }

        public ConfigWorkflowKind Kind { get; }
        public string Label { get; }
        public string Summary { get; }
        public string Detail { get; }
    }

    public static class ConfigWorkflowCatalog
    {
        public static ConfigWorkflowDescriptor LiveRuntime(string detail)
        {
            return new ConfigWorkflowDescriptor(
                ConfigWorkflowKind.LiveRuntime,
                "Live Runtime",
                "No scene rebuild. Values are read by runtime systems from config assets.",
                detail);
        }

        public static ConfigWorkflowDescriptor ProjectedVisual(string detail)
        {
            return new ConfigWorkflowDescriptor(
                ConfigWorkflowKind.ProjectedVisual,
                "Projected Visual",
                "Use targeted Apply only. Rebuild is only for generated scene structure resets.",
                detail);
        }

        public static ConfigWorkflowDescriptor StructuralReset(string detail)
        {
            return new ConfigWorkflowDescriptor(
                ConfigWorkflowKind.StructuralReset,
                "Structural Reset",
                "Rebuild regenerates the canonical prototype scene and generated prefab structure.",
                detail);
        }

        public static ConfigWorkflowDescriptor ForObject(Object config)
        {
            return config switch
            {
                GameConfig _ => GameConfig,
                RunScheduleConfig _ => RunScheduleConfig,
                RunWorldGenerationConfig _ => RunWorldGenerationConfig,
                WeaponDefinition _ => WeaponDefinition,
                WeaponCatalog _ => WeaponCatalog,
                ItemDefinition _ => ItemDefinition,
                ItemCatalog _ => ItemCatalog,
                ChestDefinition _ => ChestDefinition,
                ChestCatalog _ => ChestCatalog,
                TicketDepositDefinition _ => TicketDepositDefinition,
                TicketDepositCatalog _ => TicketDepositCatalog,
                WorldRewardPlacementCatalog _ => WorldRewardPlacementCatalog,
                HealthPickupDefinition _ => HealthPickupDefinition,
                HealthPickupCatalog _ => HealthPickupCatalog,
                HealingPropDefinition _ => HealingPropDefinition,
                HealingPropCatalog _ => HealingPropCatalog,
                UpgradeDefinition _ => UpgradeDefinition,
                UpgradeCatalog _ => UpgradeCatalog,
                TalentDefinition _ => TalentDefinition,
                TalentCatalog _ => TalentCatalog,
                PerformerDefinition _ => PerformerDefinition,
                PerformerCatalog _ => PerformerCatalog,
                XpGemDefinition _ => XpGemDefinition,
                XpGemCatalog _ => XpGemCatalog,
                EnemyDefinition _ => EnemyDefinition,
                EnemyCatalog _ => EnemyCatalog,
                HeadlinerDefinition _ => HeadlinerDefinition,
                HeadlinerCatalog _ => HeadlinerCatalog,
                CameraConfig _ => CameraConfig,
                ActorMotionVisualConfig _ => ActorMotionVisualConfig,
                GridVisualConfig _ => GridVisualConfig,
                LightingVisualConfig _ => LightingVisualConfig,
                DamageFeedbackVisualConfig _ => DamageFeedbackVisualConfig,
                EnemySpawnVisualConfig _ => EnemySpawnVisualConfig,
                VfxVisualConfig _ => VfxVisualConfig,
                GameAudioConfig _ => GameAudioConfig,
                GameHapticsConfig _ => GameHapticsConfig,
                HudVisualConfig _ => HudVisualConfig,
                XpGainCounterVisualConfig _ => XpGainCounterVisualConfig,
                WorldInteractionPromptVisualConfig _ => WorldInteractionPromptVisualConfig,
                _ => StructuralScene
            };
        }

        public static ConfigWorkflowDescriptor GameConfig => LiveRuntime(
            "Enemy speed and most balance fields affect the next Play Mode start, or newly spawned entities during Play Mode. Existing enemies keep spawn-time values.");

        public static ConfigWorkflowDescriptor RunScheduleConfig => LiveRuntime(
            "The run schedule drives Act countdowns, Showtime timing, and temporary spawn pressure. Existing active Acts keep their current schedule state until the next Act or restart.");

        public static ConfigWorkflowDescriptor RunWorldGenerationConfig => LiveRuntime(
            "The world generation config drives deterministic Act heightfield data, runtime mesh terrain, arena barrier visuals, and the Big Top environment shell.");

        public static ConfigWorkflowDescriptor WeaponDefinition => LiveRuntime(
            "Weapon definitions drive runtime firing and newly spawned projectile stats. Existing runtime projectiles keep spawn-time values.");

        public static ConfigWorkflowDescriptor WeaponCatalog => LiveRuntime(
            "The starting weapon catalog is read when a run starts. Scene rebuild is only needed if the LifetimeScope reference is missing.");

        public static ConfigWorkflowDescriptor ItemDefinition => LiveRuntime(
            "Passive item definitions are read by future world reward systems. Existing runtime item stacks keep their acquired values.");

        public static ConfigWorkflowDescriptor ItemCatalog => LiveRuntime(
            "The item catalog is read by future world reward systems. Items do not appear in normal level-up rolls for v0.3.");

        public static ConfigWorkflowDescriptor ChestDefinition => LiveRuntime(
            "Chest definitions drive Ticket cost, hold time, item rarity odds, prompt text, and newly spawned chest visuals. Existing runtime chests keep start-time values.");

        public static ConfigWorkflowDescriptor ChestCatalog => LiveRuntime(
            "The chest catalog is read by world reward systems. Scene rebuild is only needed if the LifetimeScope reference is missing or generated fixtures need reset.");

        public static ConfigWorkflowDescriptor TicketDepositDefinition => LiveRuntime(
            "Ticket deposit definitions drive hold time, payout range, prompt text, and newly spawned deposit visuals. Existing runtime deposits keep start-time values.");

        public static ConfigWorkflowDescriptor TicketDepositCatalog => LiveRuntime(
            "The Ticket deposit catalog is read by world interaction systems. Scene rebuild is only needed if spawned deposit references or generated fixtures are missing.");

        public static ConfigWorkflowDescriptor WorldRewardPlacementCatalog => LiveRuntime(
            "The world reward placement catalog controls where chests, Ticket deposits, snack props, and future shrine placements spawn at run start.");

        public static ConfigWorkflowDescriptor UpgradeDefinition => LiveRuntime(
            "Upgrade definitions are read when level-up choices roll. Already-applied runtime modifiers remain until the run ends.");

        public static ConfigWorkflowDescriptor UpgradeCatalog => LiveRuntime(
            "The upgrade catalog and rarity weights are read when choices roll. Scene rebuild is only needed if the LifetimeScope reference is missing.");

        public static ConfigWorkflowDescriptor TalentDefinition => LiveRuntime(
            "Talent definitions are shared level-up cards. Existing acquired talent levels keep their run-time modifiers until the run ends.");

        public static ConfigWorkflowDescriptor TalentCatalog => LiveRuntime(
            "The talent catalog is read when level-up choices roll. Scene rebuild is only needed if the LifetimeScope reference is missing.");

        public static ConfigWorkflowDescriptor PerformerDefinition => LiveRuntime(
            "Performer definitions drive run-start loadout, base stats, passives, and talent pool access. Existing runs keep their selected performer.");

        public static ConfigWorkflowDescriptor PerformerCatalog => LiveRuntime(
            "The performer catalog is read at run start. Scene rebuild is only needed if the LifetimeScope reference is missing.");

        public static ConfigWorkflowDescriptor XpGemDefinition => LiveRuntime(
            "XP gem definitions drive newly spawned pickup amount and color. Existing runtime pickups keep spawn-time values.");

        public static ConfigWorkflowDescriptor XpGemCatalog => LiveRuntime(
            "The XP gem catalog and enemy drop budget are read when drops spawn. Scene rebuild is only needed if the LifetimeScope reference is missing.");

        public static ConfigWorkflowDescriptor HealthPickupDefinition => LiveRuntime(
            "Treat pickup definitions drive newly spawned healing amount, shape, color, and glow. Existing runtime pickups keep spawn-time values.");

        public static ConfigWorkflowDescriptor HealthPickupCatalog => LiveRuntime(
            "The health pickup catalog is read by healing props when pickups spawn. Scene rebuild is only needed if the LifetimeScope reference is missing.");

        public static ConfigWorkflowDescriptor HealingPropDefinition => LiveRuntime(
            "Snack prop definitions drive hold time, prompt text, Treat payout range, and newly spawned prop visuals.");

        public static ConfigWorkflowDescriptor HealingPropCatalog => LiveRuntime(
            "The snack prop catalog is read by world interaction systems. Scene rebuild is only needed if spawned prop references or generated fixtures are missing.");

        public static ConfigWorkflowDescriptor EnemyDefinition => LiveRuntime(
            "Enemy definitions drive newly spawned enemy stats, hitboxes, climbing, stacking policy, and XP drop budget. Existing enemies keep spawn-time values.");

        public static ConfigWorkflowDescriptor EnemyCatalog => LiveRuntime(
            "The enemy catalog is read when enemies spawn. Scene rebuild is only needed if the LifetimeScope reference is missing.");

        public static ConfigWorkflowDescriptor HeadlinerDefinition => LiveRuntime(
            "Headliner definitions drive act-ending boss actor selection and spawn placement. Active Headliners are read when the boss phase starts.");

        public static ConfigWorkflowDescriptor HeadlinerCatalog => LiveRuntime(
            "The Headliner catalog is read when the act-ending Headliner arrives. Scene rebuild is only needed if the LifetimeScope reference is missing.");

        public static ConfigWorkflowDescriptor CameraConfig => LiveRuntime(
            "Camera tuning is read by the camera system at runtime. Restart Play Mode to refresh the starting orbit pose.");

        public static ConfigWorkflowDescriptor ActorMotionVisualConfig => LiveRuntime(
            "Actor squash/stretch is visual-only runtime tuning. It does not need generated scene changes.");

        public static ConfigWorkflowDescriptor GridVisualConfig => ProjectedVisual(
            "Apply regenerates grid textures and generated materials. Gameplay tuning and scene topology are untouched.");

        public static ConfigWorkflowDescriptor LightingVisualConfig => ProjectedVisual(
            "Apply updates RenderSettings, the directional light, camera post-processing, the volume profile, and contact-shadow assets.");

        public static ConfigWorkflowDescriptor DamageFeedbackVisualConfig => ProjectedVisual(
            "Apply regenerates damage number material/prefab assets. Combat damage values live in GameConfig.");

        public static ConfigWorkflowDescriptor EnemySpawnVisualConfig => ProjectedVisual(
            "Apply regenerates the spawn indicator prefab. Spawn rate and enemy balance still live in GameConfig.");

        public static ConfigWorkflowDescriptor VfxVisualConfig => ProjectedVisual(
            "Apply regenerates built-in ParticleSystem prefabs and shared VFX materials. Gameplay values remain in GameConfig and runtime systems.");

        public static ConfigWorkflowDescriptor GameAudioConfig => LiveRuntime(
            "Wwise event names, pitch ranges, loop RTPCs, and cooldowns are read by runtime audio systems. Re-run the Wwise import tool after changing source audio files or SoundBank content.");

        public static ConfigWorkflowDescriptor GameHapticsConfig => LiveRuntime(
            "Controller haptics are read by runtime haptics systems. Per-cue toggles, strengths, timings, priorities, and cooldowns are live runtime tuning.");

        public static ConfigWorkflowDescriptor HudVisualConfig => ProjectedVisual(
            "Apply updates UI Toolkit panel scaling and the open-scene HUD preview. Rebuild is only needed if the HUD object or UXML structure is missing.");

        public static ConfigWorkflowDescriptor XpGainCounterVisualConfig => ProjectedVisual(
            "Apply regenerates the world-space XP gain counter TMP material and prefab. XP values and leveling still live in GameConfig.");

        public static ConfigWorkflowDescriptor WorldInteractionPromptVisualConfig => ProjectedVisual(
            "Apply regenerates the world-space interaction prompt TMP material and prefab. Interaction radius and hold rules still live in GameConfig and interactable content.");

        public static ConfigWorkflowDescriptor StructuralScene => StructuralReset(
            "Use only when generated roots, prefabs, UI documents, DI wiring, layers, or scene topology need to be reset.");
    }
}
