using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TheCircussyOne.Content
{
    [CreateAssetMenu(menuName = "The Circussy One/Content/Headliner Catalog", fileName = "HeadlinerCatalog")]
    public sealed class HeadlinerCatalog : SerializedScriptableObject, IContentCatalog<HeadlinerDefinition>
    {
        private const string Tabs = "Headliner Catalog";

        [TabGroup(Tabs, "Definitions"), BoxGroup(Tabs + "/Definitions/Headliners"), LabelWidth(160), AssetSelector]
        public List<HeadlinerDefinition> headliners = new();

        [TabGroup(Tabs, "Diagnostics"), ShowInInspector, ReadOnly, LabelText("Default Headliner"), PropertyOrder(100)]
        private string DefaultHeadlinerSummary => DefaultHeadliner != null ? DefaultHeadliner.DisplayName : "None";

        public IReadOnlyList<HeadlinerDefinition> Headliners => headliners;
        public IReadOnlyList<HeadlinerDefinition> Definitions => headliners;
        public HeadlinerDefinition DefaultHeadliner => FirstActiveHeadliner();

        public bool EnsureWorkflowDefaults(HeadlinerDefinition defaultHeadliner)
        {
            bool changed = false;
            headliners ??= new List<HeadlinerDefinition>();
            if (defaultHeadliner != null && !headliners.Contains(defaultHeadliner))
            {
                headliners.Add(defaultHeadliner);
                changed = true;
            }

            return changed;
        }

        public HeadlinerDefinition ForAct(int actNumber)
        {
            if (headliners == null)
            {
                return null;
            }

            int normalizedAct = Mathf.Max(1, actNumber);
            for (int i = 0; i < headliners.Count; i++)
            {
                HeadlinerDefinition headliner = headliners[i];
                if (ContentAvailabilityRules.IsActiveAndValid(headliner) && headliner.actNumber == normalizedAct)
                {
                    return headliner;
                }
            }

            return DefaultHeadliner;
        }

        public List<ContentValidationIssue> ValidateContent()
        {
            var issues = ContentCatalogRules.ValidateDefinitions(Definitions);
            int validHeadlinerCount = 0;
            if (Definitions != null)
            {
                for (int i = 0; i < Definitions.Count; i++)
                {
                    HeadlinerDefinition headliner = Definitions[i];
                    if (headliner == null || !headliner.isActive)
                    {
                        continue;
                    }

                    if (headliner.actNumber <= 0)
                    {
                        AddIssue(issues, "headliner.invalid-act", ContentValidationSeverity.Error, headliner, "must target a positive Act number.");
                    }

                    if (headliner.enemyActor == null)
                    {
                        AddIssue(issues, "headliner.missing-enemy-actor", ContentValidationSeverity.Error, headliner, "must reference an enemy actor definition.");
                    }
                    else
                    {
                        ValidateEnemyActor(headliner, issues);
                    }

                    if (headliner.spawnDistanceFromPlayer < 0f
                        || headliner.spawnGroundClearance < 0f
                        || headliner.spawnProbeHeight < 0f
                        || headliner.spawnProbeDepth < 0f)
                    {
                        AddIssue(issues, "headliner.invalid-spawn", ContentValidationSeverity.Error, headliner, "has invalid spawn placement values.");
                    }

                    if (ContentAvailabilityRules.IsActiveAndValid(headliner)
                        && ContentAvailabilityRules.IsActiveAndValid(headliner.enemyActor)
                        && headliner.enemyActor.baseHealth > 0)
                    {
                        validHeadlinerCount++;
                    }
                }
            }

            if (validHeadlinerCount == 0)
            {
                issues.Add(new ContentValidationIssue(
                    "headliner.no-valid-headliners",
                    ContentValidationSeverity.Error,
                    "Headliner catalog has no active valid Headliner with a positive-health enemy actor."));
            }

            return issues;
        }

        private HeadlinerDefinition FirstActiveHeadliner()
        {
            if (headliners == null)
            {
                return null;
            }

            for (int i = 0; i < headliners.Count; i++)
            {
                HeadlinerDefinition headliner = headliners[i];
                if (ContentAvailabilityRules.IsActiveAndValid(headliner)
                    && headliner.enemyActor != null
                    && ContentAvailabilityRules.IsActiveAndValid(headliner.enemyActor))
                {
                    return headliner;
                }
            }

            return null;
        }

        private static void ValidateEnemyActor(HeadlinerDefinition headliner, List<ContentValidationIssue> issues)
        {
            EnemyDefinition enemy = headliner.enemyActor;
            if (!ContentAvailabilityRules.IsActiveAndValid(enemy))
            {
                AddIssue(issues, "headliner.inactive-enemy-actor", ContentValidationSeverity.Error, headliner, "references an inactive or invalid enemy actor.");
            }

            if (enemy.baseHealth <= 0)
            {
                AddIssue(issues, "headliner.non-positive-health", ContentValidationSeverity.Error, headliner, "enemy actor must have positive base health.");
            }

            if (enemy.baseMoveSpeed <= 0f)
            {
                AddIssue(issues, "headliner.non-positive-move-speed", ContentValidationSeverity.Error, headliner, "enemy actor must have positive move speed.");
            }

            if (enemy.contactDamage <= 0)
            {
                AddIssue(issues, "headliner.non-positive-contact-damage", ContentValidationSeverity.Error, headliner, "enemy actor must have positive contact damage.");
            }

            if (enemy.body == null || enemy.body.hurtboxRadius <= 0f)
            {
                AddIssue(issues, "headliner.invalid-hurtbox", ContentValidationSeverity.Error, headliner, "enemy actor must have a valid hurtbox.");
            }
        }

        private static void AddIssue(
            List<ContentValidationIssue> issues,
            string code,
            ContentValidationSeverity severity,
            HeadlinerDefinition headliner,
            string message)
        {
            string id = headliner != null ? headliner.Id : string.Empty;
            issues.Add(new ContentValidationIssue(
                code,
                severity,
                $"Headliner '{id}' {message}",
                id));
        }
    }
}
