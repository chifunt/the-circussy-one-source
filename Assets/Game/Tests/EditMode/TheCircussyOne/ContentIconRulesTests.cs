using System;
using NUnit.Framework;
using TheCircussyOne.Content;
using TheCircussyOne.Rules;
using TheCircussyOne.Visuals;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

public sealed class ContentIconRulesTests
{
    [Test]
    public void WeaponUpgradeUsesOverrideBeforeTargetWeaponIcon()
    {
        Sprite weaponIcon = CreateSprite(Color.yellow);
        Sprite overrideIcon = CreateSprite(Color.cyan);
        WeaponDefinition weapon = TheCircussyOneTestObjects.CreateWeaponDefinition(null, "juggling_ball", "Juggling Ball");
        weapon.iconSprite = weaponIcon;
        UpgradeDefinition upgrade = TheCircussyOneTestObjects.CreateWeaponStatUpgrade(weapon);
        upgrade.iconSpriteOverride = overrideIcon;

        Assert.That(ContentIconRules.ForUpgrade(upgrade), Is.SameAs(overrideIcon));

        Object.DestroyImmediate(upgrade);
        Object.DestroyImmediate(weapon);
        DestroySprite(overrideIcon);
        DestroySprite(weaponIcon);
    }

    [Test]
    public void WeaponUpgradeFallsBackToTargetWeaponIcon()
    {
        Sprite weaponIcon = CreateSprite(Color.magenta);
        WeaponDefinition weapon = TheCircussyOneTestObjects.CreateWeaponDefinition(null, "fire_hoop", "Fire Hoop");
        weapon.iconSprite = weaponIcon;
        UpgradeDefinition upgrade = TheCircussyOneTestObjects.CreateWeaponStatUpgrade(weapon);

        Assert.That(ContentIconRules.ForUpgrade(upgrade), Is.SameAs(weaponIcon));

        Object.DestroyImmediate(upgrade);
        Object.DestroyImmediate(weapon);
        DestroySprite(weaponIcon);
    }

    [Test]
    public void PerformerSpecificTalentFallsBackToPerformerPortrait()
    {
        Sprite portrait = CreateSprite(Color.red);
        var performer = ScriptableObject.CreateInstance<PerformerDefinition>();
        performer.performerId = "mira";
        performer.displayName = "Mira";
        performer.portraitSprite = portrait;

        var talent = ScriptableObject.CreateInstance<TalentDefinition>();
        talent.poolKind = TalentPoolKind.PerformerSpecific;
        talent.performerDefinition = performer;
        talent.performerId = performer.Id;

        Assert.That(ContentIconRules.ForTalent(talent), Is.SameAs(portrait));

        Object.DestroyImmediate(talent);
        Object.DestroyImmediate(performer);
        DestroySprite(portrait);
    }

    [Test]
    public void UpgradeChoiceUsesResolvedContentIcon()
    {
        Sprite icon = CreateSprite(Color.green);
        WeaponDefinition weapon = TheCircussyOneTestObjects.CreateWeaponDefinition(null, "cannon", "Cannon");
        weapon.iconSprite = icon;

        var choice = new UpgradeChoice(weapon);

        Assert.That(choice.IconSprite, Is.SameAs(icon));

        Object.DestroyImmediate(weapon);
        DestroySprite(icon);
    }

    [Test]
    public void UpgradeChoiceTalentCanUseSelectedPerformerPortraitFallback()
    {
        Sprite portrait = CreateSprite(Color.blue);
        var performer = ScriptableObject.CreateInstance<PerformerDefinition>();
        performer.performerId = "sola_hoop_dancer";
        performer.displayName = "Sola";
        performer.portraitSprite = portrait;

        var talent = ScriptableObject.CreateInstance<TalentDefinition>();
        talent.talentId = "hoop_flow";
        talent.displayName = "Hoop Flow";
        talent.poolKind = TalentPoolKind.PerformerSpecific;
        talent.performerId = performer.Id;
        var choice = new UpgradeChoice(talent, 0);

        Assert.That(ContentIconRules.ForUpgradeChoice(choice, performer), Is.SameAs(portrait));

        Object.DestroyImmediate(talent);
        Object.DestroyImmediate(performer);
        DestroySprite(portrait);
    }

    [Test]
    public void ContentIconVisualsApplySpriteFallbackAndBorderThenClear()
    {
        Sprite sprite = CreateSprite(Color.white);
        var element = new VisualElement();
        Color fallback = Color.yellow;
        Color border = Color.red;

        ContentIconVisuals.Apply(element, sprite, fallback, border);

        Assert.That(element.style.backgroundColor.value, Is.EqualTo(IconFrameBlack()));
        Assert.That(element.style.backgroundImage.keyword, Is.EqualTo(StyleKeyword.Null));
        Assert.That(element.style.borderTopColor.value, Is.EqualTo(border));
        Assert.That(element.style.borderRightColor.value, Is.EqualTo(border));
        Image image = element.Q<Image>(ContentIconVisuals.ImageElementName);
        Assert.That(image, Is.Not.Null);
        Assert.That(element.IndexOf(image), Is.EqualTo(0));
        Assert.That(image.ClassListContains(ContentIconVisuals.ImageClassName), Is.True);
        Assert.That(image.sprite, Is.SameAs(sprite));
        Assert.That(image.scaleMode, Is.EqualTo(ScaleMode.ScaleToFit));
        Assert.That(image.pickingMode, Is.EqualTo(PickingMode.Ignore));
        Assert.That(image.style.display.value, Is.EqualTo(DisplayStyle.Flex));

        ContentIconVisuals.Clear(element, Color.black);

        Assert.That(element.style.backgroundColor.value, Is.EqualTo(IconFrameBlack()));
        Assert.That(element.style.backgroundImage.keyword, Is.EqualTo(StyleKeyword.Null));
        Assert.That(image.sprite, Is.Null);
        Assert.That(image.style.display.value, Is.EqualTo(DisplayStyle.None));

        DestroySprite(sprite);
    }

    [Test]
    public void PlaceholderIconGeneratorImportsSpritesAsFullRect()
    {
        string id = "icon_import_test_" + Guid.NewGuid().ToString("N");
        string path = $"{TheCircussyOneAssetPaths.GeneratedContentIconFolder}/Weapons/{id}.png";
        WeaponDefinition weapon = TheCircussyOneTestObjects.CreateWeaponDefinition(null, id, "Icon Import Test");

        try
        {
            int assigned = TheCircussyOneContentIconGenerator.GenerateMissingForSelected(weapon);

            Assert.That(assigned, Is.EqualTo(1));
            Assert.That(weapon.iconSprite, Is.Not.Null);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            Assert.That(importer, Is.Not.Null);
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            Assert.That(settings.spriteMeshType, Is.EqualTo(SpriteMeshType.FullRect));
        }
        finally
        {
            AssetDatabase.DeleteAsset(path);
            Object.DestroyImmediate(weapon);
        }
    }

    private static Color IconFrameBlack()
    {
        return new Color(0.015f, 0.018f, 0.024f, 0.98f);
    }

    private static Sprite CreateSprite(Color color)
    {
        var texture = new Texture2D(4, 4, TextureFormat.RGBA32, false);
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply(false, false);
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), texture.width);
    }

    private static void DestroySprite(Sprite sprite)
    {
        if (sprite == null)
        {
            return;
        }

        Texture2D texture = sprite.texture;
        Object.DestroyImmediate(sprite);
        Object.DestroyImmediate(texture);
    }
}
