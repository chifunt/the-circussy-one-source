using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using TheCircussyOne.Config;

public sealed class NumericSliderAttributeFloatDrawer : OdinAttributeDrawer<NumericSliderAttribute, float>
{
    protected override void DrawPropertyLayout(GUIContent label)
    {
        float min = Mathf.Min(Attribute.Min, Attribute.Max);
        float max = Mathf.Max(Attribute.Min, Attribute.Max);
        ValueEntry.SmartValue = EditorGUILayout.Slider(label, ValueEntry.SmartValue, min, max);
    }
}

public sealed class NumericSliderAttributeIntDrawer : OdinAttributeDrawer<NumericSliderAttribute, int>
{
    protected override void DrawPropertyLayout(GUIContent label)
    {
        int min = Mathf.RoundToInt(Mathf.Min(Attribute.Min, Attribute.Max));
        int max = Mathf.RoundToInt(Mathf.Max(Attribute.Min, Attribute.Max));
        ValueEntry.SmartValue = EditorGUILayout.IntSlider(label, ValueEntry.SmartValue, min, max);
    }
}
