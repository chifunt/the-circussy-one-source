using System.Collections.Generic;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using UnityEditor;


public static partial class AuthoringDoctorRunner
{
    private sealed class WeaponContentCheck : IAuthoringCheck
    {
        public void Run(List<AuthoringCheckResult> results)
        {
            var catalog = AssetDatabase.LoadAssetAtPath<WeaponCatalog>(TheCircussyOneAssetPaths.WeaponCatalogPath);
            if (catalog == null)
            {
                return;
            }

            results.AddRange(WeaponContentResults(catalog.ValidateContent(), TheCircussyOneAssetPaths.WeaponCatalogPath));
            AddUncatalogedActiveResults(ContentWorkbenchDomain.Weapons, results);
        }
    }

    private sealed class RunScheduleContentCheck : IAuthoringCheck
    {
        public void Run(List<AuthoringCheckResult> results)
        {
            var config = AssetDatabase.LoadAssetAtPath<RunScheduleConfig>(TheCircussyOneAssetPaths.RunScheduleConfigPath);
            if (config == null)
            {
                return;
            }

            results.AddRange(RunScheduleContentResults(config.ValidateContent(), TheCircussyOneAssetPaths.RunScheduleConfigPath));
        }
    }

    private sealed class UpgradeContentCheck : IAuthoringCheck
    {
        public void Run(List<AuthoringCheckResult> results)
        {
            var catalog = AssetDatabase.LoadAssetAtPath<UpgradeCatalog>(TheCircussyOneAssetPaths.UpgradeCatalogPath);
            if (catalog == null)
            {
                return;
            }

            results.AddRange(UpgradeContentResults(catalog.ValidateContent(), TheCircussyOneAssetPaths.UpgradeCatalogPath));
            AddUncatalogedActiveResults(ContentWorkbenchDomain.Upgrades, results);
        }
    }

    private sealed class ItemContentCheck : IAuthoringCheck
    {
        public void Run(List<AuthoringCheckResult> results)
        {
            var catalog = AssetDatabase.LoadAssetAtPath<ItemCatalog>(TheCircussyOneAssetPaths.ItemCatalogPath);
            if (catalog == null)
            {
                return;
            }

            results.AddRange(ItemContentResults(catalog.ValidateContent(), TheCircussyOneAssetPaths.ItemCatalogPath));
        }
    }

    private sealed class ChestContentCheck : IAuthoringCheck
    {
        public void Run(List<AuthoringCheckResult> results)
        {
            var catalog = AssetDatabase.LoadAssetAtPath<ChestCatalog>(TheCircussyOneAssetPaths.ChestCatalogPath);
            if (catalog == null)
            {
                return;
            }

            results.AddRange(ChestContentResults(catalog.ValidateContent(), TheCircussyOneAssetPaths.ChestCatalogPath));
            AddUncatalogedActiveResults(ContentWorkbenchDomain.Chests, results);
        }
    }

    private sealed class TicketDepositContentCheck : IAuthoringCheck
    {
        public void Run(List<AuthoringCheckResult> results)
        {
            var catalog = AssetDatabase.LoadAssetAtPath<TicketDepositCatalog>(TheCircussyOneAssetPaths.TicketDepositCatalogPath);
            if (catalog == null)
            {
                return;
            }

            results.AddRange(TicketDepositContentResults(catalog.ValidateContent(), TheCircussyOneAssetPaths.TicketDepositCatalogPath));
        }
    }

    private sealed class HealthPickupContentCheck : IAuthoringCheck
    {
        public void Run(List<AuthoringCheckResult> results)
        {
            var catalog = AssetDatabase.LoadAssetAtPath<HealthPickupCatalog>(TheCircussyOneAssetPaths.HealthPickupCatalogPath);
            if (catalog == null)
            {
                return;
            }

            results.AddRange(HealthPickupContentResults(catalog.ValidateContent(), TheCircussyOneAssetPaths.HealthPickupCatalogPath));
        }
    }

    private sealed class HealingPropContentCheck : IAuthoringCheck
    {
        public void Run(List<AuthoringCheckResult> results)
        {
            var catalog = AssetDatabase.LoadAssetAtPath<HealingPropCatalog>(TheCircussyOneAssetPaths.HealingPropCatalogPath);
            if (catalog == null)
            {
                return;
            }

            results.AddRange(HealingPropContentResults(catalog.ValidateContent(), TheCircussyOneAssetPaths.HealingPropCatalogPath));
        }
    }

    private sealed class WorldRewardPlacementContentCheck : IAuthoringCheck
    {
        public void Run(List<AuthoringCheckResult> results)
        {
            var catalog = AssetDatabase.LoadAssetAtPath<WorldRewardPlacementCatalog>(TheCircussyOneAssetPaths.WorldRewardPlacementCatalogPath);
            if (catalog == null)
            {
                return;
            }

            results.AddRange(WorldRewardPlacementContentResults(catalog.ValidateContent(), TheCircussyOneAssetPaths.WorldRewardPlacementCatalogPath));
        }
    }

    private sealed class TalentContentCheck : IAuthoringCheck
    {
        public void Run(List<AuthoringCheckResult> results)
        {
            var catalog = AssetDatabase.LoadAssetAtPath<TalentCatalog>(TheCircussyOneAssetPaths.TalentCatalogPath);
            if (catalog == null)
            {
                return;
            }

            results.AddRange(TalentContentResults(catalog.ValidateContent(), TheCircussyOneAssetPaths.TalentCatalogPath));
            AddUncatalogedActiveResults(ContentWorkbenchDomain.Talents, results);
        }
    }

    private sealed class CharacterContentCheck : IAuthoringCheck
    {
        public void Run(List<AuthoringCheckResult> results)
        {
            var catalog = AssetDatabase.LoadAssetAtPath<PerformerCatalog>(TheCircussyOneAssetPaths.PerformerCatalogPath);
            if (catalog == null)
            {
                return;
            }

            results.AddRange(CharacterContentResults(catalog.ValidateContent(), TheCircussyOneAssetPaths.PerformerCatalogPath));
            AddUncatalogedActiveResults(ContentWorkbenchDomain.Performers, results);
        }
    }

    private sealed class XpGemContentCheck : IAuthoringCheck
    {
        public void Run(List<AuthoringCheckResult> results)
        {
            var catalog = AssetDatabase.LoadAssetAtPath<XpGemCatalog>(TheCircussyOneAssetPaths.XpGemCatalogPath);
            if (catalog == null)
            {
                return;
            }

            results.AddRange(XpGemContentResults(catalog.ValidateContent(), TheCircussyOneAssetPaths.XpGemCatalogPath));
        }
    }

    private sealed class EnemyContentCheck : IAuthoringCheck
    {
        public void Run(List<AuthoringCheckResult> results)
        {
            var catalog = AssetDatabase.LoadAssetAtPath<EnemyCatalog>(TheCircussyOneAssetPaths.EnemyCatalogPath);
            if (catalog == null)
            {
                return;
            }

            results.AddRange(EnemyContentResults(catalog.ValidateContent(), TheCircussyOneAssetPaths.EnemyCatalogPath));
            AddUncatalogedActiveResults(ContentWorkbenchDomain.Enemies, results);
        }
    }

    private sealed class HeadlinerContentCheck : IAuthoringCheck
    {
        public void Run(List<AuthoringCheckResult> results)
        {
            var catalog = AssetDatabase.LoadAssetAtPath<HeadlinerCatalog>(TheCircussyOneAssetPaths.HeadlinerCatalogPath);
            if (catalog == null)
            {
                return;
            }

            results.AddRange(HeadlinerContentResults(catalog.ValidateContent(), TheCircussyOneAssetPaths.HeadlinerCatalogPath));
            AddUncatalogedActiveResults(ContentWorkbenchDomain.Headliners, results);
        }
    }

    private static void AddUncatalogedActiveResults(ContentWorkbenchDomain domain, List<AuthoringCheckResult> results)
    {
        IReadOnlyList<ContentWorkbenchEntry> entries = ContentWorkbenchService.LoadEntries(domain);
        for (int i = 0; i < entries.Count; i++)
        {
            ContentWorkbenchEntry entry = entries[i];
            if (entry == null || entry.IsCataloged || !entry.IsActive)
            {
                continue;
            }

            results.Add(new AuthoringCheckResult(
                $"content.{domain.ToString().ToLowerInvariant()}.active-not-in-catalog.{entry.Id}",
                "Content",
                AuthoringCheckSeverity.Warning,
                $"{domain} content is active but uncataloged",
                $"'{entry.DisplayName}' exists at {entry.Path}, but it is not listed in the {domain} catalog and will not appear in normal runtime flows.",
                entry.Path));
        }
    }
}
