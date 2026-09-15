using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer.Unity;

namespace TheCircussyOne.Runtime
{
    public sealed class GameAudioPresenter : IStartable, IDisposable
    {
        private readonly IGameAudio audio;
        private readonly GameState state;
        private readonly RunActScheduleState scheduleState;
        private readonly RunPhaseState phaseState;
        private readonly HeadlinerDefeatState headlinerDefeatState;
        private int lastLevel = 1;

        public GameAudioPresenter(
            IGameAudio audio,
            GameState state = null,
            RunActScheduleState scheduleState = null,
            RunPhaseState phaseState = null,
            HeadlinerDefeatState headlinerDefeatState = null)
        {
            this.audio = audio ?? NullGameAudio.Instance;
            this.state = state;
            this.scheduleState = scheduleState;
            this.phaseState = phaseState;
            this.headlinerDefeatState = headlinerDefeatState;
        }

        public void Start()
        {
            if (state != null)
            {
                lastLevel = state.Level;
                state.PlayerDamaged += OnPlayerDamaged;
                state.PlayerHealed += OnPlayerHealed;
                state.LevelChanged += OnLevelChanged;
                state.GameOver += OnGameOver;
            }

            if (phaseState != null)
            {
                phaseState.PhaseChanged += OnPhaseChanged;
            }

            if (scheduleState != null)
            {
                scheduleState.EncorePressureStarted += OnEncorePressureStarted;
            }

            if (headlinerDefeatState != null)
            {
                headlinerDefeatState.DeathPositionRecorded += OnHeadlinerDefeated;
            }
        }

        public void Dispose()
        {
            if (state != null)
            {
                state.PlayerDamaged -= OnPlayerDamaged;
                state.PlayerHealed -= OnPlayerHealed;
                state.LevelChanged -= OnLevelChanged;
                state.GameOver -= OnGameOver;
            }

            if (phaseState != null)
            {
                phaseState.PhaseChanged -= OnPhaseChanged;
            }

            if (scheduleState != null)
            {
                scheduleState.EncorePressureStarted -= OnEncorePressureStarted;
            }

            if (headlinerDefeatState != null)
            {
                headlinerDefeatState.DeathPositionRecorded -= OnHeadlinerDefeated;
            }
        }

        private void OnPlayerDamaged(int finalDamage, int maxHealth, float healthPercentDamage)
        {
            audio.Play(GameAudioCue.PlayerHurt);
        }

        private void OnPlayerHealed(int healedAmount, int maxHealth, float healthPercentHealed)
        {
            audio.Play(GameAudioCue.SnackEat);
        }

        private void OnLevelChanged(int level)
        {
            if (level > lastLevel && level > 1)
            {
                audio.Play(GameAudioCue.LevelUp);
            }

            lastLevel = level;
        }

        private void OnGameOver()
        {
            audio.StopAllLoops();
            audio.Play(GameAudioCue.GameOver);
        }

        private void OnPhaseChanged(RunPhase previous, RunPhase next)
        {
            if (headlinerDefeatState == null && previous == RunPhase.BossActive && next == RunPhase.BossDefeated)
            {
                audio.Play(GameAudioCue.AnnouncementHeadlinerDefeat);
            }
        }

        private void OnHeadlinerDefeated(Vector3 _)
        {
            audio.Play(GameAudioCue.AnnouncementHeadlinerDefeat);
        }

        private void OnEncorePressureStarted()
        {
            audio.Play(GameAudioCue.AnnouncementShowtimeHeadliner);
        }
    }

    public sealed class GameUiAudioPresenter : IStartable, ITickable, ITickableWhenPaused, IDisposable
    {
        private const float RebindIntervalSeconds = 0.5f;
        private static readonly string[] ClickableClasses =
        {
            "performer-card",
            "upgrade-card",
            "reward-reveal-card",
            "marquee-button",
            "pause-menu-button"
        };

        private readonly IGameAudio audio;
        private readonly Dictionary<VisualElement, ElementCallbacks> boundElements = new();
        private float nextRebindTime;

        public GameUiAudioPresenter(IGameAudio audio)
        {
            this.audio = audio ?? NullGameAudio.Instance;
        }

        public void Start()
        {
            BindAllButtons();
        }

        public void Tick()
        {
            if (Time.unscaledTime < nextRebindTime)
            {
                return;
            }

            nextRebindTime = Time.unscaledTime + RebindIntervalSeconds;
            BindAllButtons();
        }

        public void Dispose()
        {
            foreach (KeyValuePair<VisualElement, ElementCallbacks> element in boundElements)
            {
                element.Key?.UnregisterCallback(element.Value.Clicked);
                element.Key?.UnregisterCallback(element.Value.Hovered);
            }

            boundElements.Clear();
        }

        private void BindAllButtons()
        {
            UIDocument[] documents = UnityEngine.Object.FindObjectsByType<UIDocument>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int i = 0; i < documents.Length; i++)
            {
                VisualElement root = documents[i] != null ? documents[i].rootVisualElement : null;
                if (root == null)
                {
                    continue;
                }

                var buttons = root.Query<Button>().ToList();
                for (int j = 0; j < buttons.Count; j++)
                {
                    BindInteractiveElement(buttons[j]);
                }

                for (int j = 0; j < ClickableClasses.Length; j++)
                {
                    var elements = root.Query<VisualElement>(className: ClickableClasses[j]).ToList();
                    for (int k = 0; k < elements.Count; k++)
                    {
                        BindInteractiveElement(elements[k]);
                    }
                }
            }
        }

        private void BindInteractiveElement(VisualElement element)
        {
            if (element == null || boundElements.ContainsKey(element))
            {
                return;
            }

            EventCallback<ClickEvent> clicked = _ => audio.Play(GameAudioCue.UiClick);
            EventCallback<PointerEnterEvent> hovered = _ => audio.Play(GameAudioCue.UiHover);
            element.RegisterCallback(clicked);
            element.RegisterCallback(hovered);
            boundElements[element] = new ElementCallbacks(clicked, hovered);
        }

        private readonly struct ElementCallbacks
        {
            public ElementCallbacks(EventCallback<ClickEvent> clicked, EventCallback<PointerEnterEvent> hovered)
            {
                Clicked = clicked;
                Hovered = hovered;
            }

            public EventCallback<ClickEvent> Clicked { get; }
            public EventCallback<PointerEnterEvent> Hovered { get; }
        }
    }
}
