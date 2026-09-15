using Sirenix.OdinInspector;
using UnityEditor;

public sealed class ConfigHubContentWorkbenchLauncher
{
    [TitleGroup("Designer Content")]
    [ShowInInspector, ReadOnly, MultiLineProperty(3), HideLabel]
    public string Guide =>
        "Use Content Workbench for designer-owned gameplay content: weapons, performers, enemies, upgrades, and talents. Config Hub remains the developer workflow surface.";

    [TitleGroup("Designer Content")]
    [Button(ButtonSizes.Large), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.BalanceColor")]
    public void OpenContentWorkbench()
    {
        EditorApplication.ExecuteMenuItem("Tools/The Circussy One/Content Workbench");
    }
}
