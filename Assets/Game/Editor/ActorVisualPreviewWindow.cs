using System;
using TheCircussyOne.Content;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public sealed class ActorVisualPreviewWindow : EditorWindow
{
    private const float MinimumPreviewHeight = 260f;

    private PreviewRenderUtility previewUtility;
    private GameObject previewInstance;
    private GameObject sourcePrefab;
    private ActorModelTransformProfile transformProfile;
    private string actorName;
    private string actorKind;
    private AnimationClip[] clips = Array.Empty<AnimationClip>();
    private float[] clipSpeeds = Array.Empty<float>();
    private string[] clipLabels = Array.Empty<string>();
    private int selectedClipIndex;
    private float sampleTime;
    private bool playing;
    private double lastEditorTime;
    private Vector2 scroll;

    public static ActorVisualPreviewWindow Open(PerformerDefinition performer)
    {
        ActorVisualPreviewWindow window = GetWindow<ActorVisualPreviewWindow>("Actor Visual Preview");
        window.Configure(performer);
        window.Show();
        return window;
    }

    public static ActorVisualPreviewWindow Open(EnemyDefinition enemy)
    {
        ActorVisualPreviewWindow window = GetWindow<ActorVisualPreviewWindow>("Actor Visual Preview");
        window.Configure(enemy);
        window.Show();
        return window;
    }

    private void Configure(PerformerDefinition performer)
    {
        actorKind = "Performer";
        actorName = performer != null ? performer.DisplayName : "Missing Performer";
        sourcePrefab = performer != null ? performer.worldPrefab : null;
        transformProfile = performer != null ? performer.modelTransform : null;
        PerformerAnimationProfile animation = performer != null ? performer.animation : null;
        clips = new[]
        {
            animation?.idle,
            animation?.run,
            animation?.jump,
            animation?.land
        };
        clipSpeeds = new[]
        {
            animation?.idleSpeed ?? 1f,
            animation?.runSpeed ?? 1f,
            animation?.jumpSpeed ?? 1f,
            animation?.landSpeed ?? 1f
        };
        clipLabels = new[] { "Idle", "Run", "Jump", "Land" };
        selectedClipIndex = FirstAvailableClipIndex(clips);
        sampleTime = 0f;
        playing = false;
        RebuildPreview();
        Repaint();
    }

    private void Configure(EnemyDefinition enemy)
    {
        actorKind = "Enemy";
        actorName = enemy != null ? enemy.DisplayName : "Missing Enemy";
        sourcePrefab = enemy != null ? enemy.worldPrefab : null;
        transformProfile = enemy != null ? enemy.modelTransform : null;
        EnemyAnimationProfile animation = enemy != null ? enemy.animation : null;
        clips = new[]
        {
            animation?.idle,
            animation?.run,
            animation?.floatingIdle
        };
        clipSpeeds = new[]
        {
            animation?.idleSpeed ?? 1f,
            animation?.runSpeed ?? 1f,
            animation?.floatingIdleSpeed ?? 1f
        };
        clipLabels = new[] { "Idle", "Run", "Floating Idle" };
        selectedClipIndex = FirstAvailableClipIndex(clips);
        sampleTime = 0f;
        playing = false;
        RebuildPreview();
        Repaint();
    }

    private void OnDisable()
    {
        ReleasePreview();
    }

    private void OnGUI()
    {
        scroll = EditorGUILayout.BeginScrollView(scroll);
        EditorGUILayout.LabelField($"{actorKind}: {actorName}", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Preview samples the assigned model transform and one clip at a time. Runtime Animancer blending still happens in play mode.", EditorStyles.wordWrappedMiniLabel);
        EditorGUILayout.Space(4f);

        DrawControls();
        Rect previewRect = GUILayoutUtility.GetRect(
            320f,
            Mathf.Max(MinimumPreviewHeight, position.height - 180f),
            GUILayout.ExpandWidth(true));
        DrawPreview(previewRect);

        EditorGUILayout.EndScrollView();
        TickPlayback();
    }

    private void DrawControls()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            using (new EditorGUI.DisabledScope(clips == null || clips.Length == 0))
            {
                selectedClipIndex = EditorGUILayout.Popup("Clip", selectedClipIndex, ClipPopupLabels());
            }

            using (new EditorGUI.DisabledScope(CurrentClip() == null))
            {
                if (GUILayout.Button(playing ? "Pause" : "Play", GUILayout.Width(72f)))
                {
                    playing = !playing;
                    lastEditorTime = EditorApplication.timeSinceStartup;
                }

                if (GUILayout.Button("Reset", GUILayout.Width(72f)))
                {
                    sampleTime = 0f;
                    playing = false;
                    SampleCurrentClip();
                }
            }
        }

        AnimationClip clip = CurrentClip();
        using (new EditorGUI.DisabledScope(clip == null))
        {
            float clipLength = clip != null ? Mathf.Max(0.001f, clip.length) : 1f;
            sampleTime = EditorGUILayout.Slider("Time", Mathf.Clamp(sampleTime, 0f, clipLength), 0f, clipLength);
            EditorGUILayout.LabelField("Clip Summary", clip != null ? ActorAnimationAuthoringRules.ClipSummary(clip) : "Missing", EditorStyles.wordWrappedMiniLabel);
        }

        if (sourcePrefab == null)
        {
            EditorGUILayout.HelpBox("No runtime model assigned. The actor will use the generated fallback body in game.", MessageType.Info);
        }
        else if (previewInstance == null)
        {
            EditorGUILayout.HelpBox("Preview model could not be instantiated. Check that the assigned model is a GameObject prefab or model asset.", MessageType.Warning);
        }
    }

    private void DrawPreview(Rect previewRect)
    {
        GUI.Box(previewRect, GUIContent.none);
        if (sourcePrefab == null)
        {
            DrawCenteredPreviewLabel(previewRect, "No model assigned");
            return;
        }

        if (previewUtility == null || previewInstance == null)
        {
            RebuildPreview();
        }

        if (previewUtility == null || previewInstance == null)
        {
            DrawCenteredPreviewLabel(previewRect, "Preview unavailable");
            return;
        }

        if (Event.current.type != EventType.Repaint)
        {
            return;
        }

        SampleCurrentClip();
        PositionCamera();
        previewUtility.BeginPreview(previewRect, GUIStyle.none);
        previewUtility.Render(allowScriptableRenderPipeline: true, updatefov: true);
        Texture texture = previewUtility.EndPreview();
        GUI.DrawTexture(previewRect, texture, ScaleMode.StretchToFill, false);
    }

    private void RebuildPreview()
    {
        ReleasePreview();
        if (sourcePrefab == null)
        {
            return;
        }

        previewUtility = new PreviewRenderUtility();
        previewUtility.cameraFieldOfView = 30f;
        if (previewUtility.lights != null && previewUtility.lights.Length > 0)
        {
            previewUtility.lights[0].intensity = 1.3f;
            previewUtility.lights[0].transform.rotation = Quaternion.Euler(35f, 35f, 0f);
        }

        if (previewUtility.lights != null && previewUtility.lights.Length > 1)
        {
            previewUtility.lights[1].intensity = 0.7f;
            previewUtility.lights[1].transform.rotation = Quaternion.Euler(340f, 218f, 177f);
        }

        previewInstance = InstantiatePreviewModel(sourcePrefab);
        if (previewInstance == null)
        {
            ReleasePreview();
            return;
        }

        previewInstance.hideFlags = HideFlags.HideAndDontSave;
        ApplyTransform(previewInstance.transform, transformProfile);
        DisableColliders(previewInstance);
        SampleCurrentClip();
    }

    private void ReleasePreview()
    {
        if (previewInstance != null)
        {
            DestroyImmediate(previewInstance);
            previewInstance = null;
        }

        if (previewUtility != null)
        {
            previewUtility.Cleanup();
            previewUtility = null;
        }
    }

    private GameObject InstantiatePreviewModel(GameObject prefab)
    {
        try
        {
            if (previewUtility != null)
            {
                GameObject previewPrefab = previewUtility.InstantiatePrefabInScene(prefab);
                if (previewPrefab != null)
                {
                    return previewPrefab;
                }
            }
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"Actor visual preview could not instantiate prefab '{prefab.name}' in the preview scene. Falling back to direct instantiate. {exception.Message}");
        }

        try
        {
            Object clone = Object.Instantiate((Object)prefab);
            if (clone is GameObject model)
            {
#pragma warning disable CS0618
                previewUtility?.AddSingleGO(model);
#pragma warning restore CS0618
                return model;
            }

            DestroyImmediate(clone);
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"Actor visual preview could not instantiate model '{prefab.name}'. {exception.Message}");
        }

        return null;
    }

    private void SampleCurrentClip()
    {
        AnimationClip clip = CurrentClip();
        if (previewInstance == null || clip == null)
        {
            return;
        }

        float length = Mathf.Max(0.001f, clip.length);
        sampleTime = Mathf.Clamp(sampleTime, 0f, length);
        clip.SampleAnimation(previewInstance, sampleTime);
    }

    private void PositionCamera()
    {
        if (previewUtility == null || previewInstance == null)
        {
            return;
        }

        Bounds bounds = CalculateBounds(previewInstance);
        Vector3 center = bounds.center;
        float size = Mathf.Max(1f, bounds.extents.magnitude);
        Camera camera = previewUtility.camera;
        camera.nearClipPlane = 0.01f;
        camera.farClipPlane = Mathf.Max(50f, size * 12f);
        camera.transform.position = center + new Vector3(size * 0.45f, size * 0.35f, -size * 2.8f);
        camera.transform.LookAt(center + Vector3.up * size * 0.05f);
    }

    private void TickPlayback()
    {
        if (!playing || CurrentClip() == null)
        {
            lastEditorTime = EditorApplication.timeSinceStartup;
            return;
        }

        double now = EditorApplication.timeSinceStartup;
        float deltaTime = Mathf.Max(0f, (float)(now - lastEditorTime));
        lastEditorTime = now;

        AnimationClip clip = CurrentClip();
        float speed = Mathf.Max(0.01f, CurrentClipSpeed());
        sampleTime += deltaTime * speed;
        if (clip != null && clip.length > 0f)
        {
            sampleTime = clip.isLooping ? sampleTime % clip.length : Mathf.Min(sampleTime, clip.length);
            if (!clip.isLooping && sampleTime >= clip.length)
            {
                playing = false;
            }
        }

        Repaint();
    }

    private AnimationClip CurrentClip()
    {
        if (clips == null || selectedClipIndex < 0 || selectedClipIndex >= clips.Length)
        {
            return null;
        }

        return clips[selectedClipIndex];
    }

    private float CurrentClipSpeed()
    {
        if (clipSpeeds == null || selectedClipIndex < 0 || selectedClipIndex >= clipSpeeds.Length)
        {
            return 1f;
        }

        return clipSpeeds[selectedClipIndex];
    }

    private string[] ClipPopupLabels()
    {
        if (clips == null || clipLabels == null)
        {
            return Array.Empty<string>();
        }

        string[] labels = new string[clips.Length];
        for (int i = 0; i < labels.Length; i++)
        {
            string prefix = i < clipLabels.Length ? clipLabels[i] : $"Clip {i + 1}";
            labels[i] = clips[i] != null ? $"{prefix}: {clips[i].name}" : $"{prefix}: Missing";
        }

        return labels;
    }

    private static int FirstAvailableClipIndex(AnimationClip[] availableClips)
    {
        if (availableClips == null)
        {
            return 0;
        }

        for (int i = 0; i < availableClips.Length; i++)
        {
            if (availableClips[i] != null)
            {
                return i;
            }
        }

        return 0;
    }

    private static void ApplyTransform(Transform target, ActorModelTransformProfile profile)
    {
        if (target == null)
        {
            return;
        }

        if (profile == null)
        {
            target.localPosition = Vector3.zero;
            target.localRotation = Quaternion.identity;
            target.localScale = Vector3.one;
            return;
        }

        target.localPosition = IsFinite(profile.localPosition) ? profile.localPosition : Vector3.zero;
        target.localRotation = Quaternion.Euler(IsFinite(profile.localEulerAngles) ? profile.localEulerAngles : Vector3.zero);
        target.localScale = IsValidScale(profile.localScale) ? profile.localScale : Vector3.one;
    }

    private static void DisableColliders(GameObject root)
    {
        if (root == null)
        {
            return;
        }

        Collider[] colliders = root.GetComponentsInChildren<Collider>(includeInactive: true);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] != null)
            {
                colliders[i].enabled = false;
            }
        }
    }

    private static Bounds CalculateBounds(GameObject root)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(includeInactive: true);
        if (renderers == null || renderers.Length == 0)
        {
            return new Bounds(Vector3.up, Vector3.one * 2f);
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        return bounds;
    }

    private static void DrawCenteredPreviewLabel(Rect rect, string text)
    {
        GUIStyle style = new(EditorStyles.centeredGreyMiniLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 13
        };
        GUI.Label(rect, text, style);
    }

    private static bool IsValidScale(Vector3 value)
    {
        return IsFinite(value) && value.x > 0f && value.y > 0f && value.z > 0f;
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
