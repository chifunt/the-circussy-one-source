using System;
using UnityEngine;
using VHierarchy;

namespace TheCircussyOne.Editor
{
    internal static class TheCircussyOneHierarchyStyleRules
    {
        private const string PrototypeScenePath = "Assets/Game/Scenes/TheCircussyOne.unity";
        private const string VisualTestSceneFolder = "Assets/Game/VisualTests/Scenes/";

        [Rule]
        private static void ApplyTheCircussyOneStyle(ObjectInfo info)
        {
            GameObject gameObject = info.gameObject;
            if (gameObject == null)
            {
                return;
            }

            string scenePath = gameObject.scene.path;
            if (!IsTheCircussyOneScene(scenePath))
            {
                return;
            }

            string path = GetHierarchyPath(gameObject.transform);
            ApplyPrototypeStyle(info, path);
            ApplyVisualTestStyle(info, path);
        }

        private static bool IsTheCircussyOneScene(string scenePath)
        {
            return string.Equals(scenePath, PrototypeScenePath, StringComparison.Ordinal)
                || scenePath.StartsWith(VisualTestSceneFolder, StringComparison.Ordinal);
        }

        private static void ApplyPrototypeStyle(ObjectInfo info, string path)
        {
            if (path == "Game Lifetime Scope")
            {
                Set(info, 7, "AvatarMask On Icon");
                return;
            }

            if (path == "Player" || path.StartsWith("Player/Body Scale Root", StringComparison.Ordinal))
            {
                Set(info, 8, "AvatarMask On Icon");
                return;
            }

            if (path.StartsWith("Player/Health Bar", StringComparison.Ordinal))
            {
                Set(info, 8, "Canvas Icon");
                return;
            }

            if (path.StartsWith("Player/Player Move Dust", StringComparison.Ordinal))
            {
                Set(info, 8, "ParticleSystem Icon");
                return;
            }

            if (path.StartsWith("Player/Player Jump Trail", StringComparison.Ordinal))
            {
                Set(info, 8, "ParticleSystem Icon");
                return;
            }

            if (path == "Arena Floor")
            {
                Set(info, 4, "StandaloneInputModule Icon");
                return;
            }

            if (path == "Third Person Camera")
            {
                Set(info, 6, "Camera Icon");
                return;
            }

            if (path == "HUD")
            {
                Set(info, 9, "Canvas Icon");
                return;
            }

            if (path == "Directional Light")
            {
                Set(info, 3, "DirectionalLight Icon");
                return;
            }

            if (path == "Global Lighting Volume")
            {
                Set(info, 3, "LightProbes Icon");
                return;
            }

            if (path == "Feel Feedbacks" || path.StartsWith("Feel Feedbacks/", StringComparison.Ordinal))
            {
                Set(info, 7, "Folder Icon");
                return;
            }

            if (path == "Runtime Roots")
            {
                Set(info, 9, "Folder Icon");
                return;
            }

            if (path.StartsWith("Runtime Roots/Enemies", StringComparison.Ordinal))
            {
                Set(info, 8, "AvatarMask On Icon");
                return;
            }

            if (path.StartsWith("Runtime Roots/Projectiles", StringComparison.Ordinal))
            {
                Set(info, 7, "PreMatSphere");
                return;
            }

            if (path.StartsWith("Runtime Roots/Pickups", StringComparison.Ordinal))
            {
                Set(info, 6, "ScriptableObject Icon");
                return;
            }

            if (path.StartsWith("Runtime Roots/Damage Numbers", StringComparison.Ordinal))
            {
                Set(info, 9, "Font Icon");
                return;
            }

            if (path.StartsWith("Runtime Roots/XP Gain Counter", StringComparison.Ordinal))
            {
                Set(info, 6, "Font Icon");
                return;
            }

            if (path.StartsWith("Runtime Roots/Enemy Spawn Indicators", StringComparison.Ordinal))
            {
                Set(info, 7, "PreMatCylinder");
                return;
            }

            if (path.StartsWith("Runtime Roots/VFX", StringComparison.Ordinal))
            {
                Set(info, 7, "ParticleSystem Icon");
            }
        }

        private static void ApplyVisualTestStyle(ObjectInfo info, string path)
        {
            if (path == "[Generated]" || path.StartsWith("[Generated]/Fixtures", StringComparison.Ordinal))
            {
                Set(info, 7, "Folder Icon");
                return;
            }

            if (path == "Manual Overrides")
            {
                Set(info, 1, "Favorite");
                return;
            }

            if (path.StartsWith("[Generated]/Actors", StringComparison.Ordinal))
            {
                Set(info, 8, "AvatarMask On Icon");
                return;
            }

            if (path.StartsWith("[Generated]/Damage Numbers", StringComparison.Ordinal))
            {
                Set(info, 9, "Font Icon");
                return;
            }

            if (path.StartsWith("[Generated]/XP Gain Counter", StringComparison.Ordinal))
            {
                Set(info, 6, "Font Icon");
                return;
            }

            if (path.StartsWith("[Generated]/Enemy Spawn Indicators", StringComparison.Ordinal))
            {
                Set(info, 7, "PreMatCylinder");
                return;
            }

            if (path.StartsWith("[Generated]/VFX", StringComparison.Ordinal))
            {
                Set(info, 7, "ParticleSystem Icon");
                return;
            }

            if (path.StartsWith("[Generated]/Visual Test Camera", StringComparison.Ordinal))
            {
                Set(info, 6, "Camera Icon");
                return;
            }

            if (path.StartsWith("[Generated]/Visual Test HUD", StringComparison.Ordinal))
            {
                Set(info, 9, "Canvas Icon");
                return;
            }

            if (path.StartsWith("[Generated]/Visual Test Directional Light", StringComparison.Ordinal))
            {
                Set(info, 3, "DirectionalLight Icon");
                return;
            }

            if (path.StartsWith("[Generated]/Visual Test Director", StringComparison.Ordinal))
            {
                Set(info, 7, "Settings Icon");
            }
        }

        private static string GetHierarchyPath(Transform transform)
        {
            string path = transform.name;
            while (transform.parent != null)
            {
                transform = transform.parent;
                path = transform.name + "/" + path;
            }

            return path;
        }

        private static void Set(ObjectInfo info, int colorIndex, string iconName)
        {
            info.color = colorIndex;
            info.icon = iconName;
        }
    }
}
