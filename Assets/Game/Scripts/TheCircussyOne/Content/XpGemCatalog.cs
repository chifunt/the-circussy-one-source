using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using TheCircussyOne.Rules;

namespace TheCircussyOne.Content
{
    [CreateAssetMenu(menuName = "The Circussy One/Content/XP Gem Catalog", fileName = "XpGemCatalog")]
    public sealed class XpGemCatalog : SerializedScriptableObject, IContentCatalog<XpGemDefinition>
    {
        private const string Tabs = "XP Gem Catalog";

        [TabGroup(Tabs, "Definitions"), BoxGroup(Tabs + "/Definitions/Gems"), LabelWidth(160), AssetSelector]
        public List<XpGemDefinition> gems = new();

        [TabGroup(Tabs, "Enemy Drops"), BoxGroup(Tabs + "/Enemy Drops/Default Budget"), LabelWidth(180)]
        [Min(0)] public int defaultEnemyXpBudget = XpGemDefinition.DefaultBlueXpAmount;

        [TabGroup(Tabs, "Enemy Drops"), BoxGroup(Tabs + "/Enemy Drops/Default Budget"), LabelWidth(180)]
        [EnumToggleButtons] public XpDropStyle defaultDropStyle = XpDropStyle.Compact;

        [TabGroup(Tabs, "Diagnostics"), ShowInInspector, ReadOnly, LabelText("Valid Gems"), PropertyOrder(100)]
        private int ValidGemCount => XpDropRules.ValidGemCount(Definitions);

        public IReadOnlyList<XpGemDefinition> Gems => gems;
        public IReadOnlyList<XpGemDefinition> Definitions => gems;

        public List<ContentValidationIssue> ValidateContent()
        {
            var issues = ContentCatalogRules.ValidateDefinitions(Definitions);
            int validGemCount = 0;
            if (Definitions != null)
            {
                for (int i = 0; i < Definitions.Count; i++)
                {
                    XpGemDefinition gem = Definitions[i];
                    if (gem == null)
                    {
                        continue;
                    }

                    if (gem.xpAmount <= 0)
                    {
                        issues.Add(new ContentValidationIssue(
                            "xp-gem.non-positive-xp",
                            ContentValidationSeverity.Error,
                            $"XP gem '{gem.Id}' must grant a positive XP amount.",
                            gem.Id));
                    }
                    else
                    {
                        validGemCount++;
                    }

                    if (gem.visualScale <= 0f)
                    {
                        issues.Add(new ContentValidationIssue(
                            "xp-gem.non-positive-scale",
                            ContentValidationSeverity.Warning,
                            $"XP gem '{gem.Id}' has a non-positive visual scale.",
                            gem.Id));
                    }
                }
            }

            if (validGemCount == 0)
            {
                issues.Add(new ContentValidationIssue(
                    "xp-gem.no-valid-gems",
                    ContentValidationSeverity.Error,
                    "XP gem catalog has no valid positive-value gems."));
            }

            if (defaultEnemyXpBudget < 0)
            {
                issues.Add(new ContentValidationIssue(
                    "xp-drop.negative-budget",
                    ContentValidationSeverity.Error,
                    "Default enemy XP budget cannot be negative."));
            }

            return issues;
        }

        public bool EnsureWorkflowDefaults(XpGemDefinition blueGem, XpGemDefinition greenGem, XpGemDefinition redGem, int defaultBudget)
        {
            bool changed = false;
            gems ??= new List<XpGemDefinition>();
            changed |= AddIfMissing(blueGem);
            changed |= AddIfMissing(greenGem);
            changed |= AddIfMissing(redGem);
            if (defaultEnemyXpBudget < 0)
            {
                defaultEnemyXpBudget = Mathf.Max(0, defaultBudget);
                changed = true;
            }

            return changed;
        }

        private bool AddIfMissing(XpGemDefinition gem)
        {
            if (gem == null || gems.Contains(gem))
            {
                return false;
            }

            gems.Add(gem);
            return true;
        }
    }
}
