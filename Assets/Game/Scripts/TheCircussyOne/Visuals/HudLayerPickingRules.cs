using System.Collections.Generic;
using UnityEngine.UIElements;

namespace TheCircussyOne.Visuals
{
    internal static class HudLayerPickingRules
    {
        private const string LayerSlotClass = "hud-layer-slot";
        private const string AppliedClass = "hud-layer-picking-configured";

        public static void Apply(VisualElement root)
        {
            if (root == null)
            {
                return;
            }

            if (root.ClassListContains(AppliedClass))
            {
                return;
            }

            List<VisualElement> slots = root.Query<VisualElement>(className: LayerSlotClass).ToList();
            for (int i = 0; i < slots.Count; i++)
            {
                slots[i].pickingMode = PickingMode.Ignore;
            }

            root.AddToClassList(AppliedClass);
        }
    }
}
