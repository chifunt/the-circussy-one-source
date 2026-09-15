using System.IO;
using MoreMountains.Feedbacks;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.DI;
using TheCircussyOne.Runtime;
using TheCircussyOne.Visuals;

public static partial class TheCircussyOneSceneBuilder
{
    internal static void EnsureGeneratedFolders()
    {
        EnsureFolder(PrefabFolder);
        EnsureFolder(VfxPrefabFolder);
        EnsureFolder(MaterialFolder);
        EnsureFolder(BalanceFolder);
        EnsureFolder(WeaponBalanceFolder);
        EnsureFolder(ItemBalanceFolder);
        EnsureFolder(PickupBalanceFolder);
        EnsureFolder(HealingPropBalanceFolder);
        EnsureFolder(UpgradeBalanceFolder);
        EnsureFolder(TalentBalanceFolder);
        EnsureFolder(PerformerBalanceFolder);
        EnsureFolder(EnemyBalanceFolder);
        EnsureFolder(CameraSettingsFolder);
        EnsureFolder(VisualSettingsFolder);
        EnsureFolder(VisualAssetFolder);
        EnsureFolder(ShaderFolder);
        EnsureFolder(GridTextureFolder);
        EnsureFolder(LightingVisualFolder);
        EnsureFolder(VfxVisualFolder);
        EnsureFolder(TheCircussyOneAssetPaths.VfxTextureFolder);
        EnsureFolder(UiFolder);
    }

    private static void EnsureHudAssets()
    {
        WriteTextIfDifferent(HudUxmlPath, HudUxml);
        WriteTextIfDifferent(HudUssPath, HudUss);
        AssetDatabase.ImportAsset(HudUxmlPath);
        AssetDatabase.ImportAsset(HudUssPath);

        PanelSettings panelSettings = GetOrCreateHudPanelSettings();
        EditorUtility.SetDirty(panelSettings);
    }

    public static PanelSettings GetOrCreateHudPanelSettings()
    {
        var panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>(HudPanelSettingsPath);
        if (panelSettings == null)
        {
            panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
            AssetDatabase.CreateAsset(panelSettings, HudPanelSettingsPath);
        }

        return panelSettings;
    }

    private static void WriteTextIfDifferent(string path, string contents)
    {
        if (File.Exists(path) && File.ReadAllText(path) == contents)
        {
            return;
        }

        File.WriteAllText(path, contents);
    }

    private static void EnsureFolder(string path)
    {
        if (string.IsNullOrEmpty(path) || AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        string parent = System.IO.Path.GetDirectoryName(path)?.Replace('\\', '/');
        string folderName = System.IO.Path.GetFileName(path);
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, folderName);
    }

private const string HudUxml =
@"<ui:UXML xmlns:ui=""UnityEngine.UIElements"">
    <ui:VisualElement name=""hud-root"" class=""hud-root"">
        <ui:VisualElement name=""damage-vignette"" class=""damage-vignette"" picking-mode=""Ignore"" />

        <ui:VisualElement class=""xp-strip"">
            <ui:VisualElement class=""xp-track"">
                <ui:VisualElement name=""xp-fill"" class=""xp-fill"">
                    <ui:VisualElement name=""xp-pulse"" class=""xp-pulse"" picking-mode=""Ignore"" />
                </ui:VisualElement>
                <ui:VisualElement name=""xp-segments"" class=""xp-segments"" picking-mode=""Ignore"" />
                <ui:Label name=""xp-value"" text="""" class=""xp-value-hidden"" />
            </ui:VisualElement>
        </ui:VisualElement>

        <ui:VisualElement class=""hud-under-row"">
            <ui:Label name=""level-value"" text=""1"" class=""level-badge"" />
            <ui:VisualElement class=""health-cluster"">
                <ui:VisualElement class=""health-track"">
                    <ui:VisualElement name=""health-fill"" class=""health-fill"" />
                    <ui:VisualElement name=""health-segments"" class=""health-segments"" picking-mode=""Ignore"" />
                    <ui:Label name=""health-value"" text=""100 / 100"" class=""health-value"" />
                </ui:VisualElement>
            </ui:VisualElement>

            <ui:VisualElement class=""tickets-cluster"">
                <ui:VisualElement name=""tickets-icon"" class=""hud-counter-icon tickets-icon"" picking-mode=""Ignore"" />
                <ui:Label name=""tickets-value"" text=""0"" class=""tickets-value"" />
            </ui:VisualElement>

            <ui:VisualElement name=""timer-cluster"" class=""timer-cluster"">
                <ui:VisualElement name=""timer-icon"" class=""hud-counter-icon timer-icon"" picking-mode=""Ignore"" />
                <ui:Label name=""timer-value"" text=""00:00"" class=""timer-value"" />
            </ui:VisualElement>

            <ui:VisualElement class=""kills-cluster"">
                <ui:VisualElement name=""kills-icon"" class=""hud-counter-icon kills-icon"" picking-mode=""Ignore"" />
                <ui:Label name=""kills-value"" text=""0"" class=""kills-value"" />
            </ui:VisualElement>
        </ui:VisualElement>

        <ui:VisualElement name=""intermission-overlay"" class=""intermission-overlay"" picking-mode=""Ignore"">
            <ui:Label name=""intermission-title"" text=""INTERMISSION"" class=""intermission-title"" />
            <ui:Label name=""intermission-subtitle"" text=""Preparing ACT II..."" class=""intermission-subtitle"" />
        </ui:VisualElement>

        <ui:VisualElement name=""interaction-prompt"" class=""interaction-prompt"">
            <ui:Label name=""interaction-button-glyph"" text=""E"" class=""interaction-button-glyph interaction-button-glyph--keyboard"" />
            <ui:Label name=""interaction-prompt-label"" text=""Hold to Collect Tickets"" class=""interaction-prompt-label"" />
            <ui:VisualElement class=""interaction-progress-track"">
                <ui:VisualElement name=""interaction-progress-fill"" class=""interaction-progress-fill"" />
            </ui:VisualElement>
        </ui:VisualElement>

        <ui:VisualElement name=""game-over"" class=""game-over"">
            <ui:Label text=""GAME OVER"" class=""game-over-title"" />
            <ui:Label text=""The run is over"" class=""game-over-subtitle"" />
            <ui:Button name=""restart-button"" text=""RESTART"" class=""restart-button"" />
        </ui:VisualElement>

        <ui:VisualElement name=""performer-selection"" class=""performer-selection"">
            <ui:VisualElement class=""performer-panel"">
                <ui:Label text=""CHOOSE PERFORMER"" class=""performer-heading"" />
                <ui:VisualElement class=""performer-card-row"">
                    <ui:VisualElement name=""performer-choice-0"" class=""performer-card"">
                        <ui:VisualElement name=""performer-portrait-0"" class=""performer-portrait"" />
                        <ui:Label name=""performer-name-0"" text=""Devsample Sam"" class=""performer-name"" />
                        <ui:Label name=""performer-title-0"" text=""The Dev Sample"" class=""performer-title"" />
                        <ui:Label name=""performer-description-0"" text=""Baseline dev sample."" class=""performer-description"" />
                        <ui:Label name=""performer-weapon-0"" text=""Weapon: Juggling Ball"" class=""performer-detail"" />
                        <ui:Label name=""performer-passive-0"" text=""Passive: None"" class=""performer-detail"" />
                        <ui:Label name=""performer-stats-0"" text=""Baseline stats"" class=""performer-stats"" />
                        <ui:Label name=""performer-talents-0"" text=""Default projectile rhythm."" class=""performer-talents"" />
                    </ui:VisualElement>
                    <ui:VisualElement name=""performer-choice-1"" class=""performer-card"">
                        <ui:VisualElement name=""performer-portrait-1"" class=""performer-portrait"" />
                        <ui:Label name=""performer-name-1"" text=""Bruno"" class=""performer-name"" />
                        <ui:Label name=""performer-title-1"" text=""The Cannon Strongman"" class=""performer-title"" />
                        <ui:Label name=""performer-description-1"" text=""Durable performer."" class=""performer-description"" />
                        <ui:Label name=""performer-weapon-1"" text=""Weapon: Cannon"" class=""performer-detail"" />
                        <ui:Label name=""performer-passive-1"" text=""Passive: Armor"" class=""performer-detail"" />
                        <ui:Label name=""performer-stats-1"" text=""+Armor, -Move Speed"" class=""performer-stats"" />
                        <ui:Label name=""performer-talents-1"" text=""Heavy attacks."" class=""performer-talents"" />
                    </ui:VisualElement>
                    <ui:VisualElement name=""performer-choice-2"" class=""performer-card"">
                        <ui:VisualElement name=""performer-portrait-2"" class=""performer-portrait"" />
                        <ui:Label name=""performer-name-2"" text=""Mira"" class=""performer-name"" />
                        <ui:Label name=""performer-title-2"" text=""The Knife Acrobat"" class=""performer-title"" />
                        <ui:Label name=""performer-description-2"" text=""Fast risky performer."" class=""performer-description"" />
                        <ui:Label name=""performer-weapon-2"" text=""Weapon: Knife Fan"" class=""performer-detail"" />
                        <ui:Label name=""performer-passive-2"" text=""Passive: Speed"" class=""performer-detail"" />
                        <ui:Label name=""performer-stats-2"" text=""+Move Speed, -Health"" class=""performer-stats"" />
                        <ui:Label name=""performer-talents-2"" text=""Movement tempo."" class=""performer-talents"" />
                    </ui:VisualElement>
                    <ui:VisualElement name=""performer-choice-3"" class=""performer-card"">
                        <ui:VisualElement name=""performer-portrait-3"" class=""performer-portrait"" />
                        <ui:Label name=""performer-name-3"" text=""Vox"" class=""performer-name"" />
                        <ui:Label name=""performer-title-3"" text=""The Spotlight Ringmaster"" class=""performer-title"" />
                        <ui:Label name=""performer-description-3"" text=""Reward-focused performer."" class=""performer-description"" />
                        <ui:Label name=""performer-weapon-3"" text=""Weapon: Spotlight Bolt"" class=""performer-detail"" />
                        <ui:Label name=""performer-passive-3"" text=""Passive: Luck"" class=""performer-detail"" />
                        <ui:Label name=""performer-stats-3"" text=""+Luck, +XP Gain"" class=""performer-stats"" />
                        <ui:Label name=""performer-talents-3"" text=""Reward control."" class=""performer-talents"" />
                    </ui:VisualElement>
                    <ui:VisualElement name=""performer-choice-4"" class=""performer-card"">
                        <ui:VisualElement name=""performer-portrait-4"" class=""performer-portrait"" />
                        <ui:Label name=""performer-name-4"" text=""Devsample Cappa"" class=""performer-name"" />
                        <ui:Label name=""performer-title-4"" text=""The Capsule Sample"" class=""performer-title"" />
                        <ui:Label name=""performer-description-4"" text=""Capsule-face sample."" class=""performer-description"" />
                        <ui:Label name=""performer-weapon-4"" text=""Weapon: Juggling Ball"" class=""performer-detail"" />
                        <ui:Label name=""performer-passive-4"" text=""Passive: None"" class=""performer-detail"" />
                        <ui:Label name=""performer-stats-4"" text=""Baseline stats"" class=""performer-stats"" />
                        <ui:Label name=""performer-talents-4"" text=""Capsule projectile rhythm."" class=""performer-talents"" />
                    </ui:VisualElement>
                </ui:VisualElement>
                <ui:Label text=""Stick / D-Pad: choose  •  A / Enter: start  •  1-7: quick choose"" class=""performer-hint"" />
            </ui:VisualElement>
        </ui:VisualElement>

        <ui:VisualElement name=""upgrade-selection"" class=""upgrade-selection"">
            <ui:VisualElement class=""upgrade-panel"">
                <ui:Label text=""CHOOSE UPGRADE"" class=""upgrade-heading"" />
                <ui:VisualElement class=""upgrade-card-row"">
                    <ui:VisualElement name=""upgrade-choice-0"" class=""upgrade-card"">
                        <ui:VisualElement name=""upgrade-rarity-strip-0"" class=""upgrade-rarity-strip"" />
                        <ui:Label name=""upgrade-rarity-0"" text=""COMMON"" class=""upgrade-rarity"" />
                        <ui:Label name=""upgrade-title-0"" text=""Upgrade"" class=""upgrade-title"" />
                        <ui:Label name=""upgrade-description-0"" text=""Description"" class=""upgrade-description"" />
                        <ui:Label name=""upgrade-preview-0"" text=""Stat 100% -&gt; 110%"" class=""upgrade-preview"" />
                        <ui:Label name=""upgrade-level-0"" text=""LV 0 -&gt; 1"" class=""upgrade-level"" />
                    </ui:VisualElement>
                    <ui:VisualElement name=""upgrade-choice-1"" class=""upgrade-card"">
                        <ui:VisualElement name=""upgrade-rarity-strip-1"" class=""upgrade-rarity-strip"" />
                        <ui:Label name=""upgrade-rarity-1"" text=""COMMON"" class=""upgrade-rarity"" />
                        <ui:Label name=""upgrade-title-1"" text=""Upgrade"" class=""upgrade-title"" />
                        <ui:Label name=""upgrade-description-1"" text=""Description"" class=""upgrade-description"" />
                        <ui:Label name=""upgrade-preview-1"" text=""Stat 100% -&gt; 110%"" class=""upgrade-preview"" />
                        <ui:Label name=""upgrade-level-1"" text=""LV 0 -&gt; 1"" class=""upgrade-level"" />
                    </ui:VisualElement>
                    <ui:VisualElement name=""upgrade-choice-2"" class=""upgrade-card"">
                        <ui:VisualElement name=""upgrade-rarity-strip-2"" class=""upgrade-rarity-strip"" />
                        <ui:Label name=""upgrade-rarity-2"" text=""COMMON"" class=""upgrade-rarity"" />
                        <ui:Label name=""upgrade-title-2"" text=""Upgrade"" class=""upgrade-title"" />
                        <ui:Label name=""upgrade-description-2"" text=""Description"" class=""upgrade-description"" />
                        <ui:Label name=""upgrade-preview-2"" text=""Stat 100% -&gt; 110%"" class=""upgrade-preview"" />
                        <ui:Label name=""upgrade-level-2"" text=""LV 0 -&gt; 1"" class=""upgrade-level"" />
                    </ui:VisualElement>
                </ui:VisualElement>
                <ui:Label text=""Stick / D-Pad: choose  •  A / Enter: confirm  •  1 / 2 / 3: quick choose"" class=""upgrade-hint"" />
            </ui:VisualElement>
        </ui:VisualElement>
    </ui:VisualElement>
</ui:UXML>
";

    private const string HudUss =
@".hud-root {
    position: absolute;
    left: 0;
    right: 0;
    top: 0;
    bottom: 0;
    color: rgb(245, 250, 255);
    -unity-font-style: bold;
}

.damage-vignette {
    position: absolute;
    left: 0;
    right: 0;
    top: 0;
    bottom: 0;
    opacity: 0;
}

.xp-strip {
    position: absolute;
    left: 0;
    right: 0;
    top: 0;
    height: 22px;
    padding-left: 8px;
    padding-right: 8px;
    padding-top: 4px;
}

.xp-track {
    position: absolute;
    left: 8px;
    right: 8px;
    top: 4px;
    height: 16px;
    background-color: rgba(4, 16, 24, 0.86);
    border-top-left-radius: 4px;
    border-top-right-radius: 4px;
    border-bottom-left-radius: 4px;
    border-bottom-right-radius: 4px;
    border-left-width: 1px;
    border-right-width: 1px;
    border-top-width: 1px;
    border-bottom-width: 1px;
    border-left-color: rgba(90, 230, 255, 0.42);
    border-right-color: rgba(90, 230, 255, 0.42);
    border-top-color: rgba(160, 246, 255, 0.58);
    border-bottom-color: rgba(28, 105, 130, 0.58);
    overflow: hidden;
}

.xp-fill {
    position: absolute;
    left: 0;
    top: 0;
    bottom: 0;
    width: 0%;
    background-color: rgb(49, 215, 255);
    border-top-left-radius: 3px;
    border-top-right-radius: 3px;
    border-bottom-left-radius: 3px;
    border-bottom-right-radius: 3px;
}

.xp-pulse {
    position: absolute;
    left: 0;
    right: 0;
    top: 0;
    bottom: 0;
    opacity: 0;
    background-color: rgb(255, 255, 255);
    border-top-left-radius: 3px;
    border-top-right-radius: 3px;
    border-bottom-left-radius: 3px;
    border-bottom-right-radius: 3px;
}

.xp-segments {
    position: absolute;
    left: 0;
    right: 0;
    top: 0;
    bottom: 0;
}

.xp-segment-divider {
    position: absolute;
    top: 0;
    bottom: 0;
    width: 1px;
    background-color: rgba(210, 255, 255, 0.34);
}

.level-badge {
    position: absolute;
    left: 0;
    top: 0;
    width: 24px;
    min-width: 24px;
    height: 24px;
    padding-left: 0;
    padding-right: 0;
    color: rgb(49, 215, 255);
    background-color: rgba(2, 3, 5, 0.94);
    border-left-width: 1px;
    border-right-width: 1px;
    border-top-width: 1px;
    border-bottom-width: 1px;
    border-left-color: rgb(49, 215, 255);
    border-right-color: rgb(49, 215, 255);
    border-top-color: rgb(49, 215, 255);
    border-bottom-color: rgb(49, 215, 255);
    border-top-left-radius: 4px;
    border-top-right-radius: 4px;
    border-bottom-left-radius: 4px;
    border-bottom-right-radius: 4px;
    -unity-text-align: middle-center;
    font-size: 13px;
}

.xp-value-hidden {
    display: none;
}

.hud-under-row {
    position: absolute;
    left: 16px;
    right: 16px;
    top: 28px;
    height: 28px;
}

.health-cluster {
    position: absolute;
    left: 30px;
    top: 1px;
    width: 210px;
    height: 22px;
}

.health-track {
    position: absolute;
    left: 0;
    right: 0;
    top: 0;
    bottom: 0;
    background-color: rgba(35, 7, 10, 0.82);
    border-top-left-radius: 5px;
    border-top-right-radius: 5px;
    border-bottom-left-radius: 5px;
    border-bottom-right-radius: 5px;
    border-left-width: 1px;
    border-right-width: 1px;
    border-top-width: 1px;
    border-bottom-width: 1px;
    border-left-color: rgba(255, 100, 116, 0.45);
    border-right-color: rgba(255, 100, 116, 0.45);
    border-top-color: rgba(255, 156, 166, 0.62);
    border-bottom-color: rgba(111, 22, 30, 0.65);
    overflow: hidden;
}

.health-fill {
    position: absolute;
    left: 0;
    top: 0;
    bottom: 0;
    width: 100%;
    background-color: rgb(232, 41, 62);
    border-top-left-radius: 4px;
    border-top-right-radius: 4px;
    border-bottom-left-radius: 4px;
    border-bottom-right-radius: 4px;
}

.health-segments {
    position: absolute;
    left: 0;
    right: 0;
    top: 0;
    bottom: 0;
}

.health-segment-divider {
    position: absolute;
    top: 0;
    bottom: 0;
    width: 1px;
    background-color: rgba(255, 194, 207, 0.38);
}

.health-value {
    position: absolute;
    left: 0;
    right: 0;
    top: 0;
    bottom: 0;
    color: rgb(255, 236, 239);
    -unity-text-align: middle-center;
    font-size: 12px;
    -unity-font-style: bold;
}

.timer-cluster {
    position: absolute;
    left: 0;
    right: 0;
    top: 0;
    height: 24px;
    flex-direction: row;
    align-items: center;
    justify-content: center;
}

.hud-counter-icon {
    position: relative;
    top: 0;
    width: 18px;
    height: 18px;
    min-width: 18px;
    min-height: 18px;
    -unity-background-scale-mode: scale-to-fit;
}

.timer-icon {
    margin-right: 6px;
    background-image: url(""../Art/UI/Icons/HUD/HudTimerIcon.png"");
}

.timer-value {
    height: 24px;
    color: rgb(225, 242, 246);
    -unity-text-align: middle-center;
    font-size: 14px;
    -unity-font-style: bold;
}

.intermission-overlay {
    display: none;
    position: absolute;
    left: 0;
    right: 0;
    top: 0;
    bottom: 0;
    align-items: center;
    justify-content: center;
    background-color: rgba(4, 7, 12, 0.9);
}

.intermission-title {
    color: rgb(120, 237, 255);
    font-size: 38px;
    -unity-font-style: bold;
    -unity-text-align: middle-center;
    -unity-text-outline-color: rgba(0, 0, 0, 0.95);
    -unity-text-outline-width: 3px;
}

.intermission-subtitle {
    margin-top: 12px;
    color: rgb(255, 242, 184);
    font-size: 18px;
    -unity-font-style: bold;
    -unity-text-align: middle-center;
    -unity-text-outline-color: rgba(0, 0, 0, 0.95);
    -unity-text-outline-width: 2px;
}

.kills-cluster {
    position: absolute;
    right: 0;
    top: 0;
    height: 24px;
    flex-direction: row;
    align-items: center;
    justify-content: flex-end;
}

.kills-icon {
    margin-right: 7px;
    background-image: url(""../Art/UI/Icons/HUD/HudKillsIcon.png"");
}

.kills-value {
    min-width: 32px;
    color: rgb(255, 246, 214);
    font-size: 18px;
    -unity-font-style: bold;
    -unity-text-align: middle-left;
}

.tickets-cluster {
    position: absolute;
    left: 224px;
    top: 0;
    height: 24px;
    flex-direction: row;
    align-items: center;
    justify-content: flex-start;
}

.tickets-icon {
    width: 30px;
    min-width: 30px;
    height: 18px;
    min-height: 18px;
    margin-right: 7px;
    background-image: url(""../Art/UI/Icons/HUD/HudTicketsIcon.png"");
}

.tickets-value {
    min-width: 42px;
    color: rgb(255, 245, 186);
    font-size: 18px;
    -unity-font-style: bold;
    -unity-text-align: middle-left;
}

.interaction-prompt {
    position: absolute;
    left: 50%;
    bottom: 78px;
    width: 280px;
    height: 42px;
    margin-left: -140px;
    display: none;
    flex-direction: row;
    align-items: center;
    justify-content: center;
    background-color: rgba(8, 12, 14, 0.76);
    border-top-left-radius: 6px;
    border-top-right-radius: 6px;
    border-bottom-left-radius: 6px;
    border-bottom-right-radius: 6px;
    border-left-width: 1px;
    border-right-width: 1px;
    border-top-width: 1px;
    border-bottom-width: 1px;
    border-left-color: rgba(255, 222, 91, 0.55);
    border-right-color: rgba(255, 222, 91, 0.55);
    border-top-color: rgba(255, 240, 152, 0.68);
    border-bottom-color: rgba(123, 95, 20, 0.68);
}

.interaction-button-glyph {
    width: 20px;
    height: 20px;
    margin-right: 7px;
    border-top-left-radius: 10px;
    border-top-right-radius: 10px;
    border-bottom-left-radius: 10px;
    border-bottom-right-radius: 10px;
    border-left-width: 1px;
    border-right-width: 1px;
    border-top-width: 1px;
    border-bottom-width: 1px;
    font-size: 12px;
    -unity-font-style: bold;
    -unity-text-align: middle-center;
}

.interaction-button-glyph--keyboard {
    color: rgb(255, 246, 205);
    background-color: rgba(40, 36, 24, 0.94);
    border-left-color: rgba(255, 246, 205, 0.55);
    border-right-color: rgba(255, 246, 205, 0.55);
    border-top-color: rgba(255, 246, 205, 0.72);
    border-bottom-color: rgba(255, 246, 205, 0.38);
}

.interaction-button-glyph--gamepad {
    color: white;
    background-color: rgb(38, 106, 235);
    border-left-color: rgb(131, 181, 255);
    border-right-color: rgb(131, 181, 255);
    border-top-color: rgb(190, 218, 255);
    border-bottom-color: rgb(13, 54, 156);
}

.interaction-prompt-label {
    height: 22px;
    color: rgb(255, 246, 205);
    font-size: 13px;
    -unity-font-style: bold;
    -unity-text-align: middle-center;
}

.interaction-progress-track {
    position: absolute;
    left: 12px;
    right: 12px;
    bottom: 7px;
    height: 5px;
    background-color: rgba(45, 32, 10, 0.9);
    border-top-left-radius: 3px;
    border-top-right-radius: 3px;
    border-bottom-left-radius: 3px;
    border-bottom-right-radius: 3px;
    overflow: hidden;
}

.interaction-progress-fill {
    position: absolute;
    left: 0;
    top: 0;
    bottom: 0;
    width: 0%;
    background-color: rgb(255, 218, 75);
    border-top-left-radius: 3px;
    border-top-right-radius: 3px;
    border-bottom-left-radius: 3px;
    border-bottom-right-radius: 3px;
}

.game-over {
    position: absolute;
    left: 50%;
    top: 50%;
    width: 310px;
    height: 128px;
    margin-left: -155px;
    margin-top: -64px;
    align-items: center;
    justify-content: center;
    background-color: rgba(7, 10, 15, 0.78);
    border-top-left-radius: 8px;
    border-top-right-radius: 8px;
    border-bottom-left-radius: 8px;
    border-bottom-right-radius: 8px;
    border-left-width: 1px;
    border-right-width: 1px;
    border-top-width: 1px;
    border-bottom-width: 1px;
    border-left-color: rgba(255, 80, 130, 0.65);
    border-right-color: rgba(255, 80, 130, 0.65);
    border-top-color: rgba(255, 80, 130, 0.65);
    border-bottom-color: rgba(255, 80, 130, 0.65);
    display: none;
}

.game-over-title {
    color: rgb(255, 82, 128);
    font-size: 34px;
    -unity-font-style: bold;
}

.game-over-subtitle {
    margin-top: 8px;
    color: rgb(208, 230, 238);
    font-size: 13px;
}

.restart-button {
    margin-top: 15px;
    width: 134px;
    height: 30px;
    color: rgb(12, 24, 28);
    background-color: rgb(181, 255, 239);
    border-top-left-radius: 5px;
    border-top-right-radius: 5px;
    border-bottom-left-radius: 5px;
    border-bottom-right-radius: 5px;
    border-left-width: 1px;
    border-right-width: 1px;
    border-top-width: 1px;
    border-bottom-width: 1px;
    border-left-color: rgba(255, 255, 255, 0.74);
    border-right-color: rgba(255, 255, 255, 0.52);
    border-top-color: rgba(255, 255, 255, 0.82);
    border-bottom-color: rgba(51, 158, 168, 0.72);
    -unity-font-style: bold;
    font-size: 13px;
}

.restart-button:hover {
    background-color: rgb(213, 255, 247);
}

.performer-selection {
    position: absolute;
    left: 0;
    right: 0;
    top: 0;
    bottom: 0;
    display: none;
    align-items: center;
    justify-content: center;
    background-color: rgba(2, 5, 9, 0.48);
}

.performer-panel {
    width: 980px;
    min-height: 330px;
    padding-left: 18px;
    padding-right: 18px;
    padding-top: 14px;
    padding-bottom: 14px;
    background-color: rgba(7, 12, 18, 0.9);
    border-top-left-radius: 8px;
    border-top-right-radius: 8px;
    border-bottom-left-radius: 8px;
    border-bottom-right-radius: 8px;
    border-left-width: 1px;
    border-right-width: 1px;
    border-top-width: 1px;
    border-bottom-width: 1px;
    border-left-color: rgba(90, 230, 255, 0.32);
    border-right-color: rgba(90, 230, 255, 0.32);
    border-top-color: rgba(160, 246, 255, 0.5);
    border-bottom-color: rgba(255, 80, 130, 0.32);
}

.performer-heading {
    height: 28px;
    color: rgb(181, 255, 239);
    font-size: 17px;
    -unity-text-align: middle-center;
    -unity-font-style: bold;
}

.performer-card-row {
    height: 262px;
    flex-direction: row;
    justify-content: center;
    align-items: stretch;
}

.performer-card {
    width: 152px;
    margin-left: 4px;
    margin-right: 4px;
    padding-left: 10px;
    padding-right: 10px;
    padding-top: 10px;
    padding-bottom: 10px;
    background-color: rgba(14, 22, 32, 0.94);
    border-top-left-radius: 7px;
    border-top-right-radius: 7px;
    border-bottom-left-radius: 7px;
    border-bottom-right-radius: 7px;
    border-left-width: 2px;
    border-right-width: 2px;
    border-top-width: 2px;
    border-bottom-width: 2px;
    border-left-color: rgba(181, 255, 239, 0.42);
    border-right-color: rgba(181, 255, 239, 0.42);
    border-top-color: rgba(181, 255, 239, 0.42);
    border-bottom-color: rgba(181, 255, 239, 0.42);
}

.performer-card-selected {
    background-color: rgba(28, 42, 58, 0.98);
    scale: 1.035 1.035;
    border-left-width: 4px;
    border-right-width: 4px;
    border-top-width: 4px;
    border-bottom-width: 4px;
    border-left-color: rgb(181, 255, 239);
    border-right-color: rgb(181, 255, 239);
    border-top-color: rgb(181, 255, 239);
    border-bottom-color: rgb(181, 255, 239);
}

.performer-card-unselected {
    background-color: rgba(14, 22, 32, 0.94);
}

.performer-portrait {
    height: 42px;
    border-top-left-radius: 6px;
    border-top-right-radius: 6px;
    border-bottom-left-radius: 6px;
    border-bottom-right-radius: 6px;
    margin-bottom: 8px;
    background-color: rgb(245, 194, 58);
}

.performer-name {
    color: rgb(248, 252, 255);
    font-size: 16px;
    -unity-font-style: bold;
}

.performer-title {
    margin-top: 1px;
    color: rgb(255, 218, 128);
    font-size: 11px;
    -unity-font-style: bold;
    white-space: normal;
}

.performer-description {
    margin-top: 5px;
    color: rgb(203, 222, 230);
    font-size: 11px;
    white-space: normal;
}

.performer-detail {
    margin-top: 7px;
    color: rgb(225, 242, 246);
    font-size: 11px;
    white-space: normal;
}

.performer-stats {
    margin-top: 8px;
    color: rgb(181, 255, 239);
    font-size: 11px;
    white-space: normal;
}

.performer-talents {
    margin-top: 8px;
    color: rgb(255, 235, 167);
    font-size: 11px;
    white-space: normal;
}

.performer-hint {
    height: 22px;
    margin-top: 8px;
    color: rgba(225, 242, 246, 0.82);
    font-size: 11px;
    -unity-text-align: middle-center;
}

.upgrade-selection {
    position: absolute;
    left: 0;
    right: 0;
    top: 0;
    bottom: 0;
    display: none;
    align-items: center;
    justify-content: center;
    background-color: rgba(2, 5, 9, 0.45);
}

.upgrade-panel {
    width: 760px;
    min-height: 250px;
    padding-left: 18px;
    padding-right: 18px;
    padding-top: 14px;
    padding-bottom: 14px;
    background-color: rgba(7, 12, 18, 0.88);
    border-top-left-radius: 8px;
    border-top-right-radius: 8px;
    border-bottom-left-radius: 8px;
    border-bottom-right-radius: 8px;
    border-left-width: 1px;
    border-right-width: 1px;
    border-top-width: 1px;
    border-bottom-width: 1px;
    border-left-color: rgba(90, 230, 255, 0.32);
    border-right-color: rgba(90, 230, 255, 0.32);
    border-top-color: rgba(160, 246, 255, 0.5);
    border-bottom-color: rgba(255, 80, 130, 0.32);
}

.upgrade-heading {
    height: 28px;
    color: rgb(181, 255, 239);
    font-size: 17px;
    -unity-text-align: middle-center;
    -unity-font-style: bold;
}

.upgrade-card-row {
    height: 168px;
    flex-direction: row;
    justify-content: center;
    align-items: stretch;
}

.upgrade-card {
    width: 230px;
    margin-left: 7px;
    margin-right: 7px;
    padding-left: 12px;
    padding-right: 12px;
    padding-top: 10px;
    padding-bottom: 10px;
    background-color: rgba(14, 22, 32, 0.94);
    border-top-left-radius: 7px;
    border-top-right-radius: 7px;
    border-bottom-left-radius: 7px;
    border-bottom-right-radius: 7px;
    border-left-width: 2px;
    border-right-width: 2px;
    border-top-width: 2px;
    border-bottom-width: 2px;
    border-left-color: rgba(235, 235, 225, 0.72);
    border-right-color: rgba(235, 235, 225, 0.72);
    border-top-color: rgba(235, 235, 225, 0.72);
    border-bottom-color: rgba(235, 235, 225, 0.72);
}

.upgrade-card-selected {
    background-color: rgba(28, 42, 58, 0.98);
    scale: 1.045 1.045;
    border-left-width: 4px;
    border-right-width: 4px;
    border-top-width: 4px;
    border-bottom-width: 4px;
}

.upgrade-card-unselected {
    background-color: rgba(14, 22, 32, 0.94);
}

.upgrade-rarity-strip {
    height: 5px;
    margin-left: -12px;
    margin-right: -12px;
    margin-top: -10px;
    margin-bottom: 8px;
    border-top-left-radius: 5px;
    border-top-right-radius: 5px;
    background-color: rgb(181, 255, 239);
}

.upgrade-rarity {
    color: rgb(255, 235, 167);
    font-size: 10px;
}

.upgrade-title {
    margin-top: 3px;
    color: rgb(248, 252, 255);
    font-size: 16px;
    -unity-font-style: bold;
}

.upgrade-description {
    margin-top: 7px;
    color: rgb(203, 222, 230);
    font-size: 12px;
    white-space: normal;
}

.upgrade-preview {
    margin-top: 8px;
    color: rgb(225, 242, 246);
    font-size: 12px;
    white-space: normal;
}

.upgrade-level {
    margin-top: 8px;
    color: rgb(181, 255, 239);
    font-size: 11px;
}

.upgrade-hint {
    height: 22px;
    margin-top: 8px;
    color: rgba(225, 242, 246, 0.82);
    font-size: 11px;
    -unity-text-align: middle-center;
}
";
}
