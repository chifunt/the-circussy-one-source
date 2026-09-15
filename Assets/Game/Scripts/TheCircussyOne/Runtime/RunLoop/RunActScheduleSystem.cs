using System;
using Cysharp.Threading.Tasks;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.Rules;
using TheCircussyOne.Visuals;
using VContainer.Unity;

namespace TheCircussyOne.Runtime
{
    public sealed class RunActScheduleSystem : IStartable, ITickable, ITickableWhenPaused, IDisposable
    {
        private readonly RunActScheduleState scheduleState;
        private readonly RunPhaseState phaseState;
        private readonly IGameTime time;

        public RunActScheduleSystem(RunActScheduleState scheduleState, RunPhaseState phaseState, IGameTime time)
        {
            this.scheduleState = scheduleState;
            this.phaseState = phaseState;
            this.time = time;
        }

        public void Start()
        {
            if (phaseState != null)
            {
                phaseState.PhaseChanged += OnPhaseChanged;
            }

            SyncToPhase();
        }

        public void Tick()
        {
            if (phaseState == null)
            {
                return;
            }

            float deltaTime = time != null ? time.DeltaTime : 0f;
            if (phaseState.Is(RunPhase.WorldActive))
            {
                scheduleState?.Tick(deltaTime);
                return;
            }

            if (phaseState.Is(RunPhase.Encore))
            {
                scheduleState?.TickEncore(deltaTime);
            }
        }

        public void Dispose()
        {
            if (phaseState != null)
            {
                phaseState.PhaseChanged -= OnPhaseChanged;
            }
        }

        private void OnPhaseChanged(RunPhase previous, RunPhase next)
        {
            SyncToPhase();
        }

        private void SyncToPhase()
        {
            if (phaseState == null || scheduleState == null)
            {
                return;
            }

            if (phaseState.Is(RunPhase.PerformerSelection))
            {
                scheduleState.Reset();
                return;
            }

            if (phaseState.Is(RunPhase.WorldActive))
            {
                scheduleState.BeginAct(phaseState.WorldIndex);
                return;
            }

            if (phaseState.Is(RunPhase.Encore))
            {
                scheduleState.BeginEncore();
            }
        }
    }

    public sealed class RunActProgressionSystem : IStartable, ITickable, ITickableWhenPaused, IDisposable, IRunResettable
    {
        private readonly RunActScheduleState scheduleState;
        private readonly RunPhaseState phaseState;
        private readonly RunScheduleConfig config;
        private readonly HeadlinerCatalog headlinerCatalog;
        private readonly RunWorldLifecycleSystem worldLifecycle;
        private readonly RunSeedState runSeedState;
        private readonly IRunTransitionService transitionService;
        private readonly RunResetCoordinator resetCoordinator;
        private readonly RunPlayerStartPlacementSystem playerStartPlacement;
        private readonly HeadlinerDefeatState headlinerDefeatState;
        private readonly PlayerView player;

        private bool intermissionRunning;
        private float intermissionElapsedSeconds;
        private bool headlinerEncoreDeadlineRunning;
        private float headlinerEncoreDeadlineElapsedSeconds;

        public RunActProgressionSystem(
            RunActScheduleState scheduleState,
            RunPhaseState phaseState,
            RunScheduleConfig config,
            HeadlinerCatalog headlinerCatalog = null,
            RunWorldLifecycleSystem worldLifecycle = null,
            RunSeedState runSeedState = null,
            IRunTransitionService transitionService = null,
            RunResetCoordinator resetCoordinator = null,
            RunPlayerStartPlacementSystem playerStartPlacement = null,
            HeadlinerDefeatState headlinerDefeatState = null,
            PlayerView player = null)
        {
            this.scheduleState = scheduleState;
            this.phaseState = phaseState;
            this.config = config != null ? config : RunScheduleConfig.CreateRuntimeDefault();
            this.headlinerCatalog = headlinerCatalog;
            this.worldLifecycle = worldLifecycle;
            this.runSeedState = runSeedState;
            this.transitionService = transitionService;
            this.resetCoordinator = resetCoordinator;
            this.playerStartPlacement = playerStartPlacement;
            this.headlinerDefeatState = headlinerDefeatState;
            this.player = player;
            this.config.EnsureWorkflowDefaults();
        }

        public void Start()
        {
            if (scheduleState != null)
            {
                scheduleState.ActFinaleBecameDue += OnActFinaleDue;
            }

            if (phaseState != null)
            {
                phaseState.PhaseChanged += OnPhaseChanged;
            }
        }

        public void ResetRunState(RunResetContext context)
        {
            intermissionRunning = false;
            intermissionElapsedSeconds = 0f;
            headlinerEncoreDeadlineRunning = false;
            headlinerEncoreDeadlineElapsedSeconds = 0f;
        }

        public void Tick()
        {
            Tick(UnityEngine.Time.unscaledDeltaTime);
        }

        public void Tick(float deltaTime)
        {
            if (phaseState == null)
            {
                return;
            }

            float clampedDelta = Math.Max(0f, deltaTime);
            if (phaseState.Is(RunPhase.BossWarning)
                && phaseState.PhaseElapsedSeconds >= Math.Max(0f, config.actFinaleWarningSeconds))
            {
                phaseState.ActivateBoss();
                return;
            }

            if (TickHeadlinerEncoreDeadline(clampedDelta))
            {
                return;
            }

            if (config.actFinaleProxyAutoDefeat
                && !HasRealHeadlinerForCurrentAct()
                && phaseState.Is(RunPhase.BossActive)
                && phaseState.PhaseElapsedSeconds >= Math.Max(0f, config.actFinaleProxyBossSeconds))
            {
                RecordProxyHeadlinerDeathPosition();
                phaseState.MarkBossDefeated();
                return;
            }

            if (!intermissionRunning)
            {
                return;
            }

            intermissionElapsedSeconds += clampedDelta;
            if (intermissionElapsedSeconds < Math.Max(0f, config.intermissionSeconds))
            {
                return;
            }

            CompleteIntermission();
        }

        public void Dispose()
        {
            if (scheduleState != null)
            {
                scheduleState.ActFinaleBecameDue -= OnActFinaleDue;
            }

            if (phaseState != null)
            {
                phaseState.PhaseChanged -= OnPhaseChanged;
            }
        }

        private void OnActFinaleDue(RunActFinaleEvent _)
        {
            if (phaseState != null && phaseState.Is(RunPhase.WorldActive))
            {
                phaseState.StartBossWarning();
            }
        }

        private void OnPhaseChanged(RunPhase previous, RunPhase next)
        {
            if (next == RunPhase.BossActive)
            {
                headlinerEncoreDeadlineRunning = true;
                headlinerEncoreDeadlineElapsedSeconds = 0f;
                return;
            }

            if (next == RunPhase.BossDefeated)
            {
                return;
            }

            if (next == RunPhase.Encore)
            {
                headlinerEncoreDeadlineRunning = false;
                headlinerEncoreDeadlineElapsedSeconds = 0f;
                scheduleState?.BeginEncore();
                intermissionRunning = false;
                intermissionElapsedSeconds = 0f;
                return;
            }

            if (previous == RunPhase.Encore && next != RunPhase.Encore)
            {
                scheduleState?.EndEncore();
            }

            if (next == RunPhase.Intermission)
            {
                intermissionElapsedSeconds = 0f;
                intermissionRunning = !TryStartIntermissionTransition();
                return;
            }

            if (next != RunPhase.Intermission)
            {
                intermissionRunning = false;
                intermissionElapsedSeconds = 0f;
            }

            headlinerEncoreDeadlineRunning = false;
            headlinerEncoreDeadlineElapsedSeconds = 0f;
        }

        private bool TickHeadlinerEncoreDeadline(float deltaTime)
        {
            if (!headlinerEncoreDeadlineRunning
                || phaseState == null
                || (!phaseState.Is(RunPhase.BossActive) && !phaseState.Is(RunPhase.BossDefeated)))
            {
                return false;
            }

            headlinerEncoreDeadlineElapsedSeconds += deltaTime;
            if (headlinerEncoreDeadlineElapsedSeconds < Math.Max(0f, config.headlinerEncoreDeadlineSeconds))
            {
                return false;
            }

            phaseState.BeginEncore();
            return true;
        }

        private bool HasRealHeadlinerForCurrentAct()
        {
            int actNumber = scheduleState != null && scheduleState.HasActiveAct
                ? scheduleState.ActNumber
                : phaseState?.WorldIndex ?? 1;
            HeadlinerDefinition headliner = headlinerCatalog != null ? headlinerCatalog.ForAct(actNumber) : null;
            return ContentAvailabilityRules.IsActiveAndValid(headliner)
                && ContentAvailabilityRules.IsActiveAndValid(headliner.enemyActor);
        }

        private void RecordProxyHeadlinerDeathPosition()
        {
            if (headlinerDefeatState == null || player == null)
            {
                return;
            }

            headlinerDefeatState.RecordDeathPositionIfMissing(player.Position);
        }

        private int WorldSeed(int worldIndex)
        {
            return runSeedState != null
                ? runSeedState.Combine(8301, worldIndex)
                : DeterministicSeed.Combine(8301, worldIndex);
        }

        private bool TryStartIntermissionTransition()
        {
            if (transitionService == null || !transitionService.IsEnabled)
            {
                return false;
            }

            int nextActNumber = phaseState != null ? phaseState.WorldIndex + 1 : 1;
            return transitionService.PlayAsync(
                AnnouncementText(config.intermissionAnnouncementTitle, "INTERMISSION"),
                FormatIntermissionSubtitle(nextActNumber),
                CompleteIntermissionAsync,
                Math.Max(0f, config.intermissionSeconds));
        }

        private void CompleteIntermission()
        {
            if (phaseState == null || !phaseState.Is(RunPhase.Intermission))
            {
                return;
            }

            intermissionRunning = false;
            intermissionElapsedSeconds = 0f;
            phaseState.BeginNextWorld();
            resetCoordinator?.ResetForWorldTransition(phaseState.WorldIndex, clearWorld: worldLifecycle == null);
            // Generate the next act before entering gameplay so phase listeners see a fully prepared world.
            WorldGenerationResult result = default;
            if (worldLifecycle != null)
            {
                result = worldLifecycle.TransitionToWorld(phaseState.WorldIndex, WorldSeed(phaseState.WorldIndex));
            }

            playerStartPlacement?.Apply(result);
            phaseState.BeginWorld();
        }

        private async UniTask CompleteIntermissionAsync()
        {
            if (phaseState == null || !phaseState.Is(RunPhase.Intermission))
            {
                return;
            }

            intermissionRunning = false;
            intermissionElapsedSeconds = 0f;
            phaseState.BeginNextWorld();
            resetCoordinator?.ResetForWorldTransition(phaseState.WorldIndex, clearWorld: worldLifecycle == null);
            WorldGenerationResult result = default;
            if (worldLifecycle != null)
            {
                result = await worldLifecycle.TransitionToWorldAsync(phaseState.WorldIndex, WorldSeed(phaseState.WorldIndex));
            }

            playerStartPlacement?.Apply(result);
            phaseState.BeginWorld();
        }

        private string FormatIntermissionSubtitle(int nextActNumber)
        {
            string actLabel = RunActScheduleRules.ActRomanLabel(nextActNumber);
            int seed = runSeedState != null
                ? runSeedState.Combine(4103, nextActNumber)
                : DeterministicSeed.Combine(4103, nextActNumber);
            string format = RunActScheduleRules.PickSubtitleVariant(
                config.intermissionAnnouncementSubtitleFormats,
                config.intermissionAnnouncementSubtitleFormat,
                seed);
            try
            {
                return string.Format(format, actLabel);
            }
            catch (FormatException)
            {
                return $"Preparing {actLabel}...";
            }
        }

        private static string AnnouncementText(string value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        }
    }
}
