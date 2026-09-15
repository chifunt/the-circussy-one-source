using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TheCircussyOne.Content
{
    [CreateAssetMenu(menuName = "The Circussy One/Content/Enemy Catalog", fileName = "EnemyCatalog")]
    public sealed class EnemyCatalog : SerializedScriptableObject, IContentCatalog<EnemyDefinition>
    {
        private const string Tabs = "Enemy Catalog";

        [TabGroup(Tabs, "Definitions"), BoxGroup(Tabs + "/Definitions/Enemies"), LabelWidth(160), AssetSelector]
        public List<EnemyDefinition> enemies = new();

        [TabGroup(Tabs, "Definitions"), BoxGroup(Tabs + "/Definitions/Encore Enemies"), LabelWidth(160), AssetSelector]
        [InfoBox("Optional separate spawn pool used after Encore grace. Empty list falls back to the normal enemy list.")]
        public List<EnemyDefinition> encoreEnemies = new();

        [TabGroup(Tabs, "Diagnostics"), ShowInInspector, ReadOnly, LabelText("Default Spawn"), PropertyOrder(100)]
        private string DefaultSpawnSummary => DefaultEnemy != null ? DefaultEnemy.DisplayName : "None";

        public IReadOnlyList<EnemyDefinition> Enemies => enemies;
        public IReadOnlyList<EnemyDefinition> EncoreEnemies => encoreEnemies;
        public IReadOnlyList<EnemyDefinition> Definitions => enemies;
        public EnemyDefinition DefaultEnemy => FirstValidEnemy();

        public bool EnsureWorkflowDefaults(EnemyDefinition normalEnemy)
        {
            bool changed = false;
            enemies ??= new List<EnemyDefinition>();
            encoreEnemies ??= new List<EnemyDefinition>();
            if (normalEnemy != null && !enemies.Contains(normalEnemy))
            {
                enemies.Add(normalEnemy);
                changed = true;
            }

            return changed;
        }

        public bool TryPickSpawnEnemy(bool useEncorePool, float random01, out EnemyDefinition enemy)
        {
            enemy = PickWeighted(useEncorePool ? encoreEnemies : null, random01);
            if (enemy != null)
            {
                return true;
            }

            enemy = PickWeighted(enemies, random01);
            return enemy != null;
        }

        public List<ContentValidationIssue> ValidateContent()
        {
            var issues = ContentCatalogRules.ValidateDefinitions(Definitions);
            int validEnemyCount = 0;
            if (Definitions != null)
            {
                for (int i = 0; i < Definitions.Count; i++)
                {
                    EnemyDefinition enemy = Definitions[i];
                    if (enemy == null)
                    {
                        continue;
                    }

                    if (!enemy.isActive)
                    {
                        continue;
                    }

                    if (enemy.baseHealth <= 0)
                    {
                        AddIssue(issues, "enemy.non-positive-health", ContentValidationSeverity.Error, enemy, "must have positive base health.");
                    }
                    else if (enemy.isActive)
                    {
                        validEnemyCount++;
                    }

                    if (enemy.healthGrowthPerMinute < 0f)
                    {
                        AddIssue(issues, "enemy.negative-health-growth", ContentValidationSeverity.Error, enemy, "cannot have negative health growth.");
                    }

                    if (enemy.baseMoveSpeed <= 0f)
                    {
                        AddIssue(issues, "enemy.non-positive-move-speed", ContentValidationSeverity.Error, enemy, "must have positive move speed.");
                    }

                    if (enemy.moveSpeedGrowthPerMinute < 0f)
                    {
                        AddIssue(issues, "enemy.negative-move-speed-growth", ContentValidationSeverity.Error, enemy, "cannot have negative move speed growth.");
                    }

                    if (enemy.turnDegreesPerSecond <= 0f)
                    {
                        AddIssue(issues, "enemy.non-positive-turn-rate", ContentValidationSeverity.Error, enemy, "must have a positive turn rate.");
                    }

                    if (enemy.contactDamage <= 0)
                    {
                        AddIssue(issues, "enemy.non-positive-contact-damage", ContentValidationSeverity.Error, enemy, "must have positive contact damage.");
                    }

                    if (!System.Enum.IsDefined(typeof(EnemyBehaviorType), enemy.behaviorType))
                    {
                        AddIssue(issues, "enemy.invalid-behavior-type", ContentValidationSeverity.Error, enemy, "has an invalid behavior type.");
                    }

                    if (enemy.spawnWeight <= 0f)
                    {
                        AddIssue(issues, "enemy.non-positive-spawn-weight", ContentValidationSeverity.Error, enemy, "must have a positive spawn weight.");
                    }

                    if (enemy.xpBudget < 0)
                    {
                        AddIssue(issues, "enemy.negative-xp-budget", ContentValidationSeverity.Error, enemy, "cannot have a negative XP budget.");
                    }

                    ValidateBody(enemy, issues);
                    ValidateLocomotion(enemy, issues);
                    ValidateClimb(enemy, issues);
                    ValidateStack(enemy, issues);
                    ActorAnimationAuthoringRules.AddEnemyValidationIssues(enemy, issues);
                }
            }

            if (validEnemyCount == 0)
            {
                issues.Add(new ContentValidationIssue(
                    "enemy.no-valid-enemies",
                    ContentValidationSeverity.Error,
                    "Enemy catalog has no active valid positive-health enemies."));
            }

            return issues;
        }

        private EnemyDefinition FirstValidEnemy()
        {
            if (enemies == null)
            {
                return null;
            }

            for (int i = 0; i < enemies.Count; i++)
            {
                EnemyDefinition enemy = enemies[i];
                if (ContentAvailabilityRules.IsActiveAndValid(enemy) && enemy.baseHealth > 0)
                {
                    return enemy;
                }
            }

            return null;
        }

        private static EnemyDefinition PickWeighted(IReadOnlyList<EnemyDefinition> pool, float random01)
        {
            if (pool == null || pool.Count == 0)
            {
                return null;
            }

            float totalWeight = 0f;
            for (int i = 0; i < pool.Count; i++)
            {
                EnemyDefinition enemy = pool[i];
                if (!ContentAvailabilityRules.IsActiveAndValid(enemy) || enemy.baseHealth <= 0f || enemy.spawnWeight <= 0f)
                {
                    continue;
                }

                totalWeight += enemy.spawnWeight;
            }

            if (totalWeight <= 0f)
            {
                return null;
            }

            float pick = Mathf.Clamp01(random01) * totalWeight;
            float cursor = 0f;
            for (int i = 0; i < pool.Count; i++)
            {
                EnemyDefinition enemy = pool[i];
                if (!ContentAvailabilityRules.IsActiveAndValid(enemy) || enemy.baseHealth <= 0f || enemy.spawnWeight <= 0f)
                {
                    continue;
                }

                cursor += enemy.spawnWeight;
                if (pick <= cursor)
                {
                    return enemy;
                }
            }

            for (int i = pool.Count - 1; i >= 0; i--)
            {
                EnemyDefinition enemy = pool[i];
                if (ContentAvailabilityRules.IsActiveAndValid(enemy) && enemy.baseHealth > 0f && enemy.spawnWeight > 0f)
                {
                    return enemy;
                }
            }

            return null;
        }

        private static void ValidateLocomotion(EnemyDefinition enemy, List<ContentValidationIssue> issues)
        {
            if (enemy.locomotion == null)
            {
                AddIssue(issues, "enemy.missing-locomotion-profile", ContentValidationSeverity.Error, enemy, "is missing a locomotion profile.");
                return;
            }

            if (!System.Enum.IsDefined(typeof(EnemyLocomotionMode), enemy.locomotion.mode))
            {
                AddIssue(issues, "enemy.invalid-locomotion-mode", ContentValidationSeverity.Error, enemy, "has an invalid locomotion mode.");
            }

            if (enemy.locomotion.hoverHeight < 0f || enemy.locomotion.hoverSeconds <= 0f)
            {
                AddIssue(issues, "enemy.invalid-floating-hover", ContentValidationSeverity.Error, enemy, "has invalid floating hover values.");
            }

            if (enemy.locomotion.mode == EnemyLocomotionMode.Floating
                && enemy.stack != null
                && enemy.stack.policy == EnemyStackPolicy.SupportBased)
            {
                AddIssue(issues, "enemy.floating-support-stacking", ContentValidationSeverity.Warning, enemy, "is Floating but has support stacking enabled.");
            }
        }

        private static void ValidateBody(EnemyDefinition enemy, List<ContentValidationIssue> issues)
        {
            if (enemy.body == null)
            {
                AddIssue(issues, "enemy.missing-body-profile", ContentValidationSeverity.Error, enemy, "is missing a body profile.");
                return;
            }

            if (enemy.body.hurtboxRadius <= 0f)
            {
                AddIssue(issues, "enemy.non-positive-hurtbox-radius", ContentValidationSeverity.Error, enemy, "must have a positive hurtbox radius.");
            }

            if (enemy.body.contactHitboxSize.x <= 0f || enemy.body.contactHitboxSize.y <= 0f || enemy.body.contactHitboxSize.z <= 0f)
            {
                AddIssue(issues, "enemy.invalid-contact-hitbox-size", ContentValidationSeverity.Error, enemy, "must have positive contact hitbox dimensions.");
            }

            if (enemy.body.movementBodyRadius <= 0f || enemy.body.movementBodyHeight <= 0f)
            {
                AddIssue(issues, "enemy.invalid-movement-body", ContentValidationSeverity.Error, enemy, "must have positive movement body radius and height.");
            }
        }

        private static void ValidateClimb(EnemyDefinition enemy, List<ContentValidationIssue> issues)
        {
            if (enemy.climb == null)
            {
                AddIssue(issues, "enemy.missing-climb-profile", ContentValidationSeverity.Error, enemy, "is missing a climb profile.");
                return;
            }

            if (enemy.climb.climbSpeedMultiplier < 0f || enemy.climb.fallbackClimbSpeed < 0f)
            {
                AddIssue(issues, "enemy.invalid-climb-speed", ContentValidationSeverity.Error, enemy, "has invalid climb speed values.");
            }

            if (enemy.climb.gravity < 0f || enemy.climb.terminalFallSpeed <= 0f)
            {
                AddIssue(issues, "enemy.invalid-climb-gravity", ContentValidationSeverity.Error, enemy, "has invalid climb gravity or terminal fall speed.");
            }

            if (enemy.climb.environmentProbeIntervalFrames <= 0)
            {
                AddIssue(issues, "enemy.invalid-probe-interval", ContentValidationSeverity.Error, enemy, "must probe on a positive frame interval.");
            }
        }

        private static void ValidateStack(EnemyDefinition enemy, List<ContentValidationIssue> issues)
        {
            if (enemy.stack == null)
            {
                AddIssue(issues, "enemy.missing-stack-profile", ContentValidationSeverity.Error, enemy, "is missing a stack profile.");
                return;
            }

            if (enemy.stack.pileRadius <= 0f
                || enemy.stack.pileStartCount <= 0
                || enemy.stack.pileEnemiesPerLayer <= 0
                || enemy.stack.maxStackLayers < 0
                || enemy.stack.layerHeightMultiplier < 0f
                || enemy.stack.supportClimbSpeedMultiplier <= 0f
                || enemy.stack.supportClimbSeparationMultiplier < 0f
                || enemy.stack.supportClimbSeparationMultiplier > 1f)
            {
                AddIssue(issues, "enemy.invalid-stack-settings", ContentValidationSeverity.Error, enemy, "has invalid stack settings.");
            }

            if (enemy.stack.policy == EnemyStackPolicy.GroundOnly && enemy.stack.canClimbEnemies)
            {
                AddIssue(issues, "enemy.ground-only-climbs-enemies", ContentValidationSeverity.Warning, enemy, "is GroundOnly but still marked as able to climb enemies.");
            }
        }

        private static void AddIssue(
            List<ContentValidationIssue> issues,
            string code,
            ContentValidationSeverity severity,
            EnemyDefinition enemy,
            string message)
        {
            string id = enemy != null ? enemy.Id : null;
            issues.Add(new ContentValidationIssue(
                code,
                severity,
                $"Enemy '{id}' {message}",
                id));
        }
    }
}
