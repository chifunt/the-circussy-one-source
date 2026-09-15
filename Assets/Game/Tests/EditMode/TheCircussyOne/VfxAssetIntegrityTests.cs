using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class VfxAssetIntegrityTests
{
    private const string FirstPartyAssetRoot = "Assets/Game";

    [Test]
    public void FirstPartyParticlePrefabsUseResolvableRendererMaterials()
    {
        var failures = new List<string>();
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { FirstPartyAssetRoot });

        foreach (string prefabGuid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(prefabGuid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                failures.Add($"{path}: prefab could not be loaded.");
                continue;
            }

            ParticleSystemRenderer[] renderers = prefab.GetComponentsInChildren<ParticleSystemRenderer>(includeInactive: true);
            foreach (ParticleSystemRenderer renderer in renderers)
            {
                ValidateMaterial(path, renderer.name, "particle", renderer.sharedMaterial, failures);

                ParticleSystem.TrailModule trails = renderer.GetComponent<ParticleSystem>().trails;
                if (trails.enabled)
                {
                    ValidateMaterial(path, renderer.name, "trail", renderer.trailMaterial, failures);
                }
            }
        }

        Assert.That(failures, Is.Empty, string.Join("\n", failures));
    }

    private static void ValidateMaterial(
        string prefabPath,
        string rendererName,
        string role,
        Material material,
        ICollection<string> failures)
    {
        string context = $"{prefabPath} [{rendererName}] {role} material";
        if (material == null)
        {
            failures.Add($"{context}: missing material.");
            return;
        }

        Shader shader = material.shader;
        if (shader == null)
        {
            failures.Add($"{context}: {material.name} has no shader.");
            return;
        }

        if (shader.name == "Hidden/InternalErrorShader")
        {
            failures.Add($"{context}: {material.name} resolves to Unity's error shader.");
            return;
        }

        if (!shader.isSupported)
        {
            failures.Add($"{context}: shader {shader.name} is not supported by the active graphics API.");
        }
    }
}
