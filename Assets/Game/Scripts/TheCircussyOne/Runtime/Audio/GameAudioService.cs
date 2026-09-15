using System;
using System.Collections.Generic;
using System.Reflection;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;
using UnityEngine;
using VContainer.Unity;

namespace TheCircussyOne.Runtime
{
    public sealed class GameAudioService : IGameAudio, IStartable, ITickable, ITickableWhenPaused, IRunResettable, IDisposable
    {
        private readonly GameAudioConfig config;
        private readonly XpGainCounterVisualConfig xpGainCounterConfig;
        private readonly IGameTime gameTime;
        private readonly RunPauseState pauseState;
        private readonly GameState gameState;
        private readonly RunPhaseState phaseState;
        private readonly IWwiseRuntimeAudioBackend backend;
        private readonly Dictionary<object, LoopHandle> loops = new(ReferenceEqualityComparer.Instance);
        private readonly List<TransientEmitter> transientEmitters = new();
        private readonly Dictionary<GameAudioCue, float> cooldowns = new();
        private readonly HashSet<string> loadedBanks = new(StringComparer.Ordinal);
        private readonly List<PendingBankPreload> pendingBankPreloads = new();

        private GameObject root;
        private GameObject musicEmitter;
        private bool pausedLastFrame;
        private bool gameplayMusicPlaying;
        private bool gameplayMusicDistant;
        private float gameplayMusicVolume = -1f;
        private int xpPitchStep;
        private float xpPitchResetRemainingSeconds;

        public GameAudioService(
            GameAudioConfig config,
            IGameTime gameTime = null,
            RunPauseState pauseState = null,
            GameState gameState = null,
            RunPhaseState phaseState = null,
            XpGainCounterVisualConfig xpGainCounterConfig = null)
            : this(config, gameTime, pauseState, new WwiseRuntimeAudioBackend(), xpGainCounterConfig, gameState, phaseState)
        {
        }

        internal GameAudioService(
            GameAudioConfig config,
            IGameTime gameTime,
            RunPauseState pauseState,
            IWwiseRuntimeAudioBackend backend,
            XpGainCounterVisualConfig xpGainCounterConfig = null,
            GameState gameState = null,
            RunPhaseState phaseState = null)
        {
            this.config = config != null ? config : GameAudioConfig.CreateRuntimeDefault();
            this.xpGainCounterConfig = xpGainCounterConfig;
            this.gameTime = gameTime;
            this.pauseState = pauseState;
            this.gameState = gameState;
            this.phaseState = phaseState;
            this.backend = backend ?? new WwiseRuntimeAudioBackend();
            this.config.EnsureWorkflowDefaults();
        }

        public void Start()
        {
            EnsureRoot();
            QueueConfiguredSoundBanks();
            TryLoadPendingSoundBanks();
            if (gameState != null)
            {
                gameState.GameOver += OnGameOver;
            }
        }

        public void Tick()
        {
            TryLoadPendingSoundBanks();
            float deltaTime = DeltaTime();
            TickCooldowns(deltaTime);
            TickXpPitchReset(deltaTime);
            TickTransientEmitters(deltaTime);
            TickGameplayMusic();

            bool paused = pauseState != null && pauseState.IsPaused;
            if (paused && !pausedLastFrame)
            {
                StopLoopsOnPause();
            }

            pausedLastFrame = paused;
        }

        public void ResetRunState(RunResetContext context)
        {
            StopGameplayMusic();
            StopAllLoops();
            xpPitchStep = 0;
            xpPitchResetRemainingSeconds = 0f;
            cooldowns.Clear();
        }

        public void Dispose()
        {
            if (gameState != null)
            {
                gameState.GameOver -= OnGameOver;
            }

            StopGameplayMusic();
            StopAllLoops();
            for (int i = transientEmitters.Count - 1; i >= 0; i--)
            {
                GameAudioEmitterFactory.Release(transientEmitters[i].Emitter);
            }

            transientEmitters.Clear();
            if (root != null)
            {
                GameAudioEmitterFactory.Release(root);
                root = null;
            }
        }

        private void OnGameOver()
        {
            StopGameplayMusic();
        }

        public void Play(GameAudioCue cue)
        {
            if (!CanPlay(cue))
            {
                return;
            }

            string eventName = config.EventName(cue);
            if (!TryLoadBank(eventName))
            {
                return;
            }

            GameObject emitter = EnsureRoot();
            backend.PostEvent(eventName, emitter);
            StartCooldown(cue);
        }

        public void PlayAt(GameAudioCue cue, Vector3 position)
        {
            if (!CanPlay(cue))
            {
                return;
            }

            string eventName = config.EventName(cue);
            if (!TryLoadBank(eventName))
            {
                return;
            }

            GameObject emitter = CreateTransientEmitter(position);
            backend.PostEvent(eventName, emitter);
            StartCooldown(cue);
        }

        public void PlayXpCollect(Vector3 position)
        {
            if (!CanPlay(GameAudioCue.XpCollect))
            {
                return;
            }

            GameObject emitter = CreateTransientEmitter(position);
            float pitchOffsetCents = config.XpCollectPitchLiftCents(xpPitchStep);
            string eventName = config.EventName(GameAudioCue.XpCollect);
            if (!TryLoadBank(eventName))
            {
                return;
            }

            SetPitchOffsetCents(emitter, pitchOffsetCents);
            backend.PostEvent(eventName, emitter);
            xpPitchStep++;
            xpPitchResetRemainingSeconds = config.XpCollectPitchResetDurationSeconds(xpGainCounterConfig);
        }

        public void StartLoop(GameAudioCue cue, object owner, Vector3 position, float initialProgress = 0f)
        {
            if (!config.audioEnabled || owner == null || loops.ContainsKey(owner))
            {
                return;
            }

            string eventName = config.EventName(cue);
            if (string.IsNullOrWhiteSpace(eventName))
            {
                return;
            }

            if (!TryLoadBank(eventName))
            {
                return;
            }

            GameObject emitter = CreateLoopEmitter(cue, owner, position);
            var handle = new LoopHandle(cue, emitter);
            loops[owner] = handle;
            UpdateLoopPitch(cue, emitter, initialProgress);
            UpdateLoopRtpc(cue, emitter, initialProgress);
            backend.PostEvent(eventName, emitter);
        }

        public void UpdateLoop(GameAudioCue cue, object owner, Vector3 position, float progress01)
        {
            if (owner == null || !loops.TryGetValue(owner, out LoopHandle handle) || handle.Cue != cue || handle.Emitter == null)
            {
                return;
            }

            handle.Emitter.transform.position = position;
            UpdateLoopPitch(cue, handle.Emitter, progress01);
            UpdateLoopRtpc(cue, handle.Emitter, progress01);
        }

        public void StopLoop(GameAudioCue cue, object owner)
        {
            if (owner == null || !loops.TryGetValue(owner, out LoopHandle handle))
            {
                return;
            }

            if (handle.Cue == cue)
            {
                StopLoop(owner, handle);
            }
        }

        public void StopAllLoops()
        {
            if (loops.Count <= 0)
            {
                return;
            }

            var owners = new List<object>(loops.Keys);
            for (int i = 0; i < owners.Count; i++)
            {
                if (loops.TryGetValue(owners[i], out LoopHandle handle))
                {
                    StopLoop(owners[i], handle);
                }
            }
        }

        private void StopLoopsOnPause()
        {
            if (loops.Count <= 0)
            {
                return;
            }

            var owners = new List<object>(loops.Keys);
            for (int i = 0; i < owners.Count; i++)
            {
                if (loops.TryGetValue(owners[i], out LoopHandle handle) && ShouldStopOnPause(handle.Cue))
                {
                    StopLoop(owners[i], handle);
                }
            }
        }

        private static bool ShouldStopOnPause(GameAudioCue cue)
        {
            return cue != GameAudioCue.UiSheenLoop;
        }

        private bool CanPlay(GameAudioCue cue)
        {
            return config.audioEnabled
                && !string.IsNullOrWhiteSpace(config.EventName(cue))
                && (!cooldowns.TryGetValue(cue, out float remaining) || remaining <= 0f);
        }

        private void StartCooldown(GameAudioCue cue)
        {
            float seconds = cue switch
            {
                GameAudioCue.EnemyHit => config.enemyHitCooldownSeconds,
                GameAudioCue.PlayerHurt => config.playerHurtCooldownSeconds,
                GameAudioCue.TicketCollect => config.ticketCollectCooldownSeconds,
                GameAudioCue.FireHoopHit => config.fireHoopHitCooldownSeconds,
                GameAudioCue.JugglingBallHit
                    or GameAudioCue.CannonHit
                    or GameAudioCue.KnifeFanHit
                    or GameAudioCue.SpotlightBoltHit => config.weaponHitCooldownSeconds,
                _ => 0f
            };
            if (seconds > 0f)
            {
                cooldowns[cue] = seconds;
            }
        }

        private void StopLoop(object owner, LoopHandle handle)
        {
            loops.Remove(owner);
            string stopEvent = config.StopEventName(handle.Cue);
            if (!string.IsNullOrWhiteSpace(stopEvent) && handle.Emitter != null && TryLoadBank(stopEvent))
            {
                backend.PostEvent(stopEvent, handle.Emitter);
            }

            GameAudioEmitterFactory.Release(handle.Emitter);
        }

        private void QueueConfiguredSoundBanks()
        {
            if (!config.audioEnabled)
            {
                pendingBankPreloads.Clear();
                return;
            }

            if (config.loadMainBankOnStart)
            {
                AddPendingBank(config.mainSoundBankName, eventBank: false);
            }

            if (!config.preloadEventSoundBanksOnStart)
            {
                return;
            }

            IReadOnlyList<string> eventBanks = config.EventSoundBankNames();
            for (int i = 0; i < eventBanks.Count; i++)
            {
                AddPendingBank(eventBanks[i], eventBank: true);
            }
        }

        private void AddPendingBank(string bankName, bool eventBank)
        {
            string key = BankCacheKey(bankName, eventBank);
            if (string.IsNullOrWhiteSpace(bankName) || loadedBanks.Contains(key))
            {
                return;
            }

            for (int i = 0; i < pendingBankPreloads.Count; i++)
            {
                if (pendingBankPreloads[i].CacheKey == key)
                {
                    return;
                }
            }

            pendingBankPreloads.Add(new PendingBankPreload(bankName, eventBank, key));
        }

        private void TryLoadPendingSoundBanks()
        {
            if (!config.audioEnabled || pendingBankPreloads.Count <= 0)
            {
                return;
            }

            for (int i = pendingBankPreloads.Count - 1; i >= 0; i--)
            {
                PendingBankPreload preload = pendingBankPreloads[i];
                if (TryLoadBank(preload.BankName, preload.EventBank))
                {
                    pendingBankPreloads.RemoveAt(i);
                }
            }
        }

        private bool TryLoadBank(string bankName, bool eventBank = true)
        {
            string key = BankCacheKey(bankName, eventBank);
            if (string.IsNullOrWhiteSpace(bankName) || loadedBanks.Contains(key))
            {
                return true;
            }

            if (!backend.LoadBank(bankName, eventBank))
            {
                return false;
            }

            loadedBanks.Add(key);
            return true;
        }

        private static string BankCacheKey(string bankName, bool eventBank)
        {
            return eventBank ? $"event:{bankName}" : $"user:{bankName}";
        }

        private void TickGameplayMusic()
        {
            if (!config.gameplayMusicEnabled || !config.audioEnabled)
            {
                if (gameplayMusicPlaying)
                {
                    StopGameplayMusic();
                }

                return;
            }

            bool shouldPlay = ShouldGameplayMusicPlay();
            if (shouldPlay && !gameplayMusicPlaying)
            {
                StartGameplayMusic();
            }
            else if (!shouldPlay && gameplayMusicPlaying)
            {
                StopGameplayMusic();
            }

            if (gameplayMusicPlaying)
            {
                UpdateGameplayMusicPresentation();
            }
        }

        private bool ShouldGameplayMusicPlay()
        {
            if (gameState != null && gameState.IsGameOver)
            {
                return false;
            }

            if (pauseState != null
                && (pauseState.HasReason(RunPauseReasons.PerformerSelection)
                    || pauseState.HasReason(RunPauseReasons.RunTransition)))
            {
                return false;
            }

            if (!gameplayMusicPlaying
                && IsMusicDistantPresentationActive())
            {
                return false;
            }

            if (phaseState == null)
            {
                return true;
            }

            return phaseState.CurrentPhase is RunPhase.WorldActive
                or RunPhase.BossWarning
                or RunPhase.BossActive
                or RunPhase.BossDefeated
                or RunPhase.Encore
                or RunPhase.NextWorldTransition
                or RunPhase.Intermission;
        }

        private void StartGameplayMusic()
        {
            if (gameplayMusicPlaying || string.IsNullOrWhiteSpace(config.gameplayMusicEvent))
            {
                return;
            }

            if (!TryLoadBank(config.gameplayMusicEvent))
            {
                return;
            }

            GameObject emitter = EnsureMusicEmitter();
            gameplayMusicPlaying = true;
            gameplayMusicDistant = false;
            ApplyMusicPresentation(config.musicNormalPresentationValue, config.musicNormalVolume);
            backend.PostEvent(config.gameplayMusicEvent, emitter);
        }

        private void StopGameplayMusic()
        {
            if (!gameplayMusicPlaying)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(config.stopGameplayMusicEvent)
                && musicEmitter != null
                && TryLoadBank(config.stopGameplayMusicEvent))
            {
                backend.PostEvent(config.stopGameplayMusicEvent, musicEmitter);
            }

            gameplayMusicPlaying = false;
            gameplayMusicDistant = false;
            gameplayMusicVolume = -1f;
        }

        private void UpdateGameplayMusicPresentation()
        {
            bool distant = IsMusicDistantPresentationActive();
            if (distant == gameplayMusicDistant)
            {
                return;
            }

            gameplayMusicDistant = distant;
            ApplyMusicPresentation(
                distant ? config.musicDistantPresentationValue : config.musicNormalPresentationValue,
                distant ? config.musicDistantVolume : config.musicNormalVolume);
        }

        private void ApplyMusicPresentation(float presentationValue, float volume)
        {
            if (musicEmitter == null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(config.musicPresentationRtpcName))
            {
                backend.SetRtpc(config.musicPresentationRtpcName, presentationValue, musicEmitter);
            }

            float clampedVolume = Mathf.Clamp01(volume);
            if (Mathf.Approximately(gameplayMusicVolume, clampedVolume))
            {
                return;
            }

            gameplayMusicVolume = clampedVolume;
            backend.SetOutputBusVolume(musicEmitter, clampedVolume);
        }

        private bool IsMusicDistantPresentationActive()
        {
            return pauseState != null
                && (pauseState.HasReason(RunPauseReasons.UpgradeSelection)
                    || pauseState.HasReason(RunPauseReasons.RewardReveal)
                    || pauseState.HasReason(RunPauseReasons.PauseMenu));
        }

        private void UpdateLoopRtpc(GameAudioCue cue, GameObject emitter, float progress01)
        {
            if (emitter == null)
            {
                return;
            }

            float clamped = Mathf.Clamp01(progress01);
            if (cue == GameAudioCue.InteractionHoldLoop && !string.IsNullOrWhiteSpace(config.interactionProgressRtpcName))
            {
                backend.SetRtpc(config.interactionProgressRtpcName, clamped, emitter);
            }
            else if (cue == GameAudioCue.PlayerMovementLoop && !string.IsNullOrWhiteSpace(config.playerMovementSpeedRtpcName))
            {
                backend.SetRtpc(config.playerMovementSpeedRtpcName, clamped, emitter);
            }
        }

        private void UpdateLoopPitch(GameAudioCue cue, GameObject emitter, float progress01)
        {
            if (cue == GameAudioCue.InteractionHoldLoop)
            {
                SetPitchOffsetCents(emitter, config.InteractionHoldLoopPitchOffsetCents(progress01));
            }
        }

        private void SetPitchOffsetCents(GameObject emitter, float cents)
        {
            if (emitter == null || string.IsNullOrWhiteSpace(config.pitchRtpcName))
            {
                return;
            }

            backend.SetRtpc(config.pitchRtpcName, cents, emitter);
        }

        private GameObject EnsureRoot()
        {
            if (root != null)
            {
                return root;
            }

            root = GameAudioEmitterFactory.Create("Runtime Audio");
            return root;
        }

        private GameObject EnsureMusicEmitter()
        {
            if (musicEmitter != null)
            {
                return musicEmitter;
            }

            musicEmitter = GameAudioEmitterFactory.CreateChild("Runtime Music", EnsureRoot().transform, Vector3.zero);
            return musicEmitter;
        }

        private GameObject CreateTransientEmitter(Vector3 position)
        {
            GameObject emitter = GameAudioEmitterFactory.CreateChild("Audio One Shot", EnsureRoot().transform, position);
            transientEmitters.Add(new TransientEmitter(emitter, Mathf.Max(0.25f, config.oneShotEmitterLifetimeSeconds)));
            return emitter;
        }

        private GameObject CreateLoopEmitter(GameAudioCue cue, object owner, Vector3 position)
        {
            return GameAudioEmitterFactory.CreateChild($"Audio Loop - {cue}", EnsureRoot().transform, position);
        }

        private void TickCooldowns(float deltaTime)
        {
            if (cooldowns.Count <= 0 || deltaTime <= 0f)
            {
                return;
            }

            var cues = new List<GameAudioCue>(cooldowns.Keys);
            for (int i = 0; i < cues.Count; i++)
            {
                GameAudioCue cue = cues[i];
                float next = cooldowns[cue] - deltaTime;
                if (next <= 0f)
                {
                    cooldowns.Remove(cue);
                }
                else
                {
                    cooldowns[cue] = next;
                }
            }
        }

        private void TickXpPitchReset(float deltaTime)
        {
            if (xpPitchResetRemainingSeconds <= 0f)
            {
                xpPitchStep = 0;
                return;
            }

            xpPitchResetRemainingSeconds = Mathf.Max(0f, xpPitchResetRemainingSeconds - deltaTime);
            if (xpPitchResetRemainingSeconds <= 0f)
            {
                xpPitchStep = 0;
            }
        }

        private void TickTransientEmitters(float deltaTime)
        {
            for (int i = transientEmitters.Count - 1; i >= 0; i--)
            {
                TransientEmitter emitter = transientEmitters[i];
                float nextLifetime = emitter.RemainingSeconds - deltaTime;
                if (nextLifetime > 0f)
                {
                    transientEmitters[i] = new TransientEmitter(emitter.Emitter, nextLifetime);
                    continue;
                }

                GameAudioEmitterFactory.Release(emitter.Emitter);
                transientEmitters.RemoveAt(i);
            }
        }

        private float DeltaTime()
        {
            return Mathf.Max(0f, gameTime != null ? gameTime.DeltaTime : Time.unscaledDeltaTime);
        }

        private readonly struct LoopHandle
        {
            public LoopHandle(GameAudioCue cue, GameObject emitter)
            {
                Cue = cue;
                Emitter = emitter;
            }

            public GameAudioCue Cue { get; }
            public GameObject Emitter { get; }
        }

        private readonly struct TransientEmitter
        {
            public TransientEmitter(GameObject emitter, float remainingSeconds)
            {
                Emitter = emitter;
                RemainingSeconds = remainingSeconds;
            }

            public GameObject Emitter { get; }
            public float RemainingSeconds { get; }
        }

        private readonly struct PendingBankPreload
        {
            public PendingBankPreload(string bankName, bool eventBank, string cacheKey)
            {
                BankName = bankName;
                EventBank = eventBank;
                CacheKey = cacheKey;
            }

            public string BankName { get; }
            public bool EventBank { get; }
            public string CacheKey { get; }
        }

        private sealed class ReferenceEqualityComparer : IEqualityComparer<object>
        {
            public static readonly ReferenceEqualityComparer Instance = new();

            public new bool Equals(object x, object y)
            {
                return ReferenceEquals(x, y);
            }

            public int GetHashCode(object obj)
            {
                return obj != null ? System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj) : 0;
            }
        }
    }

    public interface IWwiseRuntimeAudioBackend
    {
        bool LoadBank(string bankName, bool eventBank);
        void PostEvent(string eventName, GameObject emitter);
        void SetRtpc(string rtpcName, float value, GameObject emitter);
        void SetOutputBusVolume(GameObject emitter, float volume);
        void StopAllOnEmitter(GameObject emitter);
    }

    public sealed class WwiseRuntimeAudioBackend : IWwiseRuntimeAudioBackend
    {
        private readonly Type soundEngineType;
        private readonly MethodInfo isInitializedMethod;
        private readonly MethodInfo loadBankMethod;
        private readonly MethodInfo loadBankWithTypeMethod;
        private readonly MethodInfo prepareEventMethod;
        private readonly MethodInfo postEventMethod;
        private readonly MethodInfo setRtpcMethod;
        private readonly MethodInfo setOutputBusVolumeMethod;
        private readonly MethodInfo stopAllMethod;
        private GameObject cachedListener;

        public WwiseRuntimeAudioBackend()
        {
            soundEngineType = Type.GetType("AkUnitySoundEngine, AK.Wwise.Unity.API") ?? Type.GetType("AkUnitySoundEngine");
            if (soundEngineType == null)
            {
                return;
            }

            isInitializedMethod = soundEngineType.GetMethod("IsInitialized", BindingFlags.Public | BindingFlags.Static, null, Type.EmptyTypes, null);
            loadBankMethod = soundEngineType.GetMethod(
                "LoadBank",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { typeof(string), typeof(uint).MakeByRefType() },
                null);
            loadBankWithTypeMethod = soundEngineType.GetMethod(
                "LoadBank",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { typeof(string), typeof(uint).MakeByRefType(), typeof(uint) },
                null);
            prepareEventMethod = FindPrepareEventMethod(soundEngineType);
            postEventMethod = soundEngineType.GetMethod(
                "PostEvent",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { typeof(string), typeof(GameObject) },
                null);
            setRtpcMethod = soundEngineType.GetMethod(
                "SetRTPCValue",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { typeof(string), typeof(float), typeof(GameObject) },
                null);
            setOutputBusVolumeMethod = soundEngineType.GetMethod(
                "SetGameObjectOutputBusVolume",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { typeof(GameObject), typeof(GameObject), typeof(float) },
                null);
            stopAllMethod = soundEngineType.GetMethod(
                "StopAll",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { typeof(GameObject) },
                null);
        }

        public bool LoadBank(string bankName, bool eventBank)
        {
            if (string.IsNullOrWhiteSpace(bankName) || !IsInitialized())
            {
                return false;
            }

            try
            {
                object result;
                if (loadBankWithTypeMethod != null)
                {
                    object[] args = { bankName, 0u, eventBank ? 30u : 0u };
                    result = loadBankWithTypeMethod.Invoke(null, args);
                    return IsBankLoadSuccess(result, bankName) && (!eventBank || PrepareEvent(bankName));
                }

                if (loadBankMethod == null)
                {
                    return false;
                }

                object[] fallbackArgs = { bankName, 0u };
                result = loadBankMethod.Invoke(null, fallbackArgs);
                return IsBankLoadSuccess(result, bankName) && (!eventBank || PrepareEvent(bankName));
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Wwise LoadBank failed for '{bankName}': {exception.GetBaseException().Message}");
                return false;
            }
        }

        private bool PrepareEvent(string eventName)
        {
            if (prepareEventMethod == null)
            {
                Debug.LogWarning($"Wwise PrepareEvent unavailable for '{eventName}'. Loose event media may not load.");
                return false;
            }

            try
            {
                Type preparationType = prepareEventMethod.GetParameters()[0].ParameterType;
                object loadValue = Enum.Parse(preparationType, "Preparation_Load");
                object result = prepareEventMethod.Invoke(null, new object[] { loadValue, new[] { eventName }, 1u });
                if (IsSuccess(result))
                {
                    return true;
                }

                Debug.LogWarning($"Wwise PrepareEvent failed for '{eventName}': {result}");
                return false;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Wwise PrepareEvent failed for '{eventName}': {exception.GetBaseException().Message}");
                return false;
            }
        }

        public void PostEvent(string eventName, GameObject emitter)
        {
            if (string.IsNullOrWhiteSpace(eventName) || emitter == null || postEventMethod == null || !IsInitialized())
            {
                return;
            }

            try
            {
                postEventMethod.Invoke(null, new object[] { eventName, emitter });
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Wwise PostEvent failed for '{eventName}': {exception.GetBaseException().Message}");
            }
        }

        public void SetRtpc(string rtpcName, float value, GameObject emitter)
        {
            if (string.IsNullOrWhiteSpace(rtpcName) || emitter == null || setRtpcMethod == null || !IsInitialized())
            {
                return;
            }

            try
            {
                setRtpcMethod.Invoke(null, new object[] { rtpcName, value, emitter });
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Wwise SetRTPCValue failed for '{rtpcName}': {exception.GetBaseException().Message}");
            }
        }

        public void SetOutputBusVolume(GameObject emitter, float volume)
        {
            if (emitter == null || setOutputBusVolumeMethod == null || !IsInitialized())
            {
                return;
            }

            GameObject listener = FindListener();
            if (listener == null)
            {
                return;
            }

            try
            {
                setOutputBusVolumeMethod.Invoke(null, new object[] { emitter, listener, Mathf.Clamp01(volume) });
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Wwise SetGameObjectOutputBusVolume failed: {exception.GetBaseException().Message}");
            }
        }

        public void StopAllOnEmitter(GameObject emitter)
        {
            if (emitter == null || stopAllMethod == null || !IsInitialized())
            {
                return;
            }

            try
            {
                stopAllMethod.Invoke(null, new object[] { emitter });
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Wwise StopAll failed: {exception.GetBaseException().Message}");
            }
        }

        private GameObject FindListener()
        {
            if (cachedListener != null)
            {
                return cachedListener;
            }

            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                cachedListener = mainCamera.gameObject;
                return cachedListener;
            }

            Type listenerType = Type.GetType("AkAudioListener, AK.Wwise.Unity.MonoBehaviour")
                ?? Type.GetType("AkAudioListener, AK.Wwise.Unity.API")
                ?? Type.GetType("AkAudioListener");
            if (listenerType == null)
            {
                return null;
            }

            UnityEngine.Object listener = UnityEngine.Object.FindFirstObjectByType(listenerType, FindObjectsInactive.Exclude);
            cachedListener = listener is Component component ? component.gameObject : null;
            return cachedListener;
        }

        private bool IsInitialized()
        {
            if (isInitializedMethod == null)
            {
                return soundEngineType != null;
            }

            try
            {
                return isInitializedMethod.Invoke(null, null) is bool initialized && initialized;
            }
            catch
            {
                return false;
            }
        }

        private static MethodInfo FindPrepareEventMethod(Type soundEngineType)
        {
            MethodInfo[] methods = soundEngineType.GetMethods(BindingFlags.Public | BindingFlags.Static);
            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo method = methods[i];
                if (method.Name != "PrepareEvent")
                {
                    continue;
                }

                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.Length == 3
                    && parameters[1].ParameterType == typeof(string[])
                    && parameters[2].ParameterType == typeof(uint))
                {
                    return method;
                }
            }

            return null;
        }

        private static bool IsBankLoadSuccess(object result, string bankName)
        {
            if (IsSuccess(result) || IsBankAlreadyLoaded(result))
            {
                return true;
            }

            Debug.LogWarning($"Wwise LoadBank failed for '{bankName}': {result}");
            return false;
        }

        private static bool IsSuccess(object result)
        {
            return ResultCode(result) == 1;
        }

        private static bool IsBankAlreadyLoaded(object result)
        {
            return ResultCode(result) == 69;
        }

        private static int ResultCode(object result)
        {
            if (result == null)
            {
                return -1;
            }

            try
            {
                return Convert.ToInt32(result);
            }
            catch (Exception)
            {
                return -1;
            }
        }
    }
}
