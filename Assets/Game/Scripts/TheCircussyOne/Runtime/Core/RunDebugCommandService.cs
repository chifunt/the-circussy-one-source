using System;
using System.Collections.Generic;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.Rules;
using TheCircussyOne.Visuals;
using UnityEngine;

namespace TheCircussyOne.Runtime
{
    public readonly struct RunDebugOption
    {
        public RunDebugOption(string id, string label, bool active = true, bool owned = false)
        {
            Id = id ?? string.Empty;
            Label = label ?? string.Empty;
            Active = active;
            Owned = owned;
        }

        public string Id { get; }
        public string Label { get; }
        public bool Active { get; }
        public bool Owned { get; }
        public bool IsValid => !string.IsNullOrWhiteSpace(Id);
    }

    public readonly struct RunDebugSnapshot
    {
        public RunDebugSnapshot(
            bool available,
            int tickets,
            int health,
            int maxHealth,
            int experience,
            int level,
            RunPhase phase,
            float phaseElapsedSeconds,
            int worldIndex,
            IReadOnlyList<RunDebugOption> weapons,
            IReadOnlyList<RunDebugOption> ownedWeapons,
            IReadOnlyList<RunDebugOption> items,
            IReadOnlyList<RunDebugOption> talents,
            IReadOnlyList<RunDebugOption> upgrades,
            bool hasActSchedule = false,
            int actNumber = 0,
            string actName = null,
            float actElapsedSeconds = 0f,
            float actRemainingSeconds = 0f,
            float nextShowtimeRemainingSeconds = -1f,
            bool isShowtimeActive = false,
            float showtimeRemainingSeconds = 0f,
            bool isActFinaleDue = false,
            bool isEncoreActive = false,
            float encoreElapsedSeconds = 0f,
            int encorePressureStep = 0,
            bool isEncoreGraceActive = false,
            float encoreGraceRemainingSeconds = 0f,
            float encorePressureElapsedSeconds = 0f,
            RunWorldLayoutMode worldLayoutMode = RunWorldLayoutMode.Generated)
        {
            Available = available;
            Tickets = tickets;
            Health = health;
            MaxHealth = maxHealth;
            Experience = experience;
            Level = level;
            Phase = phase;
            PhaseElapsedSeconds = phaseElapsedSeconds;
            WorldIndex = worldIndex;
            Weapons = weapons ?? Array.Empty<RunDebugOption>();
            OwnedWeapons = ownedWeapons ?? Array.Empty<RunDebugOption>();
            Items = items ?? Array.Empty<RunDebugOption>();
            Talents = talents ?? Array.Empty<RunDebugOption>();
            Upgrades = upgrades ?? Array.Empty<RunDebugOption>();
            HasActSchedule = hasActSchedule;
            ActNumber = actNumber;
            ActName = actName ?? string.Empty;
            ActElapsedSeconds = actElapsedSeconds;
            ActRemainingSeconds = actRemainingSeconds;
            NextShowtimeRemainingSeconds = nextShowtimeRemainingSeconds;
            IsShowtimeActive = isShowtimeActive;
            ShowtimeRemainingSeconds = showtimeRemainingSeconds;
            IsActFinaleDue = isActFinaleDue;
            IsEncoreActive = isEncoreActive;
            EncoreElapsedSeconds = encoreElapsedSeconds;
            EncorePressureStep = encorePressureStep;
            IsEncoreGraceActive = isEncoreGraceActive;
            EncoreGraceRemainingSeconds = encoreGraceRemainingSeconds;
            EncorePressureElapsedSeconds = encorePressureElapsedSeconds;
            WorldLayoutMode = worldLayoutMode;
        }

        public bool Available { get; }
        public int Tickets { get; }
        public int Health { get; }
        public int MaxHealth { get; }
        public int Experience { get; }
        public int Level { get; }
        public RunPhase Phase { get; }
        public float PhaseElapsedSeconds { get; }
        public int WorldIndex { get; }
        public IReadOnlyList<RunDebugOption> Weapons { get; }
        public IReadOnlyList<RunDebugOption> OwnedWeapons { get; }
        public IReadOnlyList<RunDebugOption> Items { get; }
        public IReadOnlyList<RunDebugOption> Talents { get; }
        public IReadOnlyList<RunDebugOption> Upgrades { get; }
        public bool HasActSchedule { get; }
        public int ActNumber { get; }
        public string ActName { get; }
        public float ActElapsedSeconds { get; }
        public float ActRemainingSeconds { get; }
        public float NextShowtimeRemainingSeconds { get; }
        public bool IsShowtimeActive { get; }
        public float ShowtimeRemainingSeconds { get; }
        public bool IsActFinaleDue { get; }
        public bool IsEncoreActive { get; }
        public float EncoreElapsedSeconds { get; }
        public int EncorePressureStep { get; }
        public bool IsEncoreGraceActive { get; }
        public float EncoreGraceRemainingSeconds { get; }
        public float EncorePressureElapsedSeconds { get; }
        public RunWorldLayoutMode WorldLayoutMode { get; }
    }

    public sealed class RunDebugCommandService
    {
        private readonly WeaponCatalog weaponCatalog;
        private readonly ItemCatalog itemCatalog;
        private readonly TalentCatalog talentCatalog;
        private readonly UpgradeCatalog upgradeCatalog;
        private readonly WeaponLoadout weaponLoadout;
        private readonly ItemInventory itemInventory;
        private readonly RunCurrencyState currencyState;
        private readonly GameState state;
        private readonly UpgradeRunState upgradeRunState;
        private readonly TalentEffectApplier talentEffectApplier;
        private readonly UpgradeEffectApplier upgradeEffectApplier;
        private readonly RunPhaseState phaseState;
        private readonly RunActScheduleState scheduleState;
        private readonly RunWorldLifecycleSystem worldLifecycle;
        private readonly RunSeedState runSeedState;
        private readonly RunWorldLayoutState worldLayoutState;
        private readonly RunResetCoordinator resetCoordinator;
        private readonly RunPlayerStartPlacementSystem playerStartPlacement;
        private readonly HeadlinerDefeatState headlinerDefeatState;
        private readonly PlayerView player;

        public RunDebugCommandService(
            WeaponCatalog weaponCatalog,
            ItemCatalog itemCatalog,
            TalentCatalog talentCatalog,
            UpgradeCatalog upgradeCatalog,
            WeaponLoadout weaponLoadout,
            ItemInventory itemInventory,
            RunCurrencyState currencyState,
            GameState state,
            UpgradeRunState upgradeRunState,
            TalentEffectApplier talentEffectApplier,
            UpgradeEffectApplier upgradeEffectApplier,
            RunPhaseState phaseState = null,
            RunActScheduleState scheduleState = null,
            RunWorldLifecycleSystem worldLifecycle = null,
            RunSeedState runSeedState = null,
            RunWorldLayoutState worldLayoutState = null,
            RunResetCoordinator resetCoordinator = null,
            RunPlayerStartPlacementSystem playerStartPlacement = null,
            HeadlinerDefeatState headlinerDefeatState = null,
            PlayerView player = null)
        {
            this.weaponCatalog = weaponCatalog;
            this.itemCatalog = itemCatalog;
            this.talentCatalog = talentCatalog;
            this.upgradeCatalog = upgradeCatalog;
            this.weaponLoadout = weaponLoadout;
            this.itemInventory = itemInventory;
            this.currencyState = currencyState;
            this.state = state;
            this.upgradeRunState = upgradeRunState;
            this.talentEffectApplier = talentEffectApplier;
            this.upgradeEffectApplier = upgradeEffectApplier;
            this.phaseState = phaseState;
            this.scheduleState = scheduleState;
            this.worldLifecycle = worldLifecycle;
            this.runSeedState = runSeedState;
            this.worldLayoutState = worldLayoutState;
            this.resetCoordinator = resetCoordinator;
            this.playerStartPlacement = playerStartPlacement;
            this.headlinerDefeatState = headlinerDefeatState;
            this.player = player;
        }

        public bool IsAvailable => Application.isEditor || Debug.isDebugBuild;

        public RunDebugSnapshot BuildSnapshot()
        {
            float nextShowtime = -1f;
            if (scheduleState != null)
            {
                _ = scheduleState.TryGetNextShowtimeRemainingSeconds(out nextShowtime);
            }

            return new RunDebugSnapshot(
                IsAvailable,
                currencyState?.Tickets ?? 0,
                state?.Health ?? 0,
                state?.MaxHealth ?? 0,
                state?.Experience ?? 0,
                state?.Level ?? 1,
                phaseState?.CurrentPhase ?? RunPhase.PerformerSelection,
                phaseState?.PhaseElapsedSeconds ?? 0f,
                phaseState?.WorldIndex ?? 1,
                BuildWeaponOptions(ownedOnly: false),
                BuildWeaponOptions(ownedOnly: true),
                BuildOptions(itemCatalog?.Items),
                BuildOptions(talentCatalog?.Talents),
                BuildOptions(upgradeCatalog?.Upgrades),
                scheduleState?.HasActiveAct ?? false,
                scheduleState?.ActNumber ?? 0,
                scheduleState?.ActName,
                scheduleState?.ActElapsedSeconds ?? 0f,
                scheduleState?.ActRemainingSeconds ?? 0f,
                nextShowtime,
                scheduleState?.IsShowtimeActive ?? false,
                scheduleState?.ShowtimeRemainingSeconds ?? 0f,
                scheduleState?.IsActFinaleDue ?? false,
                scheduleState?.IsEncoreActive ?? false,
                scheduleState?.EncoreElapsedSeconds ?? 0f,
                scheduleState?.EncorePressureStep ?? 0,
                scheduleState?.IsEncoreGraceActive ?? false,
                scheduleState?.EncoreGraceRemainingSeconds ?? 0f,
                scheduleState?.EncorePressureElapsedSeconds ?? 0f,
                worldLayoutState?.Mode ?? RunWorldLayoutMode.Generated);
        }

        public bool AddTickets(int amount)
        {
            if (!IsAvailable || amount <= 0 || currencyState == null)
            {
                return false;
            }

            currencyState.AddTickets(amount);
            return true;
        }

        public bool AddExperience(int amount)
        {
            if (!IsAvailable || amount <= 0 || state == null)
            {
                return false;
            }

            state.AddExperience(amount);
            return true;
        }

        public bool HealPlayer(int amount)
        {
            if (!IsAvailable || amount <= 0 || state == null)
            {
                return false;
            }

            int before = state.Health;
            state.HealPlayer(amount);
            return state.Health > before;
        }

        public bool DamagePlayer(int amount)
        {
            if (!IsAvailable || amount <= 0 || state == null)
            {
                return false;
            }

            int before = state.Health;
            state.DamagePlayer(amount, "Debug Damage");
            return state.Health < before;
        }

        public bool StartBossWarning()
        {
            return IsAvailable && phaseState != null && phaseState.TrySetPhase(RunPhase.BossWarning);
        }

        public bool TriggerNextShowtime()
        {
            return IsAvailable && scheduleState != null && scheduleState.ForceNextShowtime();
        }

        public bool JumpToActFinale()
        {
            if (!IsAvailable || scheduleState == null || phaseState == null)
            {
                return false;
            }

            bool forced = scheduleState.ForceActFinale();
            if (forced && phaseState.Is(RunPhase.WorldActive))
            {
                phaseState.StartBossWarning();
            }

            return forced;
        }

        public bool ActivateProxyHeadliner()
        {
            return ActivateBoss();
        }

        public bool DefeatProxyHeadliner()
        {
            return DefeatBoss();
        }

        public bool SkipEncoreGrace()
        {
            return IsAvailable && scheduleState != null && scheduleState.ForceEndEncoreGrace();
        }

        public bool CompleteStageDoor()
        {
            if (!IsAvailable || phaseState == null || !phaseState.Is(RunPhase.Encore))
            {
                return false;
            }

            if (scheduleState != null && scheduleState.HasNextAct)
            {
                RunPhase before = phaseState.CurrentPhase;
                phaseState.EnterIntermission();
                return before != phaseState.CurrentPhase;
            }

            return FinishRun();
        }

        public bool EnterNextAct()
        {
            if (!IsAvailable || phaseState == null || !phaseState.Is(RunPhase.Intermission))
            {
                return false;
            }

            int before = phaseState.WorldIndex;
            phaseState.BeginNextWorld();
            if (phaseState.WorldIndex <= before)
            {
                return false;
            }

            resetCoordinator?.ResetForWorldTransition(phaseState.WorldIndex, clearWorld: worldLifecycle == null);
            WorldGenerationResult result = default;
            if (worldLifecycle != null)
            {
                result = worldLifecycle.TransitionToWorld(phaseState.WorldIndex, WorldSeed(phaseState.WorldIndex));
            }

            playerStartPlacement?.Apply(result);
            phaseState.BeginWorld();
            scheduleState?.BeginAct(phaseState.WorldIndex);
            return phaseState.Is(RunPhase.WorldActive);
        }

        public bool ActivateBoss()
        {
            return IsAvailable && phaseState != null && phaseState.TrySetPhase(RunPhase.BossActive);
        }

        public bool DefeatBoss()
        {
            if (!IsAvailable || phaseState == null)
            {
                return false;
            }

            RunPhase before = phaseState.CurrentPhase;
            if (phaseState.Is(RunPhase.WorldActive))
            {
                scheduleState?.ForceActFinale();
                phaseState.StartBossWarning();
            }

            if (phaseState.Is(RunPhase.BossWarning))
            {
                phaseState.ActivateBoss();
            }

            RecordProxyHeadlinerDeathPosition();
            phaseState.MarkBossDefeated();
            return before != phaseState.CurrentPhase || phaseState.Is(RunPhase.Encore);
        }

        private void RecordProxyHeadlinerDeathPosition()
        {
            if (headlinerDefeatState == null || player == null)
            {
                return;
            }

            headlinerDefeatState.RecordDeathPositionIfMissing(player.Position);
        }

        public bool FinishRun()
        {
            return IsAvailable && phaseState != null && phaseState.TrySetPhase(RunPhase.Finished);
        }

        public bool BeginNextWorld()
        {
            if (!IsAvailable || phaseState == null)
            {
                return false;
            }

            int before = phaseState.WorldIndex;
            phaseState.BeginNextWorld();
            return phaseState.WorldIndex > before;
        }

        private int WorldSeed(int worldIndex)
        {
            return runSeedState != null
                ? runSeedState.Combine(8301, worldIndex)
                : DeterministicSeed.Combine(8301, worldIndex);
        }

        public bool AddWeapon(string weaponId)
        {
            if (!IsAvailable || weaponLoadout == null)
            {
                return false;
            }

            WeaponDefinition weapon = FindById(weaponCatalog?.AvailableWeapons, weaponId);
            return weapon != null && weaponLoadout.AddWeapon(weapon);
        }

        public bool RemoveWeapon(string weaponId)
        {
            return IsAvailable && weaponLoadout != null && weaponLoadout.RemoveWeapon(weaponId);
        }

        public bool AddItem(string itemId, int amount)
        {
            if (!IsAvailable || itemInventory == null || amount <= 0)
            {
                return false;
            }

            ItemDefinition item = FindById(itemCatalog?.Items, itemId);
            return item != null && itemInventory.AddItem(item, amount, ItemGrantSource.WorldItem) > 0;
        }

        public bool RemoveItem(string itemId, int amount)
        {
            return IsAvailable
                && itemInventory != null
                && amount > 0
                && itemInventory.RemoveItem(itemId, amount) > 0;
        }

        public bool ApplyTalent(string talentId, ContentRarity rarity)
        {
            if (!IsAvailable || talentEffectApplier == null)
            {
                return false;
            }

            TalentDefinition talent = FindById(talentCatalog?.Talents, talentId);
            return talent != null
                && ContentAvailabilityRules.IsActiveAndValid(talent)
                && talentEffectApplier.Apply(talent, rarity);
        }

        public bool ApplyUpgrade(string upgradeId, ContentRarity rarity)
        {
            if (!IsAvailable || upgradeEffectApplier == null || upgradeRunState == null)
            {
                return false;
            }

            UpgradeDefinition upgrade = FindById(upgradeCatalog?.Upgrades, upgradeId);
            if (upgrade == null || !ContentAvailabilityRules.IsActiveAndValid(upgrade))
            {
                return false;
            }

            if (weaponLoadout == null || !weaponLoadout.OwnsWeapon(upgrade.weaponId))
            {
                return false;
            }

            return upgradeEffectApplier.Apply(new UpgradeChoice(upgrade, upgradeRunState.GetLevel(upgrade), rarity));
        }

        private List<RunDebugOption> BuildWeaponOptions(bool ownedOnly)
        {
            var options = new List<RunDebugOption>();
            IReadOnlyList<WeaponDefinition> weapons = weaponCatalog?.AvailableWeapons;
            if (weapons == null)
            {
                return options;
            }

            for (int i = 0; i < weapons.Count; i++)
            {
                WeaponDefinition weapon = weapons[i];
                if (weapon == null || !ContentId.IsValidValue(weapon.Id))
                {
                    continue;
                }

                bool owned = weaponLoadout != null && weaponLoadout.OwnsWeapon(weapon.Id);
                if (ownedOnly && !owned)
                {
                    continue;
                }

                options.Add(new RunDebugOption(weapon.Id, LabelFor(weapon), weapon.IsActive, owned));
            }

            return options;
        }

        private static List<RunDebugOption> BuildOptions<T>(IReadOnlyList<T> definitions)
            where T : UnityEngine.Object, IContentDefinition
        {
            var options = new List<RunDebugOption>();
            if (definitions == null)
            {
                return options;
            }

            for (int i = 0; i < definitions.Count; i++)
            {
                T definition = definitions[i];
                if (definition == null || !ContentId.IsValidValue(definition.Id))
                {
                    continue;
                }

                options.Add(new RunDebugOption(definition.Id, LabelFor(definition), ContentAvailabilityRules.IsActive(definition)));
            }

            return options;
        }

        private static T FindById<T>(IReadOnlyList<T> definitions, string id)
            where T : UnityEngine.Object, IContentDefinition
        {
            if (definitions == null || string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            for (int i = 0; i < definitions.Count; i++)
            {
                T definition = definitions[i];
                if (definition != null && string.Equals(definition.Id, id, StringComparison.Ordinal))
                {
                    return definition;
                }
            }

            return null;
        }

        private static string LabelFor(IContentDefinition definition)
        {
            string displayName = string.IsNullOrWhiteSpace(definition.DisplayName) ? definition.Id : definition.DisplayName;
            return $"{displayName} ({definition.Id})";
        }
    }
}
