using UnityEngine;

namespace TheCircussyOne.Visuals
{
    internal static class HudColorExtensions
    {
        public static Color WithAlpha(this Color color, float alpha)
        {
            color.a *= Mathf.Clamp01(alpha);
            return color;
        }
    }
}
