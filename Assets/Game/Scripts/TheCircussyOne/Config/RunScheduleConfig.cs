using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TheCircussyOne.Content;
using UnityEngine;

namespace TheCircussyOne.Config
{
    [Serializable]
    public sealed class RunScheduleActDefinition
    {
        [LabelText("Act Name")]
        public string displayName = "Opening Act";

        [Min(1f), SuffixLabel("sec")]
        public float durationSeconds = 420f;

        [ListDrawerSettings(DefaultExpandedState = true), SuffixLabel("sec")]
        public List<float> showtimeSeconds = new() { 120f, 300f };

        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? "Act" : displayName.Trim();
        public float DurationSeconds => Mathf.Max(1f, durationSeconds);
        public IReadOnlyList<float> ShowtimeSeconds => showtimeSeconds != null ? showtimeSeconds : Array.Empty<float>();
    }

    [Serializable]
    public sealed class RunStageDoorTravelDefinition
    {
        [ReadOnly]
        public string actLabel = "Act I";

        [Min(0f), SuffixLabel("sec")]
        public float minTravelSeconds = 15f;

        [Min(0f), SuffixLabel("sec")]
        public float maxTravelSeconds = 25f;

        public float MinTravelSeconds => Mathf.Max(0f, minTravelSeconds);
        public float MaxTravelSeconds => Mathf.Max(MinTravelSeconds, maxTravelSeconds);
        public float MidpointSeconds => (MinTravelSeconds + MaxTravelSeconds) * 0.5f;
    }

    [CreateAssetMenu(menuName = "The Circussy One/Run Schedule Config", fileName = "RunScheduleConfig")]
    [InfoBox("LIVE RUNTIME: Act durations and Showtime pressure are read by runtime systems. Scene rebuild is only needed if the LifetimeScope reference is missing.")]
    public sealed class RunScheduleConfig : SerializedScriptableObject
    {
        private const string Tabs = "Run Schedule";

        [TabGroup(Tabs, "Acts"), BoxGroup(Tabs + "/Acts/Show Schedule"), ListDrawerSettings(DefaultExpandedState = true)]
        public List<RunScheduleActDefinition> acts = new();

        [TabGroup(Tabs, "Showtime"), BoxGroup(Tabs + "/Showtime/Pressure"), LabelWidth(220), SuffixLabel("sec")]
        [Min(0.1f)]
        public float showtimePressureSeconds = 20f;

        [TabGroup(Tabs, "Showtime"), BoxGroup(Tabs + "/Showtime/Pressure"), LabelWidth(220), SuffixLabel("x")]
        [InfoBox("Lower values spawn faster during Showtime. 0.75 means 25% shorter spawn intervals.")]
        [Min(0.05f)]
        public float showtimeSpawnIntervalMultiplier = 0.75f;

        [TabGroup(Tabs, "Showtime"), BoxGroup(Tabs + "/Showtime/Pressure"), LabelWidth(220), SuffixLabel("x")]
        [InfoBox("Higher values allow more active enemies during Showtime. 1.15 means 15% more enemies.")]
        [Min(0.1f)]
        public float showtimeMaxEnemiesMultiplier = 1.15f;

        [TabGroup(Tabs, "Showtime"), BoxGroup(Tabs + "/Showtime/Announcements"), LabelWidth(220)]
        public string showtimeAnnouncementTitle = "SHOWTIME!";

        [HideInInspector]
        public string showtimeAnnouncementSubtitle = "A swarm enters the ring!";

        [TabGroup(Tabs, "Showtime"), BoxGroup(Tabs + "/Showtime/Announcements"), LabelWidth(220)]
        [ListDrawerSettings(DefaultExpandedState = true)]
        public List<string> showtimeAnnouncementSubtitles = new()
        {
            "A swarm enters the ring!",
            "The crowd surges toward the spotlight!",
            "Fresh trouble spills into the ring!"
        };

        [TabGroup(Tabs, "Headliner"), BoxGroup(Tabs + "/Headliner/Announcements"), LabelWidth(220)]
        public string actFinaleAnnouncementTitle = "HEADLINER!";

        [HideInInspector]
        public string actFinaleAnnouncementSubtitle = "The Headliner enters!";

        [TabGroup(Tabs, "Headliner"), BoxGroup(Tabs + "/Headliner/Announcements"), LabelWidth(220)]
        [ListDrawerSettings(DefaultExpandedState = true)]
        public List<string> actFinaleAnnouncementSubtitles = new()
        {
            "The Headliner enters!",
            "The main attraction storms the stage!",
            "The ring shakes for the final act!"
        };

        [TabGroup(Tabs, "Headliner"), BoxGroup(Tabs + "/Headliner/Phase Proxy"), LabelWidth(220), SuffixLabel("sec")]
        [InfoBox("Phase Proxy delay before the debug boss phase is considered active. This is not a real boss fight yet.")]
        [Min(0f)]
        public float actFinaleWarningSeconds = 2f;

        [TabGroup(Tabs, "Headliner"), BoxGroup(Tabs + "/Headliner/Phase Proxy"), LabelWidth(220)]
        [InfoBox("Temporary v0.3 proxy. When enabled, Boss Active auto-completes after the proxy duration until real Headliners exist.")]
        public bool actFinaleProxyAutoDefeat = true;

        [TabGroup(Tabs, "Headliner"), BoxGroup(Tabs + "/Headliner/Phase Proxy"), LabelWidth(220), SuffixLabel("sec")]
        [Min(0f)]
        public float actFinaleProxyBossSeconds = 6f;

        [TabGroup(Tabs, "Headliner"), BoxGroup(Tabs + "/Headliner/Encore Deadline"), LabelWidth(220), SuffixLabel("sec")]
        [InfoBox("Encore starts this many seconds after the Headliner becomes active. Normal enemy pressure continues until the deadline.")]
        [Min(0f)]
        public float headlinerEncoreDeadlineSeconds = 90f;

        [TabGroup(Tabs, "Encore"), BoxGroup(Tabs + "/Encore/Pressure"), LabelWidth(220), SuffixLabel("sec")]
        [InfoBox("Short escape setup after the Headliner Encore deadline expires. Stage Door appears during Encore once the Headliner death position exists, but Encore pressure does not ramp until this finishes.")]
        [Min(0f)]
        public float encoreGraceSeconds = 10f;

        [TabGroup(Tabs, "Encore"), BoxGroup(Tabs + "/Encore/Pressure"), LabelWidth(220), SuffixLabel("sec")]
        [InfoBox("Encore counts upward after the Headliner Encore deadline expires while the player escapes to the Stage Door.")]
        [Min(0.1f)]
        public float encorePressureIntervalSeconds = 30f;

        [TabGroup(Tabs, "Encore"), BoxGroup(Tabs + "/Encore/Pressure"), LabelWidth(220), SuffixLabel("x/step")]
        [InfoBox("Applied once per Encore pressure step. Lower values spawn faster; 0.9 means 10% shorter intervals each step.")]
        [Min(0.05f)]
        public float encoreSpawnIntervalMultiplierPerStep = 0.9f;

        [TabGroup(Tabs, "Encore"), BoxGroup(Tabs + "/Encore/Pressure"), LabelWidth(220), SuffixLabel("x/step")]
        [InfoBox("Applied once per Encore pressure step. Higher values raise the active enemy cap.")]
        [Min(1f)]
        public float encoreMaxEnemiesMultiplierPerStep = 1.1f;

        [TabGroup(Tabs, "Encore"), BoxGroup(Tabs + "/Encore/Stage Door"), LabelWidth(220)]
        [ListDrawerSettings(DefaultExpandedState = true, DraggableItems = false)]
        public List<RunStageDoorTravelDefinition> stageDoorTravelSeconds = CreateDefaultStageDoorTravel();

        [TabGroup(Tabs, "Encore"), BoxGroup(Tabs + "/Encore/Stage Door"), LabelWidth(220), SuffixLabel("u")]
        [Min(0f)]
        public float stageDoorGroundClearance = 0.04f;

        [TabGroup(Tabs, "Encore"), BoxGroup(Tabs + "/Encore/Stage Door"), LabelWidth(220), SuffixLabel("u")]
        [Min(0f)]
        public float stageDoorProbeHeight = 16f;

        [TabGroup(Tabs, "Encore"), BoxGroup(Tabs + "/Encore/Stage Door"), LabelWidth(220), SuffixLabel("u")]
        [Min(0f)]
        public float stageDoorProbeDepth = 32f;

        [TabGroup(Tabs, "Intermission"), BoxGroup(Tabs + "/Intermission/Transition"), LabelWidth(220), SuffixLabel("sec")]
        [Min(0f)]
        public float intermissionSeconds = 1.5f;

        [TabGroup(Tabs, "Intermission"), BoxGroup(Tabs + "/Intermission/Announcement"), LabelWidth(220)]
        public string intermissionAnnouncementTitle = "INTERMISSION";

        [HideInInspector]
        public string intermissionAnnouncementSubtitleFormat = "Preparing {0}...";

        [TabGroup(Tabs, "Intermission"), BoxGroup(Tabs + "/Intermission/Announcement"), LabelWidth(220)]
        [InfoBox("{0} is replaced with the next Act roman label, such as ACT II.")]
        [ListDrawerSettings(DefaultExpandedState = true)]
        public List<string> intermissionAnnouncementSubtitleFormats = new()
        {
            "Preparing {0}...",
            "Resetting the ring for {0}...",
            "The crew clears the stage for {0}..."
        };

        [TabGroup(Tabs, "Curtain Call"), BoxGroup(Tabs + "/Curtain Call/Announcement"), LabelWidth(220)]
        public string curtainCallAnnouncementTitle = "CURTAIN CALL";

        [HideInInspector]
        public string curtainCallAnnouncementSubtitle = "The show is complete.";

        [TabGroup(Tabs, "Curtain Call"), BoxGroup(Tabs + "/Curtain Call/Announcement"), LabelWidth(220)]
        [ListDrawerSettings(DefaultExpandedState = true)]
        public List<string> curtainCallAnnouncementSubtitles = new()
        {
            "The show is complete.",
            "The final bow is yours.",
            "The lights fall on a finished run."
        };

        public IReadOnlyList<RunScheduleActDefinition> Acts => acts;

        public static RunScheduleConfig CreateRuntimeDefault()
        {
            var config = CreateInstance<RunScheduleConfig>();
            config.ApplyDefaultSchedule();
            return config;
        }

        public bool EnsureWorkflowDefaults()
        {
            bool changed = false;
            if (acts == null)
            {
                acts = new List<RunScheduleActDefinition>();
                changed = true;
            }

            if (acts.Count == 0)
            {
                ApplyDefaultSchedule();
                changed = true;
            }

            if (showtimePressureSeconds <= 0f)
            {
                showtimePressureSeconds = 20f;
                changed = true;
            }

            if (showtimeSpawnIntervalMultiplier <= 0f)
            {
                showtimeSpawnIntervalMultiplier = 0.75f;
                changed = true;
            }

            if (showtimeMaxEnemiesMultiplier <= 0f)
            {
                showtimeMaxEnemiesMultiplier = 1.15f;
                changed = true;
            }

            if (encorePressureIntervalSeconds <= 0f)
            {
                encorePressureIntervalSeconds = 30f;
                changed = true;
            }

            if (encoreGraceSeconds < 0f)
            {
                encoreGraceSeconds = 10f;
                changed = true;
            }

            if (encoreSpawnIntervalMultiplierPerStep <= 0f)
            {
                encoreSpawnIntervalMultiplierPerStep = 0.9f;
                changed = true;
            }

            if (encoreMaxEnemiesMultiplierPerStep < 1f)
            {
                encoreMaxEnemiesMultiplierPerStep = 1.1f;
                changed = true;
            }

            changed |= EnsureStageDoorTravelDefaults();

            if (stageDoorGroundClearance < 0f)
            {
                stageDoorGroundClearance = 0.04f;
                changed = true;
            }

            if (stageDoorProbeHeight <= 0f)
            {
                stageDoorProbeHeight = 16f;
                changed = true;
            }

            if (stageDoorProbeDepth <= 0f)
            {
                stageDoorProbeDepth = 32f;
                changed = true;
            }

            changed |= EnsureText(ref showtimeAnnouncementTitle, "SHOWTIME!");
            changed |= EnsureText(ref actFinaleAnnouncementTitle, "HEADLINER!");
            changed |= EnsureText(ref intermissionAnnouncementTitle, "INTERMISSION");
            changed |= EnsureText(ref curtainCallAnnouncementTitle, "CURTAIN CALL");
            changed |= EnsureVariantList(
                ref showtimeAnnouncementSubtitles,
                ref showtimeAnnouncementSubtitle,
                "A swarm enters the ring!",
                "The crowd surges toward the spotlight!",
                "Fresh trouble spills into the ring!");
            changed |= EnsureVariantList(
                ref actFinaleAnnouncementSubtitles,
                ref actFinaleAnnouncementSubtitle,
                "The Headliner enters!",
                "The main attraction storms the stage!",
                "The ring shakes for the final act!");
            changed |= EnsureVariantList(
                ref intermissionAnnouncementSubtitleFormats,
                ref intermissionAnnouncementSubtitleFormat,
                "Preparing {0}...",
                "Resetting the ring for {0}...",
                "The crew clears the stage for {0}...");
            changed |= EnsureVariantList(
                ref curtainCallAnnouncementSubtitles,
                ref curtainCallAnnouncementSubtitle,
                "The show is complete.",
                "The final bow is yours.",
                "The lights fall on a finished run.");

            if (actFinaleWarningSeconds < 0f)
            {
                actFinaleWarningSeconds = 0f;
                changed = true;
            }

            if (actFinaleProxyBossSeconds < 0f)
            {
                actFinaleProxyBossSeconds = 6f;
                changed = true;
            }

            if (headlinerEncoreDeadlineSeconds < 0f)
            {
                headlinerEncoreDeadlineSeconds = 90f;
                changed = true;
            }

            if (intermissionSeconds < 0f)
            {
                intermissionSeconds = 1.5f;
                changed = true;
            }

            return changed;
        }

        public void ApplyDefaultSchedule()
        {
            acts = new List<RunScheduleActDefinition>
            {
                CreateAct("Opening Act", 7f * 60f, 2f * 60f, 5f * 60f),
                CreateAct("Center Ring", 9f * 60f, 2f * 60f, 5f * 60f, 7.5f * 60f),
                CreateAct("Grand Finale", 11f * 60f, 2f * 60f, 5.5f * 60f, 8.5f * 60f)
            };
            showtimePressureSeconds = 20f;
            showtimeSpawnIntervalMultiplier = 0.75f;
            showtimeMaxEnemiesMultiplier = 1.15f;
            showtimeAnnouncementTitle = "SHOWTIME!";
            showtimeAnnouncementSubtitle = "A swarm enters the ring!";
            showtimeAnnouncementSubtitles = new List<string>
            {
                "A swarm enters the ring!",
                "The crowd surges toward the spotlight!",
                "Fresh trouble spills into the ring!"
            };
            actFinaleAnnouncementTitle = "HEADLINER!";
            actFinaleAnnouncementSubtitle = "The Headliner enters!";
            actFinaleAnnouncementSubtitles = new List<string>
            {
                "The Headliner enters!",
                "The main attraction storms the stage!",
                "The ring shakes for the final act!"
            };
            actFinaleWarningSeconds = 2f;
            actFinaleProxyAutoDefeat = true;
            actFinaleProxyBossSeconds = 6f;
            headlinerEncoreDeadlineSeconds = 90f;
            encoreGraceSeconds = 10f;
            encorePressureIntervalSeconds = 30f;
            encoreSpawnIntervalMultiplierPerStep = 0.9f;
            encoreMaxEnemiesMultiplierPerStep = 1.1f;
            stageDoorTravelSeconds = CreateDefaultStageDoorTravel();
            stageDoorGroundClearance = 0.04f;
            stageDoorProbeHeight = 16f;
            stageDoorProbeDepth = 32f;
            intermissionSeconds = 1.5f;
            intermissionAnnouncementTitle = "INTERMISSION";
            intermissionAnnouncementSubtitleFormat = "Preparing {0}...";
            intermissionAnnouncementSubtitleFormats = new List<string>
            {
                "Preparing {0}...",
                "Resetting the ring for {0}...",
                "The crew clears the stage for {0}..."
            };
            curtainCallAnnouncementTitle = "CURTAIN CALL";
            curtainCallAnnouncementSubtitle = "The show is complete.";
            curtainCallAnnouncementSubtitles = new List<string>
            {
                "The show is complete.",
                "The final bow is yours.",
                "The lights fall on a finished run."
            };
        }

        public RunScheduleActDefinition GetActForWorldIndex(int worldIndex)
        {
            EnsureWorkflowDefaults();
            if (acts == null || acts.Count == 0)
            {
                return null;
            }

            int index = Mathf.Clamp(worldIndex - 1, 0, acts.Count - 1);
            return acts[index];
        }

        public RunStageDoorTravelDefinition GetStageDoorTravelForAct(int actNumber)
        {
            EnsureWorkflowDefaults();
            if (stageDoorTravelSeconds == null || stageDoorTravelSeconds.Count == 0)
            {
                return CreateStageDoorTravel("Act I", 15f, 25f);
            }

            int index = Mathf.Clamp(actNumber - 1, 0, stageDoorTravelSeconds.Count - 1);
            return stageDoorTravelSeconds[index];
        }

        public List<ContentValidationIssue> ValidateContent()
        {
            var issues = new List<ContentValidationIssue>();
            if (acts == null || acts.Count == 0)
            {
                issues.Add(new ContentValidationIssue("run-schedule.no-acts", ContentValidationSeverity.Error, "Run schedule has no Acts."));
                return issues;
            }

            if (showtimePressureSeconds <= 0f)
            {
                issues.Add(new ContentValidationIssue("run-schedule.invalid-pressure-duration", ContentValidationSeverity.Error, "Showtime pressure duration must be greater than zero."));
            }

            if (showtimeSpawnIntervalMultiplier <= 0f)
            {
                issues.Add(new ContentValidationIssue("run-schedule.invalid-spawn-interval-multiplier", ContentValidationSeverity.Error, "Showtime spawn interval multiplier must be greater than zero."));
            }

            if (showtimeMaxEnemiesMultiplier <= 0f)
            {
                issues.Add(new ContentValidationIssue("run-schedule.invalid-max-enemies-multiplier", ContentValidationSeverity.Error, "Showtime max enemy multiplier must be greater than zero."));
            }

            if (encorePressureIntervalSeconds <= 0f)
            {
                issues.Add(new ContentValidationIssue("run-schedule.invalid-encore-interval", ContentValidationSeverity.Error, "Encore pressure interval must be greater than zero."));
            }

            if (encoreGraceSeconds < 0f)
            {
                issues.Add(new ContentValidationIssue("run-schedule.invalid-encore-grace", ContentValidationSeverity.Error, "Encore grace seconds cannot be negative."));
            }

            if (encoreSpawnIntervalMultiplierPerStep <= 0f)
            {
                issues.Add(new ContentValidationIssue("run-schedule.invalid-encore-spawn-multiplier", ContentValidationSeverity.Error, "Encore spawn interval multiplier per step must be greater than zero."));
            }

            if (encoreMaxEnemiesMultiplierPerStep < 1f)
            {
                issues.Add(new ContentValidationIssue("run-schedule.invalid-encore-max-enemies-multiplier", ContentValidationSeverity.Error, "Encore max enemies multiplier per step must be at least one."));
            }

            if (stageDoorTravelSeconds == null || stageDoorTravelSeconds.Count == 0)
            {
                issues.Add(new ContentValidationIssue("run-schedule.no-stage-door-travel", ContentValidationSeverity.Error, "Stage Door needs at least one travel-time range."));
            }
            else
            {
                for (int i = 0; i < stageDoorTravelSeconds.Count; i++)
                {
                    RunStageDoorTravelDefinition travel = stageDoorTravelSeconds[i];
                    if (travel == null)
                    {
                        issues.Add(new ContentValidationIssue("run-schedule.null-stage-door-travel", ContentValidationSeverity.Error, $"Stage Door travel range {i + 1} is null."));
                        continue;
                    }

                    if (travel.minTravelSeconds < 0f || travel.maxTravelSeconds < travel.minTravelSeconds)
                    {
                        issues.Add(new ContentValidationIssue("run-schedule.invalid-stage-door-travel", ContentValidationSeverity.Error, $"{travel.actLabel} Stage Door travel range must be non-negative and max must be at least min."));
                    }
                }
            }

            if (stageDoorGroundClearance < 0f || stageDoorProbeHeight <= 0f || stageDoorProbeDepth <= 0f)
            {
                issues.Add(new ContentValidationIssue("run-schedule.invalid-stage-door-grounding", ContentValidationSeverity.Error, "Stage Door grounding probe values must be positive and clearance cannot be negative."));
            }

            if (actFinaleWarningSeconds < 0f)
            {
                issues.Add(new ContentValidationIssue("run-schedule.invalid-finale-warning", ContentValidationSeverity.Error, "Headliner warning seconds cannot be negative."));
            }

            if (actFinaleProxyBossSeconds < 0f)
            {
                issues.Add(new ContentValidationIssue("run-schedule.invalid-proxy-boss-duration", ContentValidationSeverity.Error, "Headliner proxy boss seconds cannot be negative."));
            }

            if (headlinerEncoreDeadlineSeconds < 0f)
            {
                issues.Add(new ContentValidationIssue("run-schedule.invalid-headliner-encore-deadline", ContentValidationSeverity.Error, "Headliner Encore deadline seconds cannot be negative."));
            }

            if (intermissionSeconds < 0f)
            {
                issues.Add(new ContentValidationIssue("run-schedule.invalid-intermission", ContentValidationSeverity.Error, "Intermission seconds cannot be negative."));
            }

            ValidateSubtitleVariants(issues, showtimeAnnouncementSubtitles, "run-schedule.no-showtime-subtitles", "Showtime needs at least one subtitle variant.");
            ValidateSubtitleVariants(issues, actFinaleAnnouncementSubtitles, "run-schedule.no-finale-subtitles", "Headliner arrival needs at least one subtitle variant.");
            ValidateSubtitleVariants(issues, intermissionAnnouncementSubtitleFormats, "run-schedule.no-intermission-subtitles", "Intermission needs at least one subtitle variant.");
            ValidateSubtitleVariants(issues, curtainCallAnnouncementSubtitles, "run-schedule.no-curtain-call-subtitles", "Curtain Call needs at least one subtitle variant.");

            for (int actIndex = 0; actIndex < acts.Count; actIndex++)
            {
                RunScheduleActDefinition act = acts[actIndex];
                if (act == null)
                {
                    issues.Add(new ContentValidationIssue("run-schedule.null-act", ContentValidationSeverity.Error, $"Run schedule contains a null Act at index {actIndex}."));
                    continue;
                }

                string actLabel = string.IsNullOrWhiteSpace(act.displayName) ? $"Act {actIndex + 1}" : act.displayName;
                if (string.IsNullOrWhiteSpace(act.displayName))
                {
                    issues.Add(new ContentValidationIssue("run-schedule.missing-act-name", ContentValidationSeverity.Error, $"Act {actIndex + 1} is missing a display name."));
                }

                if (act.durationSeconds <= 0f)
                {
                    issues.Add(new ContentValidationIssue("run-schedule.invalid-act-duration", ContentValidationSeverity.Error, $"{actLabel} duration must be greater than zero."));
                    continue;
                }

                if (act.showtimeSeconds == null)
                {
                    issues.Add(new ContentValidationIssue("run-schedule.null-showtimes", ContentValidationSeverity.Error, $"{actLabel} Showtime list is null."));
                    continue;
                }

                var seenTimes = new HashSet<int>();
                for (int showtimeIndex = 0; showtimeIndex < act.showtimeSeconds.Count; showtimeIndex++)
                {
                    float timestamp = act.showtimeSeconds[showtimeIndex];
                    if (timestamp <= 0f || timestamp >= act.durationSeconds)
                    {
                        issues.Add(new ContentValidationIssue("run-schedule.invalid-showtime-time", ContentValidationSeverity.Error, $"{actLabel} Showtime {showtimeIndex + 1} must be after 0 and before the Headliner."));
                    }

                    int roundedMilliseconds = Mathf.RoundToInt(timestamp * 1000f);
                    if (!seenTimes.Add(roundedMilliseconds))
                    {
                        issues.Add(new ContentValidationIssue("run-schedule.duplicate-showtime-time", ContentValidationSeverity.Error, $"{actLabel} has duplicate Showtime timestamp {timestamp:0.##} seconds."));
                    }
                }
            }

            return issues;
        }

        private static RunScheduleActDefinition CreateAct(string displayName, float durationSeconds, params float[] showtimes)
        {
            return new RunScheduleActDefinition
            {
                displayName = displayName,
                durationSeconds = durationSeconds,
                showtimeSeconds = new List<float>(showtimes)
            };
        }

        private bool EnsureStageDoorTravelDefaults()
        {
            bool changed = false;
            if (stageDoorTravelSeconds == null)
            {
                stageDoorTravelSeconds = new List<RunStageDoorTravelDefinition>();
                changed = true;
            }

            List<RunStageDoorTravelDefinition> defaults = CreateDefaultStageDoorTravel();
            for (int i = 0; i < defaults.Count; i++)
            {
                if (i >= stageDoorTravelSeconds.Count)
                {
                    stageDoorTravelSeconds.Add(defaults[i]);
                    changed = true;
                    continue;
                }

                RunStageDoorTravelDefinition current = stageDoorTravelSeconds[i];
                if (current == null)
                {
                    stageDoorTravelSeconds[i] = defaults[i];
                    changed = true;
                    continue;
                }

                if (current.actLabel != defaults[i].actLabel)
                {
                    current.actLabel = defaults[i].actLabel;
                    changed = true;
                }

                if (current.maxTravelSeconds < current.minTravelSeconds)
                {
                    current.maxTravelSeconds = current.minTravelSeconds;
                    changed = true;
                }
            }

            return changed;
        }

        private static List<RunStageDoorTravelDefinition> CreateDefaultStageDoorTravel()
        {
            return new List<RunStageDoorTravelDefinition>
            {
                CreateStageDoorTravel("Act I", 15f, 25f),
                CreateStageDoorTravel("Act II", 25f, 40f),
                CreateStageDoorTravel("Act III", 35f, 55f)
            };
        }

        private static RunStageDoorTravelDefinition CreateStageDoorTravel(string actLabel, float minSeconds, float maxSeconds)
        {
            return new RunStageDoorTravelDefinition
            {
                actLabel = actLabel,
                minTravelSeconds = minSeconds,
                maxTravelSeconds = maxSeconds
            };
        }

        private static bool EnsureText(ref string value, string fallback)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                value = value.Trim();
                return false;
            }

            value = fallback;
            return true;
        }

        private static bool EnsureVariantList(ref List<string> variants, ref string fallback, params string[] defaults)
        {
            bool changed = false;
            if (variants == null)
            {
                variants = new List<string>();
                changed = true;
            }

            for (int i = variants.Count - 1; i >= 0; i--)
            {
                string value = variants[i];
                if (string.IsNullOrWhiteSpace(value))
                {
                    variants.RemoveAt(i);
                    changed = true;
                    continue;
                }

                string trimmed = value.Trim();
                if (trimmed != value)
                {
                    variants[i] = trimmed;
                    changed = true;
                }
            }

            if (variants.Count == 0)
            {
                if (!string.IsNullOrWhiteSpace(fallback))
                {
                    variants.Add(fallback.Trim());
                }
                else if (defaults != null)
                {
                    for (int i = 0; i < defaults.Length; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(defaults[i]))
                        {
                            variants.Add(defaults[i].Trim());
                        }
                    }
                }

                changed = true;
            }

            if (variants.Count > 0 && fallback != variants[0])
            {
                fallback = variants[0];
                changed = true;
            }

            return changed;
        }

        private static void ValidateSubtitleVariants(
            List<ContentValidationIssue> issues,
            IReadOnlyList<string> variants,
            string issueId,
            string message)
        {
            if (variants == null)
            {
                issues.Add(new ContentValidationIssue(issueId, ContentValidationSeverity.Error, message));
                return;
            }

            for (int i = 0; i < variants.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(variants[i]))
                {
                    return;
                }
            }

            issues.Add(new ContentValidationIssue(issueId, ContentValidationSeverity.Error, message));
        }
    }
}
