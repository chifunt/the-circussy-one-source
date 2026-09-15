using System.Linq;
using NUnit.Framework;
using UnityEngine;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.Runtime;
using TheCircussyOne.Rules;

public sealed class EnemyDefinitionTests
{
    private GameConfig _config;

    [SetUp]
    public void SetUp()
    {
        TheCircussyOneTestObjects.DestroyAll();
        _config = TheCircussyOneTestObjects.CreateConfig();
    }

    [TearDown]
    public void TearDown()
    {
        TheCircussyOneTestObjects.DestroyAll();
        TheCircussyOneTestObjects.Destroy(_config);
    }

    [Test]
    public void NormalEnemyDefaultsMatchGameConfigAndEnableSupportStacking()
    {
        _config.enemyHealth = 11;
        _config.enemyHealthGrowthPerMinute = 3.5f;
        _config.enemyMoveSpeed = 3.25f;
        _config.enemyMoveSpeedGrowthPerMinute = 0.45f;
        _config.enemyTurnDegreesPerSecond = 90f;
        _config.enemyContactDamage = 7;
        _config.enemyPileClimbingEnabled = true;
        _config.enemyPileMaxLayers = 4;
        _config.enemySupportClimbSpeedMultiplier = 0.35f;
        _config.enemySupportClimbSeparationMultiplier = 0.2f;
        var blueGem = TheCircussyOneTestObjects.CreateXpGemDefinition("blue_xp_gem", "Blue XP Gem", 7, Color.cyan);
        var catalog = TheCircussyOneTestObjects.CreateXpGemCatalog(blueGem);
        catalog.defaultEnemyXpBudget = 30;
        catalog.defaultDropStyle = XpDropStyle.Scatter;

        EnemyDefinition definition = ScriptableObject.CreateInstance<EnemyDefinition>();
        definition.ApplyDefaultsFromGameConfig(_config, catalog);

        Assert.That(definition.enemyId, Is.EqualTo("normal_enemy"));
        Assert.That(definition.displayName, Is.EqualTo("Normal Enemy"));
        Assert.That(definition.rarity, Is.EqualTo(ContentRarity.Common));
        Assert.That(definition.Tags.HasTag(ContentTag.Swarm), Is.True);
        Assert.That(definition.Tags.HasTag(ContentTag.Damage), Is.True);
        Assert.That(definition.Tags.HasTag(ContentTag.Movement), Is.True);
        Assert.That(definition.Tags.HasTag(ContentTag.Physical), Is.False);
        Assert.That(definition.behaviorType, Is.EqualTo(EnemyBehaviorType.Chaser));
        Assert.That(definition.spawnWeight, Is.EqualTo(100f));
        Assert.That(definition.baseHealth, Is.EqualTo(_config.enemyHealth));
        Assert.That(definition.healthGrowthPerMinute, Is.EqualTo(_config.enemyHealthGrowthPerMinute));
        Assert.That(definition.baseMoveSpeed, Is.EqualTo(_config.enemyMoveSpeed));
        Assert.That(definition.moveSpeedGrowthPerMinute, Is.EqualTo(_config.enemyMoveSpeedGrowthPerMinute));
        Assert.That(definition.turnDegreesPerSecond, Is.EqualTo(_config.enemyTurnDegreesPerSecond));
        Assert.That(definition.contactDamage, Is.EqualTo(_config.enemyContactDamage));
        Assert.That(definition.xpBudget, Is.EqualTo(30));
        Assert.That(definition.dropStyle, Is.EqualTo(XpDropStyle.Scatter));
        Assert.That(definition.body.hurtboxRadius, Is.EqualTo(_config.enemyHurtboxRadius));
        Assert.That(definition.body.contactHitboxSize, Is.EqualTo(_config.enemyContactHitboxSize));
        Assert.That(definition.body.movementBodyRadius, Is.EqualTo(_config.enemyMovementBodyRadius));
        Assert.That(definition.locomotion.mode, Is.EqualTo(EnemyLocomotionMode.Grounded));
        Assert.That(definition.climb.canClimbEnvironment, Is.EqualTo(_config.enemyClimbingEnabled));
        Assert.That(definition.stack.policy, Is.EqualTo(EnemyStackPolicy.SupportBased));
        Assert.That(definition.stack.canClimbEnemies, Is.True);
        Assert.That(definition.stack.canBeStackedOn, Is.True);
        Assert.That(definition.stack.maxStackLayers, Is.EqualTo(_config.enemyPileMaxLayers));
        Assert.That(definition.stack.supportClimbSpeedMultiplier, Is.EqualTo(_config.enemySupportClimbSpeedMultiplier));
        Assert.That(definition.stack.supportClimbSeparationMultiplier, Is.EqualTo(_config.enemySupportClimbSeparationMultiplier));

        Object.DestroyImmediate(definition);
        Object.DestroyImmediate(blueGem);
        Object.DestroyImmediate(catalog);
    }

    [Test]
    public void EnemyCatalogValidatesBadDefinitions()
    {
        EnemyDefinition valid = TheCircussyOneTestObjects.CreateEnemyDefinition(_config);
        EnemyDefinition duplicate = TheCircussyOneTestObjects.CreateEnemyDefinition(_config);
        EnemyDefinition invalid = TheCircussyOneTestObjects.CreateEnemyDefinition(_config);
        invalid.enemyId = "";
        invalid.displayName = "";
        invalid.baseHealth = 0;
        invalid.baseMoveSpeed = 0f;
        invalid.contactDamage = 0;
        invalid.behaviorType = (EnemyBehaviorType)999;
        invalid.spawnWeight = 0f;
        invalid.body.contactHitboxSize = Vector3.zero;
        invalid.locomotion.hoverSeconds = 0f;
        invalid.stack.maxStackLayers = -1;
        invalid.stack.supportClimbSpeedMultiplier = 0f;
        invalid.stack.supportClimbSeparationMultiplier = 1.5f;
        EnemyCatalog catalog = TheCircussyOneTestObjects.CreateEnemyCatalog(valid, duplicate, invalid, null);

        var issues = catalog.ValidateContent();

        Assert.That(issues.Any(issue => issue.Code == "content.duplicate-id"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "content.invalid-id"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "content.missing-display-name"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "catalog.null-entry"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "enemy.non-positive-health"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "enemy.non-positive-move-speed"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "enemy.non-positive-contact-damage"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "enemy.invalid-behavior-type"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "enemy.non-positive-spawn-weight"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "enemy.invalid-contact-hitbox-size"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "enemy.invalid-floating-hover"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "enemy.invalid-stack-settings"), Is.True);

        Object.DestroyImmediate(valid);
        Object.DestroyImmediate(duplicate);
        Object.DestroyImmediate(invalid);
        Object.DestroyImmediate(catalog);
    }

    [Test]
    public void EnemyFactorySpawnsFromDefinitionValues()
    {
        _config.enemyHealth = 4;
        _config.enemyMoveSpeed = 2f;
        _config.enemyContactDamage = 2;
        EnemyDefinition definition = TheCircussyOneTestObjects.CreateEnemyDefinition(_config);
        definition.baseHealth = 9;
        definition.healthGrowthPerMinute = 0f;
        definition.baseMoveSpeed = 4f;
        definition.moveSpeedGrowthPerMinute = 0f;
        definition.turnDegreesPerSecond = 90f;
        definition.contactDamage = 7;
        definition.behaviorType = EnemyBehaviorType.Ranged;
        definition.spawnWeight = 35f;
        definition.body.hurtboxRadius = 0.8f;
        definition.body.hurtboxHeightOffset = 1.2f;
        definition.body.contactHitboxSize = new Vector3(0.6f, 0.7f, 0.8f);
        definition.body.contactHitboxOffset = new Vector3(0.1f, 0.5f, 0.4f);
        definition.body.movementBodyRadius = 0.45f;
        definition.body.movementBodyHeight = 1.2f;
        definition.body.movementBodyOffset = new Vector3(0.05f, 0.6f, 0.02f);
        definition.locomotion.mode = EnemyLocomotionMode.Floating;
        definition.locomotion.hoverHeight = 0.35f;
        definition.locomotion.hoverSeconds = 0.6f;
        definition.xpBudget = 6;
        definition.dropStyle = XpDropStyle.Bonus;
        EnemyCatalog catalog = TheCircussyOneTestObjects.CreateEnemyCatalog(definition);
        var registry = new ActorRegistry();
        var factory = new EnemyFactory(
            _config,
            null,
            catalog,
            registry,
            TheCircussyOneTestObjects.CreateEnemyPrefab(),
            TheCircussyOneTestObjects.CreateRoot("Enemies").transform);

        EnemyRuntime enemy = factory.Spawn(Vector3.zero, level: 1, elapsedSeconds: 0f);
        var hurtbox = enemy.View.Hurtbox.Collider as SphereCollider;
        var contact = enemy.View.ContactHitboxCollider as BoxCollider;
        var movement = enemy.View.MovementBodyCollider as CapsuleCollider;

        Assert.That(enemy.Definition, Is.SameAs(definition));
        Assert.That(enemy.MaxHealth, Is.EqualTo(9));
        Assert.That(enemy.MoveSpeed, Is.EqualTo(4f));
        Assert.That(enemy.TurnDegreesPerSecond, Is.EqualTo(90f));
        Assert.That(enemy.ContactDamage, Is.EqualTo(7));
        Assert.That(enemy.BehaviorType, Is.EqualTo(EnemyBehaviorType.Ranged));
        Assert.That(enemy.SpawnWeight, Is.EqualTo(35f));
        Assert.That(enemy.XpBudget, Is.EqualTo(6));
        Assert.That(enemy.DropStyle, Is.EqualTo(XpDropStyle.Bonus));
        Assert.That(enemy.LocomotionProfile.mode, Is.EqualTo(EnemyLocomotionMode.Floating));
        Assert.That(enemy.LocomotionProfile.hoverHeight, Is.EqualTo(0.35f));
        Assert.That(enemy.LocomotionProfile.hoverSeconds, Is.EqualTo(0.6f));
        Assert.That(enemy.View.IsIdleHoverActive, Is.True);
        Assert.That(hurtbox.radius, Is.EqualTo(0.8f).Within(0.0001f));
        Assert.That(enemy.View.Hurtbox.transform.localPosition, Is.EqualTo(new Vector3(0f, 1.2f, 0f)));
        Assert.That(contact.size, Is.EqualTo(definition.body.contactHitboxSize));
        Assert.That(enemy.View.ContactHitbox.transform.localPosition, Is.EqualTo(definition.body.contactHitboxOffset));
        Assert.That(movement.radius, Is.EqualTo(definition.body.movementBodyRadius).Within(0.0001f));
        Assert.That(movement.height, Is.EqualTo(definition.body.movementBodyHeight).Within(0.0001f));
        Assert.That(enemy.View.MovementBody.transform.localPosition, Is.EqualTo(definition.body.movementBodyOffset));

        Object.DestroyImmediate(definition);
        Object.DestroyImmediate(catalog);
    }

    [Test]
    public void EnemyCatalogWarnsWhenFloatingEnemyUsesSupportStacking()
    {
        EnemyDefinition floating = TheCircussyOneTestObjects.CreateEnemyDefinition(_config);
        floating.locomotion.mode = EnemyLocomotionMode.Floating;
        floating.stack.policy = EnemyStackPolicy.SupportBased;
        floating.stack.canClimbEnemies = true;
        floating.stack.canBeStackedOn = true;
        EnemyCatalog catalog = TheCircussyOneTestObjects.CreateEnemyCatalog(floating);

        var issues = catalog.ValidateContent();

        Assert.That(issues.Any(issue => issue.Code == "enemy.floating-support-stacking"), Is.True);

        Object.DestroyImmediate(floating);
        Object.DestroyImmediate(catalog);
    }

    [Test]
    public void EnemyCatalogWarnsWhenFloatingModelHasNoFloatingIdleClip()
    {
        EnemyDefinition floating = TheCircussyOneTestObjects.CreateEnemyDefinition(_config);
        floating.worldPrefab = TheCircussyOneTestObjects.CreateRuntimeModelPrefab("Floating Enemy Model");
        floating.animation.idle = new AnimationClip { name = "Ground Idle" };
        floating.locomotion.mode = EnemyLocomotionMode.Floating;
        EnemyCatalog catalog = TheCircussyOneTestObjects.CreateEnemyCatalog(floating);

        var issues = catalog.ValidateContent();

        Assert.That(issues.Any(issue => issue.Code == "enemy.floating-missing-animation"), Is.True);

        Object.DestroyImmediate(floating.animation.idle);
        Object.DestroyImmediate(floating);
        Object.DestroyImmediate(catalog);
    }

    [Test]
    public void ChangingLegacyConfigAfterCatalogExistsDoesNotOverwriteDefinition()
    {
        EnemyDefinition definition = TheCircussyOneTestObjects.CreateEnemyDefinition(_config);
        definition.baseHealth = 13;
        definition.baseMoveSpeed = 4.5f;
        EnemyCatalog catalog = TheCircussyOneTestObjects.CreateEnemyCatalog(definition);
        _config.enemyHealth = 99;
        _config.enemyMoveSpeed = 9f;
        var registry = new ActorRegistry();
        var factory = new EnemyFactory(
            _config,
            null,
            catalog,
            registry,
            TheCircussyOneTestObjects.CreateEnemyPrefab(),
            TheCircussyOneTestObjects.CreateRoot("Enemies").transform);

        EnemyRuntime enemy = factory.Spawn(Vector3.zero, level: 1, elapsedSeconds: 0f);

        Assert.That(enemy.MaxHealth, Is.EqualTo(13));
        Assert.That(enemy.MoveSpeed, Is.EqualTo(4.5f));

        Object.DestroyImmediate(definition);
        Object.DestroyImmediate(catalog);
    }
}
