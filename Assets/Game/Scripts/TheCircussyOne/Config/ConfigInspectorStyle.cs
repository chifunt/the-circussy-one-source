using UnityEngine;

namespace TheCircussyOne.Config
{
    public static class ConfigInspectorStyle
    {
        public const string BalanceHex = "FF8A3D";
        public const string CombatHex = "FF5A5F";
        public const string PlayerHex = "8E7CFF";
        public const string CameraHex = "64B5FF";
        public const string VisualHex = "41D6C3";
        public const string LightingHex = "9AE66E";
        public const string CurrencyHex = "FFD75A";
        public const string WarningHex = "FFB84D";

        public static Color BalanceColor => Hex(BalanceHex);
        public static Color CombatColor => Hex(CombatHex);
        public static Color PlayerColor => Hex(PlayerHex);
        public static Color CameraColor => Hex(CameraHex);
        public static Color VisualColor => Hex(VisualHex);
        public static Color LightingColor => Hex(LightingHex);
        public static Color CurrencyColor => Hex(CurrencyHex);
        public static Color WarningColor => Hex(WarningHex);

        private static Color Hex(string hex)
        {
            return ColorUtility.TryParseHtmlString("#" + hex, out Color color) ? color : Color.white;
        }
    }
}
