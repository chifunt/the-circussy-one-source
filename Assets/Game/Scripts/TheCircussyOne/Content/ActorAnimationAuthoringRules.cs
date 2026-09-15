using System.Collections.Generic;
using UnityEngine;

namespace TheCircussyOne.Content
{
    public enum ActorAnimationDiagnosticSeverity
    {
        Info,
        Warning,
        Error
    }

    public readonly struct ActorAnimationAuthoringDiagnostic
    {
        public ActorAnimationAuthoringDiagnostic(string code, ActorAnimationDiagnosticSeverity severity, string message)
        {
            Code = code;
            Severity = severity;
            Message = message;
        }

        public string Code { get; }
        public ActorAnimationDiagnosticSeverity Severity { get; }
        public string Message { get; }
    }

    public static class ActorAnimationAuthoringRules
    {
        public static List<ActorAnimationAuthoringDiagnostic> PerformerDiagnostics(PerformerDefinition performer)
        {
            var diagnostics = new List<ActorAnimationAuthoringDiagnostic>();
            if (performer == null)
            {
                diagnostics.Add(Error("actor.missing-definition", "No performer definition selected."));
                return diagnostics;
            }

            AddModelDiagnostics(diagnostics, performer.worldPrefab, performer.modelTransform);
            AddPerformerAnimationDiagnostics(diagnostics, performer);
            return diagnostics;
        }

        public static List<ActorAnimationAuthoringDiagnostic> EnemyDiagnostics(EnemyDefinition enemy)
        {
            var diagnostics = new List<ActorAnimationAuthoringDiagnostic>();
            if (enemy == null)
            {
                diagnostics.Add(Error("actor.missing-definition", "No enemy definition selected."));
                return diagnostics;
            }

            AddModelDiagnostics(diagnostics, enemy.worldPrefab, enemy.modelTransform);
            AddEnemyAnimationDiagnostics(diagnostics, enemy);
            return diagnostics;
        }

        public static void AddPerformerValidationIssues(PerformerDefinition performer, List<ContentValidationIssue> issues)
        {
            AddValidationIssues(PerformerDiagnostics(performer), performer?.Id, issues);
        }

        public static void AddEnemyValidationIssues(EnemyDefinition enemy, List<ContentValidationIssue> issues)
        {
            AddValidationIssues(EnemyDiagnostics(enemy), enemy?.Id, issues);
        }

        public static string ClipSummary(AnimationClip clip)
        {
            if (clip == null)
            {
                return "Missing";
            }

            string loop = clip.isLooping ? "looping" : "one-shot";
            return $"{clip.name} ({clip.length:0.##}s, {loop})";
        }

        private static void AddModelDiagnostics(
            List<ActorAnimationAuthoringDiagnostic> diagnostics,
            GameObject worldPrefab,
            ActorModelTransformProfile transformProfile)
        {
            if (worldPrefab == null)
            {
                diagnostics.Add(Info("actor.fallback-body", "No runtime model assigned. The generated actor body will be used."));
            }

            if (transformProfile == null)
            {
                diagnostics.Add(Warning("actor.missing-transform-profile", "Model transform profile is missing; runtime falls back to zero position, zero rotation, and unit scale."));
                return;
            }

            if (!IsFinite(transformProfile.localPosition) || !IsFinite(transformProfile.localEulerAngles))
            {
                diagnostics.Add(Error("actor.invalid-transform-values", "Model transform contains NaN or Infinity values."));
            }

            if (!IsFinite(transformProfile.localScale)
                || transformProfile.localScale.x <= 0f
                || transformProfile.localScale.y <= 0f
                || transformProfile.localScale.z <= 0f)
            {
                diagnostics.Add(Error("actor.invalid-transform-scale", "Model scale must be finite and greater than zero on every axis."));
            }
        }

        private static void AddPerformerAnimationDiagnostics(
            List<ActorAnimationAuthoringDiagnostic> diagnostics,
            PerformerDefinition performer)
        {
            PerformerAnimationProfile animation = performer.animation;
            if (animation == null)
            {
                if (performer.worldPrefab != null)
                {
                    diagnostics.Add(Warning("performer.model-missing-animation", $"Performer '{performer.Id}' has a runtime model but no animation profile; the model will stay in bind pose."));
                }

                return;
            }

            if (performer.worldPrefab != null && animation.idle == null && animation.run == null)
            {
                diagnostics.Add(Warning("performer.model-missing-animation", $"Performer '{performer.Id}' has a runtime model but no idle or run animation clip; the model will stay in bind pose."));
            }

            if (performer.worldPrefab != null && animation.jump == null)
            {
                diagnostics.Add(Info("performer.missing-jump-clip", "No jump clip assigned. Procedural jump squash/stretch remains the visual fallback."));
            }

            if (performer.worldPrefab != null && animation.land == null)
            {
                diagnostics.Add(Info("performer.missing-land-clip", "No land clip assigned. Procedural landing squash/stretch remains the visual fallback."));
            }

            AddInvalidSpeedDiagnostics(
                diagnostics,
                "performer.invalid-animation-speed",
                animation.idleSpeed,
                animation.runSpeed,
                animation.jumpSpeed,
                animation.landSpeed);
            AddRunThresholdDiagnostics(
                diagnostics,
                "performer.invalid-run-thresholds",
                animation.runThreshold01,
                animation.runExitThreshold01);
            AddLoopingClipDiagnostic(diagnostics, "performer.non-looping-locomotion-clip", "Idle", animation.idle);
            AddLoopingClipDiagnostic(diagnostics, "performer.non-looping-locomotion-clip", "Run", animation.run);
            diagnostics.Add(Info("performer.idle-clip", $"Idle: {ClipSummary(animation.idle)}"));
            diagnostics.Add(Info("performer.run-clip", $"Run: {ClipSummary(animation.run)}"));
            diagnostics.Add(Info("performer.jump-clip", $"Jump: {ClipSummary(animation.jump)}"));
            diagnostics.Add(Info("performer.land-clip", $"Land: {ClipSummary(animation.land)}"));
        }

        private static void AddEnemyAnimationDiagnostics(
            List<ActorAnimationAuthoringDiagnostic> diagnostics,
            EnemyDefinition enemy)
        {
            EnemyAnimationProfile animation = enemy.animation;
            if (animation == null)
            {
                if (enemy.worldPrefab != null)
                {
                    diagnostics.Add(Warning("enemy.model-missing-animation", $"Enemy '{enemy.Id}' has a runtime model but no animation profile; the model will stay in bind pose."));
                }

                return;
            }

            if (enemy.worldPrefab != null && animation.idle == null && animation.run == null && animation.floatingIdle == null)
            {
                diagnostics.Add(Warning("enemy.model-missing-animation", $"Enemy '{enemy.Id}' has a runtime model but no animation clip; the model will stay in bind pose."));
            }

            if (enemy.worldPrefab != null
                && enemy.locomotion != null
                && enemy.locomotion.mode == EnemyLocomotionMode.Floating
                && animation.floatingIdle == null)
            {
                diagnostics.Add(Warning("enemy.floating-missing-animation", $"Floating enemy '{enemy.Id}' has no floating idle clip; the model can stay in bind pose while hovering."));
            }

            AddInvalidSpeedDiagnostics(
                diagnostics,
                "enemy.invalid-animation-speed",
                animation.idleSpeed,
                animation.runSpeed,
                animation.floatingIdleSpeed);
            AddRunThresholdDiagnostics(
                diagnostics,
                "enemy.invalid-run-thresholds",
                animation.runThreshold01,
                animation.runExitThreshold01);
            AddLoopingClipDiagnostic(diagnostics, "enemy.non-looping-locomotion-clip", "Idle", animation.idle);
            AddLoopingClipDiagnostic(diagnostics, "enemy.non-looping-locomotion-clip", "Run", animation.run);
            AddLoopingClipDiagnostic(diagnostics, "enemy.non-looping-locomotion-clip", "Floating idle", animation.floatingIdle);
            diagnostics.Add(Info("enemy.idle-clip", $"Idle: {ClipSummary(animation.idle)}"));
            diagnostics.Add(Info("enemy.run-clip", $"Run: {ClipSummary(animation.run)}"));
            diagnostics.Add(Info("enemy.floating-idle-clip", $"Floating Idle: {ClipSummary(animation.floatingIdle)}"));
        }

        private static void AddLoopingClipDiagnostic(
            List<ActorAnimationAuthoringDiagnostic> diagnostics,
            string code,
            string label,
            AnimationClip clip)
        {
            if (clip != null && !clip.isLooping)
            {
                diagnostics.Add(Warning(code, $"{label} clip '{clip.name}' is not marked Loop Time; runtime locomotion can stop on its last frame."));
            }
        }

        private static void AddInvalidSpeedDiagnostics(
            List<ActorAnimationAuthoringDiagnostic> diagnostics,
            string code,
            params float[] speeds)
        {
            for (int i = 0; i < speeds.Length; i++)
            {
                if (!IsFinite(speeds[i]) || speeds[i] <= 0f)
                {
                    diagnostics.Add(Error(code, "Animation speed values must be finite and greater than zero."));
                    return;
                }
            }
        }

        private static void AddRunThresholdDiagnostics(
            List<ActorAnimationAuthoringDiagnostic> diagnostics,
            string code,
            float enter,
            float exit)
        {
            if (!IsFinite(enter) || !IsFinite(exit) || enter < 0f || enter > 1f || exit < 0f || exit > 1f)
            {
                diagnostics.Add(Error(code, "Run thresholds must be between 0 and 1."));
                return;
            }

            if (exit > enter)
            {
                diagnostics.Add(Warning(code, "Run exit threshold should not be greater than the run enter threshold."));
            }
        }

        private static void AddValidationIssues(
            IReadOnlyList<ActorAnimationAuthoringDiagnostic> diagnostics,
            string contentId,
            List<ContentValidationIssue> issues)
        {
            if (diagnostics == null || issues == null)
            {
                return;
            }

            for (int i = 0; i < diagnostics.Count; i++)
            {
                ActorAnimationAuthoringDiagnostic diagnostic = diagnostics[i];
                if (diagnostic.Severity == ActorAnimationDiagnosticSeverity.Info)
                {
                    continue;
                }

                issues.Add(new ContentValidationIssue(
                    diagnostic.Code,
                    diagnostic.Severity == ActorAnimationDiagnosticSeverity.Error ? ContentValidationSeverity.Error : ContentValidationSeverity.Warning,
                    diagnostic.Message,
                    contentId));
            }
        }

        private static ActorAnimationAuthoringDiagnostic Info(string code, string message)
        {
            return new ActorAnimationAuthoringDiagnostic(code, ActorAnimationDiagnosticSeverity.Info, message);
        }

        private static ActorAnimationAuthoringDiagnostic Warning(string code, string message)
        {
            return new ActorAnimationAuthoringDiagnostic(code, ActorAnimationDiagnosticSeverity.Warning, message);
        }

        private static ActorAnimationAuthoringDiagnostic Error(string code, string message)
        {
            return new ActorAnimationAuthoringDiagnostic(code, ActorAnimationDiagnosticSeverity.Error, message);
        }

        private static bool IsFinite(Vector3 value)
        {
            return IsFinite(value.x) && IsFinite(value.y) && IsFinite(value.z);
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
