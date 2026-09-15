using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using TheCircussyOne.Visuals;

public static partial class TheCircussyOneSceneBuilder
{
    private const string InteractableSelectionOutlineFeatureName = "The Circussy One Interactable Selection Outline";

    internal static void EnsureInteractableSelectionOutlineRendererFeature()
    {
        AssetDatabase.ImportAsset(InteractableSelectionMaskShaderPath);
        AssetDatabase.ImportAsset(InteractableSelectionOutlineShaderPath);

        var rendererData = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(PcRendererDataPath);
        if (rendererData == null)
        {
            return;
        }

        Shader maskShader = AssetDatabase.LoadAssetAtPath<Shader>(InteractableSelectionMaskShaderPath)
            ?? Shader.Find("TheCircussyOne/Interactable Selection Mask");
        Shader outlineShader = AssetDatabase.LoadAssetAtPath<Shader>(InteractableSelectionOutlineShaderPath)
            ?? Shader.Find("TheCircussyOne/Interactable Selection Outline");
        if (maskShader == null || outlineShader == null)
        {
            return;
        }

        InteractableSelectionOutlineRendererFeature feature = FindInteractableSelectionOutlineFeature(rendererData);
        if (feature == null)
        {
            feature = ScriptableObject.CreateInstance<InteractableSelectionOutlineRendererFeature>();
            feature.name = InteractableSelectionOutlineFeatureName;
            AssetDatabase.AddObjectToAsset(feature, rendererData);
            AppendRendererFeature(rendererData, feature);
        }

        ConfigureInteractableSelectionOutlineFeature(feature, maskShader, outlineShader);
        feature.SetActive(true);
        EditorUtility.SetDirty(feature);
        EditorUtility.SetDirty(rendererData);
        AssetDatabase.SaveAssets();
    }

    private static InteractableSelectionOutlineRendererFeature FindInteractableSelectionOutlineFeature(UniversalRendererData rendererData)
    {
        for (int i = 0; i < rendererData.rendererFeatures.Count; i++)
        {
            if (rendererData.rendererFeatures[i] is InteractableSelectionOutlineRendererFeature feature)
            {
                return feature;
            }
        }

        return null;
    }

    private static void AppendRendererFeature(UniversalRendererData rendererData, InteractableSelectionOutlineRendererFeature feature)
    {
        using var serialized = new SerializedObject(rendererData);
        SerializedProperty features = serialized.FindProperty("m_RendererFeatures");
        if (features != null)
        {
            features.arraySize++;
            features.GetArrayElementAtIndex(features.arraySize - 1).objectReferenceValue = feature;
        }

        SerializedProperty featureMap = serialized.FindProperty("m_RendererFeatureMap");
        if (featureMap != null)
        {
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(feature, out _, out long localId);
            featureMap.arraySize++;
            featureMap.GetArrayElementAtIndex(featureMap.arraySize - 1).longValue = localId;
        }

        serialized.ApplyModifiedProperties();
    }

    private static void ConfigureInteractableSelectionOutlineFeature(
        InteractableSelectionOutlineRendererFeature feature,
        Shader maskShader,
        Shader outlineShader)
    {
        using var serialized = new SerializedObject(feature);
        serialized.FindProperty("maskShader").objectReferenceValue = maskShader;
        serialized.FindProperty("compositeShader").objectReferenceValue = outlineShader;
        serialized.FindProperty("fallbackColor").colorValue = new Color(1f, 0.98f, 0.72f, 1f);
        serialized.FindProperty("fallbackWidthPixels").floatValue = 3f;
        serialized.FindProperty("occludeHiddenPixels").boolValue = true;
        serialized.FindProperty("passEvent").intValue = (int)RenderPassEvent.AfterRenderingPostProcessing;
        serialized.ApplyModifiedProperties();
    }
}
