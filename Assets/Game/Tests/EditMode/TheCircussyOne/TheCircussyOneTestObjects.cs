using NUnit.Framework;
using UnityEngine;

internal static partial class TheCircussyOneTestObjects
{
    private const string TestPrefix = "TCO Test ";

    public static int RequireLayer(string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        Assert.That(layer, Is.GreaterThanOrEqualTo(0), $"Missing Unity layer '{layerName}'. Rebuild the The Circussy One prototype scene before running these tests.");
        return layer;
    }

    public static void DestroyAll()
    {
        foreach (GameObject gameObject in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (gameObject == null)
            {
                continue;
            }

            if (gameObject.name.StartsWith(TestPrefix))
            {
                Object.DestroyImmediate(gameObject);
            }
        }
    }

    public static void Destroy(Object target)
    {
        if (target != null)
        {
            Object.DestroyImmediate(target);
        }
    }
}
